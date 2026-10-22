using Sci.Entidades;

namespace Sci.Web.Models
{
    public class PostAntecedentesJudicialesPreModel
    {
        public int? PRELIMINARESID { get; set; }
        public DateTime FECHA_ANTECEDENTES_JUD { get; set; }
        public string? DETALLE_ANTECEDENTES_JUD { get; set; }
    }

    public class PutAntecedentesJudicialesPreModel
    {
        public DateTime FECHA_ANTECEDENTES_JUD { get; set; }
        public string? DETALLE_ANTECEDENTES_JUD { get; set; }
    }

    public class PostAntecedentesJudicialesIncModel
    {
        public int? INCIDENTESID { get; set; }
        public DateTime FECHA_ANTECEDENTES_JUD { get; set; }
        public string? DETALLE_ANTECEDENTES_JUD { get; set; }
    }

    public class PutAntecedentesJudicialesIncModel
    {
        public DateTime FECHA_ANTECEDENTES_JUD { get; set; }
        public string? DETALLE_ANTECEDENTES_JUD { get; set; }
    }

    public class PostAntecedentesJudicialesFinModel
    {
        public int? FINALESID { get; set; }
        public DateTime FECHA_ANTECEDENTES_JUD { get; set; }
        public string? DETALLE_ANTECEDENTES_JUD { get; set; }
    }

    public class PutAntecedentesJudicialesFinModel
    {
        public DateTime FECHA_ANTECEDENTES_JUD { get; set; }
        public string? DETALLE_ANTECEDENTES_JUD { get; set; }
    }

}
