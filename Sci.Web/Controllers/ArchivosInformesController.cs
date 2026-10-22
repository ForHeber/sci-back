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
    public class ArchivosInformesController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public ArchivosInformesController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista todos los Archivos de Informes de Preliminares

        [HttpGet("[action]/{preliminaresId}")]
        public async Task<ActionResult> GetListArchivosInformesPre(
            [FromRoute] int preliminaresId
            )
        {
            try
            {
                var respuesta = await _sciContext.ArchivosInformesPre
                .Where(i => (i.ESTADO == 1) && (i.PRELIMINARESID == preliminaresId))
                .OrderBy(i => i.FECHA_ARCHIVO)
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

        // Inserta un Archivo de Informe en Preliminares

        [HttpPost("[action]")]
        public async Task<ActionResult<archivos_informes_pre>> PostArchivosInformesPre(
            PostArchivosInformesPreModel model
            )
        {
            try
            {
                archivos_informes_pre nuevoArchivoInformesPre = new archivos_informes_pre
                {

                    PRELIMINARESID = model.PRELIMINARESID,
                    FECHA_ARCHIVO = model.FECHA_ARCHIVO,
                    DESCRIPCION_ARCHIVO = model.DESCRIPCION_ARCHIVO,
                    NOMBRE_ARCHIVO = model.NOMBRE_ARCHIVO,
                    URL_ARCHIVO = model.URL_ARCHIVO,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.ArchivosInformesPre.AddAsync(nuevoArchivoInformesPre);


               // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "P",                 // Informe Preliminar
                        TIPO_INFORME_ID = model.PRELIMINARESID,
                        TIPO_ANTECEDENTE = "ARC",           // Archivos de Informes
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_ARCHIVO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DESCRIPCION_ARCHIVO,
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

                return Ok(nuevoArchivoInformesPre);
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

        // Actualiza un Archivo de Informe en Preliminares

        [HttpPut("[action]/{preliminaresId},{archivosInformesId}")]
        public async Task<ActionResult<archivos_informes_pre>> PutArchivosInformesPre(
            PutArchivosInformesPreModel model, 
            [FromRoute] int preliminaresId, 
            [FromRoute] int archivosInformesId)
        {
            try
            {
                archivos_informes_pre? archivos_informes_pre = await _sciContext.ArchivosInformesPre
                .Where(i => (i.PRELIMINARESID == preliminaresId) && (i.ID == archivosInformesId))
                .FirstOrDefaultAsync();

                if (archivos_informes_pre == null)
                {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Archivos de Informes del Informe Preliminar", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                archivos_informes_pre.FECHA_ARCHIVO = model.FECHA_ARCHIVO;
                archivos_informes_pre.DESCRIPCION_ARCHIVO = model.DESCRIPCION_ARCHIVO;
                archivos_informes_pre.NOMBRE_ARCHIVO = model.NOMBRE_ARCHIVO;
                archivos_informes_pre.URL_ARCHIVO = model.URL_ARCHIVO;

                archivos_informes_pre.USUARIO = int.Parse(User.GetClaimValue("Run"));
                archivos_informes_pre.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(archivos_informes_pre)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(archivos_informes_pre.USUARIO) &&
                            p.Metadata.Name != nameof(archivos_informes_pre.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "P",                 // Informe Preliminar
                            TIPO_INFORME_ID = preliminaresId,
                            TIPO_ANTECEDENTE = "ARC",           // Archivos Informes
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
                return Ok(archivos_informes_pre);
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

        // Elimina Virtualmente un Archivo de Informe en Preliminares

        [HttpDelete("[action]/{preliminaresId},{archivosInformesId}")]
        public async Task<ActionResult<archivos_informes_pre>> DeleteArchivosInformesPre(
            [FromRoute] int preliminaresId, 
            [FromRoute] int archivosInformesId
            )
        {
            try
            {
                archivos_informes_pre? archivos_informes_pre = await _sciContext.ArchivosInformesPre
                .Where(i => (i.PRELIMINARESID == preliminaresId) && (i.ID == archivosInformesId))
                .FirstOrDefaultAsync();

                if (archivos_informes_pre == null)
                {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Archivos de Informes a eliminar", JsonConvert.SerializeObject("Nro. Preliminar: " + preliminaresId + " Nro. Archivo Informe: " + archivosInformesId), _usuariosContext
                    );
                    return NotFound();
                }

                archivos_informes_pre.USUARIO = int.Parse(User.GetClaimValue("Run"));
                archivos_informes_pre.ACTUALIZACION = DateTime.Now;
                archivos_informes_pre.ESTADO = 0;


               // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "P",                 // Informe Preliminar
                        TIPO_INFORME_ID = preliminaresId,
                        TIPO_ANTECEDENTE = "ARC",           // Archivos Informes
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {archivos_informes_pre.FECHA_ARCHIVO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = archivos_informes_pre.DESCRIPCION_ARCHIVO,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Preliminar: " + preliminaresId + " Nro. Archivo Informe: " + archivosInformesId), _usuariosContext
                );
                return Ok(archivos_informes_pre);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Preliminar: " + preliminaresId + " Nro. Archivo Informe: " + archivosInformesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todos los Archivos de Informes de Incidentes

        [HttpGet("[action]/{incidentesId}")]
        public async Task<ActionResult> GetListArchivosInformesInc(
            [FromRoute] int incidentesId
            )
        {
            try
            {
                var respuesta = await _sciContext.ArchivosInformesInc
                .Where(i => (i.ESTADO == 1) && (i.INCIDENTESID == incidentesId))
                .OrderBy(i => i.FECHA_ARCHIVO)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Incidentes: " + incidentesId), _usuariosContext
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

        // Inserta un Archivo de Informe en Incidentes

        [HttpPost("[action]")]
        public async Task<ActionResult<archivos_informes_inc>> PostArchivosInformesInc(
            PostArchivosInformesIncModel model
            )
        {
            try
            {
                archivos_informes_inc nuevoArchivoInformesInc = new archivos_informes_inc
                {

                    INCIDENTESID = model.INCIDENTESID,
                    FECHA_ARCHIVO = model.FECHA_ARCHIVO,
                    DESCRIPCION_ARCHIVO = model.DESCRIPCION_ARCHIVO,
                    NOMBRE_ARCHIVO = model.NOMBRE_ARCHIVO,
                    URL_ARCHIVO = model.URL_ARCHIVO,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.ArchivosInformesInc.AddAsync(nuevoArchivoInformesInc);


              // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe Incidentes
                        TIPO_INFORME_ID = model.INCIDENTESID,
                        TIPO_ANTECEDENTE = "ARC",           // Archivos de Informes
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_ARCHIVO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DESCRIPCION_ARCHIVO,
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

                return Ok(nuevoArchivoInformesInc);
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

        // Actualiza un Archivo de Informe en Incidentes

        [HttpPut("[action]/{incidentesId},{archivosInformesId}")]
        public async Task<ActionResult<archivos_informes_inc>> PutArchivosInformesInc(
            PutArchivosInformesIncModel model, 
            [FromRoute] int incidentesId, 
            [FromRoute] int archivosInformesId)
        {
            try
            {
                archivos_informes_inc? archivos_informes_inc = await _sciContext.ArchivosInformesInc
                .Where(i => (i.INCIDENTESID == incidentesId) && (i.ID == archivosInformesId))
                .FirstOrDefaultAsync();

                if (archivos_informes_inc == null)
                {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Archivos de Informes del Informe Incidentes", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                archivos_informes_inc.FECHA_ARCHIVO = model.FECHA_ARCHIVO;
                archivos_informes_inc.DESCRIPCION_ARCHIVO = model.DESCRIPCION_ARCHIVO;
                archivos_informes_inc.NOMBRE_ARCHIVO = model.NOMBRE_ARCHIVO;
                archivos_informes_inc.URL_ARCHIVO = model.URL_ARCHIVO;

                archivos_informes_inc.USUARIO = int.Parse(User.GetClaimValue("Run"));
                archivos_informes_inc.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(archivos_informes_inc)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(archivos_informes_inc.USUARIO) &&
                            p.Metadata.Name != nameof(archivos_informes_inc.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "I",                 // Informe Incidente
                            TIPO_INFORME_ID = incidentesId,
                            TIPO_ANTECEDENTE = "ARC",           // Archivos Informes
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
                return Ok(archivos_informes_inc);
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

        // Elimina Virtualmente un Archivo de Informe en Incidentes

        [HttpDelete("[action]/{incidentesId},{archivosInformesId}")]
        public async Task<ActionResult<archivos_informes_inc>> DeleteArchivosInformesInc(
            [FromRoute] int incidentesId, 
            [FromRoute] int archivosInformesId
            )
        {
            try
            {
                archivos_informes_inc? archivos_informes_inc = await _sciContext.ArchivosInformesInc
                .Where(i => (i.INCIDENTESID == incidentesId) && (i.ID == archivosInformesId))
                .FirstOrDefaultAsync();

                if (archivos_informes_inc == null)
                {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Archivos de Informes a eliminar", JsonConvert.SerializeObject("Nro. Incidente: " + incidentesId + " Nro. Archivo Informe: " + archivosInformesId), _usuariosContext
                    );
                    return NotFound();
                }

                archivos_informes_inc.USUARIO = int.Parse(User.GetClaimValue("Run"));
                archivos_informes_inc.ACTUALIZACION = DateTime.Now;
                archivos_informes_inc.ESTADO = 0;


               // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe Incidente
                        TIPO_INFORME_ID = incidentesId,
                        TIPO_ANTECEDENTE = "ARC",           // Archivos Informes
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {archivos_informes_inc.FECHA_ARCHIVO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = archivos_informes_inc.DESCRIPCION_ARCHIVO,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Incidente: " + incidentesId + " Nro. Archivo Informe: " + archivosInformesId), _usuariosContext
                );
                return Ok(archivos_informes_inc);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Incidente: " + incidentesId + " Nro. Archivo Informe: " + archivosInformesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todos los Archivos de Informes de Finales

        [HttpGet("[action]/{finalesId}")]
        public async Task<ActionResult> GetListArchivosInformesFin(
            [FromRoute] int finalesId
            )
        {
            try
            {
                var respuesta = await _sciContext.ArchivosInformesFin
                .Where(i => (i.ESTADO == 1) && (i.FINALESID == finalesId))
                .OrderBy(i => i.FECHA_ARCHIVO)
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

        // Inserta un Archivo de Informe en Finales

        [HttpPost("[action]")]
        public async Task<ActionResult<archivos_informes_fin>> PostArchivosInformesFin(
            PostArchivosInformesFinModel model
            )
        {
            try
            {
                archivos_informes_fin nuevoArchivoInformesFin = new archivos_informes_fin
                {

                    FINALESID = model.FINALESID,
                    FECHA_ARCHIVO = model.FECHA_ARCHIVO,
                    DESCRIPCION_ARCHIVO = model.DESCRIPCION_ARCHIVO,
                    NOMBRE_ARCHIVO = model.NOMBRE_ARCHIVO,
                    URL_ARCHIVO = model.URL_ARCHIVO,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.ArchivosInformesFin.AddAsync(nuevoArchivoInformesFin);


              // Si el Rol del usuario es Operador BINTE, se registra el ingreso.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre el ingreso.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = model.FINALESID,
                        TIPO_ANTECEDENTE = "ARC",           // Archivos de Informes
                        TIPO_EVENTO = "I",                  // Evento de Ingreso
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"INGRESO {model.FECHA_ARCHIVO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = model.DESCRIPCION_ARCHIVO,
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

                return Ok(nuevoArchivoInformesFin);
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

        // Actualiza un Archivo de Informe en Finales

        [HttpPut("[action]/{finalesId},{archivosInformesId}")]
        public async Task<ActionResult<archivos_informes_fin>> PutArchivosInformesFin(
            PutArchivosInformesFinModel model, 
            [FromRoute] int finalesId, 
            [FromRoute] int archivosInformesId)
        {
            try
            {
                archivos_informes_fin? archivos_informes_fin = await _sciContext.ArchivosInformesFin
                .Where(i => (i.FINALESID == finalesId) && (i.ID == archivosInformesId))
                .FirstOrDefaultAsync();

                if (archivos_informes_fin == null)
                {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Archivos de Informes del Informe Finales", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                archivos_informes_fin.FECHA_ARCHIVO = model.FECHA_ARCHIVO;
                archivos_informes_fin.DESCRIPCION_ARCHIVO = model.DESCRIPCION_ARCHIVO;
                archivos_informes_fin.NOMBRE_ARCHIVO = model.NOMBRE_ARCHIVO;
                archivos_informes_fin.URL_ARCHIVO = model.URL_ARCHIVO;

                archivos_informes_fin.USUARIO = int.Parse(User.GetClaimValue("Run"));
                archivos_informes_fin.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(archivos_informes_fin)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(archivos_informes_fin.USUARIO) &&
                            p.Metadata.Name != nameof(archivos_informes_fin.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "F",                 // Informe Finales
                            TIPO_INFORME_ID = finalesId,
                            TIPO_ANTECEDENTE = "ARC",           // Archivos Informes
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
                return Ok(archivos_informes_fin);
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

        // Elimina Virtualmente un Archivo de Informe en Finales

        [HttpDelete("[action]/{finalesId},{archivosInformesId}")]
        public async Task<ActionResult<archivos_informes_fin>> DeleteArchivosInformesFin(
            [FromRoute] int finalesId, 
            [FromRoute] int archivosInformesId
            )
        {
            try
            {
                archivos_informes_fin? archivos_informes_fin = await _sciContext.ArchivosInformesFin
                .Where(i => (i.FINALESID == finalesId) && (i.ID == archivosInformesId))
                .FirstOrDefaultAsync();

                if (archivos_informes_fin == null)
                {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Archivos de Informes a eliminar", JsonConvert.SerializeObject("Nro. Final: " + finalesId + " Nro. Archivo Informe: " + archivosInformesId), _usuariosContext
                    );
                    return NotFound();
                }

                archivos_informes_fin.USUARIO = int.Parse(User.GetClaimValue("Run"));
                archivos_informes_fin.ACTUALIZACION = DateTime.Now;
                archivos_informes_fin.ESTADO = 0;


               // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = finalesId,
                        TIPO_ANTECEDENTE = "ARC",           // Archivos Informes
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {archivos_informes_fin.FECHA_ARCHIVO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = archivos_informes_fin.DESCRIPCION_ARCHIVO,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Final: " + finalesId + " Nro. Archivo Informe: " + archivosInformesId), _usuariosContext
                );
                return Ok(archivos_informes_fin);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Final: " + finalesId + " Nro. Archivo Informe: " + archivosInformesId), _usuariosContext
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