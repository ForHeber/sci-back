using Sci.Entidades;

namespace Sci.Web.Models
{
    public class PostMantencionTipoIncidenteModel
    {
        public string? DESCRIPCION { get; set; }
        public string? COLOR { get; set; }
        public int? TIPO_GRUPO_INCIDENTEID { get; set; }

        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }
    }

    public class PutMantencionTipoIncidenteModel
    {
        public string? DESCRIPCION { get; set; }
        public string? COLOR { get; set; }
        public int? TIPO_GRUPO_INCIDENTEID { get; set; }
    }


}