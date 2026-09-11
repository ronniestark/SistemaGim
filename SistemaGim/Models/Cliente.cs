using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGim.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int ClienteId { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        public bool Estado { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Navegación
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
        public virtual ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
    }
}