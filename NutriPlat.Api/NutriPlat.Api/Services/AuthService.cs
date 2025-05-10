// NutriPlat.Api/Services/AuthService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration; // Para IConfiguration
using Microsoft.IdentityModel.Tokens; // Para SymmetricSecurityKey, SigningCredentials
using NutriPlat.Api.Models; // Para ApplicationUser
using NutriPlat.Shared.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; // Para JwtSecurityTokenHandler, JwtSecurityToken
using System.Security.Claims; // Para Claim
using System.Text; // Para Encoding
using System.Threading.Tasks;

namespace NutriPlat.Api.Services
{
    /// <summary>
    /// Implementación del servicio de autenticación.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager; // Aunque no lo usemos directamente para login con JWT, es útil tenerlo.
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterRequestDto registerDto)
        {
            var user = new ApplicationUser
            {
                UserName = registerDto.Email, // UserName en IdentityUser suele ser el email
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                AppRole = registerDto.Role // Asignamos el rol de nuestra enum compartida
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                // Asignar el rol de Identity. Los roles deben existir en la BD.
                // Para el MVP, podríamos crear los roles en Program.cs o manualmente.
                // Aquí asumimos que el rol (convertido a string) ya existe.
                await _userManager.AddToRoleAsync(user, registerDto.Role.ToString());
            }

            return result;
        }

        public async Task<TokenResponseDto?> LoginUserAsync(LoginRequestDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            // Verificar si el usuario existe y la contraseña es correcta
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return null; // Credenciales inválidas
            }

            // Si el usuario es válido, generar el token JWT
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), // ID del usuario
                new Claim(JwtRegisteredClaimNames.Sub, user.Email), // Sujeto del token (email)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único del token
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"), // Nombre completo
                // new Claim("app_role", user.AppRole.ToString()) // Rol de nuestra enum, si queremos añadirlo como claim personalizado
            };

            // Obtener los roles de Identity del usuario y añadirlos como claims
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Leer la configuración del token desde appsettings.json
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no está configurada.")));
            var tokenValidityInMinutes = Convert.ToInt32(jwtSettings["TokenValidityInMinutes"] ?? "60"); // Por defecto 60 minutos

            var token = new JwtSecurityToken(
                issuer: jwtSettings["ValidIssuer"],
                audience: jwtSettings["ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(tokenValidityInMinutes), // Usar UtcNow para consistencia
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new TokenResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = token.ValidTo,
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = userRoles.FirstOrDefault() ?? user.AppRole.ToString() // Devolver el primer rol de Identity o el AppRole
            };
        }
    }
}
