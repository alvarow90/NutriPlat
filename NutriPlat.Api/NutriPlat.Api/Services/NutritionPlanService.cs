// NutriPlat.Api/Services/NutritionPlanService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriPlat.Api.Data;
using NutriPlat.Api.Models;
using NutriPlat.Shared.Dtos;
using NutriPlat.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NutriPlat.Api.Services
{
    public class NutritionPlanService : INutritionPlanService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NutritionPlanService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public NutritionPlanService(AppDbContext context, ILogger<NutritionPlanService> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<NutritionPlanDto?> CreatePlanAsync(NutritionPlanDto planDto, string creatorUserId)
        {
            var creator = await _userManager.FindByIdAsync(creatorUserId);
            if (creator == null || (creator.AppRole != UserRole.Nutritionist && creator.AppRole != UserRole.Admin))
            {
                _logger.LogWarning("Usuario {CreatorUserId} intentó crear un plan sin ser Nutricionista o Admin.", creatorUserId);
                return null;
            }

            var planEntity = new NutritionPlanEntity
            {
                Name = planDto.Name,
                Description = planDto.Description,
                NutritionistId = creatorUserId
            };
            _context.NutritionPlans.Add(planEntity);
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) { _logger.LogError(ex, "Error DB al crear plan para {CreatorUserId}.", creatorUserId); return null; }
            return MapearPlanEntidadADto(planEntity);
        }

        public async Task<IEnumerable<NutritionPlanDto>> GetAllPlansAsync()
        {
            return await _context.NutritionPlans
                .AsNoTracking()
                .Select(p => MapearPlanEntidadADto(p)!)
                .ToListAsync();
        }

        public async Task<NutritionPlanDto?> GetPlanByIdAsync(string planId)
        {
            var planEntity = await _context.NutritionPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId);
            return planEntity == null ? null : MapearPlanEntidadADto(planEntity);
        }

        public async Task<NutritionPlanDto?> UpdatePlanAsync(string planId, NutritionPlanDto planDto, string requestingUserId)
        {
            var planEntity = await _context.NutritionPlans.FindAsync(planId);
            if (planEntity == null) { _logger.LogWarning("Actualización fallida: Plan {PlanId} no encontrado.", planId); return null; }

            var requester = await _userManager.FindByIdAsync(requestingUserId);
            bool isAdmin = requester != null && requester.AppRole == UserRole.Admin;

            if (planEntity.NutritionistId != requestingUserId && !isAdmin)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó actualizar plan {PlanId} sin autorización. Propietario: {OwnerId}", requestingUserId, planId, planEntity.NutritionistId);
                return null;
            }
            planEntity.Name = planDto.Name;
            planEntity.Description = planDto.Description;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) { _logger.LogError(ex, "Error DB al actualizar plan {PlanId}.", planId); return null; }
            return MapearPlanEntidadADto(planEntity);
        }

        public async Task<bool> DeletePlanAsync(string planId, string requestingUserId)
        {
            var planEntity = await _context.NutritionPlans.FindAsync(planId);
            if (planEntity == null) { _logger.LogWarning("Eliminación fallida: Plan {PlanId} no encontrado.", planId); return false; }

            var requester = await _userManager.FindByIdAsync(requestingUserId);
            bool isAdmin = requester != null && requester.AppRole == UserRole.Admin;

            if (planEntity.NutritionistId != requestingUserId && !isAdmin)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó eliminar plan {PlanId} sin autorización. Propietario: {OwnerId}", requestingUserId, planId, planEntity.NutritionistId);
                return false;
            }
            _context.NutritionPlans.Remove(planEntity);
            try { return await _context.SaveChangesAsync() > 0; }
            catch (DbUpdateException ex) { _logger.LogError(ex, "Error DB al eliminar plan {PlanId}.", planId); return false; }
        }

        public async Task<UserNutritionPlanDto?> AssignPlanToClientAsync(AssignNutritionPlanDto assignmentDto, string nutritionistId)
        {
            var nutritionist = await _userManager.FindByIdAsync(nutritionistId);
            if (nutritionist == null || nutritionist.AppRole != UserRole.Nutritionist)
            {
                _logger.LogWarning("Intento de asignación por usuario {NutritionistId} que no es Nutricionista o no existe.", nutritionistId);
                return null;
            }

            var client = await _userManager.FindByIdAsync(assignmentDto.ClientId);
            if (client == null || client.AppRole != UserRole.Client)
            {
                _logger.LogWarning("Intento de asignar plan a usuario {ClientId} que no es Cliente o no existe.", assignmentDto.ClientId);
                return null;
            }

            // --- NUEVA VERIFICACIÓN: ¿Está el cliente vinculado a ESTE nutricionista? ---
            if (client.MyNutritionistId != nutritionistId)
            {
                _logger.LogWarning("Nutricionista {NutritionistId} intentó asignar plan a cliente {ClientId} no vinculado directamente a él. Cliente vinculado a: {ActualNutritionistId}",
                    nutritionistId, assignmentDto.ClientId, client.MyNutritionistId ?? "Nadie");
                return null; // O devolver un DTO de error específico, o lanzar una excepción de negocio.
            }
            // --- FIN DE LA NUEVA VERIFICACIÓN ---

            var nutritionPlan = await _context.NutritionPlans.FindAsync(assignmentDto.NutritionPlanId);
            if (nutritionPlan == null)
            {
                _logger.LogWarning("Intento de asignar plan de nutrición inexistente con ID: {NutritionPlanId}", assignmentDto.NutritionPlanId);
                return null;
            }

            var existingAssignment = await _context.UserNutritionPlans
                .FirstOrDefaultAsync(unp => unp.ClientId == assignmentDto.ClientId && unp.NutritionPlanId == assignmentDto.NutritionPlanId);

            if (existingAssignment != null)
            {
                _logger.LogInformation("Intento de reasignar plan {NutritionPlanId} a cliente {ClientId} que ya está asignado. Actualizando asignación existente.", assignmentDto.NutritionPlanId, assignmentDto.ClientId);
                // Lógica para actualizar la asignación existente si se desea (ej. fechas, estado activo)
                existingAssignment.StartDate = assignmentDto.StartDate;
                existingAssignment.EndDate = assignmentDto.EndDate;
                existingAssignment.IsActive = assignmentDto.IsActive;
                existingAssignment.AssignedDate = DateTime.UtcNow; // Actualizar fecha de "última asignación/modificación"
                // No cambiamos AssignedByNutritionistId aquí, asumimos que es el mismo que la asignó originalmente o un admin.
            }
            else
            {
                var newAssignment = new UserNutritionPlan
                {
                    ClientId = assignmentDto.ClientId,
                    NutritionPlanId = assignmentDto.NutritionPlanId,
                    AssignedByNutritionistId = nutritionistId, // El nutricionista que realiza la acción
                    AssignedDate = DateTime.UtcNow,
                    StartDate = assignmentDto.StartDate,
                    EndDate = assignmentDto.EndDate,
                    IsActive = assignmentDto.IsActive
                };
                _context.UserNutritionPlans.Add(newAssignment);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar la asignación del plan {NutritionPlanId} al cliente {ClientId}.", assignmentDto.NutritionPlanId, assignmentDto.ClientId);
                return null;
            }

            // Obtener la asignación (nueva o actualizada) para devolverla
            var finalAssignment = await _context.UserNutritionPlans
                .Include(unp => unp.NutritionPlan) // Incluir para obtener nombre/descripción del plan
                .Include(unp => unp.Client) // Incluir para obtener datos del cliente (si es necesario para el DTO)
                .Include(unp => unp.AssignedByNutritionist) // Incluir para obtener datos del nutricionista (si es necesario para el DTO)
                .FirstOrDefaultAsync(unp => unp.ClientId == assignmentDto.ClientId && unp.NutritionPlanId == assignmentDto.NutritionPlanId);

            if (finalAssignment == null || finalAssignment.NutritionPlan == null || finalAssignment.Client == null || finalAssignment.AssignedByNutritionist == null)
            {
                _logger.LogError("No se pudo recuperar la asignación completa después de guardar para el plan {NutritionPlanId} y cliente {ClientId}.", assignmentDto.NutritionPlanId, assignmentDto.ClientId);
                return null; // Algo salió mal al recuperar los datos para el DTO
            }

            return MapearAsignacionEntidadADto(finalAssignment, finalAssignment.NutritionPlan, finalAssignment.Client, finalAssignment.AssignedByNutritionist);
        }

        public async Task<IEnumerable<UserNutritionPlanDto>> GetAssignedPlansForClientAsync(string clientId)
        {
            var client = await _userManager.FindByIdAsync(clientId);
            if (client == null || client.AppRole != UserRole.Client)
            {
                _logger.LogWarning("Intento de obtener planes para usuario no cliente o inexistente: {ClientId}", clientId);
                return new List<UserNutritionPlanDto>();
            }

            var assignments = await _context.UserNutritionPlans
                .AsNoTracking()
                .Where(unp => unp.ClientId == clientId)
                .Include(unp => unp.NutritionPlan)
                .Include(unp => unp.AssignedByNutritionist)
                .Select(unp => MapearAsignacionEntidadADto(
                    unp,
                    unp.NutritionPlan!,
                    client, // Ya tenemos el objeto cliente
                    unp.AssignedByNutritionist!
                ))
                .ToListAsync();

            return assignments;
        }

        public async Task<IEnumerable<UserNutritionPlanDto>> GetAssignmentsByNutritionistAsync(string nutritionistId)
        {
            var nutritionist = await _userManager.FindByIdAsync(nutritionistId);
            if (nutritionist == null || nutritionist.AppRole != UserRole.Nutritionist)
            {
                _logger.LogWarning("Intento de obtener asignaciones para usuario no nutricionista o inexistente: {NutritionistId}", nutritionistId);
                return new List<UserNutritionPlanDto>();
            }

            var assignments = await _context.UserNutritionPlans
                .AsNoTracking()
                .Where(unp => unp.AssignedByNutritionistId == nutritionistId)
                .Include(unp => unp.NutritionPlan)
                .Include(unp => unp.Client)
                .Select(unp => MapearAsignacionEntidadADto(
                    unp,
                    unp.NutritionPlan!,
                    unp.Client!,
                    nutritionist // Ya tenemos el objeto nutricionista
                ))
                .ToListAsync();

            return assignments;
        }

        public async Task<bool> UnassignPlanFromClientAsync(string clientId, string nutritionPlanId, string requestingUserId)
        {
            var assignment = await _context.UserNutritionPlans
                .FirstOrDefaultAsync(unp => unp.ClientId == clientId && unp.NutritionPlanId == nutritionPlanId);

            if (assignment == null)
            {
                _logger.LogWarning("Intento de desasignar un plan no asignado. Cliente: {ClientId}, Plan: {NutritionPlanId}", clientId, nutritionPlanId);
                return false;
            }

            var requester = await _userManager.FindByIdAsync(requestingUserId);
            bool isAdmin = requester != null && requester.AppRole == UserRole.Admin;

            // Solo el nutricionista que asignó o un admin pueden desasignar.
            if (assignment.AssignedByNutritionistId != requestingUserId && !isAdmin)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó desasignar plan de cliente {ClientId} sin autorización. Asignado por: {AssignedBy}",
                    requestingUserId, clientId, assignment.AssignedByNutritionistId);
                return false;
            }

            _context.UserNutritionPlans.Remove(assignment);
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar la asignación del plan {NutritionPlanId} al cliente {ClientId}.", nutritionPlanId, clientId);
                return false;
            }
        }

        private static NutritionPlanDto? MapearPlanEntidadADto(NutritionPlanEntity? entity)
        {
            if (entity == null) return null;
            return new NutritionPlanDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                NutritionistId = entity.NutritionistId
            };
        }

        private static UserNutritionPlanDto MapearAsignacionEntidadADto(
            UserNutritionPlan assignment,
            NutritionPlanEntity plan,
            ApplicationUser client,
            ApplicationUser nutritionist)
        {
            return new UserNutritionPlanDto
            {
                ClientId = assignment.ClientId,
                NutritionPlanId = assignment.NutritionPlanId,
                NutritionPlanName = plan.Name,
                NutritionPlanDescription = plan.Description,
                AssignedByNutritionistId = assignment.AssignedByNutritionistId,
                AssignedDate = assignment.AssignedDate,
                StartDate = assignment.StartDate,
                EndDate = assignment.EndDate,
                IsActive = assignment.IsActive
            };
        }
    }
}
