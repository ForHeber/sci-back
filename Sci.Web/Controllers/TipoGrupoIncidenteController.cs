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
    public class TipoGrupoIncidenteController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public TipoGrupoIncidenteController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista los Tipos de Grupo de Incidentes para su selección

        [HttpGet("[action]")]
        public async Task<ActionResult<List<tipo_grupo_incidente>>> GetListTipoGrupoIncidente() {
            try
            {
                List<tipo_grupo_incidente> tipo_grupo_incidente = await _sciContext.TipoGrupoIncidente
                .Where(tgi => tgi.ESTADO == 1)
                .OrderBy(tgi => tgi.DESCRIPCION)
                .ToListAsync();
                
                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(tipo_grupo_incidente);
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

        // Lista los Tipos de Grupo de Incidentes para su Mantención

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListMantencionTipoGrupoIncidente(
            [FromQuery] string? page,
            [FromQuery] string? items,
            [FromQuery] string? search
        ) {
            try
            {
                var tiposGruposIncidentes = _sciContext.TipoGrupoIncidente
                .Where(tgi => tgi.ESTADO == 1)
                .OrderBy(tgi => tgi.DESCRIPCION)          
                .AsQueryable();

                if (!string.IsNullOrEmpty(search)) {

                    tiposGruposIncidentes = tiposGruposIncidentes.Where(
                        tgi => (tgi.DESCRIPCION ?? string.Empty).ToLower().Contains(search!.ToLower())
                    );

                }
                tiposGruposIncidentes = tiposGruposIncidentes.AsQueryable();

                //Inicializa variables de paginación
                int totalRegistros = tiposGruposIncidentes.Count();
                double pageResult = string.IsNullOrEmpty(items) ? totalRegistros : double.Parse(items);
                double pageCount = Math.Ceiling(totalRegistros / pageResult);

                List<tipo_grupo_incidente>? respuesta = await tiposGruposIncidentes
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

        // Inserta un Tipo de Grupo de Incidente Nuevo

        [HttpPost("[action]")]
        public async Task<ActionResult<tipo_grupo_incidente>> PostMantencionTipoGrupoIncidente(PostMantencionTipoGrupoIncidenteModel model) {
            try
            {
                tipo_grupo_incidente nuevoTipoGrupoIncidente = new tipo_grupo_incidente {
                    DESCRIPCION = model.DESCRIPCION,
                    COLOR = model.COLOR,

                    PROPIETARIO = int.Parse(User.GetClaimValue("Run")),
                    CREACION = DateTime.Now,
                    USUARIO = int.Parse(User.GetClaimValue("Run")),
                    ACTUALIZACION = DateTime.Now,
                    ESTADO = 1
                };

                await _sciContext.TipoGrupoIncidente.AddAsync(nuevoTipoGrupoIncidente);
                await _sciContext.SaveChangesAsync();

                Auditor.registroAuditor(
                    "Post", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(nuevoTipoGrupoIncidente);
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

        // Actualiza un Tipo de Grupo de Incidente

        [HttpPut("[action]/{tipoGrupoIncidenteId}")]
        public async Task<ActionResult<tipo_grupo_incidente>> PutMantencionTipoGrupoIncidente(PutMantencionTipoGrupoIncidenteModel model, [FromRoute] int tipoGrupoIncidenteId) {
            try
            {
                tipo_grupo_incidente? tiposGruposIncidentes = await _sciContext.TipoGrupoIncidente
                .Where(tgi => tgi.ID == tipoGrupoIncidenteId)
                .FirstOrDefaultAsync();

                if (tiposGruposIncidentes == null) {
                    Auditor.registroAuditor(
                        "Put", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Tipo de Grupo de Incidente para Actualizar", JsonConvert.SerializeObject(model), _usuariosContext
                    );

                    return NotFound();
                }

                tiposGruposIncidentes.DESCRIPCION = model.DESCRIPCION;
                tiposGruposIncidentes.COLOR = model.COLOR;

                tiposGruposIncidentes.USUARIO = int.Parse(User.GetClaimValue("Run"));
                tiposGruposIncidentes.ACTUALIZACION = DateTime.Now;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Put", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(model), _usuariosContext
                );

                return Ok(tiposGruposIncidentes);
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

        // Elimina un Tipo de Grupo de Incidente

        [HttpDelete("[action]/{tipoGrupoIncidenteId}")]
        public async Task<ActionResult<tipo_grupo_incidente>> DeleteMantencionTipoGrupoIncidente([FromRoute] int tipoGrupoIncidenteId) {
            try
            {
                tipo_grupo_incidente? tiposGruposIncidentes = await _sciContext.TipoGrupoIncidente
                .Where(tgi => tgi.ID == tipoGrupoIncidenteId)
                .FirstOrDefaultAsync();

                if (tiposGruposIncidentes == null) {
                    Auditor.registroAuditor(
                        "Delete", 404, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                        Util.GetActionName(ControllerContext), "No se ha encontrado el Tipo de Grupo de Incidente a Eliminar", JsonConvert.SerializeObject("ID Tipo Grupo Incidente: "+tipoGrupoIncidenteId), _usuariosContext
                    );

                    return NotFound();
                }

                tiposGruposIncidentes.USUARIO = int.Parse(User.GetClaimValue("Run"));
                tiposGruposIncidentes.ACTUALIZACION = DateTime.Now;
                tiposGruposIncidentes.ESTADO = 0;
                await _sciContext.SaveChangesAsync();
                
                Auditor.registroAuditor(
                    "Delete", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject("ID Tipo Grupo Incidente: "+tipoGrupoIncidenteId), _usuariosContext
                );

                return Ok(tiposGruposIncidentes);
            }
            catch (Exception ex)
            {
                Auditor.registroAuditor(
                "Delete", 500, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                Util.GetActionName(ControllerContext), ex.Message, JsonConvert.SerializeObject("ID Tipo Grupo Incidente: "+tipoGrupoIncidenteId), _usuariosContext
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
