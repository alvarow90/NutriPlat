// NutriPlat.Api/Services/IAuthService.cs
using Microsoft.AspNetCore.Identity; // Para IdentityResult
using NutriPlat.Shared.Dtos.Auth; // Para RegisterRequestDto, LoginRequestDto, TokenResponseDto
using System.Threading.Tasks;

namespace NutriPlat.Api.Services
{
    /// <summary>
    /// Interfaz para el servicio de autenticación.
    /// Define las operaciones para registrar usuarios e iniciar sesión.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="registerDto">Datos para el registro del usuario.</param>
        /// <returns>Un IdentityResult indicando el resultado de la operación de registro.</returns>
        Task<IdentityResult> RegisterUserAsync(RegisterRequestDto registerDto);

        /// <summary>
        /// Intenta iniciar sesión para un usuario existente.
        /// </summary>
        /// <param name="loginDto">Credenciales de inicio de sesión.</param>
        /// <returns>Un TokenResponseDto si el inicio de sesión es exitoso; de lo contrario, null.</returns>
        Task<TokenResponseDto?> LoginUserAsync(LoginRequestDto loginDto);
    }
}
