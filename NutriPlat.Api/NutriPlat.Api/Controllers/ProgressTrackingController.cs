// NutriPlat.Api/Controllers/ProgressTrackingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Para UserManager
using Microsoft.AspNetCore.Mvc;
using NutriPlat.Api.Models; // Para ApplicationUser
using NutriPlat.Api.Services; // Para IProgressTrackingService
using NutriPlat.Shared.Dtos; // Para ProgressEntryDto
using NutriPlat.Shared.Enums; // Para UserRole
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
// using Microsoft.EntityFrameworkCore; // No es necesario aquí si no usamos _context directamente

namespace NutriPlat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Todas las acciones en este controlador requieren autenticación por defecto
    public class ProgressTrackingController : ControllerBase
    {
        private readonly IProgressTrackingService _progressService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProgressTrackingController> _logger;

        public ProgressTrackingController(
            IProgressTrackingService progressService,
            UserManager<ApplicationUser> userManager,
            ILogger<ProgressTrackingController> logger)
        {
            _progressService = progressService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// [Cliente] Crea una nueva entrada de progreso para el usuario autenticado.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Client))]
        [ProducesResponseType(typeof(ProgressEntryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateMyProgressEntry([FromBody] ProgressEntryDto progressDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogWarning("Cliente no identificado desde el token al intentar crear entrada de progreso.");
                return Unauthorized("No se pudo identificar al cliente desde el token.");
            }

            var createdEntry = await _progressService.CreateProgressEntryAsync(progressDto, clientId);

            if (createdEntry == null)
            {
                _logger.LogError("Falló la creación de la entrada de progreso para el cliente {ClientId}.", clientId);
                return BadRequest(new { Message = "No se pudo crear la entrada de progreso." });
            }

            return CreatedAtAction(nameof(GetMyProgressEntryById), new { entryId = createdEntry.Id }, createdEntry);
        }

        /// <summary>
        /// [Cliente] Obtiene todas las entradas de progreso del usuario autenticado.
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = nameof(UserRole.Client))]
        [ProducesResponseType(typeof(IEnumerable<ProgressEntryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyProgressEntries()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogWarning("Cliente no identificado desde el token al intentar obtener sus entradas de progreso.");
                return Unauthorized("No se pudo identificar al cliente desde el token.");
            }

            var entries = await _progressService.GetProgressEntriesForClientAsync(clientId, clientId, false);
            return Ok(entries);
        }

        /// <summary>
        /// [Profesional/Admin] Obtiene todas las entradas de progreso para un cliente específico.
        /// </summary>
        [HttpGet("client/{clientId}")]
        [Authorize(Roles = $"{nameof(UserRole.Nutritionist)},{nameof(UserRole.Trainer)},{nameof(UserRole.Admin)}")]
        [ProducesResponseType(typeof(IEnumerable<ProgressEntryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProgressEntriesForSpecificClient(string clientId)
        {
            var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(requestingUserId)) return Unauthorized("Token inválido.");

            var clientUser = await _userManager.FindByIdAsync(clientId);
            if (clientUser == null || clientUser.AppRole != UserRole.Client)
            {
                return NotFound(new { Message = $"Cliente con ID '{clientId}' no encontrado o no es un cliente." });
            }

            var requester = await _userManager.FindByIdAsync(requestingUserId);
            if (requester == null) return Unauthorized("Usuario solicitante no válido.");

            bool isAdmin = requester.AppRole == UserRole.Admin;
            bool isLinkedNutritionist = requester.AppRole == UserRole.Nutritionist && clientUser.MyNutritionistId == requestingUserId;
            bool isLinkedTrainer = requester.AppRole == UserRole.Trainer && clientUser.MyTrainerId == requestingUserId;
            bool canAccess = isAdmin || isLinkedNutritionist || isLinkedTrainer;

            if (!canAccess)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó acceder a entradas de progreso del cliente {ClientId} sin ser Admin o profesional vinculado.", requestingUserId, clientId);
                return Forbid();
            }

            var entries = await _progressService.GetProgressEntriesForClientAsync(clientId, requestingUserId, true);
            return Ok(entries);
        }

        /// <summary>
        /// Obtiene una entrada de progreso específica por su ID.
        /// Accesible por el Cliente dueño, Profesional vinculado al dueño, o Admin.
        /// </summary>
        [HttpGet("{entryId}")]
        [ProducesResponseType(typeof(ProgressEntryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProgressEntryById(string entryId)
        {
            var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(requestingUserId)) return Unauthorized("Token inválido.");

            var entry = await _progressService.GetProgressEntryByIdAsync(entryId, requestingUserId);
            if (entry == null)
            {
                return NotFound(new { Message = $"Entrada de progreso con ID '{entryId}' no encontrada o no tiene permiso para verla." });
            }
            return Ok(entry);
        }

        /// <summary>
        /// [Cliente] Actualiza una entrada de progreso existente. Solo el creador puede actualizar.
        /// </summary>
        [HttpPut("{entryId}")]
        [Authorize(Roles = nameof(UserRole.Client))]
        [ProducesResponseType(typeof(ProgressEntryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMyProgressEntry(string entryId, [FromBody] ProgressEntryDto progressDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(requestingUserId)) return Unauthorized("Token inválido.");

            var updatedEntry = await _progressService.UpdateProgressEntryAsync(entryId, progressDto, requestingUserId);

            if (updatedEntry == null)
            {
                // El servicio devuelve null si no se encontró o el usuario no es el propietario.
                // Para un feedback más preciso, podríamos intentar obtener la entrada primero (sin la lógica de permisos del servicio)
                // o el servicio podría devolver un resultado más detallado.
                // Por ahora, si falla, asumimos que no se encontró o no tenía permiso.
                var originalEntry = await _progressService.GetProgressEntryByIdAsync(entryId, requestingUserId);
                if (originalEntry == null)
                {
                    return NotFound(new { Message = $"Entrada de progreso con ID '{entryId}' no encontrada." });
                }
                // Si existe pero no se pudo actualizar, es porque no es el dueño (según lógica del servicio)
                return Forbid();
            }
            return Ok(updatedEntry);
        }

        /// <summary>
        /// Elimina una entrada de progreso. Solo el Cliente creador o un Admin pueden eliminar.
        /// </summary>
        [HttpDelete("{entryId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMyProgressEntry(string entryId)
        {
            var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(requestingUserId))
            {
                _logger.LogWarning("Usuario no identificado desde el token al intentar eliminar entrada de progreso {EntryId}.", entryId);
                return Unauthorized("Token inválido.");
            }

            var success = await _progressService.DeleteProgressEntryAsync(entryId, requestingUserId);

            if (!success)
            {
                // El servicio devuelve false si no se encontró o el usuario no tiene permiso.
                // Para dar un feedback más preciso entre 404 y 403 al cliente:
                var entryForCheck = await _progressService.GetProgressEntryByIdAsync(entryId, requestingUserId);
                if (entryForCheck == null)
                {
                    // Si GetProgressEntryByIdAsync (que ya tiene lógica de permisos) devuelve null,
                    // podría ser que la entrada no exista en absoluto, o que el usuario actual
                    // no tenga permiso para verla (y por ende, tampoco para eliminarla si no es admin).
                    // Para un 404 más certero si la entrada realmente no existe:
                    // (Esta lógica podría ser más compleja si el servicio no distingue "no existe" de "no permitido")
                    // Por ahora, si el servicio de borrado falla, y no podemos "ver" la entrada, asumimos NotFound o Forbidden.
                    // Una forma simple es devolver NotFound, asumiendo que si no puede borrarla y no puede verla, es como si no existiera para él.
                    return NotFound(new { Message = $"Entrada de progreso con ID '{entryId}' no encontrada o no tiene permiso para eliminarla." });
                }
                // Si la entrada existe y el usuario la puede ver, pero DeleteProgressEntryAsync falló,
                // es probable que sea por la lógica de permisos de eliminación (no es dueño ni admin).
                return Forbid();
            }

            return NoContent(); // Éxito
        }
    }
}
