using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class indices_alertas
    {
        [Key]
        public int ID { get; set; }
        public string? TIPO_INFORME { get; set; }
        public int? ALERTA_AMARILLA { get; set; }
        public int? ALERTA_ROJA { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }
        
    }
}
