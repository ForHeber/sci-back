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
    public class AntecedentesJudicialesController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public AntecedentesJudicialesController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista todos los Antecedentes Judiciales de Informe Preliminar

        [HttpGet("[action]/{preliminaresId}")]
        public async Task<ActionResult> GetListAntecedentesJudicialesPre(
            [FromRoute] int preliminaresId
            )
        {
            try
            {
                var respuesta = await _sciContext.AntecedentesJudicialesPre
                .Where(i => (i.ESTADO == 1) && (i.PRELIMINARESID == preliminaresId))
                .OrderBy(i => i.FECHA_ANTECEDENTES_JUD)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new { respuesta });
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Preliminar: " + preliminaresId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Inserta un Antecedente Judicial en Informe Preliminar

        [HttpPost("[action]")]
        public async Task<ActionResult<antecedentes_judiciales_pre>> PostAntecedentesJudicialesPre(
            PostAntecedentesJudicialesPreModel model
            )
        {
            try
            {
                antecedentes_judiciales_pre nuevoAntecedentesJudicialesPre = new antecedentes_judiciales_pre
                {

                    PRELIMINARESID = model.PRELIMINARESID,
                    FECHA_ANTECEDENTES_JUD = model.FECHA_ANTECEDENTES_JUD,
                    DETALLE_ANTECEDENTES_JUD = model.DETALLE_ANTECEDENTES_JUD,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.AntecedentesJudicialesPre.AddAsync(nuevoAntecedentesJudicialesPre);


                 // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso del involucrado.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "P",                 // Informe Preliminar
                        TIPO_INFORME_ID = model.PRELIMINARESID,
                        TIPO_ANTECEDENTE = "JUD",           // Antecedentes Judiciales
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_ANTECEDENTES_JUD}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DETALLE_ANTECEDENTES_JUD,
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

                return Ok(nuevoAntecedentesJudicialesPre);
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

        // Actualiza un Antecedente Judicial en Informe Preliminar

        [HttpPut("[action]/{preliminaresId},{antecedenteJudicialId}")]
        public async Task<ActionResult<antecedentes_judiciales_pre>> PutAntecedentesJudicialesPre(
            PutAntecedentesJudicialesPreModel model, 
            [FromRoute] int preliminaresId, 
            [FromRoute] int antecedenteJudicialId
            ) 
        {
            try
            {
                antecedentes_judiciales_pre? antecedentes_judiciales_pre = await _sciContext.AntecedentesJudicialesPre
                .Where( i => ( i.PRELIMINARESID == preliminaresId ) && ( i.ID == antecedenteJudicialId ) )
                .FirstOrDefaultAsync();

                if ( antecedentes_judiciales_pre == null ) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Antecedentes Judiciales del Informe Preliminar", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                antecedentes_judiciales_pre.FECHA_ANTECEDENTES_JUD = model.FECHA_ANTECEDENTES_JUD;
                antecedentes_judiciales_pre.DETALLE_ANTECEDENTES_JUD = model.DETALLE_ANTECEDENTES_JUD;
                antecedentes_judiciales_pre.USUARIO = int.Parse(User.GetClaimValue("Run"));
                antecedentes_judiciales_pre.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(antecedentes_judiciales_pre)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(antecedentes_judiciales_pre.USUARIO) &&
                            p.Metadata.Name != nameof(antecedentes_judiciales_pre.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "P",                 // Informe Preliminar
                            TIPO_INFORME_ID = preliminaresId,
                            TIPO_ANTECEDENTE = "JUD",           // Antecedentes Judiciales
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
                return Ok(antecedentes_judiciales_pre);
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

        // Elimina Virtualmente un Antecedente Judicial en Informe Preliminar

        [HttpDelete("[action]/{preliminaresId},{antecedenteJudicialId}")]
        public async Task<ActionResult<antecedentes_judiciales_pre>> DeleteAntecedentesJudicialesPre(
            [FromRoute] int preliminaresId, 
            [FromRoute] int antecedenteJudicialId
            ) 
        {
            try
            {
                antecedentes_judiciales_pre? antecedentes_judiciales_pre = await _sciContext.AntecedentesJudicialesPre
                .Where( i => ( i.PRELIMINARESID == preliminaresId ) && ( i.ID == antecedenteJudicialId ) )
                .FirstOrDefaultAsync();

                if ( antecedentes_judiciales_pre == null ) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Antecedente Judicial a eliminar", JsonConvert.SerializeObject("Nro. Preliminar: "+preliminaresId+" Nro. Antecedente Judicial: "+antecedenteJudicialId), _usuariosContext
                    );
                    return NotFound();
                }

                antecedentes_judiciales_pre.USUARIO = int.Parse(User.GetClaimValue("Run"));
                antecedentes_judiciales_pre.ACTUALIZACION = DateTime.Now;
                antecedentes_judiciales_pre.ESTADO = 0;


                // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "P",                 // Informe Preliminar
                        TIPO_INFORME_ID = preliminaresId,
                        TIPO_ANTECEDENTE = "JUD",           // Antecedentes Judiciales
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {antecedentes_judiciales_pre.FECHA_ANTECEDENTES_JUD}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = antecedentes_judiciales_pre.DETALLE_ANTECEDENTES_JUD,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Preliminar: "+preliminaresId+" Nro. Antecedente Judicial: "+antecedenteJudicialId), _usuariosContext
                );
                return Ok(antecedentes_judiciales_pre);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Preliminar: "+preliminaresId+" Nro. Antecedente Judicial: "+antecedenteJudicialId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todos los Antecedentes Judiciales de Informe Incidentes

        [HttpGet("[action]/{incidentesId}")]
        public async Task<ActionResult> GetListAntecedentesJudicialesInc(
            [FromRoute] int incidentesId
            )
        {
            try
            {
                var respuesta = await _sciContext.AntecedentesJudicialesInc
                .Where(i => (i.ESTADO == 1) && (i.INCIDENTESID == incidentesId))
                .OrderBy(i => i.FECHA_ANTECEDENTES_JUD)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new { respuesta });
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Incidentes: " + incidentesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Inserta un Antecedente Judicial en Informe Incidentes

        [HttpPost("[action]")]
        public async Task<ActionResult<antecedentes_judiciales_inc>> PostAntecedentesJudicialesInc(
            PostAntecedentesJudicialesIncModel model
            )
        {
            try
            {
                antecedentes_judiciales_inc nuevoAntecedentesJudicialesInc = new antecedentes_judiciales_inc
                {

                    INCIDENTESID = model.INCIDENTESID,
                    FECHA_ANTECEDENTES_JUD = model.FECHA_ANTECEDENTES_JUD,
                    DETALLE_ANTECEDENTES_JUD = model.DETALLE_ANTECEDENTES_JUD,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.AntecedentesJudicialesInc.AddAsync(nuevoAntecedentesJudicialesInc);


                 // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso del involucrado.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe Incidentes
                        TIPO_INFORME_ID = model.INCIDENTESID,
                        TIPO_ANTECEDENTE = "JUD",           // Antecedentes Judiciales
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_ANTECEDENTES_JUD}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DETALLE_ANTECEDENTES_JUD,
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

                return Ok(nuevoAntecedentesJudicialesInc);
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

        // Actualiza un Antecedente Judicial en Informe Incidentes

        [HttpPut("[action]/{incidentesId},{antecedenteJudicialId}")]
        public async Task<ActionResult<antecedentes_judiciales_inc>> PutAntecedentesJudicialesInc(
            PutAntecedentesJudicialesIncModel model, 
            [FromRoute] int incidentesId, 
            [FromRoute] int antecedenteJudicialId
            ) 
        {
            try
            {
                antecedentes_judiciales_inc? antecedentes_judiciales_inc = await _sciContext.AntecedentesJudicialesInc
                .Where( i => ( i.INCIDENTESID == incidentesId ) && ( i.ID == antecedenteJudicialId ) )
                .FirstOrDefaultAsync();

                if ( antecedentes_judiciales_inc == null ) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Antecedentes Judiciales del Informe Incidente", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                antecedentes_judiciales_inc.FECHA_ANTECEDENTES_JUD = model.FECHA_ANTECEDENTES_JUD;
                antecedentes_judiciales_inc.DETALLE_ANTECEDENTES_JUD = model.DETALLE_ANTECEDENTES_JUD;
                antecedentes_judiciales_inc.USUARIO = int.Parse(User.GetClaimValue("Run"));
                antecedentes_judiciales_inc.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(antecedentes_judiciales_inc)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(antecedentes_judiciales_inc.USUARIO) &&
                            p.Metadata.Name != nameof(antecedentes_judiciales_inc.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "I",                 // Informe Incidentes
                            TIPO_INFORME_ID = incidentesId,
                            TIPO_ANTECEDENTE = "JUD",           // Antecedentes Judiciales
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
                return Ok(antecedentes_judiciales_inc);
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

        // Elimina Virtualmente un Antecedente Judicial en Informe Incidentes

        [HttpDelete("[action]/{incidentesId},{antecedenteJudicialId}")]
        public async Task<ActionResult<antecedentes_judiciales_inc>> DeleteAntecedentesJudicialesInc(
            [FromRoute] int incidentesId, 
            [FromRoute] int antecedenteJudicialId
            ) 
        {
            try
            {
                antecedentes_judiciales_inc? antecedentes_judiciales_inc = await _sciContext.AntecedentesJudicialesInc
                .Where( i => ( i.INCIDENTESID == incidentesId ) && ( i.ID == antecedenteJudicialId ) )
                .FirstOrDefaultAsync();

                if ( antecedentes_judiciales_inc == null ) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Antecedente Judicial a eliminar", JsonConvert.SerializeObject("Nro. Incidentes: "+incidentesId+" Nro. Antecedente Judicial: "+antecedenteJudicialId), _usuariosContext
                    );
                    return NotFound();
                }

                antecedentes_judiciales_inc.USUARIO = int.Parse(User.GetClaimValue("Run"));
                antecedentes_judiciales_inc.ACTUALIZACION = DateTime.Now;
                antecedentes_judiciales_inc.ESTADO = 0;


               // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe Incidente
                        TIPO_INFORME_ID = incidentesId,
                        TIPO_ANTECEDENTE = "JUD",           // Antecedentes Judiciales
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {antecedentes_judiciales_inc.FECHA_ANTECEDENTES_JUD}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = antecedentes_judiciales_inc.DETALLE_ANTECEDENTES_JUD,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Incidentes: "+incidentesId+" Nro. Antecedente Judicial: "+antecedenteJudicialId), _usuariosContext
                );
                return Ok(antecedentes_judiciales_inc);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Incidentes: "+incidentesId+" Nro. Antecedente Judicial: "+antecedenteJudicialId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todos los Antecedentes Judiciales de Informe Finales

        [HttpGet("[action]/{finalesId}")]
        public async Task<ActionResult> GetListAntecedentesJudicialesFin(
            [FromRoute] int finalesId
            )
        {
            try
            {
                var respuesta = await _sciContext.AntecedentesJudicialesFin
                .Where(i => (i.ESTADO == 1) && (i.FINALESID == finalesId))
                .OrderBy(i => i.FECHA_ANTECEDENTES_JUD)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
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

        // Inserta un Antecedente Judicial en Informe Finales

        [HttpPost("[action]")]
        public async Task<ActionResult<antecedentes_judiciales_fin>> PostAntecedentesJudicialesFin(
            PostAntecedentesJudicialesFinModel model
            )
        {
            try
            {
                antecedentes_judiciales_fin nuevoAntecedentesJudicialesFin = new antecedentes_judiciales_fin
                {

                    FINALESID = model.FINALESID,
                    FECHA_ANTECEDENTES_JUD = model.FECHA_ANTECEDENTES_JUD,
                    DETALLE_ANTECEDENTES_JUD = model.DETALLE_ANTECEDENTES_JUD,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.AntecedentesJudicialesFin.AddAsync(nuevoAntecedentesJudicialesFin);


                // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso del involucrado.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = model.FINALESID,
                        TIPO_ANTECEDENTE = "JUD",           // Antecedentes Judiciales
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_ANTECEDENTES_JUD}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DETALLE_ANTECEDENTES_JUD,
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

                return Ok(nuevoAntecedentesJudicialesFin);
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

        // Actualiza un Antecedente Judicial en Informe Finales

        [HttpPut("[action]/{finalesId},{antecedenteJudicialId}")]
        public async Task<ActionResult<antecedentes_judiciales_fin>> PutAntecedentesJudicialesFin(
            PutAntecedentesJudicialesFinModel model, 
            [FromRoute] int finalesId, 
            [FromRoute] int antecedenteJudicialId
            ) 
        {
            try
            {
                antecedentes_judiciales_fin? antecedentes_judiciales_fin = await _sciContext.AntecedentesJudicialesFin
                .Where( i => ( i.FINALESID == finalesId ) && ( i.ID == antecedenteJudicialId ) )
                .FirstOrDefaultAsync();

                if ( antecedentes_judiciales_fin == null ) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Antecedentes Judiciales del Informe Final", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                antecedentes_judiciales_fin.FECHA_ANTECEDENTES_JUD = model.FECHA_ANTECEDENTES_JUD;
                antecedentes_judiciales_fin.DETALLE_ANTECEDENTES_JUD = model.DETALLE_ANTECEDENTES_JUD;
                antecedentes_judiciales_fin.USUARIO = int.Parse(User.GetClaimValue("Run"));
                antecedentes_judiciales_fin.ACTUALIZACION = DateTime.Now;


               // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(antecedentes_judiciales_fin)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(antecedentes_judiciales_fin.USUARIO) &&
                            p.Metadata.Name != nameof(antecedentes_judiciales_fin.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "F",                 // Informe Finales
                            TIPO_INFORME_ID = finalesId,
                            TIPO_ANTECEDENTE = "JUD",           // Antecedentes Judiciales
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
                return Ok(antecedentes_judiciales_fin);
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

        // Elimina Virtualmente un Antecedente Judicial en Informe Final

        [HttpDelete("[action]/{finalesId},{antecedenteJudicialId}")]
        public async Task<ActionResult<antecedentes_judiciales_fin>> DeleteAntecedentesJudicialesFin(
            [FromRoute] int finalesId, 
            [FromRoute] int antecedenteJudicialId
            ) 
        {
            try
            {
                antecedentes_judiciales_fin? antecedentes_judiciales_fin = await _sciContext.AntecedentesJudicialesFin
                .Where( i => ( i.FINALESID == finalesId ) && ( i.ID == antecedenteJudicialId ) )
                .FirstOrDefaultAsync();

                if ( antecedentes_judiciales_fin == null ) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Antecedente Judicial a eliminar", JsonConvert.SerializeObject("Nro. Finales: "+finalesId+" Nro. Antecedente Judicial: "+antecedenteJudicialId), _usuariosContext
                    );
                    return NotFound();
                }

                antecedentes_judiciales_fin.USUARIO = int.Parse(User.GetClaimValue("Run"));
                antecedentes_judiciales_fin.ACTUALIZACION = DateTime.Now;
                antecedentes_judiciales_fin.ESTADO = 0;


               // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = finalesId,
                        TIPO_ANTECEDENTE = "JUD",           // Antecedentes Judiciales
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {antecedentes_judiciales_fin.FECHA_ANTECEDENTES_JUD}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = antecedentes_judiciales_fin.DETALLE_ANTECEDENTES_JUD,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Finales: "+finalesId+" Nro. Antecedente Judicial: "+antecedenteJudicialId), _usuariosContext
                );
                return Ok(antecedentes_judiciales_fin);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Finales: "+finalesId+" Nro. Antecedente Judicial: "+antecedenteJudicialId), _usuariosContext
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