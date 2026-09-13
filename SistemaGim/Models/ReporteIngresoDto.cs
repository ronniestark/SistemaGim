using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaGim.Models
{
    public class ReporteIngresoDto
    {
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
