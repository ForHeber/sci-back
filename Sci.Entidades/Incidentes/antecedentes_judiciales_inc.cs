using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class antecedentes_judiciales_inc
    {
        [Key]
        public int ID { get; set; }
        public int? INCIDENTESID { get; set; }
        public DateTime FECHA_ANTECEDENTES_JUD { get; set; }
        public string? DETALLE_ANTECEDENTES_JUD { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }

        [ForeignKey("INCIDENTESID")]
        public incidentes? INCIDENTES { get; set; }
    }
}
