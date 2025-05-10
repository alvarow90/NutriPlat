// NutriPlat.Api/Models/UserWorkoutRoutine.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriPlat.Api.Models // <-- Espacio de nombres importante
{
    public class UserWorkoutRoutine
    {
        [Required]
        public string ClientId { get; set; } = string.Empty;
        [ForeignKey("ClientId")]
        public virtual ApplicationUser? Client { get; set; }

        [Required]
        public string WorkoutRoutineId { get; set; } = string.Empty;
        [ForeignKey("WorkoutRoutineId")]
        public virtual WorkoutRoutineEntity? WorkoutRoutine { get; set; }

        [Required]
        public string AssignedByTrainerId { get; set; } = string.Empty;
        [ForeignKey("AssignedByTrainerId")]
        public virtual ApplicationUser? AssignedByTrainer { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
