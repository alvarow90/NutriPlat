// NutriPlat.Shared/Enums/UserRole.cs
namespace NutriPlat.Shared.Enums
{
    /// <summary>
    /// Define los roles de usuario en el sistema.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Usuario final de la aplicación (paciente/cliente).
        /// </summary>
        Client,

        /// <summary>
        /// Profesional de la nutrición.
        /// </summary>
        Nutritionist,

        /// <summary>
        /// Profesional del entrenamiento físico.
        /// </summary>
        Trainer,

        /// <summary>
        /// Administrador del sistema con todos los permisos.
        /// </summary>
        Admin
    }
}
