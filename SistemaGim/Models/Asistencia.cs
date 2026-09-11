using SistemaGim.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGim.Models
{
    [Table("Asistencias")]
    public class Asistencia
    {
        [Key]
        public int AsistenciaId { get; set; }

        public int? ClienteId { get; set; }

        public int? PagoId { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey(nameof(ClienteId))]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey(nameof(PagoId))]
        public virtual Pago? Pago { get; set; }
    }
}