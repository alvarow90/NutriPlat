// NutriPlat.Shared/Dtos/WorkoutRoutineDto.cs
using System.ComponentModel.DataAnnotations;

namespace NutriPlat.Shared.Dtos // <-- Espacio de nombres importante
{
    public class WorkoutRoutineDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre de la rutina es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción de la rutina es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string Description { get; set; } = string.Empty;

        public string? TrainerId { get; set; }
    }
}
