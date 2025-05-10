// NutriPlat.Api/Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using NutriPlat.Shared.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriPlat.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; } = string.Empty;

        [PersonalData]
        public string LastName { get; set; } = string.Empty;

        public UserRole AppRole { get; set; }

        // Vinculación Profesional
        public string? MyNutritionistId { get; set; }
        [ForeignKey("MyNutritionistId")]
        public virtual ApplicationUser? MyNutritionist { get; set; }

        public string? MyTrainerId { get; set; }
        [ForeignKey("MyTrainerId")]
        public virtual ApplicationUser? MyTrainer { get; set; }

        public virtual ICollection<ApplicationUser> LinkedClientsAsNutritionist { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<ApplicationUser> LinkedClientsAsTrainer { get; set; } = new List<ApplicationUser>();

        // Asignaciones
        public virtual ICollection<UserNutritionPlan> AssignedNutritionPlans { get; set; } = new List<UserNutritionPlan>();
        public virtual ICollection<UserWorkoutRoutine> AssignedWorkoutRoutines { get; set; } = new List<UserWorkoutRoutine>();

        // Entradas de Progreso
        public virtual ICollection<ProgressEntryEntity> ProgressEntries { get; set; } = new List<ProgressEntryEntity>(); // <-- NUEVA COLECCIÓN

        // Creaciones
        public virtual ICollection<NutritionPlanEntity> CreatedNutritionPlans { get; set; } = new List<NutritionPlanEntity>();
        public virtual ICollection<WorkoutRoutineEntity> CreatedWorkoutRoutines { get; set; } = new List<WorkoutRoutineEntity>();
    }
}
