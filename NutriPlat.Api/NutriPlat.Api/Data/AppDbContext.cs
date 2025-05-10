// NutriPlat.Api/Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NutriPlat.Api.Models; // Para todas tus entidades

namespace NutriPlat.Api.Data // <-- Espacio de nombres importante
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<NutritionPlanEntity> NutritionPlans { get; set; } = null!;
        public DbSet<WorkoutRoutineEntity> WorkoutRoutines { get; set; } = null!;
        public DbSet<UserNutritionPlan> UserNutritionPlans { get; set; } = null!;
        public DbSet<UserWorkoutRoutine> UserWorkoutRoutines { get; set; } = null!; // <-- DbSet para la nueva tabla de unión


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserNutritionPlan>(entity =>
            {
                entity.HasKey(unp => new { unp.ClientId, unp.NutritionPlanId });
                entity.HasOne(unp => unp.Client)
                    .WithMany(u => u.AssignedNutritionPlans)
                    .HasForeignKey(unp => unp.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(unp => unp.NutritionPlan)
                    .WithMany(np => np.UserAssignments)
                    .HasForeignKey(unp => unp.NutritionPlanId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(unp => unp.AssignedByNutritionist)
                    .WithMany() // No se necesita colección inversa en ApplicationUser para "asignaciones hechas por mí" a través de esta FK directa
                    .HasForeignKey(unp => unp.AssignedByNutritionistId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<UserWorkoutRoutine>(entity => // <-- Configuración para la nueva tabla de unión
            {
                entity.HasKey(uwr => new { uwr.ClientId, uwr.WorkoutRoutineId });
                entity.HasOne(uwr => uwr.Client)
                    .WithMany(u => u.AssignedWorkoutRoutines)
                    .HasForeignKey(uwr => uwr.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(uwr => uwr.WorkoutRoutine)
                    .WithMany(wr => wr.UserAssignments)
                    .HasForeignKey(uwr => uwr.WorkoutRoutineId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(uwr => uwr.AssignedByTrainer)
                    .WithMany() // No se necesita colección inversa en ApplicationUser para "asignaciones hechas por mí" a través de esta FK directa
                    .HasForeignKey(uwr => uwr.AssignedByTrainerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
