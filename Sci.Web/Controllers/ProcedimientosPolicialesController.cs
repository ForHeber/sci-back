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
    public class ProcedimientosPolicialesController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public ProcedimientosPolicialesController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista todos los Procedimientos Policiales de Preliminares

        [HttpGet("[action]/{preliminaresId}")]
        public async Task<ActionResult> GetListProcedimientosPolicialesPre(
            [FromRoute] int preliminaresId
            )
        {
            try
            {
                var respuesta = await _sciContext.ProcedimientosPolicialesPre
                .Include(i => i.CONDICION_POLICIAL)
                .Where(i => (i.ESTADO == 1) && (i.PRELIMINARESID == preliminaresId))
                .OrderBy(i => i.FECHA_PROCEDIMIENTO_POL)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Preliminares: " + preliminaresId), _usuariosContext
                );

                return Ok(new { respuesta });
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Preliminares: " + preliminaresId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Inserta un Procedimiento Policial en Preliminares

        [HttpPost("[action]")]
        public async Task<ActionResult<procedimientos_policiales_pre>> PostProcedimientosPolicialesPre(
            PostProcedimientosPolicialesPreModel model
            )
        {
            try
            {
                procedimientos_policiales_pre nuevoProcedimientosPolicialesPre = new procedimientos_policiales_pre
                {

                    PRELIMINARESID = model.PRELIMINARESID,
                    FECHA_PROCEDIMIENTO_POL = model.FECHA_PROCEDIMIENTO_POL,
                    DETALLE_PROCEDIMIENTO_POL = model.DETALLE_PROCEDIMIENTO_POL,
                    CONDICION_POLICIALID = model.CONDICION_POLICIALID,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.ProcedimientosPolicialesPre.AddAsync(nuevoProcedimientosPolicialesPre);


               // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "P",                 // Informe Preliminar
                        TIPO_INFORME_ID = model.PRELIMINARESID,
                        TIPO_ANTECEDENTE = "POL",           // Antecedentes Policiales
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_PROCEDIMIENTO_POL}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DETALLE_PROCEDIMIENTO_POL,
                        PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                        CREACION = DateTime.Now,
                        USUARIO = int.Parse(User.GetClaimValue("Run")),
                        ACTUALIZACION = DateTime.Now,
                        ESTADO = 1
                    };
                    await _sciContext.RegistroEventos.AddAsync(evento);
                }


                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(nuevoProcedimientosPolicialesPre);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                    "Post", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
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

        // Actualiza un Procedimiento Policial en Preliminares

        [HttpPut("[action]/{preliminaresId},{procedimientoPolicialId}")]
        public async Task<ActionResult<procedimientos_policiales_pre>> PutProcedimientosPolicialesPre(
            PutProcedimientosPolicialesPreModel model, 
            [FromRoute] int preliminaresId, 
            [FromRoute] int procedimientoPolicialId)
        {
            try
            {
                procedimientos_policiales_pre? procedimientos_policiales_pre = await _sciContext.ProcedimientosPolicialesPre
                .Where(i => (i.PRELIMINARESID == preliminaresId) && (i.ID == procedimientoPolicialId))
                .FirstOrDefaultAsync();

                if (procedimientos_policiales_pre == null)
                {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado Procedimiento Policial del Informe Preliminar", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                procedimientos_policiales_pre.FECHA_PROCEDIMIENTO_POL = model.FECHA_PROCEDIMIENTO_POL;
                procedimientos_policiales_pre.DETALLE_PROCEDIMIENTO_POL = model.DETALLE_PROCEDIMIENTO_POL;
                procedimientos_policiales_pre.CONDICION_POLICIALID = model.CONDICION_POLICIALID;

                procedimientos_policiales_pre.USUARIO = int.Parse(User.GetClaimValue("Run"));
                procedimientos_policiales_pre.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(procedimientos_policiales_pre)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(procedimientos_policiales_pre.USUARIO) &&
                            p.Metadata.Name != nameof(procedimientos_policiales_pre.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "P",                 // Informe Preliminar
                            TIPO_INFORME_ID = preliminaresId,
                            TIPO_ANTECEDENTE = "POL",           // Antecedentes Policiales
                            TIPO_EVENTO = "M",                  // Evento de Modificación
                            MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                            MODIFICADO_FECHA = DateTime.Now,
                            CONCEPTO = p.Metadata.Name,
                            VALOR_ANTERIOR = JsonConvert.SerializeObject(p.OriginalValue),
                            VALOR_NUEVO = JsonConvert.SerializeObject(p.CurrentValue),
                            PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                            CREACION = DateTime.Now,
                            USUARIO = int.Parse(User.GetClaimValue("Run")),
                            ACTUALIZACION = DateTime.Now,
                            ESTADO = 1
                        })
                        .ToList();

                    // Guarda los eventos de registro en la base de datos si hay alguno.
                    if (eventos.Count > 0)
                    {
                        _sciContext.RegistroEventos.AddRange(eventos);
                    }
                }


                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );
                return Ok(procedimientos_policiales_pre);
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

        // Elimina Virtualmente un Procedimiento Policial en Preliminares

        [HttpDelete("[action]/{preliminaresId},{procedimientoPolicialId}")]
        public async Task<ActionResult<procedimientos_policiales_pre>> DeleteProcedimientosPolicialesPre(
            [FromRoute] int preliminaresId, 
            [FromRoute] int procedimientoPolicialId
            )
        {
            try
            {
                procedimientos_policiales_pre? procedimientos_policiales_pre = await _sciContext.ProcedimientosPolicialesPre
                .Where(i => (i.PRELIMINARESID == preliminaresId) && (i.ID == procedimientoPolicialId))
                .FirstOrDefaultAsync();

                if (procedimientos_policiales_pre == null)
                {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Procedimiento Policial a eliminar", JsonConvert.SerializeObject("Nro. Preliminar: " + preliminaresId + " Nro. Procedimiento Policial: " + procedimientoPolicialId), _usuariosContext
                    );
                    return NotFound();
                }

                procedimientos_policiales_pre.USUARIO = int.Parse(User.GetClaimValue("Run"));
                procedimientos_policiales_pre.ACTUALIZACION = DateTime.Now;
                procedimientos_policiales_pre.ESTADO = 0;


               // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "P",                 // Informe Preliminar
                        TIPO_INFORME_ID = preliminaresId,
                        TIPO_ANTECEDENTE = "POL",           // Antecedentes Policiales
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {procedimientos_policiales_pre.FECHA_PROCEDIMIENTO_POL}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = procedimientos_policiales_pre.DETALLE_PROCEDIMIENTO_POL,
                        PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                        CREACION = DateTime.Now,
                        USUARIO = int.Parse(User.GetClaimValue("Run")),
                        ACTUALIZACION = DateTime.Now,
                        ESTADO = 1
                    };
                    await _sciContext.RegistroEventos.AddAsync(evento);
                }                


                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Delete", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Preliminar: " + preliminaresId + " Nro. Procedimiento Policial: " + procedimientoPolicialId), _usuariosContext
                );
                return Ok(procedimientos_policiales_pre);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Preliminar: " + preliminaresId + " Nro. Procedimiento Policial: " + procedimientoPolicialId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todos los Procedimientos Policiales de Incidentes

        [HttpGet("[action]/{incidentesId}")]
        public async Task<ActionResult> GetListProcedimientosPolicialesInc(
            [FromRoute] int incidentesId
            )
        {
            try
            {
                var respuesta = await _sciContext.ProcedimientosPolicialesInc
                .Include(i => i.CONDICION_POLICIAL)
                .Where(i => (i.ESTADO == 1) && (i.INCIDENTESID == incidentesId))
                .OrderBy(i => i.FECHA_PROCEDIMIENTO_POL)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Incidente: " + incidentesId), _usuariosContext
                );

                return Ok(new { respuesta });
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Incidente: " + incidentesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Inserta un Procedimiento Policial en Incidentes

        [HttpPost("[action]")]
        public async Task<ActionResult<procedimientos_policiales_inc>> PostProcedimientosPolicialesInc(
            PostProcedimientosPolicialesIncModel model
            )
        {
            try
            {
                procedimientos_policiales_inc nuevoProcedimientosPolicialesInc = new procedimientos_policiales_inc
                {

                    INCIDENTESID = model.INCIDENTESID,
                    FECHA_PROCEDIMIENTO_POL = model.FECHA_PROCEDIMIENTO_POL,
                    DETALLE_PROCEDIMIENTO_POL = model.DETALLE_PROCEDIMIENTO_POL,
                    CONDICION_POLICIALID = model.CONDICION_POLICIALID,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.ProcedimientosPolicialesInc.AddAsync(nuevoProcedimientosPolicialesInc);


              // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe Incidentes
                        TIPO_INFORME_ID = model.INCIDENTESID,
                        TIPO_ANTECEDENTE = "POL",           // Antecedentes Policiales
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_PROCEDIMIENTO_POL}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DETALLE_PROCEDIMIENTO_POL,
                        PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                        CREACION = DateTime.Now,
                        USUARIO = int.Parse(User.GetClaimValue("Run")),
                        ACTUALIZACION = DateTime.Now,
                        ESTADO = 1
                    };
                    await _sciContext.RegistroEventos.AddAsync(evento);
                }


                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(nuevoProcedimientosPolicialesInc);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                    "Post", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
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

        // Actualiza un Procedimiento Policial en Incidentes

        [HttpPut("[action]/{incidentesId},{procedimientoPolicialId}")]
        public async Task<ActionResult<procedimientos_policiales_inc>> PutProcedimientosPolicialesInc(
            PutProcedimientosPolicialesIncModel model, 
            [FromRoute] int incidentesId, 
            [FromRoute] int procedimientoPolicialId)
        {
            try
            {
                procedimientos_policiales_inc? procedimientos_policiales_inc = await _sciContext.ProcedimientosPolicialesInc
                .Where(i => (i.INCIDENTESID == incidentesId) && (i.ID == procedimientoPolicialId))
                .FirstOrDefaultAsync();

                if (procedimientos_policiales_inc == null)
                {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado Procedimiento Policial del Informe Incidentes", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                procedimientos_policiales_inc.FECHA_PROCEDIMIENTO_POL = model.FECHA_PROCEDIMIENTO_POL;
                procedimientos_policiales_inc.DETALLE_PROCEDIMIENTO_POL = model.DETALLE_PROCEDIMIENTO_POL;
                procedimientos_policiales_inc.CONDICION_POLICIALID = model.CONDICION_POLICIALID;

                procedimientos_policiales_inc.USUARIO = int.Parse(User.GetClaimValue("Run"));
                procedimientos_policiales_inc.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(procedimientos_policiales_inc)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(procedimientos_policiales_inc.USUARIO) &&
                            p.Metadata.Name != nameof(procedimientos_policiales_inc.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "I",                 // Informe Incidente
                            TIPO_INFORME_ID = incidentesId,
                            TIPO_ANTECEDENTE = "POL",           // Antecedentes Policiales
                            TIPO_EVENTO = "M",                  // Evento de Modificación
                            MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                            MODIFICADO_FECHA = DateTime.Now,
                            CONCEPTO = p.Metadata.Name,
                            VALOR_ANTERIOR = JsonConvert.SerializeObject(p.OriginalValue),
                            VALOR_NUEVO = JsonConvert.SerializeObject(p.CurrentValue),
                            PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                            CREACION = DateTime.Now,
                            USUARIO = int.Parse(User.GetClaimValue("Run")),
                            ACTUALIZACION = DateTime.Now,
                            ESTADO = 1
                        })
                        .ToList();

                    // Guarda los eventos de registro en la base de datos si hay alguno.
                    if (eventos.Count > 0)
                    {
                        _sciContext.RegistroEventos.AddRange(eventos);
                    }
                }


                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );
                return Ok(procedimientos_policiales_inc);
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

        // Elimina Virtualmente un Procedimiento Policial en Incidentes

        [HttpDelete("[action]/{incidentesId},{procedimientoPolicialId}")]
        public async Task<ActionResult<procedimientos_policiales_inc>> DeleteProcedimientosPolicialesInc(
            [FromRoute] int incidentesId, 
            [FromRoute] int procedimientoPolicialId
            )
        {
            try
            {
                procedimientos_policiales_inc? procedimientos_policiales_inc = await _sciContext.ProcedimientosPolicialesInc
                .Where(i => (i.INCIDENTESID == incidentesId) && (i.ID == procedimientoPolicialId))
                .FirstOrDefaultAsync();

                if (procedimientos_policiales_inc == null)
                {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Procedimiento Policial a eliminar", JsonConvert.SerializeObject("Nro. Incidente: " + incidentesId + " Nro. Procedimiento Policial: " + procedimientoPolicialId), _usuariosContext
                    );
                    return NotFound();
                }

                procedimientos_policiales_inc.USUARIO = int.Parse(User.GetClaimValue("Run"));
                procedimientos_policiales_inc.ACTUALIZACION = DateTime.Now;
                procedimientos_policiales_inc.ESTADO = 0;


              // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe Incidentes
                        TIPO_INFORME_ID = incidentesId,
                        TIPO_ANTECEDENTE = "POL",           // Antecedentes Policiales
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {procedimientos_policiales_inc.FECHA_PROCEDIMIENTO_POL}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = procedimientos_policiales_inc.DETALLE_PROCEDIMIENTO_POL,
                        PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                        CREACION = DateTime.Now,
                        USUARIO = int.Parse(User.GetClaimValue("Run")),
                        ACTUALIZACION = DateTime.Now,
                        ESTADO = 1
                    };
                    await _sciContext.RegistroEventos.AddAsync(evento);
                }                


                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Delete", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Incidente: " + incidentesId + " Nro. Procedimiento Policial: " + procedimientoPolicialId), _usuariosContext
                );
                return Ok(procedimientos_policiales_inc);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Incidente: " + incidentesId + " Nro. Procedimiento Policial: " + procedimientoPolicialId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todos los Procedimientos Policiales de Finales

        [HttpGet("[action]/{finalesId}")]
        public async Task<ActionResult> GetListProcedimientosPolicialesFin(
            [FromRoute] int finalesId
            )
        {
            try
            {
                var respuesta = await _sciContext.ProcedimientosPolicialesFin
                .Include(i => i.CONDICION_POLICIAL)
                .Where(i => (i.ESTADO == 1) && (i.FINALESID == finalesId))
                .OrderBy(i => i.FECHA_PROCEDIMIENTO_POL)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Final: " + finalesId), _usuariosContext
                );

                return Ok(new { respuesta });
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Final: " + finalesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Inserta un Procedimiento Policial en Finales

        [HttpPost("[action]")]
        public async Task<ActionResult<procedimientos_policiales_fin>> PostProcedimientosPolicialesFin(
            PostProcedimientosPolicialesFinModel model
            )
        {
            try
            {
                procedimientos_policiales_fin nuevoProcedimientosPolicialesFin = new procedimientos_policiales_fin
                {

                    FINALESID = model.FINALESID,
                    FECHA_PROCEDIMIENTO_POL = model.FECHA_PROCEDIMIENTO_POL,
                    DETALLE_PROCEDIMIENTO_POL = model.DETALLE_PROCEDIMIENTO_POL,
                    CONDICION_POLICIALID = model.CONDICION_POLICIALID,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.ProcedimientosPolicialesFin.AddAsync(nuevoProcedimientosPolicialesFin);


            // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = model.FINALESID,
                        TIPO_ANTECEDENTE = "POL",           // Antecedentes Policiales
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_PROCEDIMIENTO_POL}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DETALLE_PROCEDIMIENTO_POL,
                        PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                        CREACION = DateTime.Now,
                        USUARIO = int.Parse(User.GetClaimValue("Run")),
                        ACTUALIZACION = DateTime.Now,
                        ESTADO = 1
                    };
                    await _sciContext.RegistroEventos.AddAsync(evento);
                }


                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(nuevoProcedimientosPolicialesFin);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                    "Post", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
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

        // Actualiza un Procedimiento Policial en Finales

        [HttpPut("[action]/{finalesId},{procedimientoPolicialId}")]
        public async Task<ActionResult<procedimientos_policiales_fin>> PutProcedimientosPolicialesFin(
            PutProcedimientosPolicialesFinModel model, 
            [FromRoute] int finalesId, 
            [FromRoute] int procedimientoPolicialId)
        {
            try
            {
                procedimientos_policiales_fin? procedimientos_policiales_fin = await _sciContext.ProcedimientosPolicialesFin
                .Where(i => (i.FINALESID == finalesId) && (i.ID == procedimientoPolicialId))
                .FirstOrDefaultAsync();

                if (procedimientos_policiales_fin == null)
                {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado Procedimiento Policial del Informe Final", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                procedimientos_policiales_fin.FECHA_PROCEDIMIENTO_POL = model.FECHA_PROCEDIMIENTO_POL;
                procedimientos_policiales_fin.DETALLE_PROCEDIMIENTO_POL = model.DETALLE_PROCEDIMIENTO_POL;
                procedimientos_policiales_fin.CONDICION_POLICIALID = model.CONDICION_POLICIALID;

                procedimientos_policiales_fin.USUARIO = int.Parse(User.GetClaimValue("Run"));
                procedimientos_policiales_fin.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(procedimientos_policiales_fin)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(procedimientos_policiales_fin.USUARIO) &&
                            p.Metadata.Name != nameof(procedimientos_policiales_fin.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "F",                 // Informe Finales
                            TIPO_INFORME_ID = finalesId,
                            TIPO_ANTECEDENTE = "POL",           // Antecedentes Policiales
                            TIPO_EVENTO = "M",                  // Evento de Modificación
                            MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                            MODIFICADO_FECHA = DateTime.Now,
                            CONCEPTO = p.Metadata.Name,
                            VALOR_ANTERIOR = JsonConvert.SerializeObject(p.OriginalValue),
                            VALOR_NUEVO = JsonConvert.SerializeObject(p.CurrentValue),
                            PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                            CREACION = DateTime.Now,
                            USUARIO = int.Parse(User.GetClaimValue("Run")),
                            ACTUALIZACION = DateTime.Now,
                            ESTADO = 1
                        })
                        .ToList();

                    // Guarda los eventos de registro en la base de datos si hay alguno.
                    if (eventos.Count > 0)
                    {
                        _sciContext.RegistroEventos.AddRange(eventos);
                    }
                }


                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );
                return Ok(procedimientos_policiales_fin);
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

        // Elimina Virtualmente un Procedimiento Policial en Finales

        [HttpDelete("[action]/{finalesId},{procedimientoPolicialId}")]
        public async Task<ActionResult<procedimientos_policiales_fin>> DeleteProcedimientosPolicialesFin(
            [FromRoute] int finalesId, 
            [FromRoute] int procedimientoPolicialId
            )
        {
            try
            {
                procedimientos_policiales_fin? procedimientos_policiales_fin = await _sciContext.ProcedimientosPolicialesFin
                .Where(i => (i.FINALESID == finalesId) && (i.ID == procedimientoPolicialId))
                .FirstOrDefaultAsync();

                if (procedimientos_policiales_fin == null)
                {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Procedimiento Policial a eliminar", JsonConvert.SerializeObject("Nro. Final: " + finalesId + " Nro. Procedimiento Policial: " + procedimientoPolicialId), _usuariosContext
                    );
                    return NotFound();
                }

                procedimientos_policiales_fin.USUARIO = int.Parse(User.GetClaimValue("Run"));
                procedimientos_policiales_fin.ACTUALIZACION = DateTime.Now;
                procedimientos_policiales_fin.ESTADO = 0;


             // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = finalesId,
                        TIPO_ANTECEDENTE = "POL",           // Antecedentes Policiales
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {procedimientos_policiales_fin.FECHA_PROCEDIMIENTO_POL}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = procedimientos_policiales_fin.DETALLE_PROCEDIMIENTO_POL,
                        PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                        CREACION = DateTime.Now,
                        USUARIO = int.Parse(User.GetClaimValue("Run")),
                        ACTUALIZACION = DateTime.Now,
                        ESTADO = 1
                    };
                    await _sciContext.RegistroEventos.AddAsync(evento);
                }             


                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Delete", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Final: " + finalesId + " Nro. Procedimiento Policial: " + procedimientoPolicialId), _usuariosContext
                );
                return Ok(procedimientos_policiales_fin);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Final: " + finalesId + " Nro. Procedimiento Policial: " + procedimientoPolicialId), _usuariosContext
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