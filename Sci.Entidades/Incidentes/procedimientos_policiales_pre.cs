using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class procedimientos_policiales_pre
    {
        [Key]
        public int ID { get; set; }
        public int? PRELIMINARESID { get; set; }
        public DateTime FECHA_PROCEDIMIENTO_POL { get; set; }
        public int CONDICION_POLICIALID { get; set; }
        public string? DETALLE_PROCEDIMIENTO_POL { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }

        [ForeignKey("PRELIMINARESID")]
        public preliminares? PRELIMINARES { get; set; }

        [ForeignKey("CONDICION_POLICIALID")]
        public condicion_policial? CONDICION_POLICIAL { get; set; }

    }
    
}
