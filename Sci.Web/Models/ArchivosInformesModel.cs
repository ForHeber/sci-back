using Sci.Entidades;

namespace Sci.Web.Models
{
    public class PostArchivosInformesPreModel
    {
        public int PRELIMINARESID { get; set; }
        public DateTime FECHA_ARCHIVO { get; set; }
        public string? DESCRIPCION_ARCHIVO { get; set; }
        public string? NOMBRE_ARCHIVO { get; set; }
        public string? URL_ARCHIVO { get; set; }
    }

    public class PutArchivosInformesPreModel
    {
        public DateTime FECHA_ARCHIVO { get; set; }
        public string? DESCRIPCION_ARCHIVO { get; set; }
        public string? NOMBRE_ARCHIVO { get; set; }
        public string? URL_ARCHIVO { get; set; }
    }

    public class PostArchivosInformesIncModel
    {
        public int INCIDENTESID { get; set; }
        public DateTime FECHA_ARCHIVO { get; set; }
        public string? DESCRIPCION_ARCHIVO { get; set; }
        public string? NOMBRE_ARCHIVO { get; set; }
        public string? URL_ARCHIVO { get; set; }
    }

    public class PutArchivosInformesIncModel
    {
        public DateTime FECHA_ARCHIVO { get; set; }
        public string? DESCRIPCION_ARCHIVO { get; set; }
        public string? NOMBRE_ARCHIVO { get; set; }
        public string? URL_ARCHIVO { get; set; }
    }

    public class PostArchivosInformesFinModel
    {
        public int FINALESID { get; set; }
        public DateTime FECHA_ARCHIVO { get; set; }
        public string? DESCRIPCION_ARCHIVO { get; set; }
        public string? NOMBRE_ARCHIVO { get; set; }
        public string? URL_ARCHIVO { get; set; }
    }

    public class PutArchivosInformesFinModel
    {
        public DateTime FECHA_ARCHIVO { get; set; }
        public string? DESCRIPCION_ARCHIVO { get; set; }
        public string? NOMBRE_ARCHIVO { get; set; }
        public string? URL_ARCHIVO { get; set; }
    }


}