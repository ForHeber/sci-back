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
    public class IncidentesController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public IncidentesController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista todos los Informes Incidentes de la Unidad Afectada, Estado 1 (No Editables) y Estado 2 (Editables).
        // Ordenados por FECHA_PUBLICACION de forma Ascendente para que los mas antiguos se muestren primero

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListInformesIncidentes(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search,
            [FromQuery] string? unidadId,
            [FromQuery] string? tipoEstadoIngresado
        ) {
            try
            {

                if (tipoEstadoIngresado == null || tipoEstadoIngresado == "") { tipoEstadoIngresado = "00"; } 

                var incidentes = _sciContext.Incidentes
                .Include(i => i.PRELIMINARES)
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.TIPO_IDENTIFICACION)
                .Include(i => i.TIPO_ARMAMENTO)
                .Include(i => i.TIPO_INVESTIGACION)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => (i.ESTADO == 1 || i.ESTADO == 2) && (i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.FECHA_PUBLICACION) 
                .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                { 

                    incidentes = incidentes.Where(
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
                            + pr.TIPO_IDENTIFICACION!.DESCRIPCION
                            + pr.TIPO_ARMAMENTO!.DESCRIPCION
                            + pr.TIPO_INVESTIGACION!.DESCRIPCION
                            + pr.LUGAR_HECHO!
                            + pr.CIRCUNSTANCIA_HECHO!
                            + pr.DANOS_PERSONALES!
                            + pr.MEDIDAS_ADOPTADAS!
                        ).ToLower().Contains(search!.ToLower())
                    );
                }

                if (tipoEstadoIngresado == "01")
                    { incidentes = incidentes.Where(i => i.ESTADO == 2 && i.DEVUELTO == "N"); } // En Proceso por S2.
                else if (tipoEstadoIngresado == "02")
                    { incidentes = incidentes.Where(i => i.ESTADO == 1 ); } // Tramitado al Cdte.
                else if (tipoEstadoIngresado == "03")
                    { incidentes = incidentes.Where(i => i.ESTADO == 2 && i.DEVUELTO == "S"); } // Devuelto por Cdte.

                incidentes = incidentes.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = incidentes.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                var respuesta = await incidentes
                .Skip((int.Parse(string.IsNullOrEmpty(page) ? "1" : page) - 1) * (int)pageResult) // Paginación - salta los resultados de las páginas anteriores a la actuál
                .Take((int)pageResult) // Paginación - selecciona solo la cantidad de registros definidos en la variable pageResult
                .Select(inc => new
                {
                    Incidente = inc,
                    MotivoDevolucionInc = _sciContext.MotivoDevolucionInc
                        .Where(md => md.INCIDENTESID == inc.ID)
                        .OrderByDescending(md => md.FECHA_DEVOLUCION)
                        .ThenByDescending(md => md.ID)
                        .FirstOrDefault()
                })
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

        // Lista todos los Informes Incidentes de la Unidad Afectada, solor Estado 1 (No Editables) para CDTE.
        // Ordenados por FECHA_PUBLICACION de forma Ascendente para que los mas antiguos se muestren primero

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListInformesIncidentesCdte(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search,
            [FromQuery] string? unidadId
        ) {
            try
            {

                var incidentes = _sciContext.Incidentes
                .Include(i => i.PRELIMINARES)
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.TIPO_IDENTIFICACION)
                .Include(i => i.TIPO_ARMAMENTO)
                .Include(i => i.TIPO_INVESTIGACION)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => (i.ESTADO == 1) && (i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.FECHA_PUBLICACION) 
                .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                { 

                    incidentes = incidentes.Where(
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
                            + pr.TIPO_IDENTIFICACION!.DESCRIPCION
                            + pr.TIPO_ARMAMENTO!.DESCRIPCION
                            + pr.TIPO_INVESTIGACION!.DESCRIPCION
                            + pr.LUGAR_HECHO!
                            + pr.CIRCUNSTANCIA_HECHO!
                            + pr.DANOS_PERSONALES!
                            + pr.MEDIDAS_ADOPTADAS!
                        ).ToLower().Contains(search!.ToLower())
                    );
                }

                incidentes = incidentes.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = incidentes.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                var respuesta = await incidentes
                .Skip((int.Parse(string.IsNullOrEmpty(page) ? "1" : page) - 1) * (int)pageResult) // Paginación - salta los resultados de las páginas anteriores a la actuál
                .Take((int)pageResult) // Paginación - selecciona solo la cantidad de registros definidos en la variable pageResult
                .Select(inc => new
                {
                    Incidente = inc,
                    MotivoDevolucionInc = _sciContext.MotivoDevolucionInc
                        .Where(md => md.INCIDENTESID == inc.ID)
                        .OrderByDescending(md => md.FECHA_DEVOLUCION)
                        .ThenByDescending(md => md.ID)
                        .FirstOrDefault()
                })
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

        // Lista todos los Informes Incidentes con Estado = 1,2 y 3 para la Vista del BINTE.
        // Ordenados por FECHA_HECHO de forma Ascendente para que los mas antiguos se muestren primero
        // Si unidadId es "0", se listan nivel Institucional, de lo contrario se filtran por la unidad especificada

        [HttpGet("[action]/{unidadId}")]
        public async Task<ActionResult> GetListInformesIncidentesBINTE(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search,
            [FromRoute] string? unidadId,
            [FromQuery] string? tipoEstadoIngresado
        ) {
            try
            {

                if (tipoEstadoIngresado == null || tipoEstadoIngresado == "") { tipoEstadoIngresado = "00"; } 

                var incidentes = _sciContext.Incidentes
                .Include(i => i.PRELIMINARES)
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.TIPO_IDENTIFICACION)
                .Include(i => i.TIPO_ARMAMENTO)
                .Include(i => i.TIPO_INVESTIGACION)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 3) && (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.FECHA_PUBLICACION) 
                .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                { 

                    incidentes = incidentes.Where(
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
                            + pr.TIPO_IDENTIFICACION!.DESCRIPCION
                            + pr.TIPO_ARMAMENTO!.DESCRIPCION
                            + pr.TIPO_INVESTIGACION!.DESCRIPCION
                            + pr.LUGAR_HECHO!
                            + pr.CIRCUNSTANCIA_HECHO!
                            + pr.DANOS_PERSONALES!
                            + pr.MEDIDAS_ADOPTADAS!
                        ).ToLower().Contains(search!.ToLower())
                    );
                }

                if (tipoEstadoIngresado == "01")
                    { incidentes = incidentes.Where(i => i.ESTADO == 2 && i.DEVUELTO == "N"); } // En Proceso por S2.
                else if (tipoEstadoIngresado == "02")
                    { incidentes = incidentes.Where(i => i.ESTADO == 1 ); } // Tramitado al Cdte.
                else if (tipoEstadoIngresado == "03")
                    { incidentes = incidentes.Where(i => i.ESTADO == 2 && i.DEVUELTO == "S"); } // Devuelto por Cdte.
                else if (tipoEstadoIngresado == "04")
                    { incidentes = incidentes.Where(i => i.ESTADO == 3); } // Con Informe Final.

                incidentes = incidentes.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = incidentes.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                var respuesta = await incidentes
                .Skip((int.Parse(string.IsNullOrEmpty(page) ? "1" : page) - 1) * (int)pageResult) // Paginación - salta los resultados de las páginas anteriores a la actuál
                .Take((int)pageResult) // Paginación - selecciona solo la cantidad de registros definidos en la variable pageResult
                .Select(inc => new
                {
                    Incidente = inc,
                    MotivoDevolucionInc = _sciContext.MotivoDevolucionInc
                        .Where(md => md.INCIDENTESID == inc.ID)
                        .OrderByDescending(md => md.FECHA_DEVOLUCION)
                        .ThenByDescending(md => md.ID)
                        .FirstOrDefault()
                })
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

        // Actualiza un Informe de Incidentes de la Unidad Afectada.

        [HttpPut("[action]/{preliminaresId},{incidentesId}")]
        public async Task<ActionResult<incidentes>> PutInformesIncidentes(
            PutIncidentesModel model, 
            [FromRoute] int preliminaresId,
            [FromRoute] int incidentesId
            ) 
        {
            try
            {
                incidentes? incidentes = await _sciContext.Incidentes
                .Where(i => i.PRELIMINARESID == preliminaresId && i.ID == incidentesId)
                .FirstOrDefaultAsync();

                if (incidentes == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe de Incidente para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                incidentes.RUT_INFORMANTE = model.RUT_INFORMANTE;
                incidentes.UNIDAD_INFORMANTE = model.UNIDAD_INFORMANTE;
                incidentes.INCIDENTE_ACCIDENTE = model.INCIDENTE_ACCIDENTE;
                incidentes.TIPO_INGESTAID = model.TIPO_INGESTAID;
                incidentes.FECHA_INFORMANTE = model.FECHA_INFORMANTE;
                incidentes.TIPO_INCIDENTEID = model.TIPO_INCIDENTEID;
                incidentes.PERSONAL_UNIDAD = model.PERSONAL_UNIDAD;
                incidentes.UNIDAD_AFECTADA = model.UNIDAD_AFECTADA;
                incidentes.FECHA_HECHO = model.FECHA_HECHO;
                incidentes.LUGAR_HECHO = model.LUGAR_HECHO;
                incidentes.CIRCUNSTANCIA_HECHO = model.CIRCUNSTANCIA_HECHO;
                incidentes.DANOS_PERSONALES = model.DANOS_PERSONALES;
                incidentes.MEDIDAS_ADOPTADAS = model.MEDIDAS_ADOPTADAS;
                incidentes.ESPECIFICACION_VESTUARIO = model.ESPECIFICACION_VESTUARIO;
                incidentes.TIPO_IDENTIFICACIONID = model.TIPO_IDENTIFICACIONID;
                incidentes.PORTABA_ARMA = model.PORTABA_ARMA;
                incidentes.TIPO_ARMAMENTOID = model.TIPO_ARMAMENTOID;
                incidentes.ARMA_FISCAL_PARTICULAR = model.ARMA_FISCAL_PARTICULAR;
                incidentes.DETALLE_PORTE_ARMA = model.DETALLE_PORTE_ARMA;
                incidentes.REALIZA_INVESTIGACION = model.REALIZA_INVESTIGACION;
                incidentes.TIPO_INVESTIGACIONID = model.TIPO_INVESTIGACIONID;
                incidentes.DETALLE_INVESTIGACION = model.DETALLE_INVESTIGACION;
                incidentes.UNIDAD_INTELIGENCIA_ADOPTA = model.UNIDAD_INTELIGENCIA_ADOPTA;
                incidentes.CARACTERISTICAS_MATERIAL = model.CARACTERISTICAS_MATERIAL;
                incidentes.DETALLE_RESULTADO_MUERTE = model.DETALLE_RESULTADO_MUERTE;
                incidentes.DETALLE_DOCUMENTACION_PERDIDA = model.DETALLE_DOCUMENTACION_PERDIDA;
                incidentes.AUTORIDAD_REQUIERE_INFO = model.AUTORIDAD_REQUIERE_INFO;
                incidentes.MEDIDAS_ADOPTADAS_FAMILIARES = model.MEDIDAS_ADOPTADAS_FAMILIARES;
                incidentes.SINTESIS_MEDIOS_PRENSA = model.SINTESIS_MEDIOS_PRENSA;
                incidentes.MEDIDAS_DISCIPLINARIAS = model.MEDIDAS_DISCIPLINARIAS;

                incidentes.USUARIO = int.Parse(User.GetClaimValue("Run"));
                incidentes.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(incidentes)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(incidentes.USUARIO) &&
                            p.Metadata.Name != nameof(incidentes.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "I",                 // Informe Incidentes
                            TIPO_INFORME_ID = incidentesId,
                            TIPO_ANTECEDENTE = "INC",           // Antecedentes Informe Incidentes
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

                return Ok(incidentes);
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

        // Elimina Virtualmente un Informe de Incidentes Estado 0 (Eliminado).

        [HttpDelete("[action]/{preliminaresId},{incidentesId}")]
        public async Task<ActionResult<incidentes>> DeleteInformesIncidentes(
            [FromRoute] int preliminaresId,
            [FromRoute] int incidentesId
            ) 
        {
            try
            {
                incidentes? incidentes = await _sciContext.Incidentes
                .Where(i => i.PRELIMINARESID == preliminaresId && i.ID == incidentesId)
                .FirstOrDefaultAsync();

                if (incidentes == null) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe de Incidente para Eliminar", JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                    );
                    return NotFound();
                }

                incidentes.USUARIO = int.Parse(User.GetClaimValue("Run"));
                incidentes.ACTUALIZACION = DateTime.Now;
                incidentes.ESTADO = 0;


                // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "I",                 // Informe Incidentes
                        TIPO_INFORME_ID = incidentesId,
                        TIPO_ANTECEDENTE = "INC",           // Antecedentes Informe Incidentes
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {incidentes.FECHA_HECHO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = incidentes.CIRCUNSTANCIA_HECHO,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                );

                return Ok(incidentes);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Envia a Resolucion al Cdte. Unidad un Informe de Incidentes, 
        // cambiando su Estado a 1 (No Editable para el S2 y Editable para el CDTE. UNIDAD).
        // y campo DEVUELTO = "N" (No Devuelto) por que el CDTE. UNIDAD.

        [HttpPut("[action]/{preliminaresId},{incidentesId}")]
        public async Task<ActionResult<incidentes>> PutTramiteInformesIncidentes(
            [FromRoute] int preliminaresId,
            [FromRoute] int incidentesId
            ) 
        {
            try
            {

                incidentes? incidentes = await _sciContext.Incidentes
                .Where(i => i.PRELIMINARESID == preliminaresId && i.ID == incidentesId)
                .FirstOrDefaultAsync();

                if (incidentes == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe de Incidente para Tramite.", JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                    );
                    return NotFound();
                }

                incidentes.USUARIO = int.Parse(User.GetClaimValue("Run"));
                incidentes.ACTUALIZACION = DateTime.Now;
                incidentes.DEVUELTO = "N";
                incidentes.ESTADO = 1;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                );

                return Ok(incidentes);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Put", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Devuelve un Informe de Incidentes, 
        // cambiando su Estado a 2 (Editable para el S2 y No Editable para el CDTE. UNIDAD).
        // y campo DEVUELTO = "S" (Devuelto) por que el CDTE. UNIDAD.

        [HttpPut("[action]/{preliminaresId},{incidentesId}")]
        public async Task<ActionResult<incidentes>> PutDevolucionInformesIncidentes(
            PostMotivoDevolucionModel model,
            [FromRoute] int preliminaresId,
            [FromRoute] int incidentesId
            ) 
        {
            try
            {
                incidentes? incidentes = await _sciContext.Incidentes
                .Where(i => i.PRELIMINARESID == preliminaresId && i.ID == incidentesId)
                .FirstOrDefaultAsync();

                if (incidentes == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe de Incidente para Devolucion.", JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                    );
                    return NotFound();
                }

                incidentes.USUARIO = int.Parse(User.GetClaimValue("Run"));
                incidentes.ACTUALIZACION = DateTime.Now;
                incidentes.DEVUELTO = "S";
                incidentes.ESTADO = 2;
                await _sciContext.SaveChangesAsync();

                motivo_devolucion_inc nuevoMotivoDevolucionInc = new motivo_devolucion_inc {
                    INCIDENTESID = incidentesId,
                    FECHA_DEVOLUCION = DateTime.Now,
                    MOTIVO_DEVOLUCION = model.MOTIVO_DEVOLUCION,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.MotivoDevolucionInc.AddAsync(nuevoMotivoDevolucionInc);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                );

                return Ok(incidentes);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Put", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Actualiza Medida Disciplinaria del Informe Incidentes (Proposicion) por parte del CDTE.

        [HttpPut("[action]/{preliminaresId},{incidentesId}")]
        public async Task<ActionResult<incidentes>> PutMedidasDisciplinariasInformesIncidentes(
            PutMedidasDisciplinariasIncidentesModel model, 
            [FromRoute] int preliminaresId,
            [FromRoute] int incidentesId
            ) 
        {
            try
            {
                incidentes? incidentes = await _sciContext.Incidentes
                .Where(i => i.PRELIMINARESID == preliminaresId && i.ID == incidentesId)
                .FirstOrDefaultAsync();

                if (incidentes == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe de Incidente para Medidas Disciplinarias.", JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                    );
                    return NotFound();
                }

                incidentes.MEDIDAS_DISCIPLINARIAS = model.MEDIDAS_DISCIPLINARIAS;
                incidentes.USUARIO = int.Parse(User.GetClaimValue("Run"));
                incidentes.ACTUALIZACION = DateTime.Now;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(incidentes);
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

        // Crea una copia como un nuevo Informe Final con Estado 2 (Editable) para el S2
        // con todos los antecedentes complementarios de Incidentes.
        // y deja el Informe de Incidentes con Estado 3 (No Visualizable).

        [HttpPut("[action]/{preliminaresId},{incidentesId}")]
        public async Task<ActionResult<incidentes>> PutCreaInformeFinal(
            [FromRoute] int preliminaresId,
            [FromRoute] int incidentesId
            ) 
        {
            try
            {
                incidentes? incidentes = await _sciContext.Incidentes
                .Where(i => i.PRELIMINARESID == preliminaresId && i.ID == incidentesId)
                .FirstOrDefaultAsync();

                if (incidentes == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe de Incidente para crear Informe Final.", JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                    );
                    return NotFound();
                }

                incidentes.USUARIO = int.Parse(User.GetClaimValue("Run"));
                incidentes.ACTUALIZACION = DateTime.Now;
                incidentes.ESTADO = 3;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                );

                // Se crea un Informe Final con Estado 2 (Editable) para el S2 con todos los antecedentes complementarios de Incidente.
                // Copia el Incidentes al Final.

                finales nuevoFinales = new finales {

                    INCIDENTESID = incidentes.ID,
                    FECHA_TRAMITE = incidentes.FECHA_PUBLICACION,
                    DEVUELTO = "N",
                    RUT_INFORMANTE = incidentes.RUT_INFORMANTE,
                    UNIDAD_INFORMANTE = incidentes.UNIDAD_INFORMANTE,
                    INCIDENTE_ACCIDENTE = incidentes.INCIDENTE_ACCIDENTE,
                    TIPO_INGESTAID = incidentes.TIPO_INGESTAID,
                    FECHA_INFORMANTE = incidentes.FECHA_INFORMANTE,
                    TIPO_INCIDENTEID = incidentes.TIPO_INCIDENTEID,
                    PERSONAL_UNIDAD = incidentes.PERSONAL_UNIDAD,
                    UNIDAD_AFECTADA = incidentes.UNIDAD_AFECTADA,
                    FECHA_HECHO = incidentes.FECHA_HECHO,
                    LUGAR_HECHO = incidentes.LUGAR_HECHO,
                    CIRCUNSTANCIA_HECHO = incidentes.CIRCUNSTANCIA_HECHO,
                    DANOS_PERSONALES = incidentes.DANOS_PERSONALES,
                    MEDIDAS_ADOPTADAS = incidentes.MEDIDAS_ADOPTADAS,
                    ESPECIFICACION_VESTUARIO = incidentes.ESPECIFICACION_VESTUARIO,
                    TIPO_IDENTIFICACIONID = incidentes.TIPO_IDENTIFICACIONID,
                    PORTABA_ARMA = incidentes.PORTABA_ARMA,
                    TIPO_ARMAMENTOID = incidentes.TIPO_ARMAMENTOID,
                    ARMA_FISCAL_PARTICULAR = incidentes.ARMA_FISCAL_PARTICULAR,
                    DETALLE_PORTE_ARMA = incidentes.DETALLE_PORTE_ARMA,
                    REALIZA_INVESTIGACION = incidentes.REALIZA_INVESTIGACION,
                    TIPO_INVESTIGACIONID = incidentes.TIPO_INVESTIGACIONID,
                    DETALLE_INVESTIGACION = incidentes.DETALLE_INVESTIGACION,
                    UNIDAD_INTELIGENCIA_ADOPTA = incidentes.UNIDAD_INTELIGENCIA_ADOPTA,
                    CARACTERISTICAS_MATERIAL = incidentes.CARACTERISTICAS_MATERIAL,
                    DETALLE_RESULTADO_MUERTE = incidentes.DETALLE_RESULTADO_MUERTE,
                    DETALLE_DOCUMENTACION_PERDIDA = incidentes.DETALLE_DOCUMENTACION_PERDIDA,
                    AUTORIDAD_REQUIERE_INFO = incidentes.AUTORIDAD_REQUIERE_INFO,
                    MEDIDAS_ADOPTADAS_FAMILIARES = incidentes.MEDIDAS_ADOPTADAS_FAMILIARES,
                    SINTESIS_MEDIOS_PRENSA = incidentes.SINTESIS_MEDIOS_PRENSA,
                    MEDIDAS_DISCIPLINARIAS = incidentes.MEDIDAS_DISCIPLINARIAS,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 2
                };

                await _sciContext.Finales.AddAsync(nuevoFinales);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Final: "+nuevoFinales.ID), _usuariosContext
                );

                // Copia Antecedentes Judiciales del Incidente a Antecedentes Judiciales del Finales.

                var AntecedenteJudicialInc = await _sciContext.AntecedentesJudicialesInc
                    .Where(x => x.ESTADO == 1 && x.INCIDENTESID == incidentesId)
                    .AsNoTracking()
                    .ToListAsync();
                var AntecedenteJudicialFin = AntecedenteJudicialInc.Select(x => new antecedentes_judiciales_fin
                {
                    FINALESID = nuevoFinales.ID,
                    FECHA_ANTECEDENTES_JUD = x.FECHA_ANTECEDENTES_JUD,
                    DETALLE_ANTECEDENTES_JUD = x.DETALLE_ANTECEDENTES_JUD,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1 // Activo
                }).ToList();
                _sciContext.AntecedentesJudicialesFin.AddRange(AntecedenteJudicialFin);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Copia de Antecedentes Judiciales del Informe Final: "+nuevoFinales.ID+" Exitoso."), _usuariosContext
                );

                // Copia Archivos Informes del Incidentes a Archivos Informes de Finales.

                var ArchivosInformesInc = await _sciContext.ArchivosInformesInc
                    .Where(x => x.ESTADO == 1 && x.INCIDENTESID == incidentesId)
                    .AsNoTracking()
                    .ToListAsync();
                var ArchivosInformesFin = ArchivosInformesInc.Select(x => new archivos_informes_fin
                {
                    FINALESID = nuevoFinales.ID,
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
                _sciContext.ArchivosInformesFin.AddRange(ArchivosInformesFin);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Copia de Archivos Informes del Informe Final: "+nuevoFinales.ID+" Exitoso."), _usuariosContext
                );

                // Copia Involucrados del Incidente a Involucrados de Finales.

                var InvolucradosInc = await _sciContext.InvolucradosInc
                    .Where(x => x.ESTADO == 1 && x.INCIDENTESID == incidentesId)
                    .AsNoTracking()
                    .ToListAsync();
                var InvolucradosFin = InvolucradosInc.Select(x => new involucrados_fin
                {
                    FINALESID = nuevoFinales.ID,
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
                _sciContext.InvolucradosFin.AddRange(InvolucradosFin);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Copia de Involucrados del Informe Final: "+nuevoFinales.ID+" Exitoso."), _usuariosContext
                );

                // Copia Procedimientos Policiales del Incidente a Procedimientos Policiales de Finales.

                var ProcedimientosPolicialesInc = await _sciContext.ProcedimientosPolicialesInc
                    .Where(x => x.ESTADO == 1 && x.INCIDENTESID == incidentesId)
                    .AsNoTracking()
                    .ToListAsync();
                var ProcedimientosPolicialesFin = ProcedimientosPolicialesInc.Select(x => new procedimientos_policiales_fin
                {
                    FINALESID = nuevoFinales.ID,
                    FECHA_PROCEDIMIENTO_POL = x.FECHA_PROCEDIMIENTO_POL,
                    CONDICION_POLICIALID = x.CONDICION_POLICIALID,
                    DETALLE_PROCEDIMIENTO_POL = x.DETALLE_PROCEDIMIENTO_POL,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1 // Activo
                }).ToList();
                _sciContext.ProcedimientosPolicialesFin.AddRange(ProcedimientosPolicialesFin);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Copia de Procedimientos Policiales del Informe Final: "+nuevoFinales.ID+" Exitoso."), _usuariosContext
                );

                // Copia Ampliacion Informes del Incidente a Ampliacion Informes de Finales.

                var AmpliacionesInformesInc = await _sciContext.AmpliacionesInformesInc
                    .Where(x => x.ESTADO == 1 && x.INCIDENTESID == incidentesId)
                    .AsNoTracking()
                    .ToListAsync();
                var AmpliacionesInformesFin = AmpliacionesInformesInc.Select(x => new ampliaciones_informes_fin
                {
                    FINALESID = nuevoFinales.ID,
                    FECHA_AMPLIACION = x.FECHA_AMPLIACION,
                    DETALLE_AMPLIACION = x.DETALLE_AMPLIACION,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1 // Activo
                }).ToList();
                _sciContext.AmpliacionesInformesFin.AddRange(AmpliacionesInformesFin);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Copia de Ampliacion Informes del Informe Final: "+nuevoFinales.ID+" Exitoso."), _usuariosContext
                );

                return Ok(incidentes);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Put", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Informe Incidente: "+incidentesId), _usuariosContext
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