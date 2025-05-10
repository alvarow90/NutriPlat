// NutriPlat.Api/Models/ProgressEntryEntity.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq; // Para .Any()
using System.Text.Json; // Para serializar/deserializar Dictionary y List

namespace NutriPlat.Api.Models // Asegúrate de que el espacio de nombres sea correcto
{
    /// <summary>
    /// Entidad para una entrada de seguimiento de progreso que se almacenará en la base de datos.
    /// </summary>
    public class ProgressEntryEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = string.Empty; // Clave foránea al ApplicationUser

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; } // Propiedad de navegación al usuario

        [Required]
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        public decimal? WeightKg { get; set; }
        public decimal? BodyFatPercentage { get; set; }

        /// <summary>
        /// Medidas corporales almacenadas como una cadena JSON.
        /// </summary>
        public string? MeasurementsJson { get; set; }

        /// <summary>
        /// URLs de las fotos de progreso almacenadas como una cadena JSON.
        /// </summary>
        public string? PhotoUrlsJson { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        [NotMapped]
        public Dictionary<string, decimal>? Measurements
        {
            get => string.IsNullOrEmpty(MeasurementsJson)
                       ? new Dictionary<string, decimal>() // Devolver diccionario vacío si es null o vacío
                       : JsonSerializer.Deserialize<Dictionary<string, decimal>>(MeasurementsJson);
            set => MeasurementsJson = (value == null || !value.Any())
                                     ? null
                                     : JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string>? PhotoUrls
        {
            get => string.IsNullOrEmpty(PhotoUrlsJson)
                       ? new List<string>() // Devolver lista vacía si es null o vacío
                       : JsonSerializer.Deserialize<List<string>>(PhotoUrlsJson);
            set => PhotoUrlsJson = (value == null || !value.Any())
                                   ? null
                                   : JsonSerializer.Serialize(value);
        }
    }
}
