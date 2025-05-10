// NutriPlat.Shared/Dtos/UserWorkoutRoutineDto.cs
using System;

namespace NutriPlat.Shared.Dtos // <-- Espacio de nombres importante
{
    public class UserWorkoutRoutineDto
    {
        public string ClientId { get; set; } = string.Empty;
        public string WorkoutRoutineId { get; set; } = string.Empty;
        public string WorkoutRoutineName { get; set; } = string.Empty;
        public string WorkoutRoutineDescription { get; set; } = string.Empty;
        public string AssignedByTrainerId { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
