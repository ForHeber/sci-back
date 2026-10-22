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
    public class MedidasAdoptadasController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public MedidasAdoptadasController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista las Medidas Adoptadas para su selección

        [HttpGet("[action]")]
        public async Task<ActionResult<List<medidas_adoptadas>>> GetListMedidasAdoptadas()
        {
            try
            {
                List<medidas_adoptadas> medidasAdoptadas = await _sciContext.MedidasAdoptadas
                .Where(ma => ma.ESTADO == 1)
                .OrderBy(ma => ma.DESCRIPCION)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(medidasAdoptadas);
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

        // Lista las Medidas Adoptadas para su Mantención

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListMantencionMedidasAdoptadas(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search
        ) {
            try
            {
                var medidasAdoptadas = _sciContext.MedidasAdoptadas
                .Where(ma => ma.ESTADO == 1)
                .OrderBy(ma => ma.DESCRIPCION)          
                .AsQueryable();

                if (!string.IsNullOrEmpty(search)) {

                    medidasAdoptadas = medidasAdoptadas.Where(
                        ma => (ma.DESCRIPCION ?? string.Empty).ToLower().Contains(search!.ToLower())
                    );

                }
                medidasAdoptadas = medidasAdoptadas.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = medidasAdoptadas.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                List<medidas_adoptadas>? respuesta = await medidasAdoptadas
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

        // Inserta una Medida Adoptada Nueva

        [HttpPost("[action]")]
        public async Task<ActionResult<medidas_adoptadas>> PostMantencionMedidasAdoptadas(PostMantencionMedidasAdoptadasModel model) {
            try
            {
                medidas_adoptadas nuevaMedidaAdoptada = new medidas_adoptadas {
                    DESCRIPCION = model.DESCRIPCION,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.MedidasAdoptadas.AddAsync(nuevaMedidaAdoptada);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(nuevaMedidaAdoptada);
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

        // Actualiza una Medida Adoptada

        [HttpPut("[action]/{medidaAdoptadaId}")]
        public async Task<ActionResult<medidas_adoptadas>> PutMantencionMedidasAdoptadas(PutMantencionMedidasAdoptadasModel model, [FromRoute] int medidaAdoptadaId) {
            try
            {
                medidas_adoptadas? medidasAdoptadas = await _sciContext.MedidasAdoptadas
                .Where(i => i.ID == medidaAdoptadaId)
                .FirstOrDefaultAsync();

                if (medidasAdoptadas == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado la Medida Adoptada para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );

                    return NotFound();
                }

                medidasAdoptadas.DESCRIPCION = model.DESCRIPCION;

                medidasAdoptadas.USUARIO = int.Parse(User.GetClaimValue("Run"));
                medidasAdoptadas.ACTUALIZACION = DateTime.Now;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(medidasAdoptadas);
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

        // Elimina una Medida Adoptada

        [HttpDelete("[action]/{medidaAdoptadaId}")]
        public async Task<ActionResult<medidas_adoptadas>> DeleteMantencionMedidasAdoptadas([FromRoute] int medidaAdoptadaId) {
            try
            {
                medidas_adoptadas? medidasAdoptadas = await _sciContext.MedidasAdoptadas
                .Where(i => i.ID == medidaAdoptadaId)
                .FirstOrDefaultAsync();

                if (medidasAdoptadas == null) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado la Medida Adoptada a Eliminar", JsonConvert.SerializeObject("ID Medida Adoptada: "+medidaAdoptadaId), _usuariosContext
                    );

                    return NotFound();
                }

                medidasAdoptadas.USUARIO = int.Parse(User.GetClaimValue("Run"));
                medidasAdoptadas.ACTUALIZACION = DateTime.Now;
                medidasAdoptadas.ESTADO = 0;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Delete", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("ID Medida Adoptada: "+medidaAdoptadaId), _usuariosContext
                );

                return Ok(medidasAdoptadas);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("ID Medida Adoptada: "+medidaAdoptadaId), _usuariosContext
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
// 13-09-2026 23:50