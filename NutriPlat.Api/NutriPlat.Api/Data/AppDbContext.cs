// NutriPlat.Api/Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NutriPlat.Api.Models; // Para todas tus entidades

namespace NutriPlat.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSets para tus entidades principales
        public DbSet<NutritionPlanEntity> NutritionPlans { get; set; } = null!;
        public DbSet<WorkoutRoutineEntity> WorkoutRoutines { get; set; } = null!;
        public DbSet<ProgressEntryEntity> ProgressEntries { get; set; } = null!;

        // DbSets para tus entidades de unión (tablas de asignación)
        public DbSet<UserNutritionPlan> UserNutritionPlans { get; set; } = null!;
        public DbSet<UserWorkoutRoutine> UserWorkoutRoutines { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Esencial para que Identity configure sus tablas

            // Configuración para la entidad de unión UserNutritionPlan
            builder.Entity<UserNutritionPlan>(entity =>
            {
                // Definir la clave primaria compuesta
                entity.HasKey(unp => new { unp.ClientId, unp.NutritionPlanId });

                // Relación con ApplicationUser (Cliente)
                entity.HasOne(unp => unp.Client)
                    .WithMany(u => u.AssignedNutritionPlans) // En ApplicationUser
                    .HasForeignKey(unp => unp.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con NutritionPlanEntity
                entity.HasOne(unp => unp.NutritionPlan)
                    .WithMany(np => np.UserAssignments) // En NutritionPlanEntity
                    .HasForeignKey(unp => unp.NutritionPlanId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con ApplicationUser (Nutricionista que asigna)
                entity.HasOne(unp => unp.AssignedByNutritionist)
                    .WithMany() // No se necesita colección inversa explícita en ApplicationUser para "asignaciones hechas por mí" a través de esta FK
                    .HasForeignKey(unp => unp.AssignedByNutritionistId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para la entidad de unión UserWorkoutRoutine
            builder.Entity<UserWorkoutRoutine>(entity =>
            {
                // Definir la clave primaria compuesta
                entity.HasKey(uwr => new { uwr.ClientId, uwr.WorkoutRoutineId });

                // Relación con ApplicationUser (Cliente)
                entity.HasOne(uwr => uwr.Client)
                    .WithMany(u => u.AssignedWorkoutRoutines) // En ApplicationUser
                    .HasForeignKey(uwr => uwr.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con WorkoutRoutineEntity
                entity.HasOne(uwr => uwr.WorkoutRoutine)
                    .WithMany(wr => wr.UserAssignments) // En WorkoutRoutineEntity
                    .HasForeignKey(uwr => uwr.WorkoutRoutineId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con ApplicationUser (Entrenador que asigna)
                entity.HasOne(uwr => uwr.AssignedByTrainer)
                    .WithMany() // No se necesita colección inversa explícita en ApplicationUser para "asignaciones hechas por mí" a través de esta FK
                    .HasForeignKey(uwr => uwr.AssignedByTrainerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para ProgressEntryEntity
            builder.Entity<ProgressEntryEntity>(entity =>
            {
                // La clave primaria Id ya está definida con [Key] y [DatabaseGenerated] en la entidad.
                // Configurar la relación con ApplicationUser (el usuario que hizo la entrada)
                entity.HasOne(pe => pe.User) // Propiedad de navegación en ProgressEntryEntity
                    .WithMany(u => u.ProgressEntries) // Propiedad de colección en ApplicationUser
                    .HasForeignKey(pe => pe.UserId) // Clave foránea en ProgressEntryEntity
                    .OnDelete(DeleteBehavior.Cascade); // Si se elimina el usuario, se eliminan sus entradas de progreso.
            });

            // Configuración para Vinculación Cliente-Profesional en ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                // Relación: Cliente tiene un Nutricionista asignado
                entity.HasOne(client => client.MyNutritionist) // Propiedad de navegación en ApplicationUser (para el cliente)
                    .WithMany(nutritionist => nutritionist.LinkedClientsAsNutritionist) // Colección en ApplicationUser (para el nutricionista)
                    .HasForeignKey(client => client.MyNutritionistId) // Clave foránea en ApplicationUser (para el cliente)
                    .OnDelete(DeleteBehavior.SetNull) // Si se elimina el Nutricionista, el MyNutritionistId del Cliente se vuelve null
                    .IsRequired(false); // MyNutritionistId es opcional

                // Relación: Cliente tiene un Entrenador asignado
                entity.HasOne(client => client.MyTrainer) // Propiedad de navegación en ApplicationUser (para el cliente)
                    .WithMany(trainer => trainer.LinkedClientsAsTrainer) // Colección en ApplicationUser (para el entrenador)
                    .HasForeignKey(client => client.MyTrainerId) // Clave foránea en ApplicationUser (para el cliente)
                    .OnDelete(DeleteBehavior.SetNull) // Si se elimina el Entrenador, el MyTrainerId del Cliente se vuelve null
                    .IsRequired(false); // MyTrainerId es opcional
            });
        }
    }
}
