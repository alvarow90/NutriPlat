// NutriPlat.Api/Services/IWorkoutRoutineService.cs
using NutriPlat.Shared.Dtos; // <-- Directiva using CRUCIAL
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NutriPlat.Api.Services // <-- Espacio de nombres importante
{
    public interface IWorkoutRoutineService
    {
        Task<WorkoutRoutineDto?> CreateRoutineAsync(WorkoutRoutineDto routineDto, string creatorUserId);
        Task<IEnumerable<WorkoutRoutineDto>> GetAllRoutinesAsync();
        Task<WorkoutRoutineDto?> GetRoutineByIdAsync(string routineId);
        Task<WorkoutRoutineDto?> UpdateRoutineAsync(string routineId, WorkoutRoutineDto routineDto, string requestingUserId);
        Task<bool> DeleteRoutineAsync(string routineId, string requestingUserId);

        Task<UserWorkoutRoutineDto?> AssignRoutineToClientAsync(AssignWorkoutRoutineDto assignmentDto, string trainerId);
        Task<IEnumerable<UserWorkoutRoutineDto>> GetAssignedRoutinesForClientAsync(string clientId);
        Task<IEnumerable<UserWorkoutRoutineDto>> GetAssignmentsByTrainerAsync(string trainerId);
        Task<bool> UnassignRoutineFromClientAsync(string clientId, string workoutRoutineId, string requestingUserId);
    }
}
