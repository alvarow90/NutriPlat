// NutriPlat.Api/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NutriPlat.Api.Data;
using NutriPlat.Api.Models;
using NutriPlat.Api.Services;
using NutriPlat.Shared.Enums; // Para UserRole
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar CORS (Cross-Origin Resource Sharing)
// Esto es importante si tu frontend (app móvil/web) se sirve desde un origen diferente.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", // Puedes darle un nombre más restrictivo
        policy =>
        {
            policy.AllowAnyOrigin() // En producción, especifica los orígenes permitidos
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// 2. Configurar Entity Framework Core y DbContext
// Usaremos SQLite para el MVP. La cadena de conexión estará en appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// 3. Configurar ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configuración de contraseña (puedes ajustarla según tus necesidades)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false; // Simplificado para MVP
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Configuración de Lockout (opcional)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Configuración de Usuario
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true; // Asegura que los correos sean únicos
})
    .AddEntityFrameworkStores<AppDbContext>() // Usa nuestro AppDbContext
    .AddDefaultTokenProviders(); // Para tokens de reseteo de contraseña, etc.

// 4. Configurar Autenticación JWT (JSON Web Token)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new ArgumentNullException("JwtSettings:SecretKey", "JWT SecretKey no está configurada en appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true; // Guarda el token en el HttpContext después de una autenticación exitosa
    options.RequireHttpsMetadata = builder.Environment.IsProduction(); // Solo HTTPS en producción
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero, // Elimina el margen de tiempo por defecto en la validación de expiración

        ValidAudience = jwtSettings["ValidAudience"],
        ValidIssuer = jwtSettings["ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// 5. Registrar servicios personalizados
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INutritionPlanService, NutritionPlanService>();
builder.Services.AddScoped<IWorkoutRoutineService, WorkoutRoutineService>(); // Asegúrate de que este esté aquí si ya implementaste WorkoutRoutines
builder.Services.AddScoped<IUserService, UserService>(); // <-- AÑADE ESTA LÍNEA
builder.Services.AddScoped<IProgressTrackingService, ProgressTrackingService>();


// 6. Configurar Controladores
builder.Services.AddControllers()
    .AddJsonOptions(options => // Opcional: si quieres que los enums se serialicen como strings
    {
        // options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });


// 7. Configurar Swagger/OpenAPI (para documentación y pruebas de API)
builder.Services.AddEndpointsApiExplorer(); // Necesario para API Explorer (usado por Swagger)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "NutriPlat API", Version = "v1" });

    // Definir el esquema de seguridad para JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, introduce 'Bearer' seguido de un espacio y el token JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }});
});


// --- Construir la aplicación ---
var app = builder.Build();

// --- Configurar el pipeline de middleware HTTP ---

// Aplicar migraciones y sembrar datos (roles) al iniciar (opcional, pero útil para desarrollo)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync(); // Aplica migraciones pendientes

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRolesAsync(roleManager); // Método para sembrar roles
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error durante la migración o el sembrado de datos.");
    }
}


// Middleware para desarrollo (Swagger)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NutriPlat API V1");
        // c.RoutePrefix = string.Empty; // Para servir Swagger UI en la raíz
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


// --- Método auxiliar para sembrar roles ---
async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = {
        UserRole.Admin.ToString(),
        UserRole.Nutritionist.ToString(),
        UserRole.Trainer.ToString(),
        UserRole.Client.ToString()
    };

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
