using Sci.Entidades;

namespace Sci.Web.Models
{
    public class PostMantencionCondicionPolicialModel
    {
        public string? DESCRIPCION { get; set; }

        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }
    }

    public class PutMantencionCondicionPolicialModel
    {
        public string? DESCRIPCION { get; set; }
    }


}