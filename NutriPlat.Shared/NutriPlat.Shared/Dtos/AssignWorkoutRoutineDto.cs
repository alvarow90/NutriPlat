// NutriPlat.Shared/Dtos/AssignWorkoutRoutineDto.cs
using System.ComponentModel.DataAnnotations;

namespace NutriPlat.Shared.Dtos // <-- Espacio de nombres importante
{
    public class AssignWorkoutRoutineDto
    {
        [Required(ErrorMessage = "El ID del cliente es obligatorio.")]
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID de la rutina de entrenamiento es obligatorio.")]
        public string WorkoutRoutineId { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
