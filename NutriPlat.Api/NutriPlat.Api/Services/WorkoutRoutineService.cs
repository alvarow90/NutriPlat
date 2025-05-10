// NutriPlat.Api/Services/WorkoutRoutineService.cs
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
    public class WorkoutRoutineService : IWorkoutRoutineService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WorkoutRoutineService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkoutRoutineService(AppDbContext context, ILogger<WorkoutRoutineService> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<WorkoutRoutineDto?> CreateRoutineAsync(WorkoutRoutineDto routineDto, string creatorUserId)
        {
            var creator = await _userManager.FindByIdAsync(creatorUserId);
            if (creator == null || (creator.AppRole != UserRole.Trainer && creator.AppRole != UserRole.Admin))
            {
                _logger.LogWarning("Usuario {CreatorUserId} intentó crear rutina sin ser Trainer o Admin.", creatorUserId);
                return null;
            }
            var routineEntity = new WorkoutRoutineEntity
            {
                Name = routineDto.Name,
                Description = routineDto.Description,
                TrainerId = creatorUserId
            };
            _context.WorkoutRoutines.Add(routineEntity);
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) { _logger.LogError(ex, "Error DB al crear rutina para {CreatorUserId}.", creatorUserId); return null; }
            return MapearRutinaEntidadADto(routineEntity);
        }

        public async Task<IEnumerable<WorkoutRoutineDto>> GetAllRoutinesAsync()
        {
            return await _context.WorkoutRoutines
                .AsNoTracking()
                .Select(r => MapearRutinaEntidadADto(r)!)
                .ToListAsync();
        }

        public async Task<WorkoutRoutineDto?> GetRoutineByIdAsync(string routineId)
        {
            var entity = await _context.WorkoutRoutines.AsNoTracking().FirstOrDefaultAsync(r => r.Id == routineId);
            return entity == null ? null : MapearRutinaEntidadADto(entity);
        }

        public async Task<WorkoutRoutineDto?> UpdateRoutineAsync(string routineId, WorkoutRoutineDto routineDto, string requestingUserId)
        {
            var entity = await _context.WorkoutRoutines.FindAsync(routineId);
            if (entity == null) { _logger.LogWarning("Actualización fallida: Rutina {RoutineId} no encontrada.", routineId); return null; }

            var requester = await _userManager.FindByIdAsync(requestingUserId);
            bool isAdmin = requester != null && requester.AppRole == UserRole.Admin;

            if (entity.TrainerId != requestingUserId && !isAdmin)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó actualizar rutina {RoutineId} sin autorización. Propietario: {OwnerId}", requestingUserId, routineId, entity.TrainerId);
                return null;
            }
            entity.Name = routineDto.Name;
            entity.Description = routineDto.Description;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) { _logger.LogError(ex, "Error DB al actualizar rutina {RoutineId}.", routineId); return null; }
            return MapearRutinaEntidadADto(entity);
        }

        public async Task<bool> DeleteRoutineAsync(string routineId, string requestingUserId)
        {
            var entity = await _context.WorkoutRoutines.FindAsync(routineId);
            if (entity == null) { _logger.LogWarning("Eliminación fallida: Rutina {RoutineId} no encontrada.", routineId); return false; }

            var requester = await _userManager.FindByIdAsync(requestingUserId);
            bool isAdmin = requester != null && requester.AppRole == UserRole.Admin;

            if (entity.TrainerId != requestingUserId && !isAdmin)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó eliminar rutina {RoutineId} sin autorización. Propietario: {OwnerId}", requestingUserId, routineId, entity.TrainerId);
                return false;
            }
            _context.WorkoutRoutines.Remove(entity);
            try { return await _context.SaveChangesAsync() > 0; }
            catch (DbUpdateException ex) { _logger.LogError(ex, "Error DB al eliminar rutina {RoutineId}.", routineId); return false; }
        }

        public async Task<UserWorkoutRoutineDto?> AssignRoutineToClientAsync(AssignWorkoutRoutineDto assignmentDto, string trainerId)
        {
            var trainer = await _userManager.FindByIdAsync(trainerId);
            if (trainer == null || trainer.AppRole != UserRole.Trainer)
            {
                _logger.LogWarning("Intento de asignación de rutina por usuario {TrainerId} que no es Entrenador o no existe.", trainerId);
                return null;
            }

            var client = await _userManager.FindByIdAsync(assignmentDto.ClientId);
            if (client == null || client.AppRole != UserRole.Client)
            {
                _logger.LogWarning("Intento de asignar rutina a usuario {ClientId} que no es Cliente o no existe.", assignmentDto.ClientId);
                return null;
            }

            // --- NUEVA VERIFICACIÓN: ¿Está el cliente vinculado a ESTE entrenador? ---
            if (client.MyTrainerId != trainerId)
            {
                _logger.LogWarning("Entrenador {TrainerId} intentó asignar rutina a cliente {ClientId} no vinculado directamente a él. Cliente vinculado a: {ActualTrainerId}",
                    trainerId, assignmentDto.ClientId, client.MyTrainerId ?? "Nadie");
                return null; // O devolver un DTO de error específico, o lanzar una excepción de negocio.
            }
            // --- FIN DE LA NUEVA VERIFICACIÓN ---

            var workoutRoutine = await _context.WorkoutRoutines.FindAsync(assignmentDto.WorkoutRoutineId);
            if (workoutRoutine == null)
            {
                _logger.LogWarning("Intento de asignar rutina inexistente con ID: {WorkoutRoutineId}", assignmentDto.WorkoutRoutineId);
                return null;
            }

            var existingAssignment = await _context.UserWorkoutRoutines
                .FirstOrDefaultAsync(uwr => uwr.ClientId == assignmentDto.ClientId && uwr.WorkoutRoutineId == assignmentDto.WorkoutRoutineId);

            if (existingAssignment != null)
            {
                _logger.LogInformation("Intento de reasignar rutina {WorkoutRoutineId} a cliente {ClientId} que ya está asignada. Actualizando asignación existente.", assignmentDto.WorkoutRoutineId, assignmentDto.ClientId);
                existingAssignment.StartDate = assignmentDto.StartDate;
                existingAssignment.EndDate = assignmentDto.EndDate;
                existingAssignment.IsActive = assignmentDto.IsActive;
                existingAssignment.AssignedDate = DateTime.UtcNow;
            }
            else
            {
                var newAssignment = new UserWorkoutRoutine
                {
                    ClientId = assignmentDto.ClientId,
                    WorkoutRoutineId = assignmentDto.WorkoutRoutineId,
                    AssignedByTrainerId = trainerId, // El entrenador que realiza la acción
                    AssignedDate = DateTime.UtcNow,
                    StartDate = assignmentDto.StartDate,
                    EndDate = assignmentDto.EndDate,
                    IsActive = assignmentDto.IsActive
                };
                _context.UserWorkoutRoutines.Add(newAssignment);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar la asignación de la rutina {WorkoutRoutineId} al cliente {ClientId}.", assignmentDto.WorkoutRoutineId, assignmentDto.ClientId);
                return null;
            }

            var finalAssignment = await _context.UserWorkoutRoutines
                .Include(uwr => uwr.WorkoutRoutine)
                .Include(uwr => uwr.Client) // Necesario si MapearAsignacionRutinaEntidadADto usa client.FirstName, etc.
                .Include(uwr => uwr.AssignedByTrainer) // Necesario si MapearAsignacionRutinaEntidadADto usa trainer.FirstName, etc.
                .FirstOrDefaultAsync(uwr => uwr.ClientId == assignmentDto.ClientId && uwr.WorkoutRoutineId == assignmentDto.WorkoutRoutineId);

            if (finalAssignment == null || finalAssignment.WorkoutRoutine == null || finalAssignment.Client == null || finalAssignment.AssignedByTrainer == null)
            {
                _logger.LogError("No se pudo recuperar la asignación completa de rutina después de guardar para rutina {WorkoutRoutineId} y cliente {ClientId}.", assignmentDto.WorkoutRoutineId, assignmentDto.ClientId);
                return null;
            }

            return MapearAsignacionRutinaEntidadADto(finalAssignment, finalAssignment.WorkoutRoutine, finalAssignment.Client, finalAssignment.AssignedByTrainer);
        }

        public async Task<IEnumerable<UserWorkoutRoutineDto>> GetAssignedRoutinesForClientAsync(string clientId)
        {
            var client = await _userManager.FindByIdAsync(clientId);
            if (client == null || client.AppRole != UserRole.Client)
            {
                _logger.LogWarning("Intento de obtener rutinas para usuario no cliente o inexistente: {ClientId}", clientId);
                return new List<UserWorkoutRoutineDto>();
            }
            return await _context.UserWorkoutRoutines
                .AsNoTracking()
                .Where(uwr => uwr.ClientId == clientId)
                .Include(uwr => uwr.WorkoutRoutine)
                .Include(uwr => uwr.AssignedByTrainer)
                .Select(uwr => MapearAsignacionRutinaEntidadADto(uwr, uwr.WorkoutRoutine!, client, uwr.AssignedByTrainer!))
                .ToListAsync();
        }

        public async Task<IEnumerable<UserWorkoutRoutineDto>> GetAssignmentsByTrainerAsync(string trainerId)
        {
            var trainer = await _userManager.FindByIdAsync(trainerId);
            if (trainer == null || trainer.AppRole != UserRole.Trainer)
            {
                _logger.LogWarning("Intento de obtener asignaciones de rutinas para usuario no entrenador o inexistente: {TrainerId}", trainerId);
                return new List<UserWorkoutRoutineDto>();
            }
            return await _context.UserWorkoutRoutines
                .AsNoTracking()
                .Where(uwr => uwr.AssignedByTrainerId == trainerId)
                .Include(uwr => uwr.WorkoutRoutine)
                .Include(uwr => uwr.Client)
                .Select(uwr => MapearAsignacionRutinaEntidadADto(uwr, uwr.WorkoutRoutine!, uwr.Client!, trainer))
                .ToListAsync();
        }

        public async Task<bool> UnassignRoutineFromClientAsync(string clientId, string workoutRoutineId, string requestingUserId)
        {
            var assignment = await _context.UserWorkoutRoutines
                .FirstOrDefaultAsync(uwr => uwr.ClientId == clientId && uwr.WorkoutRoutineId == workoutRoutineId);
            if (assignment == null)
            {
                _logger.LogWarning("Intento de desasignar rutina no asignada. Cliente: {ClientId}, Rutina: {WorkoutRoutineId}", clientId, workoutRoutineId);
                return false;
            }

            var requester = await _userManager.FindByIdAsync(requestingUserId);
            bool isAdmin = requester != null && requester.AppRole == UserRole.Admin;

            if (assignment.AssignedByTrainerId != requestingUserId && !isAdmin)
            {
                _logger.LogWarning("Usuario {RequestingUserId} intentó desasignar rutina de cliente {ClientId} sin autorización. Asignada por: {AssignedBy}",
                    requestingUserId, clientId, assignment.AssignedByTrainerId);
                return false;
            }
            _context.UserWorkoutRoutines.Remove(assignment);
            try { return await _context.SaveChangesAsync() > 0; }
            catch (DbUpdateException ex) { _logger.LogError(ex, "Error al eliminar la asignación de la rutina {WorkoutRoutineId} al cliente {ClientId}.", workoutRoutineId, clientId); return false; }
        }

        private static WorkoutRoutineDto? MapearRutinaEntidadADto(WorkoutRoutineEntity? entity)
        {
            if (entity == null) return null;
            return new WorkoutRoutineDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                TrainerId = entity.TrainerId
            };
        }

        private static UserWorkoutRoutineDto MapearAsignacionRutinaEntidadADto(
            UserWorkoutRoutine assignment,
            WorkoutRoutineEntity routine,
            ApplicationUser client,
            ApplicationUser trainer)
        {
            return new UserWorkoutRoutineDto
            {
                ClientId = assignment.ClientId,
                WorkoutRoutineId = assignment.WorkoutRoutineId,
                WorkoutRoutineName = routine.Name,
                WorkoutRoutineDescription = routine.Description,
                AssignedByTrainerId = assignment.AssignedByTrainerId,
                AssignedDate = assignment.AssignedDate,
                StartDate = assignment.StartDate,
                EndDate = assignment.EndDate,
                IsActive = assignment.IsActive
            };
        }
    }
}
