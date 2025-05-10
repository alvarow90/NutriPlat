// NutriPlat.Api/Models/WorkoutRoutineEntity.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriPlat.Api.Models // <-- Espacio de nombres importante
{
    public class WorkoutRoutineEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string? TrainerId { get; set; }

        [ForeignKey("TrainerId")]
        public virtual ApplicationUser? Trainer { get; set; }

        public virtual ICollection<UserWorkoutRoutine> UserAssignments { get; set; } = new List<UserWorkoutRoutine>();
    }
}
