// NutriPlat.Shared/Dtos/UpdateUserRoleDto.cs
using NutriPlat.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace NutriPlat.Shared.Dtos
{
    public class UpdateUserRoleDto
    {
        [Required(ErrorMessage = "El nuevo rol es obligatorio.")]
        public UserRole NewRole { get; set; }
    }
}
