// NutriPlat.Shared/Dtos/Auth/LoginRequestDto.cs
using System.ComponentModel.DataAnnotations; // Necesario para las anotaciones de validación

namespace NutriPlat.Shared.Dtos.Auth
{
    /// <summary>
    /// Data Transfer Object para la solicitud de inicio de sesión.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }
}
