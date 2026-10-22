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
    public class TipoIngestaController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public TipoIngestaController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista los Tipos de Ingesta para su selección

        [HttpGet("[action]")]
        public async Task<ActionResult<List<tipo_ingesta>>> GetListTipoIngesta() {
            try
            {
                List<tipo_ingesta> tipo_ingesta = await _sciContext.TipoIngesta
                .Where(ti => ti.ESTADO == 1)
                .OrderBy(ti => ti.DESCRIPCION)
                .ToListAsync();
                
                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(tipo_ingesta);
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

        // Lista los Tipos de Ingesta para su Mantención

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListMantencionTipoIngesta(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search
        ) {
            try
            {
                var tiposIngesta = _sciContext.TipoIngesta
                .Where(i => i.ESTADO == 1)
                .OrderBy(i => i.DESCRIPCION)          
                .AsQueryable();

                if (!string.IsNullOrEmpty(search)) {

                    tiposIngesta = tiposIngesta.Where(
                        ti => ( ti.DESCRIPCION ?? string.Empty).ToLower().Contains(search!.ToLower())
                    );

                }
                tiposIngesta = tiposIngesta.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = tiposIngesta.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                List<tipo_ingesta>? respuesta = await tiposIngesta
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

        // Inserta un Tipo de Ingesta Nuevo

        [HttpPost("[action]")]
        public async Task<ActionResult<tipo_ingesta>> PostMantencionTipoIngesta(
            PostMantencionTipoIngestaModel model
            ) 
        {
            try
            {
                tipo_ingesta nuevoTipoIngesta = new tipo_ingesta {
                    DESCRIPCION = model.DESCRIPCION,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.TipoIngesta.AddAsync(nuevoTipoIngesta);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(nuevoTipoIngesta);
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

        // Actualiza un Tipo de Ingesta

        [HttpPut("[action]/{tipoIngestaId}")]
        public async Task<ActionResult<tipo_ingesta>> PutMantencionTipoIngesta(
            PutMantencionTipoIngestaModel model, 
            [FromRoute] int tipoIngestaId
            ) 
        {
            try
            {
                tipo_ingesta? tiposIngesta = await _sciContext.TipoIngesta
                .Where(i => i.ID == tipoIngestaId)
                .FirstOrDefaultAsync();

                if (tiposIngesta == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Tipo de Ingesta para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );

                    return NotFound();
                }

                tiposIngesta.DESCRIPCION = model.DESCRIPCION;

                tiposIngesta.USUARIO = int.Parse(User.GetClaimValue("Run"));
                tiposIngesta.ACTUALIZACION = DateTime.Now;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(tiposIngesta);
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

        // Elimina un Tipo de Ingesta

        [HttpDelete("[action]/{tipoIngestaId}")]
        public async Task<ActionResult<tipo_ingesta>> DeleteMantencionTipoIngesta(
            [FromRoute] int tipoIngestaId
            ) 
        {
            try
            {
                tipo_ingesta? tiposIngesta = await _sciContext.TipoIngesta
                .Where(i => i.ID == tipoIngestaId)
                .FirstOrDefaultAsync();

                if (tiposIngesta == null) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Tipo de Ingesta a Eliminar", JsonConvert.SerializeObject("ID Tipo Ingesta: "+tipoIngestaId), _usuariosContext
                    );

                    return NotFound();
                }

                tiposIngesta.USUARIO = int.Parse(User.GetClaimValue("Run"));
                tiposIngesta.ACTUALIZACION = DateTime.Now;
                tiposIngesta.ESTADO = 0;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Delete", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("ID Tipo Ingesta: "+tipoIngestaId), _usuariosContext
                );

                return Ok(tiposIngesta);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("ID Tipo Ingesta: "+tipoIngestaId), _usuariosContext
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
