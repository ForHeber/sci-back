using Sci.Entidades;

namespace Sci.Web.Models
{

public class TotalPorGrupoModel 
{
    public string? DESCRIPCION_GRUPO { get; set; }
    public string? COLOR_GRUPO { get; set; }
    public TotalPorMesModel[]? TOTAL_GRUPO { get; set; }
}
    public class TotalPorMesModel
    {
        public string? DESCRIPCION_MES { get; set; }
        public int? CANTIDAD_MES { get; set; }
    }

}