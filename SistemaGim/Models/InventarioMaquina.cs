using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGim.Models
{
    [Table("InventarioMaquinas")]
    public class InventarioMaquina
    {
        [Key]
        public int MaquinaId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty; // Máquina, Peso Libre, etc.

        [MaxLength(30)]
        public string? PesoEspecifico { get; set; } // Opcional (ej: 5 lbs)

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Costo { get; set; }

        public int Cantidad { get; set; } = 1;

        [Required]
        [MaxLength(30)]
        public string Estado { get; set; } = "Activo";

        public DateTime FechaAdquisicion { get; set; } = DateTime.Now;
    }
}