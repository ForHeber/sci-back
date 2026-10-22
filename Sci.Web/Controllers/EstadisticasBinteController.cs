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
    public class EstadisticasBinteController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public EstadisticasBinteController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Graficos de uso exclusivo de Estadisticas Binte


        // Estadistica de Incidentes/Finales Institucionales por Tipo Accidente o Incidente.
        // Si el parametro agnoDeseado es 0 (Cero) = Total General de todos los años.
        // Si el parametro tipoInforme es "I" = Rescata Incidentes, "F" = Rescata Finales.
        // Estados en Finales, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 4 (Cierre Temporal), Estado = 5 (Cierre Definitivo).
        // Estados en Incidentes, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 3 (Con Informe Final).

        [HttpGet("[action]/{agnoDeseado},{tipoInforme}")]
        public async Task<ActionResult> GetEstadisticasBinteCantidadIncidenteAccidente(
            [FromRoute] int agnoDeseado,
            [FromRoute] string tipoInforme
            )
        {
            try
            {
                // Validación del parámetro tipoInforme, debe ser "I" o "F"

                if (tipoInforme != "I" && tipoInforme != "F")
                {
                    return BadRequest("No se especifico de que Informe, debe ser 'I' o 'F'.");
                }

                var series = Array.Empty<int>();
                var label = Array.Empty<string>();

                // Agrupa por Incidente/Accidente y total de Incidentes o Finales segun tipoInforme.
                
                var respuestaAgrupada = tipoInforme == "F"
                ? await _sciContext.Finales
                    .Where(f => (f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5) && (agnoDeseado == 0 ? true : f.FECHA_HECHO.Year == agnoDeseado))
                    .GroupBy(f => f.INCIDENTE_ACCIDENTE)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        INCIDENTE_ACCIDENTE = g.Key,
                        CANTIDAD = g.Count()
                    })
                    .ToListAsync()
                : await _sciContext.Incidentes
                    .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && (agnoDeseado == 0 ? true : i.FECHA_HECHO.Year == agnoDeseado))
                    .GroupBy(i => i.INCIDENTE_ACCIDENTE)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        INCIDENTE_ACCIDENTE = g.Key,
                        CANTIDAD = g.Count()
                    })
                    .ToListAsync();

                // Construye la respuesta sin realocaciones repetidas.

                var respuesta = respuestaAgrupada
                .Select(g => new IncidentesFinalesViewModel
                {
                    NOMBRE = g.INCIDENTE_ACCIDENTE == "I"
                        ? "Incidente"
                        : g.INCIDENTE_ACCIDENTE == "A"
                            ? "Accidente"
                            : "N/R",
                    CANTIDAD = g.CANTIDAD
                })
                .ToList();

                // Creación de Arreglos label y series para Grafico

                foreach (IncidentesFinalesViewModel resp in respuesta)
                {
                    label = label.Append<string>(resp.NOMBRE ?? "N/R").ToArray();
                    series = series.Append<int>(resp.CANTIDAD).ToArray();
                };

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística Incidentes/Finales por Tipo Incidente o Accidente Institucionales General o por Año", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok( new { label, series } ); 
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

        // Estadistica de Incidentes/Finales Institucionales por Tipo Persona o Unidad.
        // Si el parametro agnoDeseado es 0 (Cero) = Total General de todos los años.
        // Si el parametro tipoInforme es "I" = Rescata Incidentes, "F" = Rescata Finales.
        // Estados en Finales, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 4 (Cierre Temporal), Estado = 5 (Cierre Definitivo).
        // Estados en Incidentes, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 3 (Con Informe Final).

        [HttpGet("[action]/{agnoDeseado},{tipoInforme}")]
        public async Task<ActionResult> GetEstadisticasBinteCantidadPersonaUnidad(
            [FromRoute] int agnoDeseado,
            [FromRoute] string tipoInforme
            )
        {
            try
            {
               // Validación del parámetro tipoInforme, debe ser "I" o "F"

                if (tipoInforme != "I" && tipoInforme != "F")
                {
                    return BadRequest("No se especifico de que Informe, debe ser 'I' o 'F'.");
                }

                var series = Array.Empty<int>();
                var label = Array.Empty<string>();

                // Agrupa por persona/unidad y total de incidentes/finales.

                var respuestaAgrupada = tipoInforme == "F"
                ? await _sciContext.Finales
                    .Where(f => (f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5) && (agnoDeseado == 0 ? true : f.FECHA_HECHO.Year == agnoDeseado))
                    .GroupBy(f => f.PERSONAL_UNIDAD)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        PERSONAL_UNIDAD = g.Key,
                        CANTIDAD = g.Count()
                    })
                    .ToListAsync()
                : await _sciContext.Incidentes
                    .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && (agnoDeseado == 0 ? true : i.FECHA_HECHO.Year == agnoDeseado))
                    .GroupBy(i => i.PERSONAL_UNIDAD)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        PERSONAL_UNIDAD = g.Key,
                        CANTIDAD = g.Count()
                    })
                    .ToListAsync();

                // Construye la respuesta sin realocaciones repetidas.

                var respuesta = respuestaAgrupada
                .Select(g => new IncidentesFinalesViewModel
                {
                    NOMBRE = g.PERSONAL_UNIDAD == "P"
                        ? "Persona"
                        : g.PERSONAL_UNIDAD == "U"
                            ? "Unidad"
                            : "N/R",
                    CANTIDAD = g.CANTIDAD
                })
                .ToList();

                // Creación de Arreglos label y series para Grafico

                foreach (IncidentesFinalesViewModel resp in respuesta)
                {
                    label = label.Append<string>(resp.NOMBRE ?? "N/R").ToArray();
                    series = series.Append<int>(resp.CANTIDAD).ToArray();
                };

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística Incidentes/Finales por Tipo Persona o Unidad Institucionales General o por Año", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok( new { label, series } ); 
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

        // Estadistica de Informes de Incidentes/Finales Institucionales por el Tipo de Ingesta.
        // Si el parametro agnoDeseado es 0 (Cero) = Total General de todos los años.
        // Si el parametro tipoInforme es "I" = Rescata Incidentes, "F" = Rescata Finales.
        // Estados en Finales, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 4 (Cierre Temporal), Estado = 5 (Cierre Definitivo).
        // Estados en Incidentes, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 3 (Con Informe Final).

        [HttpGet("[action]/{agnoDeseado},{tipoInforme}")]
        public async Task<ActionResult> GetEstadisticasBintePorTipoIngesta(
            [FromRoute] int agnoDeseado,
            [FromRoute] string tipoInforme
            )
        {
            try
            {
                // Validación del parámetro tipoInforme, debe ser "I" o "F"

                if (tipoInforme != "I" && tipoInforme != "F")
                {
                    return BadRequest("No se especifico de que Informe, debe ser 'I' o 'F'.");
                }

                var informesFinales = new List<finales>();
                var informesIncidentes = new List<incidentes>();

                // Creación de lista de Informes de Incidentes/Finales ordenada por DESCRIPCION de INGESTA.

                if (tipoInforme == "F")
                {
                    informesFinales = await _sciContext.Finales
                    .AsNoTracking()
                    .Include(f => f.TIPO_INGESTA)
                    .Where(f => (f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5) && (agnoDeseado == 0 ? true : f.FECHA_HECHO.Year == agnoDeseado))
                    .OrderBy(f => f.TIPO_INGESTA != null ? f.TIPO_INGESTA.DESCRIPCION : string.Empty)
                    .ToListAsync();
                }
                else
                {
                    informesIncidentes = await _sciContext.Incidentes
                    .AsNoTracking()
                    .Include(i => i.TIPO_INGESTA)
                    .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && (agnoDeseado == 0 ? true : i.FECHA_HECHO.Year == agnoDeseado))
                    .OrderBy(i => i.TIPO_INGESTA != null ? i.TIPO_INGESTA.DESCRIPCION : string.Empty)
                    .ToListAsync();
                }

                string[] label;
                int[] series;
                object listado;

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.

                if (tipoInforme == "F")
                {
                    var grupos = informesFinales
                    .GroupBy(r => new {
                        DESCRIPCION = r.TIPO_INGESTA != null ? r.TIPO_INGESTA.DESCRIPCION : string.Empty,
                        ID = r.TIPO_INGESTA != null ? r.TIPO_INGESTA.ID : 0,
                        })
                    .OrderBy(g => g.Key.DESCRIPCION)
                    .ToList();

                    label = grupos.Select(g => g.Key.DESCRIPCION ?? string.Empty).ToArray();
                    series = grupos.Select(g => g.Count()).ToArray();
                    listado = grupos.Select(g => g.ToList()).ToArray();
                }
                else
                {
                    var grupos = informesIncidentes
                    .GroupBy(r => new {
                        DESCRIPCION = r.TIPO_INGESTA != null ? r.TIPO_INGESTA.DESCRIPCION : string.Empty,
                        ID = r.TIPO_INGESTA != null ? r.TIPO_INGESTA.ID : 0,
                        })
                    .OrderBy(g => g.Key.DESCRIPCION)
                    .ToList();

                    label = grupos.Select(g => g.Key.DESCRIPCION ?? string.Empty).ToArray();
                    series = grupos.Select(g => g.Count()).ToArray();
                    listado = grupos.Select(g => g.ToList()).ToArray();
                }

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes de Incidentes/Finales por Tipo de Ingesta General o por Año ", JsonConvert.SerializeObject(""), _usuariosContext
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

        // Estadistica de Informes de Incidentes Institucionales por el Tipo de Identificacion.
        // Si el parametro agnoDeseado es 0 (Cero) = Total General de todos los años.
        // Si el parametro tipoInforme es "I" = Rescata Incidentes, "F" = Rescata Finales.
        // Estados en Finales, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 4 (Cierre Temporal), Estado = 5 (Cierre Definitivo).
        // Estados en Incidentes, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 3 (Con Informe Final).


        [HttpGet("[action]/{agnoDeseado},{tipoInforme}")]
        public async Task<ActionResult> GetEstadisticasBintePorTipoIdentificacion(
            [FromRoute] int agnoDeseado,
            [FromRoute] string tipoInforme
            )
        {
            try
            {
               // Validación del parámetro tipoInforme, debe ser "I" o "F"

                if (tipoInforme != "I" && tipoInforme != "F")
                {
                    return BadRequest("No se especifico de que Informe, debe ser 'I' o 'F'.");
                }

                var informesFinales = new List<finales>();
                var informesIncidentes = new List<incidentes>();

                // Creación de lista de Informes de Incidentes/Finales ordenada por DESCRIPCION de IDENTIFICACION.

                if (tipoInforme == "F")
                {
                    informesFinales = await _sciContext.Finales
                    .AsNoTracking()
                    .Include(f => f.TIPO_IDENTIFICACION)
                    .Where(f => (f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5) && (agnoDeseado == 0 ? true : f.FECHA_HECHO.Year == agnoDeseado))
                    .OrderBy(f => f.TIPO_IDENTIFICACION != null ? f.TIPO_IDENTIFICACION.DESCRIPCION : string.Empty)
                    .ToListAsync();
                }
                else
                {
                    informesIncidentes = await _sciContext.Incidentes
                    .AsNoTracking()
                    .Include(i => i.TIPO_IDENTIFICACION)
                    .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && (agnoDeseado == 0 ? true : i.FECHA_HECHO.Year == agnoDeseado))
                    .OrderBy(i => i.TIPO_IDENTIFICACION != null ? i.TIPO_IDENTIFICACION.DESCRIPCION : string.Empty)
                    .ToListAsync();
                }

                string[] label;
                int[] series;
                object listado;

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.

                if (tipoInforme == "F")
                {
                    var grupos = informesFinales
                    .GroupBy(r => new {
                        DESCRIPCION = r.TIPO_IDENTIFICACION != null ? r.TIPO_IDENTIFICACION.DESCRIPCION : string.Empty,
                        ID = r.TIPO_IDENTIFICACION != null ? r.TIPO_IDENTIFICACION.ID : 0,
                        })
                    .OrderBy(g => g.Key.DESCRIPCION)
                    .ToList();

                    label = grupos.Select(g => g.Key.DESCRIPCION ?? string.Empty).ToArray();
                    series = grupos.Select(g => g.Count()).ToArray();
                    listado = grupos.Select(g => g.ToList()).ToArray();
                }
                else
                {
                    var grupos = informesIncidentes
                    .GroupBy(r => new {
                        DESCRIPCION = r.TIPO_IDENTIFICACION != null ? r.TIPO_IDENTIFICACION.DESCRIPCION : string.Empty,
                        ID = r.TIPO_IDENTIFICACION != null ? r.TIPO_IDENTIFICACION.ID : 0,
                        })
                    .OrderBy(g => g.Key.DESCRIPCION)
                    .ToList();

                    label = grupos.Select(g => g.Key.DESCRIPCION ?? string.Empty).ToArray();
                    series = grupos.Select(g => g.Count()).ToArray();
                    listado = grupos.Select(g => g.ToList()).ToArray();
                }

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes de Incidentes/Finales por Tipo de Identificacion General o por Año ", JsonConvert.SerializeObject(""), _usuariosContext
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

        // Estadistica de Informes de Incidentes/Finales Institucionales por el Tipo de Armamento.
        // Si el parametro agnoDeseado es 0 (Cero) = Total General de todos los años.
        // Si el parametro tipoInforme es "I" = Rescata Incidentes, "F" = Rescata Finales.
        // Estados en Finales, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 4 (Cierre Temporal), Estado = 5 (Cierre Definitivo).
        // Estados en Incidentes, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 3 (Con Informe Final).

        [HttpGet("[action]/{agnoDeseado},{tipoInforme}")]
        public async Task<ActionResult> GetEstadisticasBintePorTipoArmamento(
            [FromRoute] int agnoDeseado,
            [FromRoute] string tipoInforme
            )
        {
            try
            {
                // Validación del parámetro tipoInforme, debe ser "I" o "F"

                if (tipoInforme != "I" && tipoInforme != "F")
                {
                    return BadRequest("No se especifico de que Informe, debe ser 'I' o 'F'.");
                }
                var informesFinales = new List<finales>();
                var informesIncidentes = new List<incidentes>();
                
                // Creación de lista de Informes de Incidentes/Finales ordenada por DESCRIPCION de ARMAMENTO.

                if (tipoInforme == "F")
                {
                    informesFinales = await _sciContext.Finales
                    .AsNoTracking()
                    .Include(f => f.TIPO_ARMAMENTO)
                    .Where(f => (f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5) && (agnoDeseado == 0 ? true : f.FECHA_HECHO.Year == agnoDeseado))
                    .OrderBy(f => f.TIPO_ARMAMENTO != null ? f.TIPO_ARMAMENTO.DESCRIPCION : string.Empty)
                    .ToListAsync();
                }
                else
                {
                    informesIncidentes = await _sciContext.Incidentes
                    .AsNoTracking()
                    .Include(i => i.TIPO_ARMAMENTO)
                    .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && (agnoDeseado == 0 ? true : i.FECHA_HECHO.Year == agnoDeseado))
                    .OrderBy(i => i.TIPO_ARMAMENTO != null ? i.TIPO_ARMAMENTO.DESCRIPCION : string.Empty)
                    .ToListAsync();
                }

                string[] label;
                int[] series;
                object listado;

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.

                if (tipoInforme == "F")
                {
                    var grupos = informesFinales
                    .GroupBy(r => new {
                        DESCRIPCION = r.TIPO_ARMAMENTO != null ? r.TIPO_ARMAMENTO.DESCRIPCION : string.Empty,
                        ID = r.TIPO_ARMAMENTO != null ? r.TIPO_ARMAMENTO.ID : 0,
                        })
                    .OrderBy(g => g.Key.DESCRIPCION)
                    .ToList();

                    label = grupos.Select(g => g.Key.DESCRIPCION ?? string.Empty).ToArray();
                    series = grupos.Select(g => g.Count()).ToArray();
                    listado = grupos.Select(g => g.ToList()).ToArray();
                }
                else
                {
                    var grupos = informesIncidentes
                    .GroupBy(r => new {
                        DESCRIPCION = r.TIPO_ARMAMENTO != null ? r.TIPO_ARMAMENTO.DESCRIPCION : string.Empty,
                        ID = r.TIPO_ARMAMENTO != null ? r.TIPO_ARMAMENTO.ID : 0,
                        })
                    .OrderBy(g => g.Key.DESCRIPCION)
                    .ToList();

                    label = grupos.Select(g => g.Key.DESCRIPCION ?? string.Empty).ToArray();
                    series = grupos.Select(g => g.Count()).ToArray();
                    listado = grupos.Select(g => g.ToList()).ToArray();
                }

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes de Incidentes/Finales por Tipo de Armamento General o por Año ", JsonConvert.SerializeObject(""), _usuariosContext
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

        // Estadistica de Incidentes/Finales Institucionales por Tipo Arma Particular/Fiscal.
        // Si el parametro agnoDeseado es 0 (Cero) = Total General de todos los años.
        // Si el parametro tipoInforme es "I" = Rescata Incidentes, "F" = Rescata Finales.
        // Estados en Finales, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 4 (Cierre Temporal), Estado = 5 (Cierre Definitivo).
        // Estados en Incidentes, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 3 (Con Informe Final).

        [HttpGet("[action]/{agnoDeseado},{tipoInforme}")]
        public async Task<ActionResult> GetEstadisticasBinteCantidadArmaFiscalParticular(
            [FromRoute] int agnoDeseado,
            [FromRoute] string tipoInforme
            )
        {
            try
            {
                // Validación del parámetro tipoInforme, debe ser "I" o "F"

                if (tipoInforme != "I" && tipoInforme != "F")
                {
                    return BadRequest("No se especifico de que Informe, debe ser 'I' o 'F'.");
                }

                var series = Array.Empty<int>();
                var label = Array.Empty<string>();

                // Agrupa por persona/unidad y total de incidentes/finales.

                var respuestaAgrupada = tipoInforme == "F"
                ? await _sciContext.Finales
                    .Where(f => (f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5) && (agnoDeseado == 0 ? true : f.FECHA_HECHO.Year == agnoDeseado))
                    .GroupBy(f => f.ARMA_FISCAL_PARTICULAR)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        ARMA_FISCAL_PARTICULAR = g.Key,
                        CANTIDAD = g.Count()
                    })
                    .ToListAsync()
                : await _sciContext.Incidentes
                    .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && (agnoDeseado == 0 ? true : i.FECHA_HECHO.Year == agnoDeseado))
                    .GroupBy(i => i.ARMA_FISCAL_PARTICULAR)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        ARMA_FISCAL_PARTICULAR = g.Key,
                        CANTIDAD = g.Count()
                    })
                    .ToListAsync();

                // Construye la respuesta sin realocaciones repetidas.

                var respuesta = respuestaAgrupada
                .Select(g => new IncidentesFinalesViewModel
                {
                    NOMBRE = g.ARMA_FISCAL_PARTICULAR == "P"
                        ? "Particular"
                        : g.ARMA_FISCAL_PARTICULAR == "F"
                            ? "Fiscal"
                            : "N/R",
                    CANTIDAD = g.CANTIDAD
                })
                .ToList();

                // Creación de Arreglos label y series para Grafico

                foreach (IncidentesFinalesViewModel resp in respuesta)
                {
                    label = label.Append<string>(resp.NOMBRE ?? "N/R").ToArray();
                    series = series.Append<int>(resp.CANTIDAD).ToArray();
                };

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística Incidentes/Finales por Tipo Arma Particular/Fiscal General o por Año", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok( new { label, series } ); 
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

        // Estadistica de Informes de Incidentes/Finales Institucionales por el Tipo de Investigacion.
        // Si el parametro agnoDeseado es 0 (Cero) = Total General de todos los años.
        // Si el parametro tipoInforme es "I" = Rescata Incidentes, "F" = Rescata Finales.
        // Estados en Finales, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 4 (Cierre Temporal), Estado = 5 (Cierre Definitivo).
        // Estados en Incidentes, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 3 (Con Informe Final).

        [HttpGet("[action]/{agnoDeseado},{tipoInforme}")]
        public async Task<ActionResult> GetEstadisticasBintePorTipoInvestigacion(
            [FromRoute] int agnoDeseado,
            [FromRoute] string tipoInforme
            )
        {
            try
            {
                // Validación del parámetro tipoInforme, debe ser "I" o "F"

                if (tipoInforme != "I" && tipoInforme != "F")
                {
                    return BadRequest("No se especifico de que Informe, debe ser 'I' o 'F'.");
                }

                var informesFinales = new List<finales>();
                var informesIncidentes = new List<incidentes>();

                // Creación de lista de Informes de Incidentes/Finales ordenada por DESCRIPCION de INVESTIGACION.

                if (tipoInforme == "F")
                {
                    informesFinales = await _sciContext.Finales
                    .AsNoTracking()
                    .Include(f => f.TIPO_INVESTIGACION)
                    .Where(f => (f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5) && (agnoDeseado == 0 ? true : f.FECHA_HECHO.Year == agnoDeseado))
                    .OrderBy(f => f.TIPO_INVESTIGACION != null ? f.TIPO_INVESTIGACION.DESCRIPCION : string.Empty)
                    .ToListAsync();
                }
                else
                {
                    informesIncidentes = await _sciContext.Incidentes
                    .AsNoTracking()
                    .Include(i => i.TIPO_INVESTIGACION)
                    .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && (agnoDeseado == 0 ? true : i.FECHA_HECHO.Year == agnoDeseado))
                    .OrderBy(i => i.TIPO_INVESTIGACION != null ? i.TIPO_INVESTIGACION.DESCRIPCION : string.Empty)
                    .ToListAsync();
                }

                string[] label;
                int[] series;
                object listado;

                // Agrupa una vez y construye la respuesta sin realocaciones repetidas.

                if (tipoInforme == "F")
                {
                    var grupos = informesFinales
                    .GroupBy(r => new {
                        DESCRIPCION = r.TIPO_INVESTIGACION != null ? r.TIPO_INVESTIGACION.DESCRIPCION : string.Empty,
                        ID = r.TIPO_INVESTIGACION != null ? r.TIPO_INVESTIGACION.ID : 0,
                        })
                    .OrderBy(g => g.Key.DESCRIPCION)
                    .ToList();

                    label = grupos.Select(g => g.Key.DESCRIPCION ?? string.Empty).ToArray();
                    series = grupos.Select(g => g.Count()).ToArray();
                    listado = grupos.Select(g => g.ToList()).ToArray();
                }
                else
                {
                    var grupos = informesIncidentes
                    .GroupBy(r => new {
                        DESCRIPCION = r.TIPO_INVESTIGACION != null ? r.TIPO_INVESTIGACION.DESCRIPCION : string.Empty,
                        ID = r.TIPO_INVESTIGACION != null ? r.TIPO_INVESTIGACION.ID : 0,
                        })
                    .OrderBy(g => g.Key.DESCRIPCION)
                    .ToList();

                    label = grupos.Select(g => g.Key.DESCRIPCION ?? string.Empty).ToArray();
                    series = grupos.Select(g => g.Count()).ToArray();
                    listado = grupos.Select(g => g.ToList()).ToArray();
                }

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Estadística de Informes de Incidentes/Finales por Tipo de Investigacion General o por Año ", JsonConvert.SerializeObject(""), _usuariosContext
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

        // Lista todos años disponibles desde Informes de Incidentes/Finales 
        // Estados en Finales, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 4 (Cierre Temporal), Estado = 5 (Cierre Definitivo).
        // Estados en Incidentes, Estado = 1 (Tramite CDTE), Estado = 2 (En Proceso S2 o Devuelto), Estado = 3 (Con Informe Final).

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListAgnosInformesIncidentesFinales() 
        {
            try
            {
                var respuesta = await _sciContext.Incidentes
                    .AsNoTracking()
                    .Where(i => i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3)
                    .Select(i => i.FECHA_HECHO.Year)
                    .Union(
                        _sciContext.Finales
                            .AsNoTracking()
                            .Where(f => f.ESTADO == 1 || f.ESTADO == 2 || f.ESTADO == 4 || f.ESTADO == 5)
                            .Select(f => f.FECHA_HECHO.Year)
                    )
                    .OrderByDescending(y => y)
                    .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Rescate de Años de Informes Incidentes/Finales", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new { respuesta } );
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
