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
    public class AmpliacionesInformesController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public AmpliacionesInformesController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista todas las Ampliaciones de Informes de Incidentes

        [HttpGet("[action]/{incidentesId}")]
        public async Task<ActionResult> GetListAmpliacionesInformesInc(
            [FromRoute] int incidentesId
            )
        {
            try
            {
                var respuesta = await _sciContext.AmpliacionesInformesInc
                .Where(i => (i.ESTADO == 1) && (i.INCIDENTESID == incidentesId))
                .OrderBy(i => i.FECHA_AMPLIACION)
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

        // Inserta Ampliaciones de Informes en Incidentes

        [HttpPost("[action]")]
        public async Task<ActionResult<ampliaciones_informes_inc>> PostAmpliacionesInformesInc(
            PostAmpliacionesInformesIncModel model
            )
        {
            try
            {
                ampliaciones_informes_inc nuevoAmpliacionesInformesInc = new ampliaciones_informes_inc
                {

                    INCIDENTESID = model.INCIDENTESID,
                    FECHA_AMPLIACION = model.FECHA_AMPLIACION,
                    DETALLE_AMPLIACION = model.DETALLE_AMPLIACION,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.AmpliacionesInformesInc.AddAsync(nuevoAmpliacionesInformesInc);

               // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe Incidentes
                        TIPO_INFORME_ID = model.INCIDENTESID,
                        TIPO_ANTECEDENTE = "AMP",           // Ampliacion Informes
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_AMPLIACION}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DETALLE_AMPLIACION,
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

                return Ok(nuevoAmpliacionesInformesInc);
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

        // Actualiza en Ampliaciones de Informes de Incidentes

        [HttpPut("[action]/{incidentesId},{ampliacionesInformesId}")]
        public async Task<ActionResult<ampliaciones_informes_inc>> PutAmpliacionesInformesInc(
            PutAmpliacionesInformesIncModel model, 
            [FromRoute] int incidentesId, 
            [FromRoute] int ampliacionesInformesId
            ) 
        {
            try
            {
                ampliaciones_informes_inc? ampliaciones_informes_inc = await _sciContext.AmpliacionesInformesInc
                .Where( i => ( i.INCIDENTESID == incidentesId ) && ( i.ID == ampliacionesInformesId ) )
                .FirstOrDefaultAsync();

                if ( ampliaciones_informes_inc == null ) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado la Ampliación Informe de Incidente para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                ampliaciones_informes_inc.FECHA_AMPLIACION = model.FECHA_AMPLIACION;
                ampliaciones_informes_inc.DETALLE_AMPLIACION = model.DETALLE_AMPLIACION;
                ampliaciones_informes_inc.USUARIO = int.Parse(User.GetClaimValue("Run"));
                ampliaciones_informes_inc.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(ampliaciones_informes_inc)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(ampliaciones_informes_inc.USUARIO) &&
                            p.Metadata.Name != nameof(ampliaciones_informes_inc.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "I",                 // Informe Incidentes
                            TIPO_INFORME_ID = incidentesId,
                            TIPO_ANTECEDENTE = "AMP",           // Ampliaciones Informes
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
                return Ok(ampliaciones_informes_inc);
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

        // Elimina en Ampliaciones de Informes de Incidentes

        [HttpDelete("[action]/{incidentesId},{ampliacionesInformesId}")]
        public async Task<ActionResult<ampliaciones_informes_inc>> DeleteAmpliacionesInformesInc(
            [FromRoute] int incidentesId, 
            [FromRoute] int ampliacionesInformesId
            ) 
        {
            try
            {
                ampliaciones_informes_inc? ampliaciones_informes_inc = await _sciContext.AmpliacionesInformesInc
                .Where( i => ( i.INCIDENTESID == incidentesId ) && ( i.ID == ampliacionesInformesId ) )
                .FirstOrDefaultAsync();

                if ( ampliaciones_informes_inc == null ) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado la Ampliación Informe de Incidente a Eliminar", JsonConvert.SerializeObject("Nro. Incidente: "+incidentesId+" Nro. Ampliación Informe: "+ampliacionesInformesId), _usuariosContext
                    );
                    return NotFound();
                }

                ampliaciones_informes_inc.USUARIO = int.Parse(User.GetClaimValue("Run"));
                ampliaciones_informes_inc.ACTUALIZACION = DateTime.Now;
                ampliaciones_informes_inc.ESTADO = 0;


              // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe Incidentes
                        TIPO_INFORME_ID = incidentesId,
                        TIPO_ANTECEDENTE = "AMP",           // Ampliaciones Informes
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {ampliaciones_informes_inc.FECHA_AMPLIACION}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = ampliaciones_informes_inc.DETALLE_AMPLIACION,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Incidente: "+incidentesId+" Nro. Ampliación Informe: "+ampliacionesInformesId), _usuariosContext
                );
                return Ok(ampliaciones_informes_inc);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Incidente: "+incidentesId+" Nro. Ampliación Informe: "+ampliacionesInformesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todas las Ampliaciones de Informes de Finales

        [HttpGet("[action]/{finalesId}")]
        public async Task<ActionResult> GetListAmpliacionesInformesFin(
            [FromRoute] int finalesId
            )
        {
            try
            {
                var respuesta = await _sciContext.AmpliacionesInformesFin
                .Where(i => (i.ESTADO == 1) && (i.FINALESID == finalesId))
                .OrderBy(i => i.FECHA_AMPLIACION)
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
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Finales: " + finalesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Inserta Ampliaciones de Informes en Finales

        [HttpPost("[action]")]
        public async Task<ActionResult<ampliaciones_informes_fin>> PostAmpliacionesInformesFin(
            PostAmpliacionesInformesFinModel model
            )
        {
            try
            {
                ampliaciones_informes_fin nuevoAmpliacionesInformesFin = new ampliaciones_informes_fin
                {

                    FINALESID = model.FINALESID,
                    FECHA_AMPLIACION = model.FECHA_AMPLIACION,
                    DETALLE_AMPLIACION = model.DETALLE_AMPLIACION,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.AmpliacionesInformesFin.AddAsync(nuevoAmpliacionesInformesFin);

               // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = model.FINALESID,
                        TIPO_ANTECEDENTE = "AMP",           // Ampliacion Informes
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_AMPLIACION}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DETALLE_AMPLIACION,
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

                return Ok(nuevoAmpliacionesInformesFin);
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

        // Actualiza en Ampliaciones de Informes de Finales

        [HttpPut("[action]/{finalesId},{ampliacionesInformesId}")]
        public async Task<ActionResult<ampliaciones_informes_fin>> PutAmpliacionesInformesFin(
            PutAmpliacionesInformesFinModel model, 
            [FromRoute] int finalesId, 
            [FromRoute] int ampliacionesInformesId
            ) 
        {
            try
            {
                ampliaciones_informes_fin? ampliaciones_informes_fin = await _sciContext.AmpliacionesInformesFin
                .Where( i => ( i.FINALESID == finalesId ) && ( i.ID == ampliacionesInformesId ) )
                .FirstOrDefaultAsync();

                if ( ampliaciones_informes_fin == null ) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado la Ampliación Informe de Finales para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                ampliaciones_informes_fin.FECHA_AMPLIACION = model.FECHA_AMPLIACION;
                ampliaciones_informes_fin.DETALLE_AMPLIACION = model.DETALLE_AMPLIACION;
                ampliaciones_informes_fin.USUARIO = int.Parse(User.GetClaimValue("Run"));
                ampliaciones_informes_fin.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(ampliaciones_informes_fin)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(ampliaciones_informes_fin.USUARIO) &&
                            p.Metadata.Name != nameof(ampliaciones_informes_fin.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "F",                 // Informe Finales
                            TIPO_INFORME_ID = finalesId,
                            TIPO_ANTECEDENTE = "AMP",           // Ampliaciones Informes
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
                return Ok(ampliaciones_informes_fin);
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

        // Elimina en Ampliaciones de Informes de Finales

        [HttpDelete("[action]/{finalesId},{ampliacionesInformesId}")]
        public async Task<ActionResult<ampliaciones_informes_fin>> DeleteAmpliacionesInformesFin(
            [FromRoute] int finalesId, 
            [FromRoute] int ampliacionesInformesId
            ) 
        {
            try
            {
                ampliaciones_informes_fin? ampliaciones_informes_fin = await _sciContext.AmpliacionesInformesFin
                .Where( i => ( i.FINALESID == finalesId ) && ( i.ID == ampliacionesInformesId ) )
                .FirstOrDefaultAsync();

                if ( ampliaciones_informes_fin == null ) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado la Ampliación Informe de Finales a eliminar", JsonConvert.SerializeObject("Nro. Final: "+finalesId+" Nro. Ampliación Informe: "+ampliacionesInformesId), _usuariosContext
                    );
                    return NotFound();
                }

                ampliaciones_informes_fin.USUARIO = int.Parse(User.GetClaimValue("Run"));
                ampliaciones_informes_fin.ACTUALIZACION = DateTime.Now;
                ampliaciones_informes_fin.ESTADO = 0;


              // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = finalesId,
                        TIPO_ANTECEDENTE = "AMP",           // Ampliaciones Informes
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {ampliaciones_informes_fin.FECHA_AMPLIACION}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = ampliaciones_informes_fin.DETALLE_AMPLIACION,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Final: "+finalesId+" Nro. Ampliación Informe: "+ampliacionesInformesId), _usuariosContext
                );
                return Ok(ampliaciones_informes_fin);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Final: "+finalesId+" Nro. Ampliación Informe: "+ampliacionesInformesId), _usuariosContext
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