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
    public class InvolucradosController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public InvolucradosController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista todos los Involucrados de Preliminares

        [HttpGet("[action]/{preliminaresId}")]
        public async Task<ActionResult<List<involucrados_pre>>> GetListInvolucradosPre(
            [FromRoute] int preliminaresId
            ) 
        {
            try
            {
                var respuesta = await _sciContext.InvolucradosPre
                .Include(i => i.PRELIMINARES)
                .Include(i => i.TIPO_INTERVINIENTE)
                .Include(i => i.TIPO_CONSUMO)
                .Include(i => i.ESTADO_SALUD)
                .Include(i => i.R_INVOLUCRADO)
                .Include(i => i.U_INVOLUCRADO)
                .Where( i => ( i.ESTADO == 1 ) && ( i.PRELIMINARESID == preliminaresId ) )
                .OrderBy(i => i.RUT_INVOLUCRADO )
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Preliminar: "+preliminaresId), _usuariosContext
                );

                return Ok(new { respuesta });
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Preliminar: "+preliminaresId), _usuariosContext
                );
                
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Inserta un Involucrado en Preliminares

        [HttpPost("[action]")]
        public async Task<ActionResult<involucrados_pre>> PostInvolucradosPre(
            PostInvolucradosPreModel model
            ) 
        {
            try
            {
                // Ingresa Involucrado 

                involucrados_pre nuevoInvolucradosPre = new involucrados_pre {

                    PRELIMINARESID = model.PRELIMINARESID,
                    TIPO_INVOLUCRADO = model.TIPO_INVOLUCRADO,
                    RUT_INVOLUCRADO = model.RUT_INVOLUCRADO,
                    UNIDAD_INVOLUCRADO = model.UNIDAD_INVOLUCRADO,
                    FECHA_NACIMIENTO = model.FECHA_NACIMIENTO,
                    TIPO_INTERVINIENTEID = model.TIPO_INTERVINIENTEID,
                    TIPO_CONSUMOID = model.TIPO_CONSUMOID,
                    ESTADO_SALUDID = model.ESTADO_SALUDID,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };
                await _sciContext.InvolucradosPre.AddAsync(nuevoInvolucradosPre);


                 // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "P",                 // Informe Preliminar
                        TIPO_INFORME_ID = model.PRELIMINARESID,
                        TIPO_ANTECEDENTE = "INV",           // Antecedentes del Involucrado
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.RUT_INVOLUCRADO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = $"{model.RUT_INVOLUCRADO}-{model.NOMBRES}-{model.APELLIDO_PATERNO}-{model.APELLIDO_MATERNO}",
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

                // Verifica existencia en PERSONAS_CIVILES del Involucrado si este es "C", si no esta se Ingresa de lo contrario se Actualizan sus datos

                if ( string.Equals(model.TIPO_INVOLUCRADO, "C", StringComparison.Ordinal) ) {

                    personas_civiles? personas_civiles = await _sciContext.PersonasCiviles
                    .Where( i => i.RUN == model.RUT_INVOLUCRADO )
                    .FirstOrDefaultAsync();

                    if ( personas_civiles == null ) {

                        personas_civiles nuevoPersonasCiviles = new personas_civiles {

                        RUN = model.RUT_INVOLUCRADO,
                        GRADO = model.GRADO,
                        NOMBRES = model.NOMBRES,
                        APELLIDO_PATERNO = model.APELLIDO_PATERNO,
                        APELLIDO_MATERNO = model.APELLIDO_MATERNO,
                        FONO = model.FONO,
                        CORREOINTER = model.CORREOINTER,

                        PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                        CREACION = DateTime.Now,
                        USUARIO = int.Parse(User.GetClaimValue("Run")),
                        ACTUALIZACION = DateTime.Now,
                        ESTADO = 1

                        };

                        await _sciContext.PersonasCiviles.AddAsync(nuevoPersonasCiviles);
                        await _sciContext.SaveChangesAsync();

                        Auditor.registroAuditor(
                            "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                            Util.GetActionName(ControllerContext), "Ok Ingreso Personas Civiles", JsonConvert.SerializeObject(model), _usuariosContext
                        );

                    } else {

                        personas_civiles.GRADO = model.GRADO;
                        personas_civiles.NOMBRES = model.NOMBRES;
                        personas_civiles.APELLIDO_PATERNO = model.APELLIDO_PATERNO;
                        personas_civiles.APELLIDO_MATERNO = model.APELLIDO_MATERNO;
                        personas_civiles.FONO = model.FONO;
                        personas_civiles.CORREOINTER = model.CORREOINTER;

                        personas_civiles.USUARIO = int.Parse(User.GetClaimValue("Run"));
                        personas_civiles.ACTUALIZACION = DateTime.Now;
                        await _sciContext.SaveChangesAsync();

                        Auditor.registroAuditor(
                            "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                            Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                        );

                    };
                };

                return Ok(nuevoInvolucradosPre);
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

        // Elimina Virtualmente un Involucrado en Preliminares

        [HttpDelete("[action]/{preliminaresId},{involucradosId}")]
        public async Task<ActionResult<involucrados_pre>> DeleteInvolucradosPre(
            [FromRoute] int preliminaresId, 
            [FromRoute] int involucradosId
            ) 
        {
            try
            {
                involucrados_pre? involucrados_pre = await _sciContext.InvolucradosPre
                .Include(i => i.R_INVOLUCRADO)
                .Where( i => ( i.PRELIMINARESID == preliminaresId ) && ( i.ID == involucradosId ) )
                .FirstOrDefaultAsync();

                if ( involucrados_pre == null ) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Involucrado a eliminar", JsonConvert.SerializeObject("Nro. Preliminar: "+preliminaresId+" Nro. Involucrado: "+involucradosId), _usuariosContext
                    );
                    return NotFound();
                }

                involucrados_pre.USUARIO = int.Parse(User.GetClaimValue("Run"));
                involucrados_pre.ACTUALIZACION = DateTime.Now;
                involucrados_pre.ESTADO = 0;


                // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "P",                 // Informe Preliminar
                        TIPO_INFORME_ID = preliminaresId,
                        TIPO_ANTECEDENTE = "INV",           // Antecedentes del Involucrado
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {involucrados_pre.RUT_INVOLUCRADO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = $"{involucrados_pre.RUT_INVOLUCRADO}-{involucrados_pre.R_INVOLUCRADO?.NOMBRES}-{involucrados_pre.R_INVOLUCRADO?.APELLIDO_PATERNO}-{involucrados_pre.R_INVOLUCRADO?.APELLIDO_MATERNO}",
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Preliminar: "+preliminaresId+" Nro. Involucrado: "+involucradosId), _usuariosContext
                );
                return Ok(involucrados_pre);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Preliminar: "+preliminaresId+" Nro. Involucrado: "+involucradosId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todos los Involucrados de Incidentes

        [HttpGet("[action]/{incidentesId}")]
        public async Task<ActionResult<List<involucrados_inc>>> GetListInvolucradosInc(
            [FromRoute] int incidentesId
            ) 
        {
            try
            {
                var respuesta = await _sciContext.InvolucradosInc
                .Include(i => i.INCIDENTES)
                .Include(i => i.TIPO_INTERVINIENTE)
                .Include(i => i.TIPO_CONSUMO)
                .Include(i => i.ESTADO_SALUD)
                .Include(i => i.R_INVOLUCRADO)
                .Include(i => i.U_INVOLUCRADO)
                .Where( i => ( i.ESTADO == 1 ) && ( i.INCIDENTESID == incidentesId ) )
                .OrderBy(i => i.RUT_INVOLUCRADO )
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Incidente: "+incidentesId), _usuariosContext
                );

                return Ok(new { respuesta });
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Incidente: "+incidentesId), _usuariosContext
                );
                
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Inserta un Involucrado en Incidentes

        [HttpPost("[action]")]
        public async Task<ActionResult<involucrados_inc>> PostInvolucradosInc(
            PostInvolucradosIncModel model
            ) 
        {
            try
            {
                // Ingresa Involucrado 

                involucrados_inc nuevoInvolucradosInc = new involucrados_inc {

                    INCIDENTESID = model.INCIDENTESID,
                    TIPO_INVOLUCRADO = model.TIPO_INVOLUCRADO,
                    RUT_INVOLUCRADO = model.RUT_INVOLUCRADO,
                    UNIDAD_INVOLUCRADO = model.UNIDAD_INVOLUCRADO,
                    FECHA_NACIMIENTO = model.FECHA_NACIMIENTO,
                    TIPO_INTERVINIENTEID = model.TIPO_INTERVINIENTEID,
                    TIPO_CONSUMOID = model.TIPO_CONSUMOID,
                    ESTADO_SALUDID = model.ESTADO_SALUDID,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };
                await _sciContext.InvolucradosInc.AddAsync(nuevoInvolucradosInc);

                 // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe Incidentes
                        TIPO_INFORME_ID = model.INCIDENTESID,
                        TIPO_ANTECEDENTE = "INV",           // Antecedentes del Involucrado
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.RUT_INVOLUCRADO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = $"{model.RUT_INVOLUCRADO}-{model.NOMBRES}-{model.APELLIDO_PATERNO}-{model.APELLIDO_MATERNO}",
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

                // Verifica existencia en PERSONAS_CIVILES del Involucrado si este es "C", si no esta se Ingresa de lo contrario se Actualizan sus datos

                if ( string.Equals(model.TIPO_INVOLUCRADO, "C", StringComparison.Ordinal) ) {

                    personas_civiles? personas_civiles = await _sciContext.PersonasCiviles
                    .Where( i => i.RUN == model.RUT_INVOLUCRADO )
                    .FirstOrDefaultAsync();

                    if ( personas_civiles == null ) {

                        personas_civiles nuevoPersonasCiviles = new personas_civiles {

                        RUN = model.RUT_INVOLUCRADO,
                        GRADO = model.GRADO,
                        NOMBRES = model.NOMBRES,
                        APELLIDO_PATERNO = model.APELLIDO_PATERNO,
                        APELLIDO_MATERNO = model.APELLIDO_MATERNO,
                        FONO = model.FONO,
                        CORREOINTER = model.CORREOINTER,

                        PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                        CREACION = DateTime.Now,
                        USUARIO = int.Parse(User.GetClaimValue("Run")),
                        ACTUALIZACION = DateTime.Now,
                        ESTADO = 1

                        };

                        await _sciContext.PersonasCiviles.AddAsync(nuevoPersonasCiviles);
                        await _sciContext.SaveChangesAsync();

                        Auditor.registroAuditor(
                            "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                            Util.GetActionName(ControllerContext), "Ok Ingreso Personas Civiles", JsonConvert.SerializeObject(model), _usuariosContext
                        );

                    } else {

                        personas_civiles.GRADO = model.GRADO;
                        personas_civiles.NOMBRES = model.NOMBRES;
                        personas_civiles.APELLIDO_PATERNO = model.APELLIDO_PATERNO;
                        personas_civiles.APELLIDO_MATERNO = model.APELLIDO_MATERNO;
                        personas_civiles.FONO = model.FONO;
                        personas_civiles.CORREOINTER = model.CORREOINTER;

                        personas_civiles.USUARIO = int.Parse(User.GetClaimValue("Run"));
                        personas_civiles.ACTUALIZACION = DateTime.Now;
                        await _sciContext.SaveChangesAsync();

                        Auditor.registroAuditor(
                            "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                            Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                        );

                    };
                };

                return Ok(nuevoInvolucradosInc);
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

        // Elimina Virtualmente un Involucrado en Incidentes

        [HttpDelete("[action]/{incidentesId},{involucradosId}")]
        public async Task<ActionResult<involucrados_inc>> DeleteInvolucradosInc(
            [FromRoute] int incidentesId, 
            [FromRoute] int involucradosId
            ) 
        {
            try
            {
                involucrados_inc? involucrados_inc = await _sciContext.InvolucradosInc
                .Include(i => i.R_INVOLUCRADO)
                .Where( i => ( i.INCIDENTESID == incidentesId ) && ( i.ID == involucradosId ) )
                .FirstOrDefaultAsync();

                if ( involucrados_inc == null ) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Involucrado a eliminar", JsonConvert.SerializeObject("Nro. Incidente: "+incidentesId+" Nro. Involucrado: "+involucradosId), _usuariosContext
                    );
                    return NotFound();
                }

                involucrados_inc.USUARIO = int.Parse(User.GetClaimValue("Run"));
                involucrados_inc.ACTUALIZACION = DateTime.Now;
                involucrados_inc.ESTADO = 0;


                // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe de Incidente
                        TIPO_INFORME_ID = incidentesId,
                        TIPO_ANTECEDENTE = "INV",           // Antecedentes del Involucrado
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {involucrados_inc.RUT_INVOLUCRADO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = $"{involucrados_inc.RUT_INVOLUCRADO}-{involucrados_inc.R_INVOLUCRADO?.NOMBRES}-{involucrados_inc.R_INVOLUCRADO?.APELLIDO_PATERNO}-{involucrados_inc.R_INVOLUCRADO?.APELLIDO_MATERNO}",
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Incidente: "+incidentesId+" Nro. Involucrado: "+involucradosId), _usuariosContext
                );
                return Ok(involucrados_inc);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Incidente: "+incidentesId+" Nro. Involucrado: "+involucradosId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todos los Involucrados de Finales

        [HttpGet("[action]/{finalesId}")]
        public async Task<ActionResult<List<involucrados_fin>>> GetListInvolucradosFin(
            [FromRoute] int finalesId
            ) 
        {
            try
            {
                var respuesta = await _sciContext.InvolucradosFin
                .Include(i => i.FINALES)
                .Include(i => i.TIPO_INTERVINIENTE)
                .Include(i => i.TIPO_CONSUMO)
                .Include(i => i.ESTADO_SALUD)
                .Include(i => i.R_INVOLUCRADO)
                .Include(i => i.U_INVOLUCRADO)
                .Where( i => ( i.ESTADO == 1 ) && ( i.FINALESID == finalesId ) )
                .OrderBy(i => i.RUT_INVOLUCRADO )
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Final: "+finalesId), _usuariosContext
                );

                return Ok(new { respuesta });
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Get", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Final: "+finalesId), _usuariosContext
                );
                
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Inserta un Involucrado en Finales

        [HttpPost("[action]")]
        public async Task<ActionResult<involucrados_fin>> PostInvolucradosFin(
            PostInvolucradosFinModel model
            ) 
        {
            try
            {
                // Ingresa Involucrado 

                involucrados_fin nuevoInvolucradosFin = new involucrados_fin {

                    FINALESID = model.FINALESID,
                    TIPO_INVOLUCRADO = model.TIPO_INVOLUCRADO,
                    RUT_INVOLUCRADO = model.RUT_INVOLUCRADO,
                    UNIDAD_INVOLUCRADO = model.UNIDAD_INVOLUCRADO,
                    FECHA_NACIMIENTO = model.FECHA_NACIMIENTO,
                    TIPO_INTERVINIENTEID = model.TIPO_INTERVINIENTEID,
                    TIPO_CONSUMOID = model.TIPO_CONSUMOID,
                    ESTADO_SALUDID = model.ESTADO_SALUDID,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };
                await _sciContext.InvolucradosFin.AddAsync(nuevoInvolucradosFin);


                 // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = model.FINALESID,
                        TIPO_ANTECEDENTE = "INV",           // Antecedentes del Involucrado
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.RUT_INVOLUCRADO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = $"{model.RUT_INVOLUCRADO}-{model.NOMBRES}-{model.APELLIDO_PATERNO}-{model.APELLIDO_MATERNO}",
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

                // Verifica existencia en PERSONAS_CIVILES del Involucrado si este es "C", si no esta se Ingresa de lo contrario se Actualizan sus datos

                if ( string.Equals(model.TIPO_INVOLUCRADO, "C", StringComparison.Ordinal) ) {

                    personas_civiles? personas_civiles = await _sciContext.PersonasCiviles
                    .Where( i => i.RUN == model.RUT_INVOLUCRADO )
                    .FirstOrDefaultAsync();

                    if ( personas_civiles == null ) {

                        personas_civiles nuevoPersonasCiviles = new personas_civiles {

                        RUN = model.RUT_INVOLUCRADO,
                        GRADO = model.GRADO,
                        NOMBRES = model.NOMBRES,
                        APELLIDO_PATERNO = model.APELLIDO_PATERNO,
                        APELLIDO_MATERNO = model.APELLIDO_MATERNO,
                        FONO = model.FONO,
                        CORREOINTER = model.CORREOINTER,

                        PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                        CREACION = DateTime.Now,
                        USUARIO = int.Parse(User.GetClaimValue("Run")),
                        ACTUALIZACION = DateTime.Now,
                        ESTADO = 1

                        };

                        await _sciContext.PersonasCiviles.AddAsync(nuevoPersonasCiviles);
                        await _sciContext.SaveChangesAsync();

                        Auditor.registroAuditor(
                            "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                            Util.GetActionName(ControllerContext), "Ok Ingreso Personas Civiles", JsonConvert.SerializeObject(model), _usuariosContext
                        );

                    } else {

                        personas_civiles.GRADO = model.GRADO;
                        personas_civiles.NOMBRES = model.NOMBRES;
                        personas_civiles.APELLIDO_PATERNO = model.APELLIDO_PATERNO;
                        personas_civiles.APELLIDO_MATERNO = model.APELLIDO_MATERNO;
                        personas_civiles.FONO = model.FONO;
                        personas_civiles.CORREOINTER = model.CORREOINTER;

                        personas_civiles.USUARIO = int.Parse(User.GetClaimValue("Run"));
                        personas_civiles.ACTUALIZACION = DateTime.Now;
                        await _sciContext.SaveChangesAsync();

                        Auditor.registroAuditor(
                            "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                            Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                        );

                    };
                };

                return Ok(nuevoInvolucradosFin);
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

        // Elimina Virtualmente un Involucrado en Finales

        [HttpDelete("[action]/{finalesId},{involucradosId}")]
        public async Task<ActionResult<involucrados_fin>> DeleteInvolucradosFin(
            [FromRoute] int finalesId, 
            [FromRoute] int involucradosId
            ) 
        {
            try
            {
                involucrados_fin? involucrados_fin = await _sciContext.InvolucradosFin
                .Include(i => i.R_INVOLUCRADO)
                .Where( i => ( i.FINALESID == finalesId ) && ( i.ID == involucradosId ) )
                .FirstOrDefaultAsync();

                if ( involucrados_fin == null ) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Involucrado a eliminar", JsonConvert.SerializeObject("Nro. Final: "+finalesId+" Nro. Involucrado: "+involucradosId), _usuariosContext
                    );
                    return NotFound();
                }

                involucrados_fin.USUARIO = int.Parse(User.GetClaimValue("Run"));
                involucrados_fin.ACTUALIZACION = DateTime.Now;
                involucrados_fin.ESTADO = 0;


               // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = finalesId,
                        TIPO_ANTECEDENTE = "INV",           // Antecedentes del Involucrado
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {involucrados_fin.RUT_INVOLUCRADO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = $"{involucrados_fin.RUT_INVOLUCRADO}-{involucrados_fin.R_INVOLUCRADO?.NOMBRES}-{involucrados_fin.R_INVOLUCRADO?.APELLIDO_PATERNO}-{involucrados_fin.R_INVOLUCRADO?.APELLIDO_MATERNO}",
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Final: "+finalesId+" Nro. Involucrado: "+involucradosId), _usuariosContext
                );
                return Ok(involucrados_fin);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Final: "+finalesId+" Nro. Involucrado: "+involucradosId), _usuariosContext
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
