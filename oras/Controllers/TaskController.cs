using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using oras.DTOs.Tasks;
using oras.Enums;
using oras.Services.Interfaces;

namespace oras.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {


        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // GET: api/Task
        // GET: api/Task?projectId=1
        // GET: api/Task?status=Pending
        // GET: api/Task?assigneeId=2
        // GET: api/Task?projectId=1&status=Completed
        [HttpGet]
        public async Task<IActionResult> GetAllTasks(
            [FromQuery] int? projectId,
            [FromQuery] AssignedTaskStatus? status,
            [FromQuery] int? assigneeId)
        {
            var roleClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                           ?? User?.FindFirst("sub");

            int? effectiveAssigneeId = assigneeId;

            // Manager and Employee must only see tasks assigned to them
            if ((roleClaim == "Manager" || roleClaim == "Employee") && userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                effectiveAssigneeId = currentUserId;
            }

            var tasks = await _taskService.GetAllTasksAsync(
                projectId,
                status,
                effectiveAssigneeId);

            return Ok(tasks);
        }

        // GET: api/Task/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            return Ok(await _taskService.GetTaskByIdAsync(id));
        }

        // POST: api/Task
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreateTask(CreateTaskDto createTaskDto)
        {
            var task = await _taskService.CreateTaskAsync(createTaskDto);

            return CreatedAtAction(
                nameof(GetTaskById),
                new { id = task.TaskId },
                task);
        }

        // PUT: api/Task/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(
            int id,
            UpdateTaskDto updateTaskDto)
        {
            return Ok(await _taskService.UpdateTaskAsync(id, updateTaskDto));
        }

        // DELETE: api/Task/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            await _taskService.DeleteTaskAsync(id);

            return NoContent();
        }


    }
}
