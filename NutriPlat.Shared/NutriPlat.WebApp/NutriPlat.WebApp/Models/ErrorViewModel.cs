// NutriPlat.WebApp/Models/ErrorViewModel.cs
namespace NutriPlat.WebApp.Models // Asegúrate de que este espacio de nombres coincida con tu proyecto
{
    /// <summary>
    /// Modelo para pasar información de error a la vista de error.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Identificador de la solicitud que causó el error.
        /// Puede ser útil para rastrear errores en los logs.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Propiedad de conveniencia para determinar si se debe mostrar el RequestId.
        /// Solo se muestra si RequestId tiene un valor.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Podrías añadir más propiedades aquí si necesitas pasar más información sobre el error a la vista,
        // como un mensaje de error específico, el código de estado, etc.
        // public string? ErrorMessage { get; set; }
        // public int? StatusCode { get; set; }
    }
}
