// NutriPlat.Api/Services/IUserService.cs
using NutriPlat.Shared.Dtos;
using NutriPlat.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NutriPlat.Api.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetClientsAsync(); // Obtiene todos los usuarios con rol Cliente
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync(); // Para Admin
        Task<bool> UpdateUserRoleAsync(string userId, UserRole newRole); // Para Admin
        Task<bool> DeleteUserAsync(string userId); // Para Admin
        Task<UserDto?> UpdateMyProfileAsync(string userId, UpdateUserProfileDto profileDto); // Para usuario autenticado

        // --- NUEVOS MÉTODOS PARA VINCULACIÓN CLIENTE-PROFESIONAL ---

        /// <summary>
        /// Vincula un cliente a un profesional (Nutricionista o Entrenador).
        /// </summary>
        /// <param name="professionalId">ID del profesional (Nutricionista o Entrenador).</param>
        /// <param name="clientId">ID del cliente a vincular.</param>
        /// <param name="professionalRole">El rol del profesional (Nutritionist o Trainer) para saber qué campo actualizar.</param>
        /// <returns>True si la vinculación fue exitosa, False en caso contrario.</returns>
        Task<bool> LinkClientToProfessionalAsync(string professionalId, string clientId, UserRole professionalRole);

        /// <summary>
        /// Desvincula un cliente de un profesional.
        /// </summary>
        /// <param name="professionalId">ID del profesional (Nutricionista o Entrenador).</param>
        /// <param name="clientId">ID del cliente a desvincular.</param>
        /// <param name="professionalRole">El rol del profesional.</param>
        /// <returns>True si la desvinculación fue exitosa, False en caso contrario.</returns>
        Task<bool> UnlinkClientFromProfessionalAsync(string professionalId, string clientId, UserRole professionalRole);

        /// <summary>
        /// Obtiene la lista de clientes vinculados a un profesional específico.
        /// </summary>
        /// <param name="professionalId">ID del profesional.</param>
        /// <param name="professionalRole">El rol del profesional (Nutritionist o Trainer).</param>
        /// <returns>Una colección de DTOs de los clientes vinculados.</returns>
        Task<IEnumerable<UserDto>> GetLinkedClientsForProfessionalAsync(string professionalId, UserRole professionalRole);

        /// <summary>
        /// Obtiene el nutricionista asignado a un cliente.
        /// </summary>
        /// <param name="clientId">ID del cliente.</param>
        /// <returns>El DTO del nutricionista asignado, o null si no hay ninguno.</returns>
        Task<UserDto?> GetMyNutritionistAsync(string clientId);

        /// <summary>
        /// Obtiene el entrenador asignado a un cliente.
        /// </summary>
        /// <param name="clientId">ID del cliente.</param>
        /// <returns>El DTO del entrenador asignado, o null si no hay ninguno.</returns>
        Task<UserDto?> GetMyTrainerAsync(string clientId);
    }
}
