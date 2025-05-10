// NutriPlat.Api/Services/IProgressTrackingService.cs
using NutriPlat.Shared.Dtos; // Para ProgressEntryDto
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NutriPlat.Api.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión del seguimiento de progreso.
    /// </summary>
    public interface IProgressTrackingService
    {
        /// <summary>
        /// Crea una nueva entrada de progreso para el usuario autenticado (cliente).
        /// </summary>
        /// <param name="progressDto">DTO con los datos de la entrada de progreso a crear.</param>
        /// <param name="clientId">ID del cliente que crea la entrada.</param>
        /// <returns>El DTO de la entrada de progreso creada, o null si falla la creación.</returns>
        Task<ProgressEntryDto?> CreateProgressEntryAsync(ProgressEntryDto progressDto, string clientId);

        /// <summary>
        /// Obtiene todas las entradas de progreso para un cliente específico.
        /// Puede ser llamado por el propio cliente o por un profesional vinculado/admin.
        /// </summary>
        /// <param name="clientId">El ID del cliente cuyas entradas de progreso se desean obtener.</param>
        /// <param name="requestingUserId">ID del usuario que realiza la solicitud (para verificación de permisos).</param>
        /// <param name="isUserAdminOrLinkedProfessional">Indica si el solicitante es un admin o un profesional vinculado al cliente.</param>
        /// <returns>Una colección de DTOs de entradas de progreso.</returns>
        Task<IEnumerable<ProgressEntryDto>> GetProgressEntriesForClientAsync(string clientId, string requestingUserId, bool isUserAdminOrLinkedProfessional);

        /// <summary>
        /// Obtiene una entrada de progreso específica por su ID.
        /// </summary>
        /// <param name="entryId">El ID de la entrada de progreso a obtener.</param>
        /// <param name="requestingUserId">ID del usuario que realiza la solicitud.</param>
        /// <returns>El DTO de la entrada de progreso si se encuentra y el usuario tiene permiso; de lo contrario, null.</returns>
        Task<ProgressEntryDto?> GetProgressEntryByIdAsync(string entryId, string requestingUserId);

        /// <summary>
        /// Actualiza una entrada de progreso existente.
        /// Solo el cliente que la creó o un administrador pueden actualizar.
        /// </summary>
        /// <param name="entryId">El ID de la entrada de progreso a actualizar.</param>
        /// <param name="progressDto">DTO con los nuevos datos para la entrada.</param>
        /// <param name="requestingUserId">ID del usuario que solicita la actualización.</param>
        /// <returns>El DTO de la entrada de progreso actualizada, o null si no se encuentra, el usuario no tiene permisos, o falla la actualización.</returns>
        Task<ProgressEntryDto?> UpdateProgressEntryAsync(string entryId, ProgressEntryDto progressDto, string requestingUserId);

        /// <summary>
        /// Elimina una entrada de progreso.
        /// Solo el cliente que la creó o un administrador pueden eliminar.
        /// </summary>
        /// <param name="entryId">El ID de la entrada de progreso a eliminar.</param>
        /// <param name="requestingUserId">ID del usuario que solicita la eliminación.</param>
        /// <returns>True si la entrada fue eliminada exitosamente; False en caso contrario.</returns>
        Task<bool> DeleteProgressEntryAsync(string entryId, string requestingUserId);
    }
}
