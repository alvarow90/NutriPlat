// NutriPlat.Shared/Dtos/Auth/TokenResponseDto.cs
namespace NutriPlat.Shared.Dtos.Auth
{
    /// <summary>
    /// Data Transfer Object para la respuesta de autenticación exitosa.
    /// Contiene el token JWT y alguna información del usuario.
    /// </summary>
    public class TokenResponseDto
    {
        /// <summary>
        /// Token de acceso JWT.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de expiración del token de acceso.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Rol del usuario (como string para la respuesta).
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}
