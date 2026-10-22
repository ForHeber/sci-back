using Sci.Entidades;

namespace Sci.Web.Models
{
    public class PostPreliminaresModel
    {
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

        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }
    }

    public class PutPreliminaresModel
    {
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
    }
}