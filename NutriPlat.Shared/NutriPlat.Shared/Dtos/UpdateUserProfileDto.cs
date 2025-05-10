// NutriPlat.Shared/Dtos/UpdateUserProfileDto.cs
using System.ComponentModel.DataAnnotations;

namespace NutriPlat.Shared.Dtos
{
    /// <summary>
    /// Data Transfer Object para actualizar la información del perfil del usuario autenticado.
    /// </summary>
    public class UpdateUserProfileDto
    {
        /// <summary>
        /// Nuevo nombre del usuario.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Nuevo apellido del usuario.
        /// </summary>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres.")]
        public string LastName { get; set; } = string.Empty;

        // Podrías añadir más campos actualizables aquí en el futuro, como:
        // [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        // public string? PhoneNumber { get; set; }
        //
        // public DateTime? DateOfBirth { get; set; }
        //
        // No permitiremos cambiar el email o el rol a través de este DTO.
        // El cambio de contraseña debería ser un endpoint separado y más seguro.
    }
}
