using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class tipo_incidente
    {
        [Key]
        public int ID { get; set; }
        public string? DESCRIPCION { get; set; }
        public string? COLOR { get; set; }
        public int? TIPO_GRUPO_INCIDENTEID { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }

        [ForeignKey("TIPO_GRUPO_INCIDENTEID")]
        public tipo_grupo_incidente? TIPO_GRUPO_INCIDENTE { get; set; }

        
    }
}
