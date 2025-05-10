// NutriPlat.Api/Models/UserNutritionPlan.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriPlat.Api.Models
{
    /// <summary>
    /// Entidad de unión para la relación muchos a muchos entre Usuarios (Clientes) y Planes de Nutrición.
    /// Representa un plan de nutrición asignado a un usuario.
    /// </summary>
    public class UserNutritionPlan
    {
        /// <summary>
        /// Identificador del usuario (Cliente) al que se le asigna el plan.
        /// Parte de la clave primaria compuesta.
        /// </summary>
        [Required]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Propiedad de navegación al usuario (Cliente).
        /// </summary>
        [ForeignKey("ClientId")]
        public virtual ApplicationUser? Client { get; set; }

        /// <summary>
        /// Identificador del Plan de Nutrición asignado.
        /// Parte de la clave primaria compuesta.
        /// </summary>
        [Required]
        public string NutritionPlanId { get; set; } = string.Empty;

        /// <summary>
        /// Propiedad de navegación al Plan de Nutrición.
        /// </summary>
        [ForeignKey("NutritionPlanId")]
        public virtual NutritionPlanEntity? NutritionPlan { get; set; }

        /// <summary>
        /// Identificador del Nutricionista que realizó la asignación.
        /// </summary>
        [Required]
        public string AssignedByNutritionistId { get; set; } = string.Empty;

        /// <summary>
        /// Propiedad de navegación al Nutricionista que asignó el plan.
        /// </summary>
        [ForeignKey("AssignedByNutritionistId")]
        public virtual ApplicationUser? AssignedByNutritionist { get; set; }

        /// <summary>
        /// Fecha en que se realizó la asignación.
        /// </summary>
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de inicio para la validez de esta asignación (opcional).
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Fecha de fin para la validez de esta asignación (opcional).
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Indica si esta asignación está actualmente activa para el cliente.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Podrías añadir más campos, como notas específicas de la asignación, etc.
    }
}
