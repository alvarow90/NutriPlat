// NutriPlat.Api/Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using NutriPlat.Shared.Enums; // Necesario para UserRole
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NutriPlat.Api.Models // <-- Espacio de nombres importante
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; } = string.Empty;

        [PersonalData]
        public string LastName { get; set; } = string.Empty;

        public UserRole AppRole { get; set; }

        public virtual ICollection<UserNutritionPlan> AssignedNutritionPlans { get; set; } = new List<UserNutritionPlan>();
        public virtual ICollection<UserWorkoutRoutine> AssignedWorkoutRoutines { get; set; } = new List<UserWorkoutRoutine>(); // <-- Para asignaciones a este usuario si es Cliente

        public virtual ICollection<NutritionPlanEntity> CreatedNutritionPlans { get; set; } = new List<NutritionPlanEntity>(); // Si es Nutricionista
        public virtual ICollection<WorkoutRoutineEntity> CreatedWorkoutRoutines { get; set; } = new List<WorkoutRoutineEntity>(); // Si es Entrenador
    }
}
