// NutriPlat.Shared/Dtos/Auth/RegisterRequestDto.cs
using NutriPlat.Shared.Enums;
using System.ComponentModel.DataAnnotations; // Necesario para las anotaciones de validación

namespace NutriPlat.Shared.Dtos.Auth
{
    /// <summary>
    /// Data Transfer Object para la solicitud de registro de un nuevo usuario.
    /// </summary>
    public class RegisterRequestDto
    {
        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres.")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario. Se utilizará como nombre de usuario.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede exceder los 100 caracteres.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña para el nuevo usuario.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Rol con el que se registrará el usuario.
        /// </summary>
        [Required(ErrorMessage = "El rol es obligatorio.")]
        public UserRole Role { get; set; } = UserRole.Client; // Valor por defecto
    }
}
