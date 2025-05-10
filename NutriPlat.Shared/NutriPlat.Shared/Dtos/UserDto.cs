// NutriPlat.Shared/Dtos/UserDto.cs
using NutriPlat.Shared.Enums;

namespace NutriPlat.Shared.Dtos
{
    /// <summary>
    /// Data Transfer Object para representar la información de un usuario (simplificado para MVP).
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Rol del usuario.
        /// </summary>
        public UserRole Role { get; set; }
    }
}
