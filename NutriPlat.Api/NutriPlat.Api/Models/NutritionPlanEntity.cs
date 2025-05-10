// NutriPlat.Api/Models/NutritionPlanEntity.cs
using System.Collections.Generic; // Para ICollection
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriPlat.Api.Models
{
    /// <summary>
    /// Entidad para los planes de nutrición que se almacenará en la base de datos.
    /// </summary>
    public class NutritionPlanEntity
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

        public string? NutritionistId { get; set; }

        // [ForeignKey("NutritionistId")]
        // public virtual ApplicationUser? Nutritionist { get; set; } // Este sería el creador del plan

        // --- NUEVA PROPIEDAD DE NAVEGACIÓN AÑADIDA AQUÍ ---
        /// <summary>
        /// Colección de asignaciones de este plan de nutrición a diferentes usuarios (clientes).
        /// </summary>
        public virtual ICollection<UserNutritionPlan> UserAssignments { get; set; } = new List<UserNutritionPlan>();
    }
}
