// NutriPlat.Shared/Dtos/AssignNutritionPlanDto.cs
using System.ComponentModel.DataAnnotations; // Para las anotaciones de validación

namespace NutriPlat.Shared.Dtos // <-- Espacio de nombres importante
{
    /// <summary>
    /// Data Transfer Object para la solicitud de asignación de un plan de nutrición a un cliente.
    /// </summary>
    public class AssignNutritionPlanDto
    {
        /// <summary>
        /// Identificador único del cliente al que se le asignará el plan.
        /// </summary>
        [Required(ErrorMessage = "El ID del cliente es obligatorio.")]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único del plan de nutrición que se va a asignar.
        /// </summary>
        [Required(ErrorMessage = "El ID del plan de nutrición es obligatorio.")]
        public string NutritionPlanId { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de inicio opcional para la validez de esta asignación.
        /// Si no se proporciona, podría tomarse la fecha actual o dejarse nula.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Fecha de fin opcional para la validez de esta asignación.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Indica si la asignación debe estar activa inmediatamente.
        /// Por defecto, se considera activa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Nota: El ID del nutricionista que realiza la asignación se obtiene del token JWT del usuario autenticado.
        // No es necesario enviarlo en este DTO de solicitud.
    }
}
