using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class archivos_informes_pre
    {
        [Key]
        public int ID { get; set; }
        public int PRELIMINARESID { get; set; }
        public DateTime FECHA_ARCHIVO { get; set; }
        public string? DESCRIPCION_ARCHIVO { get; set; }
        public string? NOMBRE_ARCHIVO { get; set; }
        public string? URL_ARCHIVO { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }

        [ForeignKey("PRELIMINARESID")]
        public preliminares? PRELIMINARES { get; set; }
    }
}
