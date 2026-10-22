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
    public class TipoIdentificacionController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public TipoIdentificacionController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista los Tipos de Identificacion para su selección

        [HttpGet("[action]")]
        public async Task<ActionResult<List<tipo_identificacion>>> GetListTipoIdentificacion() {
            try
            {
                List<tipo_identificacion> tipo_identificacion = await _sciContext.TipoIdentificacion
                .Where(ti => ti.ESTADO == 1)
                .OrderBy(ti => ti.DESCRIPCION)
                .ToListAsync();
                
                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(tipo_identificacion);
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

        // Lista los Tipos de Identificacion para su Mantención

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListMantencionTipoIdentificacion(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search
        ) {
            try
            {
                var tiposIdentificaciones = _sciContext.TipoIdentificacion
                .Where(i => i.ESTADO == 1)
                .OrderBy(i => i.DESCRIPCION)          
                .AsQueryable();

                if (!string.IsNullOrEmpty(search)) {

                    tiposIdentificaciones = tiposIdentificaciones.Where(
                        ti => ( ti.DESCRIPCION ?? string.Empty).ToLower().Contains(search!.ToLower())
                    );

                }
                tiposIdentificaciones = tiposIdentificaciones.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = tiposIdentificaciones.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                List<tipo_identificacion>? respuesta = await tiposIdentificaciones
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

        // Inserta un Tipo de Identificacion Nuevo

        [HttpPost("[action]")]
        public async Task<ActionResult<tipo_identificacion>> PostMantencionTipoIdentificacion(PostMantencionTipoIdentificacionModel model) {
            try
            {
                tipo_identificacion nuevoTipoIdentificacion = new tipo_identificacion {
                    DESCRIPCION = model.DESCRIPCION,
                    SIGLA = model.SIGLA,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.TipoIdentificacion.AddAsync(nuevoTipoIdentificacion);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(nuevoTipoIdentificacion);
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

        // Actualiza un Tipo de Identificacion

        [HttpPut("[action]/{tipoIdentificacionId}")]
        public async Task<ActionResult<tipo_identificacion>> PutMantencionTipoIdentificacion(PutMantencionTipoIdentificacionModel model, [FromRoute] int tipoIdentificacionId) {
            try
            {
                tipo_identificacion? tiposIdentificaciones = await _sciContext.TipoIdentificacion
                .Where(i => i.ID == tipoIdentificacionId)
                .FirstOrDefaultAsync();

                if (tiposIdentificaciones == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Tipo de Identificación para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );

                    return NotFound();
                }

                tiposIdentificaciones.DESCRIPCION = model.DESCRIPCION;
                tiposIdentificaciones.SIGLA = model.SIGLA;

                tiposIdentificaciones.USUARIO = int.Parse(User.GetClaimValue("Run"));
                tiposIdentificaciones.ACTUALIZACION = DateTime.Now;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(tiposIdentificaciones);
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

        // Elimina un Tipo de Identificacion

        [HttpDelete("[action]/{tipoIdentificacionId}")]
        public async Task<ActionResult<tipo_identificacion>> DeleteMantencionTipoIdentificacion([FromRoute] int tipoIdentificacionId) {
            try
            {
                tipo_identificacion? tiposIdentificaciones = await _sciContext.TipoIdentificacion
                .Where(i => i.ID == tipoIdentificacionId)
                .FirstOrDefaultAsync();

                if (tiposIdentificaciones == null) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Tipo de Indentificación a Eliminar", JsonConvert.SerializeObject("ID Tipo Identificación: "+tipoIdentificacionId), _usuariosContext
                    );

                    return NotFound();
                }

                tiposIdentificaciones.USUARIO = int.Parse(User.GetClaimValue("Run"));
                tiposIdentificaciones.ACTUALIZACION = DateTime.Now;
                tiposIdentificaciones.ESTADO = 0;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Delete", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("ID Tipo Identificación: "+tipoIdentificacionId), _usuariosContext
                );

                return Ok(tiposIdentificaciones);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("ID Tipo Identificación: "+tipoIdentificacionId), _usuariosContext
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
