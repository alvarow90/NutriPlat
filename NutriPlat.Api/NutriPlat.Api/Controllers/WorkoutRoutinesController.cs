// NutriPlat.Api/Controllers/WorkoutRoutinesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriPlat.Api.Services;
using NutriPlat.Shared.Dtos; // <-- ASEGÚRATE DE ESTE USING
using NutriPlat.Shared.Enums;
using System; // Para DateTime, Guid
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NutriPlat.Api.Controllers // Asegúrate de que este espacio de nombres sea correcto
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkoutRoutinesController : ControllerBase
    {
        private readonly IWorkoutRoutineService _workoutRoutineService;
        private readonly ILogger<WorkoutRoutinesController> _logger;

        public WorkoutRoutinesController(IWorkoutRoutineService workoutRoutineService, ILogger<WorkoutRoutinesController> logger)
        {
            _workoutRoutineService = workoutRoutineService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(UserRole.Trainer)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> CreateWorkoutRoutine([FromBody] WorkoutRoutineDto routineDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(creatorUserId)) return Unauthorized("Token inválido.");
            var createdRoutine = await _workoutRoutineService.CreateRoutineAsync(routineDto, creatorUserId);
            if (createdRoutine == null) return BadRequest(new { Message = "No se pudo crear la rutina de entrenamiento. Verifique permisos." });
            return CreatedAtAction(nameof(GetWorkoutRoutineById), new { routineId = createdRoutine.Id }, createdRoutine);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWorkoutRoutines()
        {
            var routines = await _workoutRoutineService.GetAllRoutinesAsync();
            return Ok(routines);
        }

        [HttpGet("{routineId}")]
        public async Task<IActionResult> GetWorkoutRoutineById(string routineId)
        {
            var routine = await _workoutRoutineService.GetRoutineByIdAsync(routineId);
            if (routine == null) return NotFound(new { Message = $"Rutina con ID '{routineId}' no encontrada." });
            return Ok(routine);
        }

        [HttpPut("{routineId}")]
        [Authorize(Roles = $"{nameof(UserRole.Trainer)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> UpdateWorkoutRoutine(string routineId, [FromBody] WorkoutRoutineDto routineDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(requestingUserId)) return Unauthorized("Token inválido.");
            var originalRoutine = await _workoutRoutineService.GetRoutineByIdAsync(routineId);
            if (originalRoutine == null) return NotFound(new { Message = $"Rutina con ID '{routineId}' no encontrada." });
            bool isAdmin = User.IsInRole(nameof(UserRole.Admin));
            if (originalRoutine.TrainerId != requestingUserId && !isAdmin) return Forbid();
            var updatedRoutine = await _workoutRoutineService.UpdateRoutineAsync(routineId, routineDto, requestingUserId);
            if (updatedRoutine == null) return BadRequest(new { Message = "No se pudo actualizar la rutina." });
            return Ok(updatedRoutine);
        }

        [HttpDelete("{routineId}")]
        [Authorize(Roles = $"{nameof(UserRole.Trainer)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> DeleteWorkoutRoutine(string routineId)
        {
            var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(requestingUserId)) return Unauthorized("Token inválido.");
            var routineExists = await _workoutRoutineService.GetRoutineByIdAsync(routineId);
            if (routineExists == null) return NotFound(new { Message = $"Rutina con ID '{routineId}' no encontrada." });
            bool isAdmin = User.IsInRole(nameof(UserRole.Admin));
            if (routineExists.TrainerId != requestingUserId && !isAdmin) return Forbid();
            var deleteResult = await _workoutRoutineService.DeleteRoutineAsync(routineId, requestingUserId);
            if (!deleteResult) return BadRequest(new { Message = "No se pudo eliminar la rutina." });
            return NoContent();
        }

        [HttpPost("assign")]
        [Authorize(Roles = nameof(UserRole.Trainer))]
        public async Task<IActionResult> AssignRoutineToClient([FromBody] AssignWorkoutRoutineDto assignmentDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(trainerId)) return Unauthorized("Token inválido.");
            var createdAssignment = await _workoutRoutineService.AssignRoutineToClientAsync(assignmentDto, trainerId);
            if (createdAssignment == null) return BadRequest(new { Message = "No se pudo asignar la rutina. Verifique los datos o permisos." });
            return StatusCode(StatusCodes.Status201Created, createdAssignment);
        }

        [HttpGet("client/myroutines")]
        [Authorize(Roles = nameof(UserRole.Client))]
        public async Task<IActionResult> GetMyAssignedWorkoutRoutines()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized("Token inválido.");
            var assignedRoutines = await _workoutRoutineService.GetAssignedRoutinesForClientAsync(clientId);
            return Ok(assignedRoutines);
        }

        [HttpGet("trainer/assignedroutines")]
        [Authorize(Roles = nameof(UserRole.Trainer))]
        public async Task<IActionResult> GetAssignmentsByMeAsTrainer()
        {
            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(trainerId)) return Unauthorized("Token inválido.");
            var assignments = await _workoutRoutineService.GetAssignmentsByTrainerAsync(trainerId);
            return Ok(assignments);
        }

        [HttpDelete("client/{clientId}/routine/{workoutRoutineId}/unassign")]
        [Authorize(Roles = $"{nameof(UserRole.Trainer)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> UnassignRoutineFromClient(string clientId, string workoutRoutineId)
        {
            var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(requestingUserId)) return Unauthorized("Token inválido.");
            var result = await _workoutRoutineService.UnassignRoutineFromClientAsync(clientId, workoutRoutineId, requestingUserId);
            if (!result) return NotFound(new { Message = "Asignación de rutina no encontrada o no tiene permiso para desasignar." });
            return NoContent();
        }
    }
}
