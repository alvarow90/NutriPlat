// NutriPlat.Api/Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using NutriPlat.Shared.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Para ForeignKey

namespace NutriPlat.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; } = string.Empty;

        [PersonalData]
        public string LastName { get; set; } = string.Empty;

        public UserRole AppRole { get; set; }

        // --- VINCULACIÓN EXPLÍCITA CLIENTE-PROFESIONAL ---
        // Si este usuario es un Cliente, estos campos indican su profesional asignado.
        // Si este usuario es un Profesional, estas colecciones indican sus clientes vinculados.

        /// <summary>
        /// ID del Nutricionista asignado a este usuario (si es Cliente y tiene uno).
        /// </summary>
        public string? MyNutritionistId { get; set; }

        /// <summary>
        /// Propiedad de navegación al Nutricionista asignado (si este usuario es Cliente).
        /// </summary>
        [ForeignKey("MyNutritionistId")]
        public virtual ApplicationUser? MyNutritionist { get; set; }

        /// <summary>
        /// ID del Entrenador asignado a este usuario (si es Cliente y tiene uno).
        /// </summary>
        public string? MyTrainerId { get; set; }

        /// <summary>
        /// Propiedad de navegación al Entrenador asignado (si este usuario es Cliente).
        /// </summary>
        [ForeignKey("MyTrainerId")]
        public virtual ApplicationUser? MyTrainer { get; set; }


        // --- COLECCIONES DE NAVEGACIÓN INVERSA ---
        // Si este usuario es un Nutricionista, esta es la lista de sus Clientes vinculados.
        public virtual ICollection<ApplicationUser> LinkedClientsAsNutritionist { get; set; } = new List<ApplicationUser>();

        // Si este usuario es un Entrenador, esta es la lista de sus Clientes vinculados.
        public virtual ICollection<ApplicationUser> LinkedClientsAsTrainer { get; set; } = new List<ApplicationUser>();


        // Asignaciones de Planes y Rutinas (si es Cliente)
        public virtual ICollection<UserNutritionPlan> AssignedNutritionPlans { get; set; } = new List<UserNutritionPlan>();
        public virtual ICollection<UserWorkoutRoutine> AssignedWorkoutRoutines { get; set; } = new List<UserWorkoutRoutine>();
        public virtual ICollection<ProgressEntryEntity> ProgressEntries { get; set; } = new List<ProgressEntryEntity>();


        // Planes/Rutinas CREADOS por este usuario si es Nutricionista/Entrenador
        public virtual ICollection<NutritionPlanEntity> CreatedNutritionPlans { get; set; } = new List<NutritionPlanEntity>();
        public virtual ICollection<WorkoutRoutineEntity> CreatedWorkoutRoutines { get; set; } = new List<WorkoutRoutineEntity>();
    }
}
