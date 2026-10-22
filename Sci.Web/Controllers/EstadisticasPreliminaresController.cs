using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sci.Datos;
using Sci.Entidades;
using Sci.Web.Helpers;
using Sci.Web.Models;

namespace Sci.Web.Controllers
{
    [Authorize(Roles = "541, 542, 543, 545")]
    [Route("api/[controller]")]
    [ApiController]
    public class EstadisticasPreliminaresController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public EstadisticasPreliminaresController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Estadistica de Informes Preliminares Publicados y No Publicados en el Turno del PME.
        // Estado 1 = Publicado y Estado 2 = No Publicado.

        [HttpGet("[action]")]
        public async Task<ActionResult> GetEstadisticasPreliminaresPublicadosNoPublicados() {
            try
            {

                // Creación de lista de Informes Preliminares ordenada por FECHA_HECHO.

                List<preliminares> informesPreliminares = await _sciContext.Preliminares
                .AsNoTracking()
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Include(i => i.INFORMANTE)
                .Where(i => i.ESTADO == 1 || i.ESTADO == 2)
                .OrderByDescending(i => i.FECHA_HECHO)
                .ToListAsync();

                var series  = Array.Empty<int>();
                var label   = Array.Empty<string>();
                var listado = Array.Empty<List<preliminares>>();

                // Creación de series para Gráfico

                series = new int[] {
                    informesPreliminares.Count( i => i.ESTADO == 1), // Contador de Preliminares Publicados
                    informesPreliminares.Count( i => i.ESTADO == 2), // Contador de Preliminares No Publicados
                };

                // Creación de label para Gráfico

                label = new string[] {
                    "Publicados",
                    "No Publicados",
                };

                // Creación de listas de Informes Preliminares para Gráfico

                listado = new List<preliminares>[] {
                    informesPreliminares.Where( i => i.ESTADO == 1).ToList(), // Listado de Preliminares Publicados
                    informesPreliminares.Where( i => i.ESTADO == 2).ToList(), // Listado de Preliminares No Publicados
                };

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes Preliminares del Turno", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok( new { series, label, listado } );
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject(""), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Estadistica de Informes Preliminares por el Tipo de Incidente en el Turno del PME por año.
        // (agno=0)-Total General (agno=año especifico).
        // Estado 1 = Publicado, Estado 2 = No Publicado y Estado 3 = No Visualizable

        [HttpGet("[action]/{agno}")]
        public async Task<ActionResult> GetEstadisticasPreliminaresPorTipoIncidente([FromRoute] int agno) {
            try
            {

                // Creación de lista de Informes Preliminares ordenada por DESCRIPCION de INCIDENTES.

                List<preliminares> informesPreliminares = await _sciContext.Preliminares
                .AsNoTracking()
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Include(i => i.INFORMANTE)
                .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && (agno == 0 ? true : i.FECHA_HECHO.Year == agno))
                .OrderBy(i => i.TIPO_INCIDENTE != null ? i.TIPO_INCIDENTE.DESCRIPCION : string.Empty)
                .ToListAsync();

                var series      =   Array.Empty<int>();
                var label       =   Array.Empty<string>();
                var colorBarra  =   Array.Empty<string>();
                var listado     =   Array.Empty<List<preliminares>>();

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.
                var grupos = informesPreliminares
                .GroupBy(r => new {
                    DESCRIPCION = r.TIPO_INCIDENTE != null ? r.TIPO_INCIDENTE.DESCRIPCION : string.Empty,
                    ID = r.TIPO_INCIDENTE != null ? r.TIPO_INCIDENTE.ID : 0,
                    COLOR = r.TIPO_INCIDENTE != null ? r.TIPO_INCIDENTE.COLOR : string.Empty
                })
                .OrderBy(g => g.Key.DESCRIPCION)
                .ToList();

                label = grupos.Select(g => g.Key.DESCRIPCION ?? string.Empty).ToArray();
                colorBarra = grupos.Select(g => g.Key.COLOR ?? string.Empty).ToArray();
                series = grupos.Select(g => g.Count()).ToArray();
                listado = grupos.Select(g => g.ToList()).ToArray();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes Preliminares por Tipo de Incidentes General o por Año ", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok( new { label, colorBarra, series, listado } );
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject(""), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }
        
        // Entrega cantidades de Informes Preliminares según criterios específicos.

        [HttpGet("[action]")]
        public async Task<ActionResult> GetEstadisticasPreliminaresPorEstados() {
            try
            {

                var agnoActual = DateTime.Now.Year;

                // Extrae y agrupa los datos de la base de datos para obtener los totales de preliminares por estado y año.
                var resumen = await _sciContext.Preliminares
                    .AsNoTracking()
                    .GroupBy(g1 => 1)
                    .Select(g => new
                    {
                        TotalPreliminaresTurno = g.Count(i => (i.ESTADO == 1 || i.ESTADO == 2) && i.FECHA_HECHO.Year == agnoActual),
                        TotalPreliminaresPublicados = g.Count(i => i.ESTADO == 1 && i.FECHA_HECHO.Year == agnoActual),
                        TotalPreliminaresNoPublicados = g.Count(i => i.ESTADO == 2 && i.FECHA_HECHO.Year == agnoActual),
                        TotalPreliminaresGeneral = g.Count(i => i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3),
                        TotalPreliminaresAgno = g.Count(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && i.FECHA_HECHO.Year == agnoActual)
                    })
                    .FirstOrDefaultAsync();

                // Validación de nulos y asignación de valores predeterminados en caso de que no haya resultados.
                var TotalPreliminaresTurno = resumen?.TotalPreliminaresTurno ?? 0;
                var TotalPreliminaresPublicados = resumen?.TotalPreliminaresPublicados ?? 0;
                var TotalPreliminaresNoPublicados = resumen?.TotalPreliminaresNoPublicados ?? 0;
                var TotalPreliminaresGeneral = resumen?.TotalPreliminaresGeneral ?? 0;
                var TotalPreliminaresAgno = resumen?.TotalPreliminaresAgno ?? 0;

                // Extrae los últimos 10 informes preliminares, incluyendo las relaciones necesarias.
                var Ultimos10Preliminares = await _sciContext.Preliminares
                    .AsNoTracking()
                    .Include(i => i.TIPO_INCIDENTE)
                    .Include(i => i.U_INFORMANTE)
                    .Include(i => i.U_AFECTADA)
                    .Include(i => i.INFORMANTE)
                    .Where(i => (i.ESTADO == 1 || i.ESTADO == 2) && i.FECHA_HECHO.Year == agnoActual) // Filtra solo los preliminares publicados y no publicados del Turno.
                    .OrderByDescending(i => i.ID)
                    .Take(10)
                    .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Cantidad Preliminares Por Estado", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new
                {
                    TotalPreliminaresTurno,
                    TotalPreliminaresPublicados,
                    TotalPreliminaresNoPublicados,
                    TotalPreliminaresGeneral,
                    TotalPreliminaresAgno,
                    Ultimos10Preliminares
                });
                            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject(""), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Entrega cantidades de Informes Preliminares según criterios específicos.

        [HttpGet("[action]/{unidadId}")]
        public async Task<ActionResult> GetEstadisticasPreliminaresPorEstadosBINTE(
            [FromRoute] string unidadId
            ) 
        {
            
            try
            {

                var agnoActual = DateTime.Now.Year;

                // Extrae y agrupa los datos de la base de datos para obtener los totales de preliminares por estado y año.
                var resumen = await _sciContext.Preliminares
                    .AsNoTracking()
                    .GroupBy(g1 => 1)
                    .Select(g => new
                    {
                        TotalPreliminaresPublicados = g.Count(i => (i.ESTADO == 1 || i.ESTADO == 3) && i.FECHA_HECHO.Year == agnoActual && (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId)),
                        TotalPreliminaresNoPublicados = g.Count(i => i.ESTADO == 2 && i.FECHA_HECHO.Year == agnoActual && (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId)),
                        TotalPreliminaresGeneral = g.Count(i => (i.ESTADO == 1 || i.ESTADO == 3) && (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId)),
                        TotalPreliminaresAgno = g.Count(i => (i.ESTADO == 1 || i.ESTADO == 3) && i.FECHA_HECHO.Year == agnoActual && (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId)),
                    })
                    .FirstOrDefaultAsync();

                // Validación de nulos y asignación de valores predeterminados en caso de que no haya resultados.
                var TotalPreliminaresPublicados = resumen?.TotalPreliminaresPublicados ?? 0;
                var TotalPreliminaresNoPublicados = resumen?.TotalPreliminaresNoPublicados ?? 0;
                var TotalPreliminaresGeneral = resumen?.TotalPreliminaresGeneral ?? 0;
                var TotalPreliminaresAgno = resumen?.TotalPreliminaresAgno ?? 0;

                // Extrae los últimos 10 informes preliminares, incluyendo las relaciones necesarias.
                var Ultimos10Preliminares = await _sciContext.Preliminares
                    .AsNoTracking()
                    .Include(i => i.TIPO_INCIDENTE)
                    .Include(i => i.U_INFORMANTE)
                    .Include(i => i.U_AFECTADA)
                    .Include(i => i.INFORMANTE)
                    .Where(i => (i.ESTADO == 1 || i.ESTADO == 3) && i.FECHA_HECHO.Year == agnoActual && (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId)) // Filtra solo los preliminares publicados y no publicados del Turno.
                    .OrderByDescending(i => i.ID)
                    .Take(10)
                    .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Cantidad Preliminares Por Estado", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new
                {
                    TotalPreliminaresPublicados,
                    TotalPreliminaresNoPublicados,
                    TotalPreliminaresGeneral,
                    TotalPreliminaresAgno,
                    Ultimos10Preliminares
                });
                            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject(""), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todos años disponibles para Informes Preliminares Estado = 1 (Publicados), Estado = 2 (No Publicados), Estado = 3 (Con Informe Final).

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListAgnosInformesPreliminares() 
        {
            try
            {
                var respuesta = await _sciContext.Preliminares
                    .AsNoTracking()
                    .Where(i => i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3)
                    .Select(i => i.FECHA_HECHO.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Rescate de Años de Informes Preliminares", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {  respuesta } );
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject(""), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

    }
}
// 06-10-2026 16:50