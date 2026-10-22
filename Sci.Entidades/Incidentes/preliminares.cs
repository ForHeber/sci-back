using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class preliminares
    {
        [Key]
        public int ID { get; set; }
        public int RUT_INFORMANTE { get; set; }
        public string? UNIDAD_INFORMANTE { get; set; }
        public string? INCIDENTE_ACCIDENTE { get; set; }
        public int? TIPO_INGESTAID { get; set; }
        public DateTime FECHA_INFORMANTE { get; set; }
        public int? TIPO_INCIDENTEID { get; set; }
        public string? PERSONAL_UNIDAD { get; set; }
        public string? UNIDAD_AFECTADA { get; set; }
        public DateTime FECHA_HECHO { get; set; }
        public string? LUGAR_HECHO { get; set; }
        public string? CIRCUNSTANCIA_HECHO { get; set; }
        public string? DANOS_PERSONALES { get; set; }
        public string? MEDIDAS_ADOPTADAS { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }

        [ForeignKey("TIPO_INGESTAID")]
        public tipo_ingesta? TIPO_INGESTA { get; set; }

        [ForeignKey("TIPO_INCIDENTEID")]
        public tipo_incidente? TIPO_INCIDENTE { get; set; }

        [ForeignKey("RUT_INFORMANTE")]
        public persona? INFORMANTE { get; set; }

        [ForeignKey("UNIDAD_INFORMANTE")]
        public unidad? U_INFORMANTE { get; set; }

        [ForeignKey("UNIDAD_AFECTADA")]
        public unidad? U_AFECTADA { get; set; }

    }
}
