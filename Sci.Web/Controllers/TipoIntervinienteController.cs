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
    public class TipoIntervinienteController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public TipoIntervinienteController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista los Tipos de Intervinientes para su selección

        [HttpGet("[action]")]
        public async Task<ActionResult<List<tipo_interviniente>>> GetListTipoInterviniente() {
            try
            {
                List<tipo_interviniente> tipo_interviniente = await _sciContext.TipoInterviniente
                .Where(ti => ti.ESTADO == 1)
                .OrderBy(ti => ti.DESCRIPCION)
                .ToListAsync();
                
                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(tipo_interviniente);
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

        // Lista los Tipos de Intervinientes para su Mantención

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListMantencionTipoInterviniente(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search
        ) {
            try
            {
                var tiposIntervinientes = _sciContext.TipoInterviniente
                .Where(i => i.ESTADO == 1)
                .OrderBy(i => i.DESCRIPCION)          
                .AsQueryable();

                if (!string.IsNullOrEmpty(search)) {

                    tiposIntervinientes = tiposIntervinientes.Where(
                        ti => (ti.DESCRIPCION ?? string.Empty).ToLower().Contains(search!.ToLower())
                    );

                }
                tiposIntervinientes = tiposIntervinientes.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = tiposIntervinientes.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                List<tipo_interviniente>? respuesta = await tiposIntervinientes
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

        // Inserta un Tipo de Interviniente Nuevo

        [HttpPost("[action]")]
        public async Task<ActionResult<tipo_interviniente>> PostMantencionTipoInterviniente(PostMantencionTipoIntervinienteModel model) {
            try
            {
                tipo_interviniente nuevoTipoInterviniente = new tipo_interviniente {
                    DESCRIPCION = model.DESCRIPCION,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.TipoInterviniente.AddAsync(nuevoTipoInterviniente);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(nuevoTipoInterviniente);
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

        // Actualiza un Tipo de Interviniente

        [HttpPut("[action]/{tipoIntervinienteId}")]
        public async Task<ActionResult<tipo_interviniente>> PutMantencionTipoInterviniente(PutMantencionTipoIntervinienteModel model, [FromRoute] int tipoIntervinienteId) {
            try
            {
                tipo_interviniente? tiposIntervinientes = await _sciContext.TipoInterviniente
                .Where(i => i.ID == tipoIntervinienteId)
                .FirstOrDefaultAsync();

                if (tiposIntervinientes == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Tipo de Interviniente para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );

                    return NotFound();
                }

                tiposIntervinientes.DESCRIPCION = model.DESCRIPCION;

                tiposIntervinientes.USUARIO = int.Parse(User.GetClaimValue("Run"));
                tiposIntervinientes.ACTUALIZACION = DateTime.Now;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(tiposIntervinientes);
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

        // Elimina un Tipo de Intervinientes

        [HttpDelete("[action]/{tipoIntervinienteId}")]
        public async Task<ActionResult<tipo_interviniente>> DeleteMantencionTipoInterviniente([FromRoute] int tipoIntervinienteId) {
            try
            {
                tipo_interviniente? tiposIntervinientes = await _sciContext.TipoInterviniente
                .Where(i => i.ID == tipoIntervinienteId)
                .FirstOrDefaultAsync();

                if (tiposIntervinientes == null) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Tipo de Interviniente a Eliminar", JsonConvert.SerializeObject("ID Tipo Interviniente: "+tipoIntervinienteId), _usuariosContext
                    );

                    return NotFound();
                }

                tiposIntervinientes.USUARIO = int.Parse(User.GetClaimValue("Run"));
                tiposIntervinientes.ACTUALIZACION = DateTime.Now;
                tiposIntervinientes.ESTADO = 0;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Delete", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("ID Tipo Interviniente: "+tipoIntervinienteId), _usuariosContext
                );

                return Ok(tiposIntervinientes);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("ID Tipo Interviniente: "+tipoIntervinienteId), _usuariosContext
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
