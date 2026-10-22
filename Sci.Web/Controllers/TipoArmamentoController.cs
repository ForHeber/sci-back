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
    public class TipoArmamentoController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public TipoArmamentoController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista los Tipos de Armamento para su selección

        [HttpGet("[action]")]
        public async Task<ActionResult<List<tipo_armamento>>> GetListTipoArmamento()
        {
            try
            {
                List<tipo_armamento> tipo_armamento = await _sciContext.TipoArmamento
                .Where(ta => ta.ESTADO == 1)
                .OrderBy(ta => ta.DESCRIPCION)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(tipo_armamento);
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

        // Lista los Tipos de Armamento para su Mantención

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListMantencionTipoArmamento(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search
        ) {
            try
            {
                var tiposArmamentos = _sciContext.TipoArmamento
                .Where(i => i.ESTADO == 1)
                .OrderBy(i => i.DESCRIPCION)          
                .AsQueryable();

                if (!string.IsNullOrEmpty(search)) {

                    tiposArmamentos = tiposArmamentos.Where(
                        ti => (ti.DESCRIPCION ?? string.Empty).ToLower().Contains(search!.ToLower())
                    );

                }
                tiposArmamentos = tiposArmamentos.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = tiposArmamentos.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                List<tipo_armamento>? respuesta = await tiposArmamentos
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

        // Inserta un Tipo de Armamento Nuevo

        [HttpPost("[action]")]
        public async Task<ActionResult<tipo_armamento>> PostMantencionTipoArmamento(PostMantencionTipoArmamentoModel model) {
            try
            {
                tipo_armamento nuevoTipoArmamento = new tipo_armamento {
                    DESCRIPCION = model.DESCRIPCION,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.TipoArmamento.AddAsync(nuevoTipoArmamento);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(nuevoTipoArmamento);
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

        // Actualiza un Tipo de Armamento

        [HttpPut("[action]/{tipoArmamentoId}")]
        public async Task<ActionResult<tipo_armamento>> PutMantencionTipoArmamento(PutMantencionTipoArmamentoModel model, [FromRoute] int tipoArmamentoId) {
            try
            {
                tipo_armamento? tiposArmamentos = await _sciContext.TipoArmamento
                .Where(i => i.ID == tipoArmamentoId)
                .FirstOrDefaultAsync();

                if (tiposArmamentos == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Tipo de Armamento para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );

                    return NotFound();
                }

                tiposArmamentos.DESCRIPCION = model.DESCRIPCION;

                tiposArmamentos.USUARIO = int.Parse(User.GetClaimValue("Run"));
                tiposArmamentos.ACTUALIZACION = DateTime.Now;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(tiposArmamentos);
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

        // Elimina un Tipo de Armamento

        [HttpDelete("[action]/{tipoArmamentoId}")]
        public async Task<ActionResult<tipo_armamento>> DeleteMantencionTipoArmamento([FromRoute] int tipoArmamentoId) {
            try
            {
                tipo_armamento? tiposArmamentos = await _sciContext.TipoArmamento
                .Where(i => i.ID == tipoArmamentoId)
                .FirstOrDefaultAsync();

                if (tiposArmamentos == null) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Tipo de Armamento a Eliminar", JsonConvert.SerializeObject("ID Tipo Armamento: "+tipoArmamentoId), _usuariosContext
                    );

                    return NotFound();
                }

                tiposArmamentos.USUARIO = int.Parse(User.GetClaimValue("Run"));
                tiposArmamentos.ACTUALIZACION = DateTime.Now;
                tiposArmamentos.ESTADO = 0;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Delete", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("ID Tipo Armamento: "+tipoArmamentoId), _usuariosContext
                );

                return Ok(tiposArmamentos);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("ID Tipo Armamento: "+tipoArmamentoId), _usuariosContext
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
