using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    public class registro_eventos
    {
        [Key]
        public int ID { get; set; }
        public string? TIPO_INFORME { get; set; }
        public int? TIPO_INFORME_ID { get; set; }
        public string? TIPO_ANTECEDENTE { get; set; }
        public string? TIPO_EVENTO { get; set; }
        public int MODIFICADO_POR { get; set; }
        public DateTime MODIFICADO_FECHA { get; set; }
        public string? CONCEPTO { get; set; }
        public string? VALOR_ANTERIOR { get; set; }
        public string? VALOR_NUEVO { get; set; }
        // Datos de Control
        public int? PROPIETARIO { get; set; }
        public DateTime? CREACION { get; set; }
        public int? USUARIO { get; set; }
        public DateTime? ACTUALIZACION { get; set; }
        public int? ESTADO { get; set; }
        
    }
}
