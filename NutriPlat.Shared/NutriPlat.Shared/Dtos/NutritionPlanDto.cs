// NutriPlat.Shared/Dtos/NutritionPlanDto.cs
using System.ComponentModel.DataAnnotations; // Necesario para las anotaciones de validación

namespace NutriPlat.Shared.Dtos
{
    /// <summary>
    /// Data Transfer Object para un plan de nutrición (simplificado para MVP).
    /// </summary>
    public class NutritionPlanDto
    {
        /// <summary>
        /// Identificador único del plan de nutrición.
        /// Podría ser generado por la base de datos en la API y ser null al crear.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Nombre del plan de nutrición.
        /// </summary>
        [Required(ErrorMessage = "El nombre del plan es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del plan no puede exceder los 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada del plan de nutrición.
        /// </summary>
        [Required(ErrorMessage = "La descripción del plan es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del nutriólogo que creó el plan.
        /// Este campo será asignado por la API al momento de crear el plan,
        /// basado en el usuario autenticado.
        /// </summary>
        public string? NutritionistId { get; set; }

        // Para el MVP, no incluiremos la estructura detallada de comidas diarias.
        // Se podría añadir más adelante:
        // public List<DailyMealPlanDto> DailyMeals { get; set; } = new List<DailyMealPlanDto>();
    }
}
