using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class involucrados_pre
    {
        [Key]
        public int ID { get; set; }
        public int? PRELIMINARESID { get; set; }
        public string? TIPO_INVOLUCRADO { get; set; }
        public int RUT_INVOLUCRADO { get; set; }
        public string? UNIDAD_INVOLUCRADO { get; set; }
        public DateTime? FECHA_NACIMIENTO { get; set; }
        public int? TIPO_INTERVINIENTEID { get; set; }
        public int? TIPO_CONSUMOID { get; set; }
        public int? ESTADO_SALUDID { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }

        [ForeignKey("PRELIMINARESID")]
        public preliminares? PRELIMINARES { get; set; }

        [ForeignKey("TIPO_INTERVINIENTEID")]
        public tipo_interviniente? TIPO_INTERVINIENTE { get; set; }

        [ForeignKey("TIPO_CONSUMOID")]
        public tipo_consumo? TIPO_CONSUMO { get; set; }

        [ForeignKey("ESTADO_SALUDID")]
        public estado_salud? ESTADO_SALUD { get; set; }

        [ForeignKey("RUT_INVOLUCRADO")]
        public persona_sci? R_INVOLUCRADO { get; set; }

        [ForeignKey("UNIDAD_INVOLUCRADO")]
        public unidad? U_INVOLUCRADO { get; set; }

    }
}
