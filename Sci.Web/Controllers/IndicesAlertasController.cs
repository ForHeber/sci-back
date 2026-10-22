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
    public class IndicesAlertasController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public IndicesAlertasController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista los Indices de Alerta para su Mantención

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListMantencionIndicesAlertas() {
            try
            {
                var indicesAlerta = await _sciContext.IndicesAlertas
                .Where(ia => ia.ESTADO == 1)
                .OrderBy(ia => ia.ID)          
                .ToListAsync();

                int totalRegistros = indicesAlerta.Count();
                
                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {
                    indicesAlerta,
                    totalRegistros
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

        // Actualiza un Indice de Alerta

        [HttpPut("[action]/{indiceAlertaId}")]
        public async Task<ActionResult<indices_alertas>> PutMantencionIndiceAlerta(
            PutMantencionIndicesAlertasModel model, 
            [FromRoute] int indiceAlertaId
        ) 
        {
            try
            {
                indices_alertas? indicesAlertas = await _sciContext.IndicesAlertas
                .Where(ia => ia.ID == indiceAlertaId)
                .FirstOrDefaultAsync();

                if (indicesAlertas == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Indice de Alerta para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );

                    return NotFound();
                }

                indicesAlertas.ALERTA_AMARILLA = model.ALERTA_AMARILLA;
                indicesAlertas.ALERTA_ROJA = model.ALERTA_ROJA;

                indicesAlertas.USUARIO = int.Parse(User.GetClaimValue("Run"));
                indicesAlertas.ACTUALIZACION = DateTime.Now;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(indicesAlertas);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Put", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject(model), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista los Preliminares con Estado = 2 (No publicados)
        // que se encuentren con Alerta Amarilla o Roja segun Indices configurados.

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListPreliminaresConAlertas() {
            try
            {
                var indicesAlerta = await _sciContext.IndicesAlertas
                .Where(ia => ia.ESTADO == 1 && ia.TIPO_INFORME == "Preliminares") // Rescata los Indices de Preliminares.
                .ToListAsync();

                if (indicesAlerta == null || !indicesAlerta.Any())
                {
                    Auditor.registroAuditor(
                        "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se encontraron Idices de Alerta para Preliminares", JsonConvert.SerializeObject(""), _usuariosContext
                    );
                    return NotFound();
                }

                var fechaActual = DateTime.Now;

                var preliminares = await _sciContext.Preliminares
                    .Include(pre => pre.TIPO_INCIDENTE)
                    .Where(pre => pre.ESTADO == 2)
                    .ToListAsync();

                var preliminaresConDiferencia = preliminares
                    .Select(pre => new
                    {
                        Preliminar = pre,
                        DiferenciaHoras = (fechaActual - pre.FECHA_INFORMANTE).TotalHours
                    })
                    .ToList();

                var preliminaresConAlertaAmarilla = preliminaresConDiferencia
                    .Where(pre => pre.DiferenciaHoras > indicesAlerta.First().ALERTA_AMARILLA!.Value
                        && pre.DiferenciaHoras < indicesAlerta.First().ALERTA_ROJA!.Value)
                    .ToList();

                var preliminaresConAlertaRoja = preliminaresConDiferencia
                    .Where(pre => pre.DiferenciaHoras > indicesAlerta.First().ALERTA_ROJA!.Value)
                    .ToList();

                int totalPreliminaresConAlertaAmarilla = preliminaresConAlertaAmarilla.Count;
                int totalPreliminaresConAlertaRoja = preliminaresConAlertaRoja.Count;

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {
                    preliminaresConAlertaAmarilla,
                    preliminaresConAlertaRoja,
                    totalPreliminaresConAlertaAmarilla,
                    totalPreliminaresConAlertaRoja
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

        // Lista los Incidentes con Estado = 2 (En Proceso por el S2)
        // que se encuentren con Alerta Amarilla o Roja segun Indices configurados.

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListIncidentesConAlertas() {
            try
            {
                var indicesAlerta = await _sciContext.IndicesAlertas
                .Where(ia => ia.ESTADO == 1 && ia.TIPO_INFORME == "Incidentes") // Rescata los Indices de Incidentes.
                .ToListAsync();

                if (indicesAlerta == null || !indicesAlerta.Any())
                {
                    Auditor.registroAuditor(
                        "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se encontraron Idices de Alerta para Incidentes", JsonConvert.SerializeObject(""), _usuariosContext
                    );
                    return NotFound();
                }

                var fechaActual = DateTime.Now;

                var indicentes = await _sciContext.Incidentes
                    .Include(inc => inc.TIPO_INCIDENTE)
                    .Where(inc => inc.ESTADO == 2)
                    .ToListAsync();

                var indicentesConDiferencia = indicentes
                    .Select(inc => new
                    {
                        Incidente = inc,
                        DiferenciaHoras = (fechaActual - inc.FECHA_PUBLICACION).TotalHours
                    })
                    .ToList();

                var indicentesConAlertaAmarilla = indicentesConDiferencia
                    .Where(inc => inc.DiferenciaHoras > indicesAlerta.First().ALERTA_AMARILLA!.Value
                        && inc.DiferenciaHoras < indicesAlerta.First().ALERTA_ROJA!.Value)
                    .ToList();

                var indicentesConAlertaRoja = indicentesConDiferencia
                    .Where(inc => inc.DiferenciaHoras > indicesAlerta.First().ALERTA_ROJA!.Value)
                    .ToList();

                int totalIndicentesConAlertaAmarilla = indicentesConAlertaAmarilla.Count;
                int totalIndicentesConAlertaRoja = indicentesConAlertaRoja.Count;

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {
                    indicentesConAlertaAmarilla,
                    indicentesConAlertaRoja,
                    totalIndicentesConAlertaAmarilla,
                    totalIndicentesConAlertaRoja
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

        // Lista los Finales con Estado = 2 (En Proceso por el S2)
        // que se encuentren con Alerta Amarilla o Roja segun Indices configurados.

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListFinalesConAlertas() {
            try
            {
                var indicesAlerta = await _sciContext.IndicesAlertas
                .Where(ia => ia.ESTADO == 1 && ia.TIPO_INFORME == "Finales") // Rescata los Indices de Finales.
                .ToListAsync();

                if (indicesAlerta == null || !indicesAlerta.Any())
                {
                    Auditor.registroAuditor(
                        "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se encontraron Idices de Alerta para Finales", JsonConvert.SerializeObject(""), _usuariosContext
                    );
                    return NotFound();
                }

                var fechaActual = DateTime.Now;

                var finales = await _sciContext.Finales
                    .Include(fin => fin.TIPO_INCIDENTE)
                    .Where(fin => fin.ESTADO == 2)
                    .ToListAsync();

                var finalesConDiferencia = finales
                    .Select(fin => new
                    {
                        Final = fin,
                        DiferenciaHoras = (fechaActual - fin.FECHA_TRAMITE).TotalHours
                    })
                    .ToList();

                var finalesConAlertaAmarilla = finalesConDiferencia
                    .Where(fin => fin.DiferenciaHoras > indicesAlerta.First().ALERTA_AMARILLA!.Value
                        && fin.DiferenciaHoras < indicesAlerta.First().ALERTA_ROJA!.Value)
                    .ToList();

                var finalesConAlertaRoja = finalesConDiferencia
                    .Where(fin => fin.DiferenciaHoras > indicesAlerta.First().ALERTA_ROJA!.Value)
                    .ToList();

                int totalFinalesConAlertaAmarilla = finalesConAlertaAmarilla.Count;
                int totalFinalesConAlertaRoja = finalesConAlertaRoja.Count;

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {
                    finalesConAlertaAmarilla,
                    finalesConAlertaRoja,
                    totalFinalesConAlertaAmarilla,
                    totalFinalesConAlertaRoja
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


    }
}
// 28-09-2026 23:00
