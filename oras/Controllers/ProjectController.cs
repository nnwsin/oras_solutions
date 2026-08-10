using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using oras.DTOs.Project;
using oras.Services.Interfaces;

namespace oras.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // GET: api/Project
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            return Ok(await _projectService.GetAllProjectsAsync());
        }

        // GET: api/Project/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            return Ok(await _projectService.GetProjectByIdAsync(id));
        }

        // POST: api/Project
        [HttpPost]
        public async Task<IActionResult> CreateProject(CreateProjectDto createProjectDto)
        {
            var project = await _projectService.CreateProjectAsync(createProjectDto);

            return CreatedAtAction(
                nameof(GetProjectById),
                new { id = project.ProjectId },
                project);
        }

        // PUT: api/Project/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto updateProjectDto)
        {
            return Ok(await _projectService.UpdateProjectAsync(id, updateProjectDto));
        }

        // DELETE: api/Project/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            await _projectService.DeleteProjectAsync(id);

            return NoContent();
        }
    }
}