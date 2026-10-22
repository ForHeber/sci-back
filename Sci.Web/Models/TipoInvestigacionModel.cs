using Sci.Entidades;

namespace Sci.Web.Models
{
    public class PostMantencionTipoInvestigacionModel
    {
        public string? DESCRIPCION { get; set; }
        public string? SIGLA { get; set; }

        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }
    }

    public class PutMantencionTipoInvestigacionModel
    {
        public string? DESCRIPCION { get; set; }
        public string? SIGLA { get; set; }
    }


}