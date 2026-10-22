using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class archivos_informes_fin
    {
        [Key]
        public int ID { get; set; }
        public int FINALESID { get; set; }
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

        [ForeignKey("FINALESID")]
        public finales? FINALES { get; set; }
    }
}
