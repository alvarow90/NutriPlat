// NutriPlat.Api/Controllers/NutritionPlansController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriPlat.Api.Services; // Para INutritionPlanService
using NutriPlat.Shared.Dtos; // Para DTOs
using NutriPlat.Shared.Enums; // Para UserRole
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NutriPlat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Todos los endpoints requieren autenticación por defecto
    public class NutritionPlansController : ControllerBase
    {
        private readonly INutritionPlanService _nutritionPlanService;
        private readonly ILogger<NutritionPlansController> _logger;

        public NutritionPlansController(INutritionPlanService nutritionPlanService, ILogger<NutritionPlansController> logger)
        {
            _nutritionPlanService = nutritionPlanService;
            _logger = logger;
        }

        /// <summary>
        /// Crea un nuevo plan de nutrición. Solo para Nutricionistas o Administradores.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(NutritionPlanDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = $"{nameof(UserRole.Nutritionist)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> CreateNutritionPlan([FromBody] NutritionPlanDto planDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(creatorUserId)) return Unauthorized("Token inválido.");

            // La lógica de que solo un nutricionista/admin puede crear ya está en el servicio,
            // pero el atributo [Authorize(Roles=...)] lo refuerza a nivel de endpoint.
            var createdPlan = await _nutritionPlanService.CreatePlanAsync(planDto, creatorUserId);
            if (createdPlan == null)
            {
                // El servicio devuelve null si el creador no tiene el rol adecuado o hay error de BD.
                _logger.LogWarning("Falló la creación del plan por el usuario {CreatorUserId}. Posiblemente rol incorrecto o error de servicio.", creatorUserId);
                return BadRequest(new { Message = "No se pudo crear el plan de nutrición. Verifique sus permisos o los datos." });
            }
            return CreatedAtAction(nameof(GetNutritionPlanById), new { planId = createdPlan.Id }, createdPlan);
        }

        /// <summary>
        /// Obtiene todos los planes de nutrición base. Accesible por cualquier usuario autenticado.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NutritionPlanDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllNutritionPlans()
        {
            var plans = await _nutritionPlanService.GetAllPlansAsync();
            return Ok(plans);
        }

        /// <summary>
        /// Obtiene un plan de nutrición específico por su ID. Accesible por cualquier usuario autenticado.
        /// </summary>
        [HttpGet("{planId}")]
        [ProducesResponseType(typeof(NutritionPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNutritionPlanById(string planId)
        {
            var plan = await _nutritionPlanService.GetPlanByIdAsync(planId);
            if (plan == null) return NotFound(new { Message = $"Plan de nutrición con ID '{planId}' no encontrado." });
            return Ok(plan);
        }

        /// <summary>
        /// Actualiza un plan de nutrición existente. Solo para el Nutricionista creador o Administradores.
        /// </summary>
        [HttpPut("{planId}")]
        [ProducesResponseType(typeof(NutritionPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = $"{nameof(UserRole.Nutritionist)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> UpdateNutritionPlan(string planId, [FromBody] NutritionPlanDto planDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(requestingUserId)) return Unauthorized("Token inválido.");

            var originalPlan = await _nutritionPlanService.GetPlanByIdAsync(planId);
            if (originalPlan == null) return NotFound(new { Message = $"Plan con ID '{planId}' no encontrado." });

            bool isAdmin = User.IsInRole(nameof(UserRole.Admin));
            // El servicio NutritionPlanService.UpdatePlanAsync ya contiene la lógica de
            // si el requestingUserId es el NutritionistId del plan o si es Admin.
            // El atributo [Authorize(Roles)] ya filtró que sea Nutricionista o Admin.
            // Si es Nutricionista, el servicio verificará si es el propietario.
            if (originalPlan.NutritionistId != requestingUserId && !isAdmin)
            {
                _logger.LogWarning("Nutricionista {RequestingUserId} intentó actualizar plan {PlanId} que no le pertenece.", requestingUserId, planId);
                // Aunque el servicio lo validará, podemos devolver Forbid aquí para ser más explícitos.
                // Sin embargo, si el servicio ya maneja esto devolviendo null, el BadRequest de abajo se activará.
                // Para RBAC más estricto a nivel de controlador:
                // return Forbid();
            }

            var updatedPlan = await _nutritionPlanService.UpdatePlanAsync(planId, planDto, requestingUserId);
            if (updatedPlan == null)
            {
                // Esto puede ser porque no se encontró, no tiene permiso (según lógica del servicio), o error de BD.
                // Si es por permiso y no es admin ni propietario, el servicio devuelve null.
                if (originalPlan.NutritionistId != requestingUserId && !isAdmin) return Forbid();
                return BadRequest(new { Message = "No se pudo actualizar el plan." });
            }
            return Ok(updatedPlan);
        }

        /// <summary>
        /// Elimina un plan de nutrición existente. Solo para el Nutricionista creador o Administradores.
        /// </summary>
        [HttpDelete("{planId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = $"{nameof(UserRole.Nutritionist)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> DeleteNutritionPlan(string planId)
        {
            var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(requestingUserId)) return Unauthorized("Token inválido.");

            var planExists = await _nutritionPlanService.GetPlanByIdAsync(planId);
            if (planExists == null) return NotFound(new { Message = $"Plan con ID '{planId}' no encontrado." });

            bool isAdmin = User.IsInRole(nameof(UserRole.Admin));
            // Similar a Update, el servicio ya tiene la lógica de propietario/admin.
            if (planExists.NutritionistId != requestingUserId && !isAdmin)
            {
                // return Forbid();
            }

            var deleteResult = await _nutritionPlanService.DeletePlanAsync(planId, requestingUserId);
            if (!deleteResult)
            {
                // Si es false, puede ser por no ser propietario/admin (manejado por el servicio) o error de BD.
                if (planExists.NutritionistId != requestingUserId && !isAdmin) return Forbid();
                return BadRequest(new { Message = "No se pudo eliminar el plan." });
            }
            return NoContent();
        }

        // --- Endpoints de Asignación ---

        /// <summary>
        /// Asigna un plan de nutrición a un cliente. Solo para Nutricionistas.
        /// </summary>
        [HttpPost("assign")]
        [ProducesResponseType(typeof(UserNutritionPlanDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = nameof(UserRole.Nutritionist))] // Solo Nutricionistas pueden asignar
        public async Task<IActionResult> AssignPlanToClient([FromBody] AssignNutritionPlanDto assignmentDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var nutritionistId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nutritionistId)) return Unauthorized("Token inválido.");

            var createdAssignment = await _nutritionPlanService.AssignPlanToClientAsync(assignmentDto, nutritionistId);
            if (createdAssignment == null) return BadRequest(new { Message = "No se pudo asignar el plan. Verifique los datos o permisos." });
            return StatusCode(StatusCodes.Status201Created, createdAssignment);
        }

        /// <summary>
        /// Obtiene los planes de nutrición asignados al cliente autenticado. Solo para Clientes.
        /// </summary>
        [HttpGet("client/my")]
        [ProducesResponseType(typeof(IEnumerable<UserNutritionPlanDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = nameof(UserRole.Client))] // Solo Clientes pueden ver sus planes
        public async Task<IActionResult> GetMyAssignedNutritionPlans()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized("Token inválido.");
            var assignedPlans = await _nutritionPlanService.GetAssignedPlansForClientAsync(clientId);
            return Ok(assignedPlans);
        }

        /// <summary>
        /// Obtiene todas las asignaciones de planes realizadas por el nutricionista autenticado. Solo para Nutricionistas.
        /// </summary>
        [HttpGet("nutritionist/assigned")]
        [ProducesResponseType(typeof(IEnumerable<UserNutritionPlanDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = nameof(UserRole.Nutritionist))] // Solo Nutricionistas
        public async Task<IActionResult> GetAssignmentsByMeAsNutritionist()
        {
            var nutritionistId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nutritionistId)) return Unauthorized("Token inválido.");
            var assignments = await _nutritionPlanService.GetAssignmentsByNutritionistAsync(nutritionistId);
            return Ok(assignments);
        }

        /// <summary>
        /// Desasigna un plan de nutrición de un cliente. Solo para Nutricionistas (el que asignó) o Admins.
        /// </summary>
        [HttpDelete("client/{clientId}/plan/{nutritionPlanId}/unassign")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = $"{nameof(UserRole.Nutritionist)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> UnassignPlanFromClient(string clientId, string nutritionPlanId)
        {
            var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(requestingUserId)) return Unauthorized("Token inválido.");

            // La lógica de si el nutricionista es el que asignó o si es admin está en el servicio.
            // El atributo [Authorize(Roles)] ya asegura que es Nutricionista o Admin.
            var result = await _nutritionPlanService.UnassignPlanFromClientAsync(clientId, nutritionPlanId, requestingUserId);
            if (!result) return NotFound(new { Message = "Asignación no encontrada o no tiene permiso para desasignar." });
            return NoContent();
        }
    }
}
