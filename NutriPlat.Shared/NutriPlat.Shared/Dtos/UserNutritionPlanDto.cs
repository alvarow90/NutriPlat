// NutriPlat.Shared/Dtos/UserNutritionPlanDto.cs
using System;

namespace NutriPlat.Shared.Dtos
{
    /// <summary>
    /// Data Transfer Object para mostrar la información de un plan de nutrición asignado a un usuario.
    /// </summary>
    public class UserNutritionPlanDto
    {
        /// <summary>
        /// Identificador del cliente al que se le asignó el plan.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        // Opcional: Podrías incluir más detalles del cliente si es necesario al mostrar la asignación.
        // public string? ClientFirstName { get; set; }
        // public string? ClientLastName { get; set; }
        // public string? ClientEmail { get; set; }

        /// <summary>
        /// Identificador del Plan de Nutrición asignado.
        /// </summary>
        public string NutritionPlanId { get; set; } = string.Empty;

        // Detalles del plan de nutrición asignado.
        // Podríamos anidar el NutritionPlanDto completo o solo los campos más relevantes.
        // Por simplicidad, incluimos campos clave.
        /// <summary>
        /// Nombre del plan de nutrición asignado.
        /// </summary>
        public string NutritionPlanName { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del plan de nutrición asignado.
        /// </summary>
        public string NutritionPlanDescription { get; set; } = string.Empty;


        /// <summary>
        /// Identificador del Nutricionista que realizó la asignación.
        /// </summary>
        public string AssignedByNutritionistId { get; set; } = string.Empty;

        // Opcional: Detalles del nutricionista que asignó.
        // public string? NutritionistFirstName { get; set; }
        // public string? NutritionistLastName { get; set; }

        /// <summary>
        /// Fecha en que se realizó la asignación.
        /// </summary>
        public DateTime AssignedDate { get; set; }

        /// <summary>
        /// Fecha de inicio para la validez de esta asignación.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Fecha de fin para la validez de esta asignación.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Indica si esta asignación está actualmente activa para el cliente.
        /// </summary>
        public bool IsActive { get; set; }

        // Constructor para facilitar el mapeo desde la entidad UserNutritionPlan
        public UserNutritionPlanDto() { }

        // Podrías añadir un constructor que tome la entidad UserNutritionPlan y NutritionPlanEntity
        // para facilitar el mapeo en el servicio, pero lo haremos con mapeo manual por ahora.
    }
}
