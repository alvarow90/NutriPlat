// NutriPlat.Api/Services/INutritionPlanService.cs
using NutriPlat.Shared.Dtos; // Para NutritionPlanDto, AssignNutritionPlanDto, UserNutritionPlanDto
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NutriPlat.Api.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de planes de nutrición.
    /// </summary>
    public interface INutritionPlanService
    {
        // --- Métodos CRUD existentes para Planes de Nutrición ---
        Task<NutritionPlanDto?> CreatePlanAsync(NutritionPlanDto planDto, string creatorUserId);
        Task<IEnumerable<NutritionPlanDto>> GetAllPlansAsync();
        Task<NutritionPlanDto?> GetPlanByIdAsync(string planId);
        Task<NutritionPlanDto?> UpdatePlanAsync(string planId, NutritionPlanDto planDto, string requestingUserId);
        Task<bool> DeletePlanAsync(string planId, string requestingUserId);

        // --- NUEVOS MÉTODOS PARA LA ASIGNACIÓN DE PLANES ---

        /// <summary>
        /// Asigna un plan de nutrición a un cliente.
        /// </summary>
        /// <param name="assignmentDto">DTO con los detalles de la asignación.</param>
        /// <param name="nutritionistId">ID del nutricionista que realiza la asignación.</param>
        /// <returns>El DTO de la asignación creada (UserNutritionPlanDto), o null si falla la asignación.</returns>
        Task<UserNutritionPlanDto?> AssignPlanToClientAsync(AssignNutritionPlanDto assignmentDto, string nutritionistId);

        /// <summary>
        /// Obtiene todos los planes de nutrición asignados a un cliente específico.
        /// </summary>
        /// <param name="clientId">El ID del cliente.</param>
        /// <returns>Una colección de DTOs de las asignaciones de planes de nutrición (UserNutritionPlanDto).</returns>
        Task<IEnumerable<UserNutritionPlanDto>> GetAssignedPlansForClientAsync(string clientId);

        /// <summary>
        /// Obtiene todas las asignaciones de planes de nutrición realizadas por un nutricionista específico.
        /// </summary>
        /// <param name="nutritionistId">El ID del nutricionista.</param>
        /// <returns>Una colección de DTOs de las asignaciones de planes de nutrición (UserNutritionPlanDto).</returns>
        Task<IEnumerable<UserNutritionPlanDto>> GetAssignmentsByNutritionistAsync(string nutritionistId);


        /// <summary>
        /// Elimina/desasigna un plan de nutrición de un cliente.
        /// </summary>
        /// <param name="clientId">El ID del cliente.</param>
        /// <param name="nutritionPlanId">El ID del plan de nutrición a desasignar.</param>
        /// <param name="requestingUserId">ID del usuario que solicita la desasignación (debe ser el nutricionista que asignó o un admin).</param>
        /// <returns>True si la desasignación fue exitosa; False en caso contrario.</returns>
        Task<bool> UnassignPlanFromClientAsync(string clientId, string nutritionPlanId, string requestingUserId);
    }
}
