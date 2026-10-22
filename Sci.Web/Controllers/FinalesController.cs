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
    public class FinalesController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public FinalesController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista todos los Informes Finales de la Unidad Afectada Estado = 1 (No Editables), Estado = 2 (Editables).
        // Ordenados por FECHA_TRAMITE de forma Ascendente para que los mas antiguos se muestren primero

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListInformesFinales(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search,
            [FromQuery] string? unidadId,
            [FromQuery] string? tipoEstadoIngresado
        ) {
            try
            {

                if (tipoEstadoIngresado == null || tipoEstadoIngresado == "") { tipoEstadoIngresado = "00"; } 

                var finales = _sciContext.Finales
                .Include(i => i.INCIDENTES)
                    .ThenInclude(i => i!.PRELIMINARES)
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.TIPO_IDENTIFICACION)
                .Include(i => i.TIPO_ARMAMENTO)
                .Include(i => i.TIPO_INVESTIGACION)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => (i.ESTADO == 1 || i.ESTADO == 2) && (i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.FECHA_TRAMITE) 
                .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                { 

                    finales = finales.Where(
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
                    { finales = finales.Where(i => i.ESTADO == 2 && i.DEVUELTO == "N"); } // En Proceso por S2.
                else if (tipoEstadoIngresado == "02")
                    { finales = finales.Where(i => i.ESTADO == 1 ); } // Tramitado al Cdte.
                else if (tipoEstadoIngresado == "03")
                    { finales = finales.Where(i => i.ESTADO == 2 && i.DEVUELTO == "S"); } // Devuelto por Cdte.

                finales = finales.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = finales.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                var respuesta = await finales
                .Skip((int.Parse(string.IsNullOrEmpty(page) ? "1" : page) - 1) * (int)pageResult) // Paginación - salta los resultados de las páginas anteriores a la actuál
                .Take((int)pageResult) // Paginación - selecciona solo la cantidad de registros definidos en la variable pageResult
                .Select(fin => new
                {
                    Final = fin,
                    MotivoDevolucionFin = _sciContext.MotivoDevolucionFin
                        .Where(md => md.FINALESID == fin.ID)
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

        // Lista todos los Informes Finales de la Unidad Afectada Estado = 1 (No Editables) para el CDTE.
        // Ordenados por FECHA_TRAMITE de forma Ascendente para que los mas antiguos se muestren primero

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListInformesFinalesCdte(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search,
            [FromQuery] string? unidadId
        ) {
            try
            {

                var finales = _sciContext.Finales
                .Include(i => i.INCIDENTES)
                    .ThenInclude(i => i!.PRELIMINARES)
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.TIPO_IDENTIFICACION)
                .Include(i => i.TIPO_ARMAMENTO)
                .Include(i => i.TIPO_INVESTIGACION)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => (i.ESTADO == 1) && (i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.FECHA_TRAMITE) 
                .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                { 

                    finales = finales.Where(
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

                finales = finales.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = finales.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                var respuesta = await finales
                .Skip((int.Parse(string.IsNullOrEmpty(page) ? "1" : page) - 1) * (int)pageResult) // Paginación - salta los resultados de las páginas anteriores a la actuál
                .Take((int)pageResult) // Paginación - selecciona solo la cantidad de registros definidos en la variable pageResult
                .Select(fin => new
                {
                    Final = fin,
                    MotivoDevolucionFin = _sciContext.MotivoDevolucionFin
                        .Where(md => md.FINALESID == fin.ID)
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

        // Lista todos los Informes Finales con Estado = 1,2,4 y 5 para la Vista del BINTE.
        // Ordenados por FECHA_HECHO de forma Ascendente para que los mas antiguos se muestren primero
        // Si unidadId es "0", se listan nivel Institucional, de lo contrario se filtran por la unidad especificada

        [HttpGet("[action]/{unidadId}")]
        public async Task<ActionResult> GetListInformesFinalesBINTE(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search,
            [FromRoute] string? unidadId,
            [FromQuery] string? tipoEstadoIngresado
        ) {
            try
            {

                if (tipoEstadoIngresado == null || tipoEstadoIngresado == "") { tipoEstadoIngresado = "00"; } 

                var finales = _sciContext.Finales
                .Include(i => i.INCIDENTES)
                    .ThenInclude(i => i!.PRELIMINARES)
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.TIPO_IDENTIFICACION)
                .Include(i => i.TIPO_ARMAMENTO)
                .Include(i => i.TIPO_INVESTIGACION)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => (i.ESTADO == 1 || i.ESTADO == 2 || i.ESTADO == 4 || i.ESTADO == 5) && (unidadId == "0" || i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.FECHA_TRAMITE) 
                .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                { 

                    finales = finales.Where(
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
                    { finales = finales.Where(i => i.ESTADO == 2 && i.DEVUELTO == "N"); } // En Proceso por S2.
                else if (tipoEstadoIngresado == "02")
                    { finales = finales.Where(i => i.ESTADO == 1 ); } // Tramitado al Cdte.
                else if (tipoEstadoIngresado == "03")
                    { finales = finales.Where(i => i.ESTADO == 2 && i.DEVUELTO == "S"); } // Devuelto por Cdte.
                else if (tipoEstadoIngresado == "04")
                    { finales = finales.Where(i => i.ESTADO == 4 ); } // Cerrado temporalmente.
                else if (tipoEstadoIngresado == "05")
                    { finales = finales.Where(i => i.ESTADO == 5 ); } // Cerrado definitivamente.

                finales = finales.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = finales.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                var respuesta = await finales
                .Skip((int.Parse(string.IsNullOrEmpty(page) ? "1" : page) - 1) * (int)pageResult) // Paginación - salta los resultados de las páginas anteriores a la actuál
                .Take((int)pageResult) // Paginación - selecciona solo la cantidad de registros definidos en la variable pageResult
                .Select(fin => new
                {
                    Final = fin,
                    MotivoDevolucionFin = _sciContext.MotivoDevolucionFin
                        .Where(md => md.FINALESID == fin.ID)
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

        // Actualiza un Informe Final de la Unidad Afectada.

        [HttpPut("[action]/{incidentesId},{finalesId}")]
        public async Task<ActionResult<finales>> PutInformesFinales(
            PutFinalesModel model, 
            [FromRoute] int incidentesId,
            [FromRoute] int finalesId
            ) 
        {
            try
            {
                finales? finales = await _sciContext.Finales
                .Where(i => i.INCIDENTESID == incidentesId && i.ID == finalesId)
                .FirstOrDefaultAsync();

                if (finales == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Final para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                finales.RUT_INFORMANTE = model.RUT_INFORMANTE;
                finales.UNIDAD_INFORMANTE = model.UNIDAD_INFORMANTE;
                finales.INCIDENTE_ACCIDENTE = model.INCIDENTE_ACCIDENTE;
                finales.TIPO_INGESTAID = model.TIPO_INGESTAID;
                finales.FECHA_INFORMANTE = model.FECHA_INFORMANTE;
                finales.TIPO_INCIDENTEID = model.TIPO_INCIDENTEID;
                finales.PERSONAL_UNIDAD = model.PERSONAL_UNIDAD;
                finales.UNIDAD_AFECTADA = model.UNIDAD_AFECTADA;
                finales.FECHA_HECHO = model.FECHA_HECHO;
                finales.LUGAR_HECHO = model.LUGAR_HECHO;
                finales.CIRCUNSTANCIA_HECHO = model.CIRCUNSTANCIA_HECHO;
                finales.DANOS_PERSONALES = model.DANOS_PERSONALES;
                finales.MEDIDAS_ADOPTADAS = model.MEDIDAS_ADOPTADAS;
                finales.ESPECIFICACION_VESTUARIO = model.ESPECIFICACION_VESTUARIO;
                finales.TIPO_IDENTIFICACIONID = model.TIPO_IDENTIFICACIONID;
                finales.PORTABA_ARMA = model.PORTABA_ARMA;
                finales.TIPO_ARMAMENTOID = model.TIPO_ARMAMENTOID;
                finales.ARMA_FISCAL_PARTICULAR = model.ARMA_FISCAL_PARTICULAR;
                finales.DETALLE_PORTE_ARMA = model.DETALLE_PORTE_ARMA;
                finales.REALIZA_INVESTIGACION = model.REALIZA_INVESTIGACION;
                finales.TIPO_INVESTIGACIONID = model.TIPO_INVESTIGACIONID;
                finales.DETALLE_INVESTIGACION = model.DETALLE_INVESTIGACION;
                finales.UNIDAD_INTELIGENCIA_ADOPTA = model.UNIDAD_INTELIGENCIA_ADOPTA;
                finales.CARACTERISTICAS_MATERIAL = model.CARACTERISTICAS_MATERIAL;
                finales.DETALLE_RESULTADO_MUERTE = model.DETALLE_RESULTADO_MUERTE;
                finales.DETALLE_DOCUMENTACION_PERDIDA = model.DETALLE_DOCUMENTACION_PERDIDA;
                finales.AUTORIDAD_REQUIERE_INFO = model.AUTORIDAD_REQUIERE_INFO;
                finales.MEDIDAS_ADOPTADAS_FAMILIARES = model.MEDIDAS_ADOPTADAS_FAMILIARES;
                finales.SINTESIS_MEDIOS_PRENSA = model.SINTESIS_MEDIOS_PRENSA;
                finales.MEDIDAS_DISCIPLINARIAS = model.MEDIDAS_DISCIPLINARIAS;

                finales.USUARIO = int.Parse(User.GetClaimValue("Run"));
                finales.ACTUALIZACION = DateTime.Now;


                // Si el Rol del usuario es Operador BINTE, se registran los cambios realizados.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Detecta los cambios realizados en el objeto antes de guardarlos en la base de datos.
                    _sciContext.ChangeTracker.DetectChanges();

                    // Filtra las propiedades que han sido modificadas, excluyendo USUARIO y ACTUALIZACION.
                    var antecedentesModificados = _sciContext.Entry(finales)
                        .Properties
                        .Where(p =>
                            p.IsModified &&
                            p.Metadata.Name != nameof(finales.USUARIO) &&
                            p.Metadata.Name != nameof(finales.ACTUALIZACION))
                        .ToList();

                    // Crea los eventos de registro para cada propiedad modificada.
                    var eventos = antecedentesModificados
                        .Select(p => new registro_eventos
                        {
                            TIPO_INFORME = "F",                 // Informe Finales
                            TIPO_INFORME_ID = finalesId,
                            TIPO_ANTECEDENTE = "FIN",           // Antecedentes Informe Finales
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

                return Ok(finales);
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

        // Elimina Virtualmente un Informe Final Estado 0 (Eliminado).

        [HttpDelete("[action]/{incidentesId},{finalesId}")]
        public async Task<ActionResult<finales>> DeleteInformesFinales(
            [FromRoute] int incidentesId,
            [FromRoute] int finalesId
            ) 
        {
            try
            {
                finales? finales = await _sciContext.Finales
                .Where(i => i.INCIDENTESID == incidentesId && i.ID == finalesId)
                .FirstOrDefaultAsync();

                if (finales == null) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Final para Eliminar", JsonConvert.SerializeObject("Nro. Informe Incidentes: "+incidentesId), _usuariosContext
                    );
                    return NotFound();
                }

                finales.USUARIO = int.Parse(User.GetClaimValue("Run"));
                finales.ACTUALIZACION = DateTime.Now;
                finales.ESTADO = 0;


                // Si el Rol del usuario es Operador BINTE, se registra la Eliminacion.
                if (int.Parse(User.GetClaimValue("Rol")) == 545) // Rol Operador BINTE
                {
                    // Crea el evento de registro sobre la eliminación.
                    registro_eventos evento = new registro_eventos {

                        TIPO_INFORME = "F",                 // Informe Finales
                        TIPO_INFORME_ID = finalesId,
                        TIPO_ANTECEDENTE = "FIN",           // Antecedentes Informe Finales
                        TIPO_EVENTO = "E",                  // Evento de Eliminación
                        MODIFICADO_POR = int.Parse(User.GetClaimValue("Run")),
                        MODIFICADO_FECHA = DateTime.Now,
                        CONCEPTO = $"ELIMINACION {finales.FECHA_HECHO}",
                        VALOR_ANTERIOR = "No Registra",
                        VALOR_NUEVO = finales.CIRCUNSTANCIA_HECHO,
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
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Final: "+finalesId), _usuariosContext
                );

                return Ok(finales);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Informe Final: "+finalesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Lista todos los Informes Finales con Cierre Temporal de la Unidad Afectada Estado = 4 (Cierre Temporal).
        // Ordenados por FECHA_TRAMITE de forma Ascendente para que los mas antiguos se muestren primero

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListInformesFinalesCierreTemporal(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search,
            [FromQuery] string? unidadId
        ) {
            try
            {

                var finales = _sciContext.Finales
                .Include(i => i.INCIDENTES)
                    .ThenInclude(i => i!.PRELIMINARES)
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.TIPO_IDENTIFICACION)
                .Include(i => i.TIPO_ARMAMENTO)
                .Include(i => i.TIPO_INVESTIGACION)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => (i.ESTADO == 4) && (i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.FECHA_TRAMITE) 
                .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                { 

                    finales = finales.Where(
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

                finales = finales.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = finales.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                var respuesta = await finales
                .Skip((int.Parse(string.IsNullOrEmpty(page) ? "1" : page) - 1) * (int)pageResult) // Paginación - salta los resultados de las páginas anteriores a la actuál
                .Take((int)pageResult) // Paginación - selecciona solo la cantidad de registros definidos en la variable pageResult
                .Select(fin => new
                {
                    Final = fin,
                    MotivoDevolucionFin = _sciContext.MotivoDevolucionFin
                        .Where(md => md.FINALESID == fin.ID)
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

        // Lista todos los Informes Finales Cerrados de la Unidad Afectada Estado = 5 (Cerrados).
        // Ordenados por FECHA_TRAMITE de forma Ascendente para que los mas antiguos se muestren primero

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListInformesFinalesCerrados(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search,
            [FromQuery] string? unidadId
        ) {
            try
            {

                var finales = _sciContext.Finales
                .Include(i => i.INCIDENTES)
                    .ThenInclude(i => i!.PRELIMINARES)
                .Include(i => i.TIPO_INGESTA)
                .Include(i => i.TIPO_INCIDENTE)
                .Include(i => i.TIPO_IDENTIFICACION)
                .Include(i => i.TIPO_ARMAMENTO)
                .Include(i => i.TIPO_INVESTIGACION)
                .Include(i => i.INFORMANTE)
                .Include(i => i.U_INFORMANTE)
                .Include(i => i.U_AFECTADA)
                .Where(i => (i.ESTADO == 5) && (i.UNIDAD_AFECTADA!.ToString() == unidadId))
                .OrderBy(i => i.FECHA_TRAMITE) 
                .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                { 

                    finales = finales.Where(
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

                finales = finales.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = finales.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                var respuesta = await finales
                .Skip((int.Parse(string.IsNullOrEmpty(page) ? "1" : page) - 1) * (int)pageResult) // Paginación - salta los resultados de las páginas anteriores a la actuál
                .Take((int)pageResult) // Paginación - selecciona solo la cantidad de registros definidos en la variable pageResult
                .Select(fin => new
                {
                    Final = fin,
                    MotivoDevolucionFin = _sciContext.MotivoDevolucionFin
                        .Where(md => md.FINALESID == fin.ID)
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

        // Envia a Resolucion al Cdte. Unidad un Informe Final, 
        // cambiando su Estado a 1 (No Editable para el S2 y Editable para el CDTE. UNIDAD).
        // y campo DEVUELTO = "N" (No Devuelto) por que el CDTE. UNIDAD.

        [HttpPut("[action]/{incidentesId},{finalesId}")]
        public async Task<ActionResult<finales>> PutTramiteInformesFinales(
            [FromRoute] int incidentesId,
            [FromRoute] int finalesId
            ) 
        {
            try
            {

                finales? finales = await _sciContext.Finales
                .Where(i => i.INCIDENTESID == incidentesId && i.ID == finalesId)
                .FirstOrDefaultAsync();

                if (finales == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Final para Tramite.", JsonConvert.SerializeObject("Nro. Informe Final: "+finalesId), _usuariosContext
                    );
                    return NotFound();
                }

                finales.USUARIO = int.Parse(User.GetClaimValue("Run"));
                finales.ACTUALIZACION = DateTime.Now;
                finales.DEVUELTO = "N";
                finales.ESTADO = 1;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Final: "+finalesId), _usuariosContext
                );

                return Ok(finales);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Put", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Informe Final: "+finalesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Devuelve un Informe Final, 
        // cambiando su Estado a 2 (Editable para el S2 y No Editable para el CDTE. UNIDAD).
        // y campo DEVUELTO = "S" (Devuelto) por que el CDTE. UNIDAD.

        [HttpPut("[action]/{incidentesId},{finalesId}")]
        public async Task<ActionResult<finales>> PutDevolucionInformesFinales(
            PostMotivoDevolucionModel model,
            [FromRoute] int incidentesId,
            [FromRoute] int finalesId
            ) 
        {
            try
            {
                finales? finales = await _sciContext.Finales
                .Where(i => i.INCIDENTESID == incidentesId && i.ID == finalesId)
                .FirstOrDefaultAsync();

                if (finales == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Final para Devolución.", JsonConvert.SerializeObject("Nro. Informe Final: "+finalesId), _usuariosContext
                    );
                    return NotFound();
                }

                finales.USUARIO = int.Parse(User.GetClaimValue("Run"));
                finales.ACTUALIZACION = DateTime.Now;
                finales.DEVUELTO = "S";
                finales.ESTADO = 2;
                await _sciContext.SaveChangesAsync();

                motivo_devolucion_fin nuevoMotivoDevolucionFin = new motivo_devolucion_fin {
                    FINALESID = finalesId,
                    FECHA_DEVOLUCION = DateTime.Now,
                    MOTIVO_DEVOLUCION = model.MOTIVO_DEVOLUCION,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.MotivoDevolucionFin.AddAsync(nuevoMotivoDevolucionFin);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("Nro. Informe Final: "+finalesId), _usuariosContext
                );

                return Ok(finales);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Put", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("Nro. Informe Final: "+finalesId), _usuariosContext
                );
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _sciContext.Dispose();
                _usuariosContext.Dispose();
            }
        }

        // Actualiza Medida Disciplinaria del Informe Final por parte del CDTE.

        [HttpPut("[action]/{incidentesId},{finalesId}")]
        public async Task<ActionResult<finales>> PutMedidasDisciplinariasInformesFinales(
            PutMedidasDisciplinariasFinalesModel model, 
            [FromRoute] int incidentesId,
            [FromRoute] int finalesId
            ) 
        {
            try
            {
                finales? finales = await _sciContext.Finales
                .Where(f => f.ID == finalesId && f.INCIDENTESID == incidentesId)
                .FirstOrDefaultAsync();

                if (finales == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Final para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );
                    return NotFound();
                }

                finales.MEDIDAS_DISCIPLINARIAS = model.MEDIDAS_DISCIPLINARIAS;
                finales.USUARIO = int.Parse(User.GetClaimValue("Run"));
                finales.ACTUALIZACION = DateTime.Now;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(finales);
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

        // Cierre Temporal de Informe Final por parte del S2.
        // cambia el Estado = 4 (Cierre Temporal).

        [HttpPut("[action]/{incidentesId},{finalesId}")]
        public async Task<ActionResult<finales>> PutCierreTemporalInformesFinales(
            [FromRoute] int incidentesId,
            [FromRoute] int finalesId
            ) 
        {
            try
            {
                finales? finales = await _sciContext.Finales
                .Where(f => f.ID == finalesId && f.INCIDENTESID == incidentesId)
                .FirstOrDefaultAsync();

                if (finales == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Final para su Cierre Temporal", JsonConvert.SerializeObject(""), _usuariosContext
                    );
                    return NotFound();
                }

                finales.USUARIO = int.Parse(User.GetClaimValue("Run"));
                finales.ACTUALIZACION = DateTime.Now;
                finales.ESTADO = 4;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(finales);
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

        // Recuperación de Informe Final por parte del S2.
        // cambia el Estado = 2 (Recuperación).

        [HttpPut("[action]/{incidentesId},{finalesId}")]
        public async Task<ActionResult<finales>> PutRecuperaInformesFinales(
            [FromRoute] int incidentesId,
            [FromRoute] int finalesId
            ) 
        {
            try
            {
                finales? finales = await _sciContext.Finales
                .Where(f => f.ID == finalesId && f.INCIDENTESID == incidentesId)
                .FirstOrDefaultAsync();

                if (finales == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Final para su Recuperación", JsonConvert.SerializeObject(""), _usuariosContext
                    );
                    return NotFound();
                }

                finales.USUARIO = int.Parse(User.GetClaimValue("Run"));
                finales.ACTUALIZACION = DateTime.Now;
                finales.ESTADO = 2;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(finales);
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

        // Extrae Medidas Disciplinarias del Informe Final (Solo las que posee el Informe).

        [HttpGet("[action]/{finalesId}")]
        public async Task<ActionResult<medidas_adoptadas_fin>> GetListMedidasDisciplinariasInformesFinalesEspecificas(
            [FromRoute] int finalesId
            ) 
        {
            try
            {
                // Recupera Medidas Adoptadas del Informe Final.

                var medidasAdoptadasDelInforme = await _sciContext.MedidasAdoptadasFin
                    .Include(madi => madi.MEDIDAS_ADOPTADAS)
                    .Where(madi => madi.FINALESID == finalesId && madi.ESTADO == 1)
                    .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new { medidasAdoptadasDelInforme });
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

        // Extrae Medidas Disciplinarias del Informe Final (Todas las existentes con True o False si las tiene o no).

        [HttpGet("[action]/{finalesId}")]
        public async Task<ActionResult<finales>> GetListMedidasDisciplinariasInformesFinales(
            [FromRoute] int finalesId
            ) 
        {
            try
            {
                // Recupera Medidas Adoptadas Predefinidas.

                var medidasAdoptadas = await _sciContext.MedidasAdoptadas
                .Where(ma => ma.ESTADO == 1)
                .ToListAsync();

                // Recupera Medidas Adoptadas del Informe Final.

                var medidasAdoptadasIdsEnFinal = await _sciContext.MedidasAdoptadasFin
                    .Where(maf => maf.FINALESID == finalesId && maf.ESTADO == 1)
                    .Select(maf => maf.MEDIDAS_ADOPTADASID)
                    .ToListAsync();

                // Combina las Medidas Adoptadas Predefinidas con las Medidas Adoptadas del Informe Final.

                var medidasAdoptadasConEstado = medidasAdoptadas
                    .Select(ma => new
                    {
                        ma.ID,
                        ma.DESCRIPCION,
                        estaAdoptada = medidasAdoptadasIdsEnFinal.Contains(ma.ID)
                    })
                    .ToList();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new { medidasAdoptadasConEstado });
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

        // Cierre de Informe Final por parte del S2.
        // cambia el Estado = 5 (Cierre).

        [HttpPut("[action]/{incidentesId},{finalesId}")]
        public async Task<ActionResult<finales>> PutCierreInformesFinales(
            PutListaMedidasDisciplinariasFinalesModel model,
            [FromRoute] int incidentesId,
            [FromRoute] int finalesId
            ) 
        {
            try
            {
                finales? finales = await _sciContext.Finales
                .Where(f => f.ID == finalesId && f.INCIDENTESID == incidentesId)
                .FirstOrDefaultAsync();

                if (finales == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Informe Final para su Cierre", JsonConvert.SerializeObject(""), _usuariosContext
                    );
                    return NotFound();
                }

                // Actualiza el Informe Final con el Cierre Estado = 5 (Cierre).

                finales.USUARIO = int.Parse(User.GetClaimValue("Run"));
                finales.ACTUALIZACION = DateTime.Now;
                finales.ESTADO = 5;
                await _sciContext.SaveChangesAsync();

                // Rescata las Medidas Adoptadas del Informe Final y las desactiva (ESTADO = 0).

                var medidasExistentes = await _sciContext.MedidasAdoptadasFin
                    .Where(maf => maf.FINALESID == finalesId)
                    .ToListAsync();

                foreach (var mExistente in medidasExistentes)
                {
                    mExistente.USUARIO = int.Parse(User.GetClaimValue("Run"));
                    mExistente.ACTUALIZACION = DateTime.Now;
                    mExistente.ESTADO = 0;
                }
                await _sciContext.SaveChangesAsync();

                // Rescata las Medidas Adoptadas recibidas del Model y las agrega al Informe Final.

                var medidasRecibidas = model.MEDIDAS
                    .Select(g => new { g.ID, g.ESTA_ADOPTADA })
                    .ToList();

                foreach (var mRecibida in medidasRecibidas)
                {
                    if (mRecibida.ESTA_ADOPTADA)
                    {
                        var nuevaMedida = new medidas_adoptadas_fin
                        {
                            FINALESID = finalesId,
                            MEDIDAS_ADOPTADASID = mRecibida.ID,
                            PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                            CREACION = DateTime.Now,
                            USUARIO = int.Parse(User.GetClaimValue("Run")),
                            ACTUALIZACION = DateTime.Now,
                            ESTADO = 1
                        };
                        await _sciContext.MedidasAdoptadasFin.AddAsync(nuevaMedida);
                    }
                }
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new { finales, medidasRecibidas, medidasExistentes });
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
// 28-09-2026 22:59...