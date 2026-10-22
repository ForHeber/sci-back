using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class personas_civiles
    {
        [Key]
        public int RUN { get; set; }
        public string? GRADO { get; set; }
        public string? NOMBRES { get; set; }
        public string? APELLIDO_PATERNO { get; set; }
        public string? APELLIDO_MATERNO { get; set; }
        public string? FONO { get; set; }
        public string? CORREOINTER { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }
    }
}
