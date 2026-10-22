using Sci.Entidades;

namespace Sci.Web.Models
{
    public class PutIncidentesModel
    {
        public int RUT_INFORMANTE { get; set; }
        public string? UNIDAD_INFORMANTE { get; set; }
        public string? INCIDENTE_ACCIDENTE { get; set; }
        public int? TIPO_INGESTAID { get; set; }
        public DateTime FECHA_INFORMANTE { get; set; }
        public int? TIPO_INCIDENTEID { get; set; }
        public string? PERSONAL_UNIDAD { get; set; }
        public string? UNIDAD_AFECTADA { get; set; }
        public DateTime FECHA_HECHO { get; set; }
        public string? LUGAR_HECHO { get; set; }
        public string? CIRCUNSTANCIA_HECHO { get; set; }
        public string? DANOS_PERSONALES { get; set; }
        public string? MEDIDAS_ADOPTADAS { get; set; }
        public string? ESPECIFICACION_VESTUARIO { get; set; }
        public int? TIPO_IDENTIFICACIONID { get; set; }
        public string? PORTABA_ARMA { get; set; }
        public int? TIPO_ARMAMENTOID { get; set; }
        public string? ARMA_FISCAL_PARTICULAR { get; set; }
        public string? DETALLE_PORTE_ARMA { get; set; }
        public string? REALIZA_INVESTIGACION { get; set; }
        public int? TIPO_INVESTIGACIONID { get; set; }
        public string? DETALLE_INVESTIGACION { get; set; }
        public string? UNIDAD_INTELIGENCIA_ADOPTA { get; set; }
        public string? CARACTERISTICAS_MATERIAL { get; set; }
        public string? DETALLE_RESULTADO_MUERTE { get; set; }
        public string? DETALLE_DOCUMENTACION_PERDIDA { get; set; }
        public string? AUTORIDAD_REQUIERE_INFO { get; set; }
        public string? MEDIDAS_ADOPTADAS_FAMILIARES { get; set; }
        public string? SINTESIS_MEDIOS_PRENSA { get; set; }
        public string? MEDIDAS_DISCIPLINARIAS { get; set; }
    }

    public class PutMedidasDisciplinariasIncidentesModel
    {
        public string? MEDIDAS_DISCIPLINARIAS { get; set; }
    }


    public class IncidentesFinalesViewModel
    {
        public string? NOMBRE { get; set; }
        public int ID { get; set; }
        public string? COLOR { get; set; }
        public int CANTIDAD { get; set; }
    }
}