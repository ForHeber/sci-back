using Sci.Entidades;

namespace Sci.Web.Models
{
    public class PostAmpliacionesInformesIncModel
    {
        public int? INCIDENTESID { get; set; }
        public DateTime FECHA_AMPLIACION { get; set; }
        public string? DETALLE_AMPLIACION { get; set; }
    }

    public class PutAmpliacionesInformesIncModel
    {
        public DateTime FECHA_AMPLIACION { get; set; }
        public string? DETALLE_AMPLIACION { get; set; }
    }

    public class PostAmpliacionesInformesFinModel
    {
        public int? FINALESID { get; set; }
        public DateTime FECHA_AMPLIACION { get; set; }
        public string? DETALLE_AMPLIACION { get; set; }
    }

    public class PutAmpliacionesInformesFinModel
    {
        public DateTime FECHA_AMPLIACION { get; set; }
        public string? DETALLE_AMPLIACION { get; set; }
    }

}
