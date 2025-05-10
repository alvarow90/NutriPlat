// NutriPlat.Shared/Dtos/ProgressEntryDto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NutriPlat.Shared.Dtos
{
    /// <summary>
    /// Data Transfer Object para una entrada de seguimiento de progreso.
    /// </summary>
    public class ProgressEntryDto
    {
        /// <summary>
        /// Identificador único de la entrada de progreso (generado por la API).
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Identificador del usuario (Cliente) al que pertenece esta entrada de progreso.
        /// Al crear, este será el ID del usuario autenticado.
        /// </summary>
        public string? UserId { get; set; } // Se establecerá desde el token al crear

        /// <summary>
        /// Fecha en que se registró esta entrada de progreso.
        /// </summary>
        [Required(ErrorMessage = "La fecha de la entrada es obligatoria.")]
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Peso del usuario en kilogramos (opcional).
        /// </summary>
        [Range(0, 500, ErrorMessage = "El peso debe ser un valor válido.")]
        public decimal? WeightKg { get; set; }

        /// <summary>
        /// Porcentaje de grasa corporal (opcional).
        /// </summary>
        [Range(0, 100, ErrorMessage = "El porcentaje de grasa corporal debe estar entre 0 y 100.")]
        public decimal? BodyFatPercentage { get; set; }

        /// <summary>
        /// Medidas corporales (ej. cintura, cadera, pecho en cm).
        /// Clave: Nombre de la medida (ej. "CinturaCm"). Valor: Medida.
        /// </summary>
        public Dictionary<string, decimal>? Measurements { get; set; }

        /// <summary>
        /// URLs de las fotos de progreso (opcional).
        /// Para el MVP, solo almacenaremos las URLs. La subida de archivos es más compleja.
        /// </summary>
        public List<string>? PhotoUrls { get; set; }

        /// <summary>
        /// Notas o comentarios del usuario sobre su progreso o cómo se sintió.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Las notas no pueden exceder los 1000 caracteres.")]
        public string? Notes { get; set; }

        /// <summary>
        /// Fecha de creación del registro en el sistema (solo lectura para el cliente).
        /// </summary>
        public DateTime? CreatedAt { get; /*private*/ set; } // El 'private set' es ideal si solo lo llena la API
    }
}
