using Sci.Entidades;

namespace Sci.Web.Models
{
    public class PostProcedimientosPolicialesPreModel
    {
        public int? PRELIMINARESID { get; set; }
        public DateTime FECHA_PROCEDIMIENTO_POL { get; set; }
        public string? DETALLE_PROCEDIMIENTO_POL { get; set; }
        public int CONDICION_POLICIALID { get; set; }

    }

    public class PutProcedimientosPolicialesPreModel
    {
        public DateTime FECHA_PROCEDIMIENTO_POL { get; set; }
        public string? DETALLE_PROCEDIMIENTO_POL { get; set; }
        public int CONDICION_POLICIALID { get; set; }

    }

    public class PostProcedimientosPolicialesIncModel
    {
        public int? INCIDENTESID { get; set; }
        public DateTime FECHA_PROCEDIMIENTO_POL { get; set; }
        public string? DETALLE_PROCEDIMIENTO_POL { get; set; }
        public int CONDICION_POLICIALID { get; set; }

    }

    public class PutProcedimientosPolicialesIncModel
    {
        public DateTime FECHA_PROCEDIMIENTO_POL { get; set; }
        public string? DETALLE_PROCEDIMIENTO_POL { get; set; }
        public int CONDICION_POLICIALID { get; set; }

    }

    public class PostProcedimientosPolicialesFinModel
    {
        public int? FINALESID { get; set; }
        public DateTime FECHA_PROCEDIMIENTO_POL { get; set; }
        public string? DETALLE_PROCEDIMIENTO_POL { get; set; }
        public int CONDICION_POLICIALID { get; set; }

    }

    public class PutProcedimientosPolicialesFinModel
    {
        public DateTime FECHA_PROCEDIMIENTO_POL { get; set; }
        public string? DETALLE_PROCEDIMIENTO_POL { get; set; }
        public int CONDICION_POLICIALID { get; set; }

    }


}