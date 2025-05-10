// NutriPlat.Api/Models/ProgressEntryEntity.cs
using System;
using System.Collections.Generic; // Para List<string>
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json; // Para serializar/deserializar Dictionary y List

namespace NutriPlat.Api.Models
{
    /// <summary>
    /// Entidad para una entrada de seguimiento de progreso que se almacenará en la base de datos.
    /// </summary>
    public class ProgressEntryEntity
    {
        /// <summary>
        /// Identificador único de la entrada de progreso.
        /// Será generado automáticamente por la base de datos.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Identificador del usuario (Cliente) al que pertenece esta entrada de progreso.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Propiedad de navegación al usuario (Cliente).
        /// </summary>
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        /// <summary>
        /// Fecha en que se registró esta entrada de progreso.
        /// </summary>
        [Required]
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Peso del usuario en kilogramos (opcional).
        /// </summary>
        public decimal? WeightKg { get; set; }

        /// <summary>
        /// Porcentaje de grasa corporal (opcional).
        /// </summary>
        public decimal? BodyFatPercentage { get; set; }

        /// <summary>
        /// Medidas corporales almacenadas como una cadena JSON.
        /// Ejemplo: {"CinturaCm": 80.5, "PechoCm": 102.0}
        /// </summary>
        public string? MeasurementsJson { get; set; }

        /// <summary>
        /// URLs de las fotos de progreso almacenadas como una cadena JSON.
        /// Ejemplo: ["url1.jpg", "url2.png"]
        /// </summary>
        public string? PhotoUrlsJson { get; set; }

        /// <summary>
        /// Notas o comentarios del usuario sobre su progreso o cómo se sintió.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Fecha de creación del registro en el sistema.
        /// Se establece automáticamente al añadir la entidad.
        /// </summary>
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;


        // Propiedades NotMapped para facilitar el acceso a Measurements y PhotoUrls
        // Estas no se guardan directamente en la BD, sino que usan MeasurementsJson y PhotoUrlsJson.

        /// <summary>
        /// Propiedad de conveniencia para acceder a las medidas deserializadas.
        /// No se mapea a la base de datos.
        /// </summary>
        [NotMapped]
        public Dictionary<string, decimal>? Measurements
        {
            get => string.IsNullOrEmpty(MeasurementsJson) ? null : JsonSerializer.Deserialize<Dictionary<string, decimal>>(MeasurementsJson);
            set => MeasurementsJson = value == null || !value.Any() ? null : JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Propiedad de conveniencia para acceder a las URLs de las fotos deserializadas.
        /// No se mapea a la base de datos.
        /// </summary>
        [NotMapped]
        public List<string>? PhotoUrls
        {
            get => string.IsNullOrEmpty(PhotoUrlsJson) ? null : JsonSerializer.Deserialize<List<string>>(PhotoUrlsJson);
            set => PhotoUrlsJson = value == null || !value.Any() ? null : JsonSerializer.Serialize(value);
        }
    }
}
