using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class persona_sci
    {
        [Key]
        public int RUN { get; set; }
        public string? GRADO { get; set; }
        public string? NOMBRES { get; set; }
        public string? APELLIDO_PATERNO { get; set; }
        public string? APELLIDO_MATERNO { get; set; }
        public string? FONO { get; set; }
        public string? CORREOINTER { get; set; }
    }
}
