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
    public class PreliminaresController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public PreliminaresController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista todos los Informes Preliminares Estado = 1 (No Editables) y Estado = 2 (Editables).
        // Ordenados por ID de forma Descendente para que los mas recientes se muestren primero

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListInformesPreliminares(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search
        ) {
            try
            {
                var preliminares = _sciContext.Preliminares
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => i.ESTADO == 1 || i.ESTADO == 2) 
                .OrderByDescending(i => i.ID)
                .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                { 

                    preliminares = preliminares.Where(
                        pr => (
                            pr.INFORMANTE!.RUN.ToString()
                            + pr.INFORMANTE!.NOMBRES            // Nombres del Informante Nombres,Apellido Paterno,Apellido Materno
                            + pr.INFORMANTE!.APELLIDO_PATERNO
                            + pr.INFORMANTE!.APELLIDO_MATERNO
                            + pr.INFORMANTE!.NOMBRE             // Nombre del Informante Primer Nombre,Apellido Paterno,Apellido Materno
                            + pr.INFORMANTE!.APELLIDO_PATERNO
                            + pr.INFORMANTE!.APELLIDO_MATERNO
                            + pr.INFORMANTE!.NOMBRE             // Nombre del Informante Primer Nombre,Apellido Materno,Apellido Paterno
                            + pr.INFORMANTE!.APELLIDO_MATERNO
                            + pr.INFORMANTE!.APELLIDO_PATERNO
                            + pr.U_INFORMANTE!.CODIGO
                            + pr.U_INFORMANTE!.DESCRIPCION
                            + pr.U_INFORMANTE!.SIGLA
                            + pr.U_AFECTADA!.CODIGO
                            + pr.U_AFECTADA!.DESCRIPCION
                            + pr.U_AFECTADA!.SIGLA
                            + pr.TIPO_INGESTA!.DESCRIPCION
                            + pr.TIPO_INCIDENTE!.DESCRIPCION
                            + pr.LUGAR_HECHO!
                            + pr.CIRCUNSTANCIA_HECHO!
                            + pr.DANOS_PERSONALES!
                            + pr.MEDIDAS_ADOPTADAS!
                        ).ToLower().Contains(search!.ToLower())
                    );
                }

                preliminares = preliminares.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = preliminares.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                List<preliminares>? respuesta = await preliminares
                .Skip((int.Parse(string.IsNullOrEmpty(page) ? "1" : page) - 1) * (int)pageResult) // Paginación - salta los resultados de las páginas anteriores a la actuál
                .Take((int)pageResult) // Paginación - selecciona solo la cantidad de registros definidos en la variable pageResult
                .ToListAsync();
                
                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {
                    respuesta,
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

        // Lista todos los Informes Preliminares Estado = 1 (Publicados) para la Vista de BINTE.
        // Ordenados por FECHA_HECHO de forma Descendente para que los mas recientes se muestren primero
        // Si unidadId es "0", se listan nivel Institucional, de lo contrario se filtran por la unidad especificada

        [HttpGet("[action]/{unidadId}")]
        public async Task<ActionResult> GetListInformesPreliminaresBINTE(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search,
            [FromRoute] string unidadId
        ) {
            try
            {
                var preliminares = _sciContext.Preliminares
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => (i.ESTADO == 1 || i.ESTADO == 3) && (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId)) // Solo Preliminares Publicados
                .OrderByDescending(i => i.FECHA_HECHO)
                .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                { 

                    preliminares = preliminares.Where(
                        pr => (
                            pr.INFORMANTE!.RUN.ToString()
                            + pr.INFORMANTE!.NOMBRES            // Nombres del Informante Nombres,Apellido Paterno,Apellido Materno
                            + pr.INFORMANTE!.APELLIDO_PATERNO
                            + pr.INFORMANTE!.APELLIDO_MATERNO
                            + pr.INFORMANTE!.NOMBRE             // Nombre del Informante Primer Nombre,Apellido Paterno,Apellido Materno
                            + pr.INFORMANTE!.APELLIDO_PATERNO
                            + pr.INFORMANTE!.APELLIDO_MATERNO
                            + pr.INFORMANTE!.NOMBRE             // Nombre del Informante Primer Nombre,Apellido Materno,Apellido Paterno
                            + pr.INFORMANTE!.APELLIDO_MATERNO
                            + pr.INFORMANTE!.APELLIDO_PATERNO
                            + pr.U_INFORMANTE!.CODIGO
                            + pr.U_INFORMANTE!.DESCRIPCION
                            + pr.U_INFORMANTE!.SIGLA
                            + pr.U_AFECTADA!.CODIGO
                            + pr.U_AFECTADA!.DESCRIPCION
                            + pr.U_AFECTADA!.SIGLA
                            + pr.TIPO_INGESTA!.DESCRIPCION
                            + pr.TIPO_INCIDENTE!.DESCRIPCION
                            + pr.LUGAR_HECHO!
                            + pr.CIRCUNSTANCIA_HECHO!
                            + pr.DANOS_PERSONALES!
                            + pr.MEDIDAS_ADOPTADAS!
                        ).ToLower().Contains(search!.ToLower())
                    );
                }

                preliminares = preliminares.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = preliminares.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                List<preliminares>? respuesta = await preliminares
                .Skip((int.Parse(string.IsNullOrEmpty(page) ? "1" : page) - 1) * (int)pageResult) // Paginación - salta los resultados de las páginas anteriores a la actuál
                .Take((int)pageResult) // Paginación - selecciona solo la cantidad de registros definidos en la variable pageResult
                .ToListAsync();
                
                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {
                    respuesta,
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

        // Inserta un Informe Preliminar con Estado 2 (Editable).

        [HttpPost("[action]")]
        public async Task<ActionResult<preliminares>> PostInformesPreliminares(
            PostPreliminaresModel model
            ) 
        {
            try
            {
                preliminares nuevoPreliminar = new preliminares {

                    RUT_INFORMANTE = model.RUT_INFORMANTE,
                    UNIDAD_INFORMANTE = model.UNIDAD_INFORMANTE,
                    INCIDENTE_ACCIDENTE = model.INCIDENTE_ACCIDENTE,
                    TIPO_INGESTAID = model.TIPO_INGESTAID,
                    FECHA_INFORMANTE = model.FECHA_INFORMANTE,
                    TIPO_INCIDENTEID = model.TIPO_INCIDENTEID,
                    PERSONAL_UNIDAD = model.PERSONAL_UNIDAD,
                    UNIDAD_AFECTADA = model.UNIDAD_AFECTADA,
                    FECHA_HECHO = model.FECHA_HECHO,
                    LUGAR_HECHO = model.LUGAR_HECHO,
                    CIRCUNSTANCIA_HECHO = model.CIRCUNSTANCIA_HECHO,
                    DANOS_PERSONALES = model.DANOS_PERSONALES,
                    MEDIDAS_ADOPTADAS = model.MEDIDAS_ADOPTADAS,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 2
                };

                await _sciContext.Preliminares.AddAsync(nuevoPreliminar);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(nuevoPreliminar);
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

        // Actualiza un Informe Preliminar

        [HttpPut("[action]/{preliminaresId}")]
        public async Task<ActionResult<preliminares>> PutInformesPreliminares(
            PutPreliminaresModel model, 
            [FromRoute] int preliminaresId
            ) 
        {
            try
            {
                preliminares? preliminares = await _sciContext.Preliminares
                .Where(i => i.ID == preliminaresId)
                .FirstOrDefaultAsync();

                if (preliminares == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Preliminar para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                preliminares.RUT_INFORMANTE = model.RUT_INFORMANTE;
                preliminares.UNIDAD_INFORMANTE = model.UNIDAD_INFORMANTE;
                preliminares.INCIDENTE_ACCIDENTE = model.INCIDENTE_ACCIDENTE;
                preliminares.TIPO_INGESTAID = model.TIPO_INGESTAID;
                preliminares.FECHA_INFORMANTE = model.FECHA_INFORMANTE;
                preliminares.TIPO_INCIDENTEID = model.TIPO_INCIDENTEID;
                preliminares.PERSONAL_UNIDAD = model.PERSONAL_UNIDAD;
                preliminares.UNIDAD_AFECTADA = model.UNIDAD_AFECTADA;
                preliminares.FECHA_HECHO = model.FECHA_HECHO;
                preliminares.LUGAR_HECHO = model.LUGAR_HECHO;
                preliminares.CIRCUNSTANCIA_HECHO = model.CIRCUNSTANCIA_HECHO;
                preliminares.DANOS_PERSONALES = model.DANOS_PERSONALES;
                preliminares.MEDIDAS_ADOPTADAS = model.MEDIDAS_ADOPTADAS;

                preliminares.USUARIO = int.Parse(User.GetClaimValue("Run"));
                preliminares.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(preliminares)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(preliminares.USUARIO) &&
                            p.Metadata.Name != nameof(preliminares.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "P",                 // Informe Preliminar
                            TIPO_INFORME_ID = preliminaresId,
                            TIPO_ANTECEDENTE = "PRE",           // Antecedentes Informe Preliminar
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

                return Ok(preliminares);
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

        // Elimina Virtualmente un Informe Preliminar Estado 0 (Eliminado).

        [HttpDelete("[action]/{preliminaresId}")]
        public async Task<ActionResult<preliminares>> DeleteInformesPreliminares(
            [FromRoute] int preliminaresId
            ) 
        {
            try
            {
                preliminares? preliminares = await _sciContext.Preliminares
                .Where(i => i.ID == preliminaresId)
                .FirstOrDefaultAsync();

                if (preliminares == null) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Preliminar a Eliminar", JsonConvert.SerializeObject("Nro. Informe Preliminar: "+preliminaresId), _usuariosContext
                    );
                    return NotFound();
                }

                preliminares.USUARIO = int.Parse(User.GetClaimValue("Run"));
                preliminares.ACTUALIZACION = DateTime.Now;
                preliminares.ESTADO = 0;


                // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "P",                 // Informe Preliminar
                        TIPO_INFORME_ID = preliminaresId,
                        TIPO_ANTECEDENTE = "PRE",           // Antecedentes Informe Preliminar
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {preliminares.FECHA_HECHO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = preliminares.CIRCUNSTANCIA_HECHO,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Preliminar: "+preliminaresId), _usuariosContext
                );

                return Ok(preliminares);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Informe Preliminar: "+preliminaresId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Publica un Informe Preliminar, cambiando su Estado a 1 (No Editable)
        // y crea una copia como un nuevo Informe Incidentes con Estado 2 (Editable) para el S2
        // con todos los antecedentes complementarios de Preliminar.

        [HttpPut("[action]/{preliminaresId}")]
        public async Task<ActionResult<preliminares>> PutPublicarInformesPreliminares(
            [FromRoute] int preliminaresId
            ) 
        {
            try
            {
                preliminares? preliminares = await _sciContext.Preliminares
                .Where(i => i.ID == preliminaresId)
                .FirstOrDefaultAsync();

                if (preliminares == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Preliminar para Publicar", JsonConvert.SerializeObject("Nro. Informe Preliminar: "+preliminaresId), _usuariosContext
                    );
                    return NotFound();
                }

                preliminares.USUARIO = int.Parse(User.GetClaimValue("Run"));
                preliminares.ACTUALIZACION = DateTime.Now;
                preliminares.ESTADO = 1;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Preliminar: "+preliminaresId), _usuariosContext
                );

                // Se crea un nuevo Informe Incidentes con Estado 2 (Editable) para el S2 con todos los antecedentes complementarios de Preliminar.
                // Copia el Preliminar a Incidentes.

                incidentes nuevoIncidente = new incidentes {

                    PRELIMINARESID = preliminares.ID,
                    FECHA_PUBLICACION = DateTime.Now,
                    DEVUELTO = "N",
                    RUT_INFORMANTE = preliminares.RUT_INFORMANTE,
                    UNIDAD_INFORMANTE = preliminares.UNIDAD_INFORMANTE,
                    INCIDENTE_ACCIDENTE = preliminares.INCIDENTE_ACCIDENTE,
                    TIPO_INGESTAID = preliminares.TIPO_INGESTAID,
                    FECHA_INFORMANTE = preliminares.FECHA_INFORMANTE,
                    TIPO_INCIDENTEID = preliminares.TIPO_INCIDENTEID,
                    PERSONAL_UNIDAD = preliminares.PERSONAL_UNIDAD,
                    UNIDAD_AFECTADA = preliminares.UNIDAD_AFECTADA,
                    FECHA_HECHO = preliminares.FECHA_HECHO,
                    LUGAR_HECHO = preliminares.LUGAR_HECHO,
                    CIRCUNSTANCIA_HECHO = preliminares.CIRCUNSTANCIA_HECHO,
                    DANOS_PERSONALES = preliminares.DANOS_PERSONALES,
                    MEDIDAS_ADOPTADAS = preliminares.MEDIDAS_ADOPTADAS,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 2
                };

                await _sciContext.Incidentes.AddAsync(nuevoIncidente);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Incidente: "+nuevoIncidente.ID), _usuariosContext
                );

                // Copia Antecedentes Judiciales del Preliminar a Antecedentes Judiciales del Incidente.

                var AntecedenteJudicialPre = await _sciContext.AntecedentesJudicialesPre
                    .Where(x => x.ESTADO == 1 && x.PRELIMINARESID == preliminaresId)
                    .AsNoTracking()
                    .ToListAsync();
                var AntecedenteJudicialInc = AntecedenteJudicialPre.Select(x => new antecedentes_judiciales_inc
                {
                    INCIDENTESID = nuevoIncidente.ID,
                    FECHA_ANTECEDENTES_JUD = x.FECHA_ANTECEDENTES_JUD,
                    DETALLE_ANTECEDENTES_JUD = x.DETALLE_ANTECEDENTES_JUD,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1 // Activo
                }).ToList();
                _sciContext.AntecedentesJudicialesInc.AddRange(AntecedenteJudicialInc);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Copia de Antecedentes Judiciales del Informe Incidente: "+nuevoIncidente.ID+" Exitoso."), _usuariosContext
                );

                // Copia Archivos Informes del Preliminar a Archivos Informes del Incidente.

                var ArchivosInformesPre = await _sciContext.ArchivosInformesPre
                    .Where(x => x.ESTADO == 1 && x.PRELIMINARESID == preliminaresId)
                    .AsNoTracking()
                    .ToListAsync();
                var ArchivosInformesInc = ArchivosInformesPre.Select(x => new archivos_informes_inc
                {
                    INCIDENTESID = nuevoIncidente.ID,
                    FECHA_ARCHIVO = x.FECHA_ARCHIVO,
                    DESCRIPCION_ARCHIVO = x.DESCRIPCION_ARCHIVO,
                    NOMBRE_ARCHIVO = x.NOMBRE_ARCHIVO,
                    URL_ARCHIVO = x.URL_ARCHIVO,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1 // Activo
                }).ToList();
                _sciContext.ArchivosInformesInc.AddRange(ArchivosInformesInc);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Copia de Archivos Informes del Informe Incidente: "+nuevoIncidente.ID+" Exitoso."), _usuariosContext
                );

                // Copia Involucrados del Preliminar a Involucrados del Incidente.

                var InvolucradosPre = await _sciContext.InvolucradosPre
                    .Where(x => x.ESTADO == 1 && x.PRELIMINARESID == preliminaresId)
                    .AsNoTracking()
                    .ToListAsync();
                var InvolucradosInc = InvolucradosPre.Select(x => new involucrados_inc
                {
                    INCIDENTESID = nuevoIncidente.ID,
                    TIPO_INVOLUCRADO = x.TIPO_INVOLUCRADO,
                    RUT_INVOLUCRADO = x.RUT_INVOLUCRADO,
                    UNIDAD_INVOLUCRADO = x.UNIDAD_INVOLUCRADO,
                    FECHA_NACIMIENTO = x.FECHA_NACIMIENTO,
                    TIPO_INTERVINIENTEID = x.TIPO_INTERVINIENTEID,
                    TIPO_CONSUMOID = x.TIPO_CONSUMOID,
                    ESTADO_SALUDID = x.ESTADO_SALUDID,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1 // Activo
                }).ToList();
                _sciContext.InvolucradosInc.AddRange(InvolucradosInc);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Copia de Involucrados del Informe Incidente: "+nuevoIncidente.ID+" Exitoso."), _usuariosContext
                );

                // Copia Procedimientos Policiales del Preliminar a Procedimientos Policiales del Incidente.

                var ProcedimientosPolicialesPre = await _sciContext.ProcedimientosPolicialesPre
                    .Where(x => x.ESTADO == 1 && x.PRELIMINARESID == preliminaresId)
                    .AsNoTracking()
                    .ToListAsync();
                var ProcedimientosPolicialesInc = ProcedimientosPolicialesPre.Select(x => new procedimientos_policiales_inc
                {
                    INCIDENTESID = nuevoIncidente.ID,
                    FECHA_PROCEDIMIENTO_POL = x.FECHA_PROCEDIMIENTO_POL,
                    CONDICION_POLICIALID = x.CONDICION_POLICIALID,
                    DETALLE_PROCEDIMIENTO_POL = x.DETALLE_PROCEDIMIENTO_POL,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1 // Activo
                }).ToList();
                _sciContext.ProcedimientosPolicialesInc.AddRange(ProcedimientosPolicialesInc);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Copia de Procedimientos Policiales del Informe Incidente: "+nuevoIncidente.ID+" Exitoso."), _usuariosContext
                );

                return Ok(preliminares);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Put", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Informe Preliminar: "+preliminaresId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Genera Boletin Institucional de Incidentes,
        // verifica si a quedado un Preliminar sin Publicar Estado=2 Editable y alerta,
        // de lo contrario rescata los Preliminares Estado=1 (Publicado No Editable) para Boletin.

        [HttpGet("[action]")]
        public async Task<ActionResult<preliminares>> GetGeneracionDeBoletinInstitucional() {
            try
            {

                // Se verifica si existen Preliminares sin Publicar (Estado = 2).

                var preliminaresSinPublicar = await _sciContext.Preliminares
                    .AsNoTracking()
                    .Where( i => i.ESTADO == 2 )
                    .ToListAsync();

                if (preliminaresSinPublicar.Count != 0) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "Existen Preliminares No Publicados", JsonConvert.SerializeObject(""), _usuariosContext
                    );
                    return NotFound();
                }

                // Si no existen Preliminares Sin Publicar (Estado = 2).
                // rescata para Boletin los Preliminares Publicados (Estado = 1).

                var preliminaresPublicados = await _sciContext.Preliminares
                    .Include(i => i.TIPO_INGESTA)
                    .Include(i => i.TIPO_INCIDENTE)
                    .Include(i => i.INFORMANTE)
                    .Include(i => i.U_INFORMANTE)
                    .Include(i => i.U_AFECTADA)
                    .AsNoTracking()
                    .Where( i => i.ESTADO == 1 )
                    .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Preliminares Publicados para Boletin", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(preliminaresPublicados);
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

        // Proceso de Entrega de Turno,
        // rescata los Preliminares con Estado 1-No Editable y los cambia a Estado 3-No Visualizables por Entrega de Turno.

        [HttpPut("[action]")]
        public async Task<ActionResult<preliminares>> PutEntregaDeTurnoPME() {
            try
            {

                // Rescata los Preliminares con Estado 1-No Editable.

                var preliminaresPublicados = await _sciContext.Preliminares
                    .Where( i => i.ESTADO == 1 )
                    .ToListAsync();

                // Cambia el Estado de los Preliminares Publicados (Estado = 1 No Editables) a (Estado = 3 No Visualizables).

                foreach( preliminares detallePreliminares in preliminaresPublicados )
                {
                    detallePreliminares.ESTADO = 3; // No Visualizables.
                    detallePreliminares.USUARIO = int.Parse(User.GetClaimValue("Run"));
                    detallePreliminares.ACTUALIZACION = DateTime.Now;
                    await _sciContext.SaveChangesAsync();
                };

                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Cambio de Estados Preliminares a No Visualizables", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(preliminaresPublicados);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Put", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
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
// 13-09-2026 22:54