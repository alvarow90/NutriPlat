// NutriPlat.Api/Services/UserService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriPlat.Api.Models;
using NutriPlat.Shared.Dtos;
using NutriPlat.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NutriPlat.Api.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // ... (métodos existentes: GetClientsAsync, GetUserByIdAsync, GetAllUsersAsync, UpdateUserRoleAsync, DeleteUserAsync, UpdateMyProfileAsync) ...
        public async Task<IEnumerable<UserDto>> GetClientsAsync()
        {
            var clientUsers = await _userManager.Users
                .Where(u => u.AppRole == UserRole.Client)
                .AsNoTracking().ToListAsync();
            return clientUsers.Select(MapearUsuarioAUserDto).ToList();
        }
        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? null : MapearUsuarioAUserDto(user);
        }
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _userManager.Users.AsNoTracking().Select(u => MapearUsuarioAUserDto(u)).ToListAsync();
        }
        public async Task<bool> UpdateUserRoleAsync(string userId, UserRole newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            var newRoleName = newRole.ToString();
            if (!await _roleManager.RoleExistsAsync(newRoleName)) return false;
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removalResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removalResult.Succeeded) return false;
            var addRoleResult = await _userManager.AddToRoleAsync(user, newRoleName);
            if (!addRoleResult.Succeeded) return false;
            user.AppRole = newRole;
            var updateAppRoleResult = await _userManager.UpdateAsync(user);
            return updateAppRoleResult.Succeeded;
        }
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
        public async Task<UserDto?> UpdateMyProfileAsync(string userId, UpdateUserProfileDto profileDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            user.FirstName = profileDto.FirstName;
            user.LastName = profileDto.LastName;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? MapearUsuarioAUserDto(user) : null;
        }

        // --- IMPLEMENTACIÓN DE NUEVOS MÉTODOS PARA VINCULACIÓN ---

        public async Task<bool> LinkClientToProfessionalAsync(string professionalId, string clientId, UserRole professionalRole)
        {
            var professional = await _userManager.FindByIdAsync(professionalId);
            var client = await _userManager.FindByIdAsync(clientId);

            if (professional == null || client == null)
            {
                _logger.LogWarning("Vinculación fallida: Profesional {ProfessionalId} o Cliente {ClientId} no encontrado.", professionalId, clientId);
                return false;
            }

            if (client.AppRole != UserRole.Client)
            {
                _logger.LogWarning("Vinculación fallida: Usuario {ClientId} no es un Cliente.", clientId);
                return false;
            }

            bool linkMade = false;
            if (professionalRole == UserRole.Nutritionist && professional.AppRole == UserRole.Nutritionist)
            {
                if (client.MyNutritionistId == professionalId) return true; // Ya vinculado
                if (client.MyNutritionistId != null)
                {
                    _logger.LogWarning("Cliente {ClientId} ya tiene un nutricionista asignado ({ExistingNutritionistId}). Desvincular primero.", clientId, client.MyNutritionistId);
                    return false; // Opcional: permitir sobrescribir o requerir desvinculación previa
                }
                client.MyNutritionistId = professionalId;
                linkMade = true;
            }
            else if (professionalRole == UserRole.Trainer && professional.AppRole == UserRole.Trainer)
            {
                if (client.MyTrainerId == professionalId) return true; // Ya vinculado
                if (client.MyTrainerId != null)
                {
                    _logger.LogWarning("Cliente {ClientId} ya tiene un entrenador asignado ({ExistingTrainerId}). Desvincular primero.", clientId, client.MyTrainerId);
                    return false;
                }
                client.MyTrainerId = professionalId;
                linkMade = true;
            }
            else
            {
                _logger.LogWarning("Vinculación fallida: Rol profesional {ProfessionalRole} inválido o el usuario {ProfessionalId} no tiene ese rol.", professionalRole, professionalId);
                return false;
            }

            if (linkMade)
            {
                var result = await _userManager.UpdateAsync(client);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Cliente {ClientId} vinculado exitosamente al profesional {ProfessionalId} ({ProfessionalRole}).", clientId, professionalId, professionalRole);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al actualizar cliente {ClientId} durante la vinculación: {Errors}", clientId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> UnlinkClientFromProfessionalAsync(string professionalId, string clientId, UserRole professionalRole)
        {
            // El profesionalId aquí es para verificar que quien solicita la desvinculación es el profesional correcto.
            // La desvinculación real es quitar el ID del profesional del perfil del cliente.
            var client = await _userManager.FindByIdAsync(clientId);
            if (client == null || client.AppRole != UserRole.Client) return false;

            bool unlinkMade = false;
            if (professionalRole == UserRole.Nutritionist && client.MyNutritionistId == professionalId)
            {
                client.MyNutritionistId = null;
                unlinkMade = true;
            }
            else if (professionalRole == UserRole.Trainer && client.MyTrainerId == professionalId)
            {
                client.MyTrainerId = null;
                unlinkMade = true;
            }
            else
            {
                _logger.LogWarning("Desvinculación fallida: Cliente {ClientId} no está vinculado al profesional {ProfessionalId} ({ProfessionalRole}) o rol incorrecto.", clientId, professionalId, professionalRole);
                return false; // No estaba vinculado a este profesional con ese rol, o rol incorrecto
            }

            if (unlinkMade)
            {
                var result = await _userManager.UpdateAsync(client);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Cliente {ClientId} desvinculado exitosamente del profesional {ProfessionalId} ({ProfessionalRole}).", clientId, professionalId, professionalRole);
                    return true;
                }
                else
                {
                    _logger.LogError("Error al actualizar cliente {ClientId} durante la desvinculación: {Errors}", clientId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }
            }
            return false;
        }

        public async Task<IEnumerable<UserDto>> GetLinkedClientsForProfessionalAsync(string professionalId, UserRole professionalRole)
        {
            var professional = await _userManager.FindByIdAsync(professionalId);
            if (professional == null) return new List<UserDto>();

            IQueryable<ApplicationUser> linkedClientsQuery;

            if (professionalRole == UserRole.Nutritionist && professional.AppRole == UserRole.Nutritionist)
            {
                linkedClientsQuery = _userManager.Users.Where(u => u.MyNutritionistId == professionalId && u.AppRole == UserRole.Client);
            }
            else if (professionalRole == UserRole.Trainer && professional.AppRole == UserRole.Trainer)
            {
                linkedClientsQuery = _userManager.Users.Where(u => u.MyTrainerId == professionalId && u.AppRole == UserRole.Client);
            }
            else
            {
                _logger.LogWarning("No se pueden obtener clientes vinculados para el profesional {ProfessionalId} con rol {ProfessionalRole} inválido.", professionalId, professionalRole);
                return new List<UserDto>();
            }

            return await linkedClientsQuery.AsNoTracking().Select(u => MapearUsuarioAUserDto(u)).ToListAsync();
        }

        public async Task<UserDto?> GetMyNutritionistAsync(string clientId)
        {
            var client = await _userManager.Users
                .Include(u => u.MyNutritionist) // Cargar el profesional vinculado
                .FirstOrDefaultAsync(u => u.Id == clientId && u.AppRole == UserRole.Client);

            if (client == null || client.MyNutritionist == null) return null;
            return MapearUsuarioAUserDto(client.MyNutritionist);
        }

        public async Task<UserDto?> GetMyTrainerAsync(string clientId)
        {
            var client = await _userManager.Users
                .Include(u => u.MyTrainer) // Cargar el profesional vinculado
                .FirstOrDefaultAsync(u => u.Id == clientId && u.AppRole == UserRole.Client);

            if (client == null || client.MyTrainer == null) return null;
            return MapearUsuarioAUserDto(client.MyTrainer);
        }

        private static UserDto MapearUsuarioAUserDto(ApplicationUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Role = user.AppRole
            };
        }
    }
}
