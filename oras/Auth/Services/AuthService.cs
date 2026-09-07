using oras.Auth.Dto;
using oras.Auth.Interfaces;
using oras.Enums;
using oras.Exceptions;
using oras.Models;
using oras.Repositories.Interfaces;
using oras.Services.Interfaces;

namespace oras.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtService jwtService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);

            if (existingUser != null)
                throw new BadRequestException("Email already exists.");

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = _passwordService.HashPassword(registerDto.Password),
                Role = UserRole.Employee 
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

            if (user == null)
                throw new UnauthorizedException("Invalid email or password.");

            var isPasswordValid =
                _passwordService.VerifyPassword(
                    loginDto.Password,
                    user.Password);

            if (!isPasswordValid)
                throw new UnauthorizedException("Invalid email or password.");

            var token = _jwtService.GenerateToken(user);
            var expiryInMinutes = Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(expiryInMinutes),
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}