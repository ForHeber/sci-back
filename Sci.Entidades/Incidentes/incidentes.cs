using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class incidentes
    {
        [Key]
        public int ID { get; set; }
        public int? PRELIMINARESID { get; set; }
        public DateTime FECHA_PUBLICACION { get; set; }
        public string? DEVUELTO { get; set; }
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
        public string? ESPECIFICACION_VESTUARIO { get; set; }
        public int? TIPO_IDENTIFICACIONID { get; set; }
        public string? PORTABA_ARMA { get; set; }
        public int? TIPO_ARMAMENTOID { get; set; }
        public string? ARMA_FISCAL_PARTICULAR { get; set; }
        public string? DETALLE_PORTE_ARMA { get; set; }
        public string? REALIZA_INVESTIGACION { get; set; }
        public int? TIPO_INVESTIGACIONID { get; set; }
        public string? DETALLE_INVESTIGACION { get; set; }
        public string? UNIDAD_INTELIGENCIA_ADOPTA { get; set; }
        public string? CARACTERISTICAS_MATERIAL { get; set; }
        public string? DETALLE_RESULTADO_MUERTE { get; set; }
        public string? DETALLE_DOCUMENTACION_PERDIDA { get; set; }
        public string? AUTORIDAD_REQUIERE_INFO { get; set; }
        public string? MEDIDAS_ADOPTADAS_FAMILIARES { get; set; }
        public string? SINTESIS_MEDIOS_PRENSA { get; set; }
        public string? MEDIDAS_DISCIPLINARIAS { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }

        [ForeignKey("PRELIMINARESID")]
        public preliminares? PRELIMINARES { get; set; }

        [ForeignKey("TIPO_INGESTAID")]
        public tipo_ingesta? TIPO_INGESTA { get; set; }

        [ForeignKey("TIPO_INCIDENTEID")]
        public tipo_incidente? TIPO_INCIDENTE { get; set; }

        [ForeignKey("TIPO_IDENTIFICACIONID")]
        public tipo_identificacion? TIPO_IDENTIFICACION { get; set; }

        [ForeignKey("TIPO_ARMAMENTOID")]
        public tipo_armamento? TIPO_ARMAMENTO { get; set; }

        [ForeignKey("TIPO_INVESTIGACIONID")]
        public tipo_investigacion? TIPO_INVESTIGACION { get; set; }
        
        [ForeignKey("RUT_INFORMANTE")]
        public persona? INFORMANTE { get; set; }

        [ForeignKey("UNIDAD_INFORMANTE")]
        public unidad? U_INFORMANTE { get; set; }

        [ForeignKey("UNIDAD_AFECTADA")]
        public unidad? U_AFECTADA { get; set; }

    }
}
