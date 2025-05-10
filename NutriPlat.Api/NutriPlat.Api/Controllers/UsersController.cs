// NutriPlat.Api/Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriPlat.Api.Services;
using NutriPlat.Shared.Dtos;
using NutriPlat.Shared.Enums;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NutriPlat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // ... (endpoints existentes: GetClientList, GetUser, GetAllUsers, UpdateUserRole, DeleteUser, GetMyProfile, UpdateMyProfile) ...
        [HttpGet("clients")]
        [Authorize(Roles = $"{nameof(UserRole.Nutritionist)},{nameof(UserRole.Trainer)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> GetClientList()
        {
            var clients = await _userService.GetClientsAsync();
            return Ok(clients);
        }
        [HttpGet("{userId}")]
        [Authorize(Roles = $"{nameof(UserRole.Nutritionist)},{nameof(UserRole.Trainer)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound(new { Message = $"Usuario con ID '{userId}' no encontrado." });
            return Ok(user);
        }
        [HttpGet("admin/all")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        [HttpPut("admin/{userId}/role")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateUserRoleDto newRoleDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _userService.UpdateUserRoleAsync(userId, newRoleDto.NewRole);
            if (!success)
            {
                var userExists = await _userService.GetUserByIdAsync(userId);
                if (userExists == null) return NotFound(new { Message = $"Usuario con ID '{userId}' no encontrado." });
                return BadRequest(new { Message = $"No se pudo actualizar el rol para el usuario {userId}." });
            }
            return NoContent();
        }
        [HttpDelete("admin/{userId}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var requestingAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == requestingAdminId) return BadRequest(new { Message = "Los administradores no pueden eliminarse a sí mismos." });
            var success = await _userService.DeleteUserAsync(userId);
            if (!success)
            {
                var userExists = await _userService.GetUserByIdAsync(userId);
                if (userExists == null) return NotFound(new { Message = $"Usuario con ID '{userId}' no encontrado." });
                return BadRequest(new { Message = $"No se pudo eliminar el usuario {userId}. Puede tener datos asociados." });
            }
            return NoContent();
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Token inválido.");
            var userProfile = await _userService.GetUserByIdAsync(userId);
            if (userProfile == null) return NotFound(new { Message = "Perfil de usuario no encontrado." });
            return Ok(userProfile);
        }
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto profileDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Token inválido.");
            var updatedUser = await _userService.UpdateMyProfileAsync(userId, profileDto);
            if (updatedUser == null) return BadRequest(new { Message = "No se pudo actualizar el perfil." });
            return Ok(updatedUser);
        }

        // --- NUEVOS ENDPOINTS PARA VINCULACIÓN CLIENTE-PROFESIONAL ---

        /// <summary>
        /// [Profesional] Vincula un cliente a este profesional autenticado.
        /// </summary>
        /// <param name="clientId">ID del cliente a vincular.</param>
        [HttpPost("me/clients/{clientId}/link")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = $"{nameof(UserRole.Nutritionist)},{nameof(UserRole.Trainer)}")]
        public async Task<IActionResult> LinkClientToMe(string clientId)
        {
            var professionalId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(professionalId)) return Unauthorized("Token inválido.");

            UserRole professionalRole;
            if (User.IsInRole(nameof(UserRole.Nutritionist)))
                professionalRole = UserRole.Nutritionist;
            else if (User.IsInRole(nameof(UserRole.Trainer)))
                professionalRole = UserRole.Trainer;
            else
                return Forbid("El usuario autenticado no es un profesional válido para vincular clientes.");

            var success = await _userService.LinkClientToProfessionalAsync(professionalId, clientId, professionalRole);
            if (!success)
            {
                // Podría ser que el cliente o profesional no exista, o el cliente ya esté vinculado.
                _logger.LogWarning("Falló la vinculación del cliente {ClientId} al profesional {ProfessionalId} ({ProfessionalRole}).", clientId, professionalId, professionalRole);
                return BadRequest(new { Message = "No se pudo vincular el cliente. Verifique que ambos usuarios existan, que el cliente no esté ya vinculado a otro profesional de este tipo, y que usted tenga el rol correcto." });
            }
            return NoContent(); // O Ok con algún mensaje
        }

        /// <summary>
        /// [Profesional] Desvincula un cliente de este profesional autenticado.
        /// </summary>
        /// <param name="clientId">ID del cliente a desvincular.</param>
        [HttpDelete("me/clients/{clientId}/unlink")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = $"{nameof(UserRole.Nutritionist)},{nameof(UserRole.Trainer)}")]
        public async Task<IActionResult> UnlinkClientFromMe(string clientId)
        {
            var professionalId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(professionalId)) return Unauthorized("Token inválido.");

            UserRole professionalRole;
            if (User.IsInRole(nameof(UserRole.Nutritionist)))
                professionalRole = UserRole.Nutritionist;
            else if (User.IsInRole(nameof(UserRole.Trainer)))
                professionalRole = UserRole.Trainer;
            else
                return Forbid();

            var success = await _userService.UnlinkClientFromProfessionalAsync(professionalId, clientId, professionalRole);
            if (!success)
            {
                _logger.LogWarning("Falló la desvinculación del cliente {ClientId} del profesional {ProfessionalId} ({ProfessionalRole}).", clientId, professionalId, professionalRole);
                return BadRequest(new { Message = "No se pudo desvincular el cliente. Verifique que el cliente esté vinculado a usted." });
            }
            return NoContent();
        }

        /// <summary>
        /// [Profesional] Obtiene la lista de clientes vinculados a este profesional autenticado.
        /// </summary>
        [HttpGet("me/clients")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = $"{nameof(UserRole.Nutritionist)},{nameof(UserRole.Trainer)}")]
        public async Task<IActionResult> GetMyLinkedClients()
        {
            var professionalId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(professionalId)) return Unauthorized("Token inválido.");

            UserRole professionalRole;
            if (User.IsInRole(nameof(UserRole.Nutritionist)))
                professionalRole = UserRole.Nutritionist;
            else if (User.IsInRole(nameof(UserRole.Trainer)))
                professionalRole = UserRole.Trainer;
            else
                return Forbid();

            var clients = await _userService.GetLinkedClientsForProfessionalAsync(professionalId, professionalRole);
            return Ok(clients);
        }

        /// <summary>
        /// [Cliente] Obtiene el nutricionista asignado al cliente autenticado.
        /// </summary>
        [HttpGet("me/nutritionist")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = nameof(UserRole.Client))]
        public async Task<IActionResult> GetMyAssignedNutritionist()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized("Token inválido.");

            var nutritionist = await _userService.GetMyNutritionistAsync(clientId);
            if (nutritionist == null) return NotFound(new { Message = "No tiene un nutricionista asignado." });
            return Ok(nutritionist);
        }

        /// <summary>
        /// [Cliente] Obtiene el entrenador asignado al cliente autenticado.
        /// </summary>
        [HttpGet("me/trainer")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = nameof(UserRole.Client))]
        public async Task<IActionResult> GetMyAssignedTrainer()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized("Token inválido.");

            var trainer = await _userService.GetMyTrainerAsync(clientId);
            if (trainer == null) return NotFound(new { Message = "No tiene un entrenador asignado." });
            return Ok(trainer);
        }
    }
}
