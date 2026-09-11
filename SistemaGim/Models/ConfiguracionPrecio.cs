using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGim.Models
{
    [Table("ConfiguracionPrecios")]
    public class ConfiguracionPrecio
    {
        [Key]
        public int ConfigId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Concepto { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Monto { get; set; }

        public int DiasVigencia { get; set; }
    }
}