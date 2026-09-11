using SistemaGim.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGim.Models
{
    [Table("Pagos")]
    public class Pago
    {
        [Key]
        public int PagoId { get; set; }

        public int? ClienteId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Concepto { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Monto { get; set; }

        public DateTime FechaInicioVigencia { get; set; }

        public DateTime FechaFinVigencia { get; set; }

        public DateTime FechaTransaccion { get; set; } = DateTime.Now;

        // Navegación con fuertemente tipado
        [ForeignKey(nameof(ClienteId))]
        public virtual Cliente? Cliente { get; set; }

        public virtual ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
    }
}