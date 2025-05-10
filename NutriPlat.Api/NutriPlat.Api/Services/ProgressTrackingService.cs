// NutriPlat.Api/Services/ProgressTrackingService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriPlat.Api.Data; // Para AppDbContext
using NutriPlat.Api.Models; // Para ApplicationUser, ProgressEntryEntity
using NutriPlat.Shared.Dtos; // Para ProgressEntryDto
using NutriPlat.Shared.Enums; // Para UserRole
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NutriPlat.Api.Services
{
    public class ProgressTrackingService : IProgressTrackingService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProgressTrackingService> _logger;

        public ProgressTrackingService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ProgressTrackingService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ProgressEntryDto?> CreateProgressEntryAsync(ProgressEntryDto progressDto, string clientId)
        {
            var client = await _userManager.FindByIdAsync(clientId);
            if (client == null || client.AppRole != UserRole.Client)
            {
                _logger.LogWarning("Intento de crear entrada de progreso por usuario no cliente o inexistente: {ClientId}", clientId);
                return null; // Solo los clientes pueden crear entradas para sí mismos a través de este flujo.
            }

            var entryEntity = new ProgressEntryEntity
            {
                UserId = clientId,
                EntryDate = progressDto.EntryDate,
                WeightKg = progressDto.WeightKg,
                BodyFatPercentage = progressDto.BodyFatPercentage,
                Notes = progressDto.Notes,
                // Las propiedades [NotMapped] Measurements y PhotoUrls en la entidad se encargarán de la serialización/deserialización
                // Así que asignamos directamente desde el DTO.
                Measurements = progressDto.Measurements,
                PhotoUrls = progressDto.PhotoUrls
                // CreatedAt se establece automáticamente en la entidad
            };

            _context.ProgressEntries.Add(entryEntity);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Nueva entrada de progreso creada con ID {EntryId} para el cliente {ClientId}", entryEntity.Id, clientId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar la nueva entrada de progreso para el cliente {ClientId}.", clientId);
                return null;
            }

            return MapearEntidadADto(entryEntity);
        }

        public async Task<IEnumerable<ProgressEntryDto>> GetProgressEntriesForClientAsync(string clientId, string requestingUserId, bool isUserAdminOrLinkedProfessional)
        {
            var client = await _userManager.FindByIdAsync(clientId);
            if (client == null || client.AppRole != UserRole.Client)
            {
                _logger.LogWarning("Intento de obtener entradas de progreso para un usuario no cliente o inexistente: {ClientId}", clientId);
                return new List<ProgressEntryDto>(); // Devuelve lista vacía
            }

            // Lógica de permisos:
            // 1. El cliente puede ver sus propias entradas.
            // 2. Un Admin puede ver las entradas de cualquier cliente.
            // 3. Un profesional vinculado (Nutricionista o Entrenador) puede ver las entradas de su cliente.
            if (clientId != requestingUserId && !isUserAdminOrLinkedProfessional)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó acceder a entradas de progreso del cliente {ClientId} sin autorización.", requestingUserId, clientId);
                return new List<ProgressEntryDto>(); // O lanzar una excepción de acceso denegado
            }

            var entries = await _context.ProgressEntries
                .AsNoTracking()
                .Where(pe => pe.UserId == clientId)
                .OrderByDescending(pe => pe.EntryDate) // Ordenar por fecha, las más recientes primero
                .Select(pe => MapearEntidadADto(pe)!) // El '!' es para el compilador, asumimos que MapearEntidadADto no devolverá null aquí
                .ToListAsync();

            return entries;
        }

        public async Task<ProgressEntryDto?> GetProgressEntryByIdAsync(string entryId, string requestingUserId)
        {
            var entryEntity = await _context.ProgressEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(pe => pe.Id == entryId);

            if (entryEntity == null)
            {
                _logger.LogInformation("Entrada de progreso con ID {EntryId} no encontrada.", entryId);
                return null;
            }

            // Lógica de permisos: El cliente dueño, un profesional vinculado al dueño, o un admin pueden verla.
            var requester = await _userManager.FindByIdAsync(requestingUserId);
            if (requester == null) return null; // Solicitante no encontrado

            bool isOwner = entryEntity.UserId == requestingUserId;
            bool isAdmin = requester.AppRole == UserRole.Admin;

            var entryOwner = await _userManager.FindByIdAsync(entryEntity.UserId);
            if (entryOwner == null) return null; // Dueño de la entrada no encontrado (raro)

            bool isLinkedNutritionist = requester.AppRole == UserRole.Nutritionist && entryOwner.MyNutritionistId == requestingUserId;
            bool isLinkedTrainer = requester.AppRole == UserRole.Trainer && entryOwner.MyTrainerId == requestingUserId;

            if (!isOwner && !isAdmin && !isLinkedNutritionist && !isLinkedTrainer)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó acceder a la entrada de progreso {EntryId} sin autorización.", requestingUserId, entryId);
                return null;
            }

            return MapearEntidadADto(entryEntity);
        }

        public async Task<ProgressEntryDto?> UpdateProgressEntryAsync(string entryId, ProgressEntryDto progressDto, string requestingUserId)
        {
            var entryEntity = await _context.ProgressEntries.FirstOrDefaultAsync(pe => pe.Id == entryId);

            if (entryEntity == null)
            {
                _logger.LogWarning("Intento de actualizar entrada de progreso no encontrada con ID: {EntryId}", entryId);
                return null;
            }

            // Lógica de permisos: Solo el cliente que la creó puede actualizarla.
            // (Los administradores podrían tener este permiso, pero lo mantenemos simple por ahora).
            if (entryEntity.UserId != requestingUserId)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó actualizar la entrada de progreso {EntryId} sin ser el propietario.", requestingUserId, entryId);
                return null; // No autorizado
            }

            // Actualizar campos
            entryEntity.EntryDate = progressDto.EntryDate;
            entryEntity.WeightKg = progressDto.WeightKg;
            entryEntity.BodyFatPercentage = progressDto.BodyFatPercentage;
            entryEntity.Notes = progressDto.Notes;
            entryEntity.Measurements = progressDto.Measurements; // Usa la propiedad [NotMapped] que maneja la serialización
            entryEntity.PhotoUrls = progressDto.PhotoUrls;     // Usa la propiedad [NotMapped] que maneja la serialización

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Entrada de progreso {EntryId} actualizada por el usuario {RequestingUserId}.", entryId, requestingUserId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar la entrada de progreso {EntryId}.", entryId);
                return null;
            }

            return MapearEntidadADto(entryEntity);
        }

        public async Task<bool> DeleteProgressEntryAsync(string entryId, string requestingUserId)
        {
            var entryEntity = await _context.ProgressEntries.FirstOrDefaultAsync(pe => pe.Id == entryId);

            if (entryEntity == null)
            {
                _logger.LogWarning("Intento de eliminar entrada de progreso no encontrada con ID: {EntryId}", entryId);
                return false;
            }

            // Lógica de permisos: Solo el cliente que la creó o un Admin pueden eliminarla.
            var requester = await _userManager.FindByIdAsync(requestingUserId);
            if (requester == null) return false; // Solicitante no encontrado

            bool isOwner = entryEntity.UserId == requestingUserId;
            bool isAdmin = requester.AppRole == UserRole.Admin;

            if (!isOwner && !isAdmin)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó eliminar la entrada de progreso {EntryId} sin autorización.", requestingUserId, entryId);
                return false; // No autorizado
            }

            _context.ProgressEntries.Remove(entryEntity);

            try
            {
                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    _logger.LogInformation("Entrada de progreso {EntryId} eliminada por el usuario {RequestingUserId}.", entryId, requestingUserId);
                    return true;
                }
                _logger.LogWarning("No se afectaron filas al intentar eliminar la entrada de progreso {EntryId}.", entryId);
                return false;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar la entrada de progreso {EntryId}.", entryId);
                return false;
            }
        }

        // --- Método auxiliar de Mapeo ---
        private static ProgressEntryDto? MapearEntidadADto(ProgressEntryEntity? entity)
        {
            if (entity == null) return null;

            return new ProgressEntryDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                EntryDate = entity.EntryDate,
                WeightKg = entity.WeightKg,
                BodyFatPercentage = entity.BodyFatPercentage,
                Measurements = entity.Measurements, // Usa la propiedad [NotMapped] que deserializa
                PhotoUrls = entity.PhotoUrls,     // Usa la propiedad [NotMapped] que deserializa
                Notes = entity.Notes,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
