using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class motivo_devolucion_fin
    {
        [Key]
        public int ID { get; set; }
        public int? FINALESID { get; set; }
        public DateTime FECHA_DEVOLUCION { get; set; }
        public string? MOTIVO_DEVOLUCION { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }

        //[ForeignKey("FINALESID")]
        //public finales? FINALES { get; set; }
    }
}
