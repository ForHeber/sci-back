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
    public class EstadisticasFinalesController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public EstadisticasFinalesController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Estadistica de Informes Finales Por Estados.
        // unidadId = 0 (Cero) = Finales Institucionales de lo contrario Finales de la unidad especificada.

        [HttpGet("[action]/{agno},{unidadId}")]
        public async Task<ActionResult> GetListEstadisticasFinalesPorEstados(
            [FromRoute] int agno, 
            [FromRoute] string unidadId
            ) 
        {
            try
            {

                // Creación de lista de Informes Finales ordenada por FECHA_HECHO.

                var informesFinales = await _sciContext.Finales
                .AsNoTracking()
                .Include(f => f.TIPO_INGESTA)
                .Include(f => f.TIPO_INCIDENTE)
                .Include(f => f.TIPO_IDENTIFICACION)
                .Include(f => f.TIPO_ARMAMENTO)
                .Include(f => f.TIPO_INVESTIGACION)
                .Include(f => f.INFORMANTE)
                .Include(f => f.U_INFORMANTE)
                .Include(f => f.U_AFECTADA)
                .Where(f => ( f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5 ) && 
                            (agno == 0 ? true : f.FECHA_HECHO.Year == agno) && 
                            (unidadId == "0" || f.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(f => f.FECHA_HECHO)
                .ToListAsync();

                var series  = Array.Empty<int>();
                var label   = Array.Empty<string>();
                List<object>[] listado = Array.Empty<List<object>>();

                // Creación de series para Gráfico

                series = new int[] {
                    informesFinales.Count( f => f.ESTADO == 1), // Contador de Informes Finales Tramitados a CDTE
                    informesFinales.Count( f => f.ESTADO == 2 && f.DEVUELTO == "N"), // Contador de Informes Finales En Proceso por S2
                    informesFinales.Count( f => f.ESTADO == 2 && f.DEVUELTO == "S"), // Contador de Informes Finales Devueltos por CDTE.
                    informesFinales.Count( f => f.ESTADO == 4), // Contador de Informes Finales con Cierre Temporal
                    informesFinales.Count( f => f.ESTADO == 5) // Contador de Informes Finales con Cierre Definitivo
                };

                // Creación de label para Gráfico

                label = new string[] {
                    "Tramitados a CDTE",
                    "En Proceso por S2",
                    "Devueltos por CDTE",
                    "Con Cierre Temporal",
                    "Con Cierre Definitivo"
                };

                // Creación de listas de Informes Finales para Gráfico

                listado = new List<object>[] {
                    informesFinales.Where( f => f.ESTADO == 1 ).Cast<object>().ToList(), // Listado de Informes Finales Tramitados a CDTE
                    informesFinales.Where( f => f.ESTADO == 2 && f.DEVUELTO == "N" ).Cast<object>().ToList(), // Listado de Informes Finales En Proceso por S2
                    informesFinales.Where( f => f.ESTADO == 2 && f.DEVUELTO == "S" ).Cast<object>().ToList(), // Listado de Informes Finales Devueltos por CDTE
                    informesFinales.Where( f => f.ESTADO == 4 ).Cast<object>().ToList(), // Listado de Informes Finales con Cierre Temporal
                    informesFinales.Where( f => f.ESTADO == 5 ).Cast<object>().ToList(), // Listado de Informes Finales con Cierre Definitivo
                };

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes Finales del S2", JsonConvert.SerializeObject(""), _usuariosContext
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

        // Estadistica de Informes Finales por el Tipo de Incidente en el S2 por año.
        // (agno=0)-Total General (agno=año especifico).
        // unidadId = 0 (Cero) = Finales Institucionales de lo contrario Finales de la unidad especificada.
        // Estado 1 = Tramitado al CDTE, Estado 2 = En Proceso por S2, Estado 4 = Con Cierre Temporal, Estado 5 = Con Cierre Definitivo.

        [HttpGet("[action]/{agno},{unidadId}")]
        public async Task<ActionResult> GetEstadisticasFinalesPorTipoIncidente(
            [FromRoute] int agno, 
            [FromRoute] string unidadId
            ) 
        {
            try
            {

                // Creación de lista de Informes Finales ordenada por DESCRIPCION de INCIDENTES.

                List<finales> informesFinales = await _sciContext.Finales
                .AsNoTracking()
                .Include(f => f.TIPO_INCIDENTE)
                .Include(f => f.U_INFORMANTE)
                .Include(f => f.U_AFECTADA)
                .Include(f => f.INFORMANTE)
                .Where(f => (f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5) && 
                            (agno == 0 ? true : f.FECHA_HECHO.Year == agno) && 
                            (unidadId == "0" || f.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(f => f.TIPO_INCIDENTE != null ? f.TIPO_INCIDENTE.DESCRIPCION : string.Empty)
                .ToListAsync();

                var series      =   Array.Empty<int>();
                var label       =   Array.Empty<string>();
                var colorBarra  =   Array.Empty<string>();
                var listado     =   Array.Empty<List<finales>>();

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.
                var grupos = informesFinales
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
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes Finales por Tipo de Incidentes General o por Año ", JsonConvert.SerializeObject(""), _usuariosContext
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
        
        // Entrega cantidades de Informes Finales según criterios específicos.
        // unidadId = 0 (Cero) = Finales Institucionales de lo contrario Finales de la unidad especificada.

        [HttpGet("[action]/{unidadId}")]
        public async Task<ActionResult> GetEstadisticasFinalesPorEstados(
            [FromRoute] string unidadId
            ) 
        {
            try
            {

                var agnoActual = DateTime.Now.Year;

                // Extrae y agrupa los datos de la base de datos para obtener los totales de finales por estado y año.

                var resumen = await _sciContext.Finales
                .AsNoTracking()
                .Include(f => f.TIPO_INGESTA)
                .Include(f => f.TIPO_INCIDENTE)
                .Include(f => f.TIPO_IDENTIFICACION)
                .Include(f => f.TIPO_ARMAMENTO)
                .Include(f => f.TIPO_INVESTIGACION)
                .Include(f => f.INFORMANTE)
                .Include(f => f.U_INFORMANTE)
                .Include(f => f.U_AFECTADA)
                .Where(f => ( f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5 ) && (unidadId == "0" || f.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderByDescending(f => f.FECHA_HECHO)
                .ToListAsync();

                // Extrae los últimos 10 informes finales, incluyendo las relaciones necesarias.
                var Ultimos10Finales = resumen
                    .Where(f =>  f.ESTADO != 4 && f.ESTADO != 5 && f.FECHA_HECHO.Year == agnoActual) // Excluye los finales que ya tienen cierre temporal o definitivo
                    .OrderByDescending(f => f.ID)
                    .Take(10)
                    .ToList();

                var resumenTotales = resumen
                .GroupBy(g1 => 1)
                .Select(g => new
                {
                    TotalFinalesTramitadosAlCDTE = g.Count( f => f.ESTADO == 1 && f.FECHA_HECHO.Year == agnoActual),
                    TotalFinalesEnProcesoS2 = g.Count(f => f.ESTADO == 2 && f.DEVUELTO == "N" && f.FECHA_HECHO.Year == agnoActual),
                    TotalFinalesDevueltosPorCDTE = g.Count(f => f.ESTADO == 2 && f.DEVUELTO == "S" && f.FECHA_HECHO.Year == agnoActual),
                    TotalFinalesConCierreTemporal = g.Count( f => f.ESTADO == 4 && f.FECHA_HECHO.Year == agnoActual),
                    TotalFinalesConCierreDefinitivo = g.Count( f => f.ESTADO == 5 && f.FECHA_HECHO.Year == agnoActual),
                    TotalFinalesGeneral = g.Count( f => f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5),
                    TotalFinalesAgno = g.Count(f => (f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5) && f.FECHA_HECHO.Year == agnoActual )
                })
                .FirstOrDefault();

                // Validación de nulos y asignación de valores predeterminados en caso de que no haya resultados.
                var TotalFinalesTramitadosAlCDTE = resumenTotales?.TotalFinalesTramitadosAlCDTE ?? 0;
                var TotalFinalesEnProcesoS2 = resumenTotales?.TotalFinalesEnProcesoS2 ?? 0;
                var TotalFinalesDevueltosPorCDTE = resumenTotales?.TotalFinalesDevueltosPorCDTE ?? 0;
                var TotalFinalesConCierreTemporal = resumenTotales?.TotalFinalesConCierreTemporal ?? 0;
                var TotalFinalesConCierreDefinitivo = resumenTotales?.TotalFinalesConCierreDefinitivo ?? 0;
                var TotalFinalesGeneral = resumenTotales?.TotalFinalesGeneral ?? 0;
                var TotalFinalesAgno = resumenTotales?.TotalFinalesAgno ?? 0;

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Cantidad Finales Por Estado", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new
                {
                    TotalFinalesTramitadosAlCDTE,
                    TotalFinalesEnProcesoS2,
                    TotalFinalesDevueltosPorCDTE,
                    TotalFinalesConCierreTemporal,
                    TotalFinalesConCierreDefinitivo,
                    TotalFinalesGeneral,
                    TotalFinalesAgno,
                    Ultimos10Finales
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
    
        // Estadistica de Tipos de Finales por Fecha Institucionales o por Unidad
        // Si el parametro entregado en unidad es 0 (Cero) = Finales Institucionales por Fecha
        // Si se especifica una unidad = Finales de la unidad especificada por Fecha

        [HttpGet("[action]/{unidadId},{agnoDeseado}")]
        public async Task<ActionResult> GetEstadisticasFinalesPorFecha(
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

                // Agrupa por fecha (sin hora) y total de finales.

                var unidadIdNormalizado = unidadId.Trim();

                List<finales> informesFinales = await _sciContext.Finales
                    .AsNoTracking()
                    .Include(f => f.TIPO_INCIDENTE)
                    .Include(f => f.U_INFORMANTE)
                    .Include(f => f.U_AFECTADA)
                    .Include(f => f.INFORMANTE)
                    .Where(f => (f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5) 
                                && (unidadIdNormalizado == "0" ? true : (f.UNIDAD_AFECTADA ?? string.Empty).Trim() == unidadIdNormalizado) 
                                && (agnoDeseado == 0 ? true : f.FECHA_HECHO.Year == agnoDeseado))
                    .OrderBy(f => f.FECHA_HECHO.Date)
                    .ToListAsync();

                var series      =   Array.Empty<int>();
                var label       =   Array.Empty<string>();
                var listado     =   Array.Empty<List<finales>>();

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.

                var grupos = informesFinales
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
                    Util.GetActionName(ControllerContext), "Ok Estadística Finales por Fecha (Institucionales o por Unidad) y (General o del Año)", JsonConvert.SerializeObject(""), _usuariosContext
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

        // Estadistica de Informes Finales por Grupos de Incidente en el S2 por año.
        // (agno=0)-Total General (agno=año) Especifico.
        // unidadId = 0 (Cero) = Grupo de Incidentes Institucionales de lo contrario Grupo de Incidentes de la unidad especificada.
        // Estado 1 = Tramitado al CDTE, Estado 2 = En Proceso por S2, Estado 4 = Con Cierre Temporal, Estado 5 = Con Cierre Definitivo.

        [HttpGet("[action]/{agno},{unidadId}")]
        public async Task<ActionResult> GetEstadisticasFinalesPorGrupoDeTipoIncidente(
            [FromRoute] int agno, 
            [FromRoute] string unidadId
            ) 
        {
            try
            {

                // Creación de lista de Informes de Grupos de Incidentes ordenada por DESCRIPCION de GRUPO DE INCIDENTES.

                List<finales> informesFinales = await _sciContext.Finales
                .AsNoTracking()
                .Include(i => i.TIPO_INCIDENTE)
                    .ThenInclude(i => i!.TIPO_GRUPO_INCIDENTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Include(i => i.INFORMANTE)
                .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 4 || i.ESTADO == 5) && 
                            (agno == 0 ? true : i.FECHA_HECHO.Year == agno) && 
                            (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.TIPO_INCIDENTE != null && i.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTE != null ? i.TIPO_INCIDENTE.TIPO_GRUPO_INCIDENTE.DESCRIPCION : string.Empty)
                .ToListAsync();

                var series      =   Array.Empty<int>();
                var label       =   Array.Empty<string>();
                var colorBarra  =   Array.Empty<string>();
                var listado     =   Array.Empty<List<finales>>();

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.
                
                var grupos = informesFinales
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
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes Finales por Grupo de Tipo de Incidentes General o por Año ", JsonConvert.SerializeObject(""), _usuariosContext
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

        // Estadistica de Informes Finales por Grupos de Incidente Mensual en el S2 por año.
        // (agno=0)-Total General (agno=año) Especifico.
        // unidadId = 0 (Cero) = Grupo de Incidentes Institucionales por Mes de lo contrario Grupo de Incidentes por Mes de la unidad especificada.
        // Estado 1 = Tramitado al CDTE, Estado 2 = En Proceso por S2, Estado 4 = Con Cierre Temporal, Estado 5 = Con Cierre Definitivo.

        [HttpGet("[action]/{agno},{unidadId}")]
        public async Task<ActionResult> GetEstadisticasFinalesPorGrupoDeTipoIncidenteMensual(
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

                // Obtiene el conteo de Finales agrupados por Grupo de Incidente y Mes.

                var conteos = await _sciContext.Finales
                    .AsNoTracking()
                    .Where(i =>
                        (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 4 || i.ESTADO == 5) &&
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
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes Finales por Grupo de Tipo de Incidentes Mesual General o por Año ", JsonConvert.SerializeObject(listaTotalPorGrupo), _usuariosContext
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

        // Lista todos años disponibles para Informes Finales 
        // Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 4 (Cierre Temporal), Estado = 5 (Cierre Definitivo).

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListAgnosInformesFinales() 
        {
            try
            {
                var respuesta = await _sciContext.Finales
                    .AsNoTracking()
                    .Where(f => f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5)
                    .Select(f => f.FECHA_HECHO.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Rescate de Años de Informes Finales", JsonConvert.SerializeObject(""), _usuariosContext
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
