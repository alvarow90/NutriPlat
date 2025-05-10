// NutriPlat.Api/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using NutriPlat.Api.Services; // Para IAuthService
using NutriPlat.Shared.Dtos.Auth; // Para RegisterRequestDto, LoginRequestDto, TokenResponseDto
using System.Threading.Tasks;

namespace NutriPlat.Api.Controllers
{
    [Route("api/[controller]")] // Ruta base: api/auth
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="registerDto">Datos para el registro del usuario.</param>
        /// <returns>Un resultado indicando éxito o fracaso.</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
        {
            // Validar el modelo (DataAnnotations en el DTO se encargan de esto automáticamente si [ApiController] está presente)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterUserAsync(registerDto);

            if (!result.Succeeded)
            {
                // Si IdentityResult tiene errores, los añadimos al ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState); // O un DTO de error personalizado
            }

            // Podríamos devolver el usuario creado o simplemente un Ok.
            // Por simplicidad, devolvemos Ok. Considerar devolver una ruta al recurso creado o el UserDto.
            return Ok(new { Message = "Usuario registrado exitosamente." });
        }

        /// <summary>
        /// Inicia sesión para un usuario existente.
        /// </summary>
        /// <param name="loginDto">Credenciales de inicio de sesión.</param>
        /// <returns>Un token JWT si el inicio de sesión es exitoso.</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tokenResponse = await _authService.LoginUserAsync(loginDto);

            if (tokenResponse == null)
            {
                return Unauthorized(new { Message = "Credenciales inválidas o usuario no encontrado." });
            }

            return Ok(tokenResponse);
        }
    }
}
