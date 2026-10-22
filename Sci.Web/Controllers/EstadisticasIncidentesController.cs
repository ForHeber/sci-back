using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using Sci.Datos;
using Sci.Entidades;
using Sci.Web.Helpers;
using Sci.Web.Models;

namespace Sci.Web.Controllers
{
    [Authorize(Roles = "541, 542, 543, 545")]
    [Route("api/[controller]")]
    [ApiController]
    public class EstadisticasIncidentesController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public EstadisticasIncidentesController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Estadistica de Informes de Incidentes Por Estados.
        // unidadId = 0 (Cero) = Incidentes Institucionales de lo contrario Incidentes de la unidad especificada.

        [HttpGet("[action]/{agno},{unidadId}")]
        public async Task<ActionResult> GetListEstadisticasIncidentesPorEstados(
            [FromRoute] int agno, 
            [FromRoute] string unidadId
            ) 
        {
            try
            {

                // Creación de lista de Informes de Incidentes ordenada por FECHA_HECHO.

                var informesIncidentes = await _sciContext.Incidentes
                .AsNoTracking()
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.TIPO_IDENTIFICACION)
                .Include(i => i.TIPO_ARMAMENTO)
                .Include(i => i.TIPO_INVESTIGACION)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => ( i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3 ) && 
                            (agno == 0 ? true : i.FECHA_HECHO.Year == agno) && 
                            (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.FECHA_HECHO)
                .ToListAsync();

                var series  = Array.Empty<int>();
                var label   = Array.Empty<string>();
                List<object>[] listado = Array.Empty<List<object>>();

                // Creación de series para Gráfico

                series = new int[] {
                    informesIncidentes.Count( i => i.ESTADO == 1), // Contador de Incidentes Tramitados a CDTE
                    informesIncidentes.Count( i => i.ESTADO == 2 && i.DEVUELTO == "N"), // Contador de Incidentes En Proceso por S2
                    informesIncidentes.Count( i => i.ESTADO == 2 && i.DEVUELTO == "S"), // Contador de Incidentes Devuelto por CDTE.
                    informesIncidentes.Count( i => i.ESTADO == 3), // Contador de Incidentes con Informe Final
                };

                // Creación de label para Gráfico

                label = new string[] {
                    "Tramitados a CDTE",
                    "En Proceso por S2",
                    "Devueltos por CDTE",
                    "Con Informe Final"
                };

                // Creación de listas de Informes de Incidentes para Gráfico

                listado = new List<object>[] {
                    informesIncidentes.Where( i => i.ESTADO == 1 ).Cast<object>().ToList(), // Listado de Incidentes Tramitados a CDTE
                    informesIncidentes.Where( i => i.ESTADO == 2 && i.DEVUELTO == "N" ).Cast<object>().ToList(), // Listado de Incidentes En Proceso por S2
                    informesIncidentes.Where( i => i.ESTADO == 2 && i.DEVUELTO == "S" ).Cast<object>().ToList(), // Listado de Incidentes Devueltos por CDTE
                    informesIncidentes.Where( i => i.ESTADO == 3 ).Cast<object>().ToList(), // Listado de Incidentes con Informe Final
                };

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes de Incidentes del S2", JsonConvert.SerializeObject(""), _usuariosContext
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
            }
        }

        // Estadistica de Informes de Incidentes por el Tipo de Incidente en el S2 por año.
        // (agno=0)-Total General (agno=año especifico).
        // unidadId = 0 (Cero) = Incidentes Institucionales de lo contrario Incidentes de la unidad especificada.
        // Estado 1 = Tramitado al CDTE, Estado 2 = En Proceso por S2, Estado 3 = Con Informe Final.

        [HttpGet("[action]/{agno},{unidadId}")]
        public async Task<ActionResult> GetEstadisticasIncidentesPorTipoIncidente(
            [FromRoute] int agno, 
            [FromRoute] string unidadId
            ) 
        {
            try
            {

                // Creación de lista de Informes de Incidentes ordenada por DESCRIPCION de INCIDENTES.

                List<incidentes> informesIncidentes = await _sciContext.Incidentes
                .AsNoTracking()
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Include(i => i.INFORMANTE)
                .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && 
                            (agno == 0 ? true : i.FECHA_HECHO.Year == agno) && 
                            (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.TIPO_INCIDENTE != null ? i.TIPO_INCIDENTE.DESCRIPCION : string.Empty)
                .ToListAsync();

                var series      =   Array.Empty<int>();
                var label       =   Array.Empty<string>();
                var colorBarra  =   Array.Empty<string>();
                var listado     =   Array.Empty<List<incidentes>>();

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.
                var grupos = informesIncidentes
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
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes de Incidentes por Tipo de Incidentes General o por Año ", JsonConvert.SerializeObject(""), _usuariosContext
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
            }
        }
        
        // Entrega cantidades de Informes de Incidentes según criterios específicos.
        // unidadId = 0 (Cero) = Incidentes Institucionales de lo contrario Incidentes de la unidad especificada.

        [HttpGet("[action]/{unidadId}")]
        public async Task<ActionResult> GetEstadisticasIncidentesPorEstados(
            [FromRoute] string unidadId
            ) 
        {
            try
            {

                var agnoActual = DateTime.Now.Year;

                // Extrae y agrupa los datos de la base de datos para obtener los totales de incidentes por estado y año.

                var resumen = await _sciContext.Incidentes
                .AsNoTracking()
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.TIPO_IDENTIFICACION)
                .Include(i => i.TIPO_ARMAMENTO)
                .Include(i => i.TIPO_INVESTIGACION)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => ( i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3 ) && (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.FECHA_HECHO)
                .ToListAsync();

                // Extrae los últimos 10 informes de incidentes, incluyendo las relaciones necesarias.
                var Ultimos10Incidentes = resumen
                    .Where(i => i.ESTADO != 3 && i.FECHA_HECHO.Year == agnoActual) // Excluye los incidentes que ya tienen informe final
                    .OrderByDescending(i => i.ID)
                    .Take(10)
                    .ToList();

                var resumenTotales = resumen
                .GroupBy(g1 => 1)
                .Select(g => new
                {
                    TotalIncidentesTramitadosAlCDTE = g.Count( i => i.ESTADO == 1 && i.FECHA_HECHO.Year == agnoActual),
                    TotalIncidentesEnProcesoS2 = g.Count(i => i.ESTADO == 2 && i.DEVUELTO == "N" && i.FECHA_HECHO.Year == agnoActual),
                    TotalIncidentesDevueltosPorCDTE = g.Count(i => i.ESTADO == 2 && i.DEVUELTO == "S" && i.FECHA_HECHO.Year == agnoActual),
                    TotalIncidentesConInformeFinal = g.Count( i => i.ESTADO == 3 && i.FECHA_HECHO.Year == agnoActual),
                    TotalIncidentesGeneral = g.Count( i => i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3),
                    TotalIncidentesAgno = g.Count(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && i.FECHA_HECHO.Year == agnoActual)
                })
                .FirstOrDefault();

                // Validación de nulos y asignación de valores predeterminados en caso de que no haya resultados.
                var TotalIncidentesTramitadosAlCDTE = resumenTotales?.TotalIncidentesTramitadosAlCDTE ?? 0;
                var TotalIncidentesEnProcesoS2 = resumenTotales?.TotalIncidentesEnProcesoS2 ?? 0;
                var TotalIncidentesDevueltosPorCDTE = resumenTotales?.TotalIncidentesDevueltosPorCDTE ?? 0;
                var TotalIncidentesConInformeFinal = resumenTotales?.TotalIncidentesConInformeFinal ?? 0;
                var TotalIncidentesGeneral = resumenTotales?.TotalIncidentesGeneral ?? 0;
                var TotalIncidentesAgno = resumenTotales?.TotalIncidentesAgno ?? 0;

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Cantidad Incidentes Por Estado", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new
                {
                    TotalIncidentesTramitadosAlCDTE,
                    TotalIncidentesEnProcesoS2,
                    TotalIncidentesDevueltosPorCDTE,
                    TotalIncidentesConInformeFinal,
                    TotalIncidentesGeneral,
                    TotalIncidentesAgno,
                    Ultimos10Incidentes
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
            }
        }
    
        // Estadistica de Tipos de Incidentes por Fecha Institucionales o por Unidad
        // Si el parametro entregado en unidad es 0 (Cero) = Incidentes Institucionales por Fecha
        // Si se especifica una unidad = Incidentes de la unidad especificada por Fecha

        [HttpGet("[action]/{unidadId},{agnoDeseado}")]
        public async Task<ActionResult> GetEstadisticasIncidentesPorFecha(
            [FromRoute] string unidadId, 
            [FromRoute] int agnoDeseado
            )
        {
            try
            {

                if (string.IsNullOrEmpty(unidadId))
                {
                    Auditor.registroAuditor(
                        "Get", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "Falta definir Unidad a consultar", JsonConvert.SerializeObject(""), _usuariosContext
                    );
                    return NotFound();
                }

                // Agrupa por fecha (sin hora) y total de incidentes.

                var unidadIdNormalizado = unidadId.Trim();

                List<incidentes> informesIncidentes = await _sciContext.Incidentes
                    .AsNoTracking()
                    .Include(i => i.TIPO_INCIDENTE)
                    .Include(i => i.U_INFORMANTE)
                    .Include(i => i.U_AFECTADA)
                    .Include(i => i.INFORMANTE)
                    .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) 
                                && (unidadIdNormalizado == "0" ? true : (i.UNIDAD_AFECTADA ?? string.Empty).Trim() == unidadIdNormalizado) 
                                && (agnoDeseado == 0 ? true : i.FECHA_HECHO.Year == agnoDeseado))
                    .OrderBy(i => i.FECHA_HECHO.Date)
                    .ToListAsync();

                var series      =   Array.Empty<int>();
                var label       =   Array.Empty<string>();
                var listado     =   Array.Empty<List<incidentes>>();

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.

                var grupos = informesIncidentes
                .GroupBy(r => new {
                    FECHA_HECHO = r.FECHA_HECHO.Date,
                })
                .OrderBy(g => g.Key.FECHA_HECHO.Date)
                .ToList();

                label = grupos.Select(g => g.Key.FECHA_HECHO.ToString("dd-MM-yyyy")).ToArray();
                series = grupos.Select(g => g.Count()).ToArray();
                listado = grupos.Select(g => g.ToList()).ToArray();


                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística Incidentes por Fecha (Institucionales o por Unidad) y (General o del Año)", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok( new { label, series, listado } ); 
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

        // Estadistica de Informes de Incidentes por Grupos de Incidente en el S2 por año.
        // (agno=0)-Total General (agno=año) Especifico.
        // unidadId = 0 (Cero) = Grupo de Incidentes Institucionales de lo contrario Grupo de Incidentes de la unidad especificada.
        // Estado 1 = Tramitado al CDTE, Estado 2 = En Proceso por S2, Estado 3 = Con Informe Final.

        [HttpGet("[action]/{agno},{unidadId}")]
        public async Task<ActionResult> GetEstadisticasIncidentesPorGrupoDeTipoIncidente(
            [FromRoute] int agno, 
            [FromRoute] string unidadId
            ) 
        {
            try
            {

                // Creación de lista de Informes de Grupos de Incidentes ordenada por DESCRIPCION de GRUPO DE INCIDENTES.

                List<incidentes> informesIncidentes = await _sciContext.Incidentes
                .AsNoTracking()
                .Include(i => i.TIPO_INCIDENTE)
                    .ThenInclude(i => i!.TIPO_GRUPO_INCIDENTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Include(i => i.INFORMANTE)
                .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && 
                            (agno == 0 ? true : i.FECHA_HECHO.Year == agno) && 
                            (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.TIPO_INCIDENTE != null && i.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTE != null ? i.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTE.DESCRIPCION : string.Empty)
                .ToListAsync();

                var series      =   Array.Empty<int>();
                var label       =   Array.Empty<string>();
                var colorBarra  =   Array.Empty<string>();
                var listado     =   Array.Empty<List<incidentes>>();

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.
                
                var grupos = informesIncidentes
                .GroupBy(r => new {
                    DESCRIPCION = r.TIPO_INCIDENTE != null && r.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTE != null ? r.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTE.DESCRIPCION : string.Empty,
                    ID = r.TIPO_INCIDENTE != null && r.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTE != null ? r.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTE.ID : 0,
                    COLOR = r.TIPO_INCIDENTE != null && r.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTE != null ? r.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTE.COLOR : string.Empty
                })
                .OrderBy(g => g.Key.DESCRIPCION)
                .ToList();

                label = grupos.Select(g => g.Key.DESCRIPCION ?? string.Empty).ToArray();
                colorBarra = grupos.Select(g => g.Key.COLOR ?? string.Empty).ToArray();
                series = grupos.Select(g => g.Count()).ToArray();
                listado = grupos.Select(g => g.ToList()).ToArray();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes de Incidentes por Grupo de Tipo de Incidentes General o por Año ", JsonConvert.SerializeObject(""), _usuariosContext
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
            }
        }

        // Estadistica de Informes de Incidentes por Grupos de Incidente Mensual en el S2 por año.
        // (agno=0)-Total General (agno=año) Especifico.
        // unidadId = 0 (Cero) = Grupo de Incidentes Institucionales por Mes de lo contrario Grupo de Incidentes por Mes de la unidad especificada.
        // Estado 1 = Tramitado al CDTE, Estado 2 = En Proceso por S2, Estado 3 = Con Informe Final.

        [HttpGet("[action]/{agno},{unidadId}")]
        public async Task<ActionResult> GetEstadisticasIncidentesPorGrupoDeTipoIncidenteMensual(
            [FromRoute] int agno, 
            [FromRoute] string unidadId
            ) 
        {
            try
            {
                // Se inicializa la lista para almacenar los resultados del Total por Grupos de Incidentes y sus Meses.

                var listaTotalPorGrupo = new List<TotalPorGrupoModel>();

                // Obtiene la lista de grupos de incidentes.

                List<tipo_grupo_incidente> gruposIncidentes = await _sciContext.TipoGrupoIncidente
                .AsNoTracking()
                .OrderBy(gi => gi.ID)
                .ToListAsync();

                // Obtiene el conteo de incidentes agrupados por Grupo de Incidente y Mes.

                var conteos = await _sciContext.Incidentes
                    .AsNoTracking()
                    .Where(i =>
                        (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) &&
                        (agno == 0 || i.FECHA_HECHO.Year == agno) &&
                        (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId) &&
                        i.TIPO_INCIDENTE != null &&
                        i.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTEID != null)
                    .GroupBy(i => new
                    {
                        GrupoId = i.TIPO_INCIDENTE!.TIPO_GRUPO_INCIDENTEID,
                        Mes = i.FECHA_HECHO.Month
                    })
                    .Select(g => new
                    {
                        g.Key.GrupoId,
                        g.Key.Mes,
                        Cantidad = g.Count()
                    })
                    .ToListAsync();

                // Convierte la lista de conteos en un diccionario para un acceso rápido por Grupo y Mes.

                var conteosPorGrupoYMes = conteos.ToDictionary(
                    x => (x.GrupoId, x.Mes),
                    x => x.Cantidad);

                // Itera sobre cada grupo de incidentes para construir la lista de totales por grupo y mes.

                foreach (var grupo in gruposIncidentes)
                {
                    // Se inicializa la lista para almacenar los resultados del Total por Mes del grupo actual.

                    var listaTotalPorMes = new List<TotalPorMesModel>();

                    // Itera sobre los meses del año (1 a 12).
                    foreach (var indiceMes in Enumerable.Range(1, 12))
                    {
                        var cantidadMes = conteosPorGrupoYMes.TryGetValue(
                            (grupo.ID, indiceMes),
                            out var cantidad)
                                ? cantidad
                                : 0;

                        // Se agrega el resultado de Mes a la lista.

                        listaTotalPorMes.Add(new TotalPorMesModel
                        {
                            DESCRIPCION_MES = CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.GetMonthName(indiceMes),
                            CANTIDAD_MES = cantidadMes
                        });
                    }

                    // Construye el objeto TotalPorGrupoModel para el grupo actual.

                    var totalDelGrupo = new TotalPorGrupoModel
                    {
                        DESCRIPCION_GRUPO = grupo.DESCRIPCION,
                        COLOR_GRUPO = grupo.COLOR,
                        TOTAL_GRUPO = listaTotalPorMes.ToArray()
                    };

                    // Asigna resultado a listaTotalPorGrupo.

                    listaTotalPorGrupo.Add(totalDelGrupo);
                }

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes de Incidentes por Grupo de Tipo de Incidentes Mesual General o por Año ", JsonConvert.SerializeObject(listaTotalPorGrupo), _usuariosContext
                );

                return Ok( listaTotalPorGrupo );
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

        // Lista todos años disponibles para Informes Incidentes 
        // Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 3 (Con Informe Final).

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListAgnosInformesIncidentes() 
        {
            try
            {
                var respuesta = await _sciContext.Incidentes
                    .AsNoTracking()
                    .Where(i => i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3)
                    .Select(i => i.FECHA_HECHO.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Rescate de Años de Informes Incidentes", JsonConvert.SerializeObject(""), _usuariosContext
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
// 09-10-2026 13:00
