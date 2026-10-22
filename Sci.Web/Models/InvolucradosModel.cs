using Sci.Entidades;

namespace Sci.Web.Models
{
    public class PostInvolucradosPreModel
    {
        public int? PRELIMINARESID { get; set; }
        public string? TIPO_INVOLUCRADO { get; set; }
        public int RUT_INVOLUCRADO { get; set; }
        public string? UNIDAD_INVOLUCRADO { get; set; }
        public DateTime? FECHA_NACIMIENTO { get; set; }
        public int? TIPO_INTERVINIENTEID { get; set; }
        public int? TIPO_CONSUMOID { get; set; }
        public int? ESTADO_SALUDID { get; set; }

        public string? GRADO { get; set; }
        public string? NOMBRES { get; set; }
        public string? APELLIDO_PATERNO { get; set; }
        public string? APELLIDO_MATERNO { get; set; }
        public string? FONO { get; set; }
        public string? CORREOINTER { get; set; }
    }

    public class PostInvolucradosIncModel
    {
        public int? INCIDENTESID { get; set; }
        public string? TIPO_INVOLUCRADO { get; set; }
        public int RUT_INVOLUCRADO { get; set; }
        public string? UNIDAD_INVOLUCRADO { get; set; }
        public DateTime? FECHA_NACIMIENTO { get; set; }
        public int? TIPO_INTERVINIENTEID { get; set; }
        public int? TIPO_CONSUMOID { get; set; }
        public int? ESTADO_SALUDID { get; set; }

        public string? GRADO { get; set; }
        public string? NOMBRES { get; set; }
        public string? APELLIDO_PATERNO { get; set; }
        public string? APELLIDO_MATERNO { get; set; }
        public string? FONO { get; set; }
        public string? CORREOINTER { get; set; }
    }

    public class PostInvolucradosFinModel
    {
        public int? FINALESID { get; set; }
        public string? TIPO_INVOLUCRADO { get; set; }
        public int RUT_INVOLUCRADO { get; set; }
        public string? UNIDAD_INVOLUCRADO { get; set; }
        public DateTime? FECHA_NACIMIENTO { get; set; }
        public int? TIPO_INTERVINIENTEID { get; set; }
        public int? TIPO_CONSUMOID { get; set; }
        public int? ESTADO_SALUDID { get; set; }

        public string? GRADO { get; set; }
        public string? NOMBRES { get; set; }
        public string? APELLIDO_PATERNO { get; set; }
        public string? APELLIDO_MATERNO { get; set; }
        public string? FONO { get; set; }
        public string? CORREOINTER { get; set; }
    }


}