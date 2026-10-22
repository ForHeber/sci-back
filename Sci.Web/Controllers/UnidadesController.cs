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
    public class UnidadesController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public UnidadesController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Lista todas las Unidades para el SCI

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListUnidades() {
            try
            {

                List<unidad> listaUnidades = await _sciContext.Unidades
                .OrderBy(lu => lu.DESCRIPCION)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Lista de Unidades para El SCI", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {
                    listaUnidades
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

        // Lista todas las Unidades que existen en los informes de incidentes, para el SCI

        [HttpGet("[action]")]
        public async Task<ActionResult> GetListUnidadesIncidentes() {
            try
            {

                var listaUnidadesIncidentes = await _sciContext.Incidentes
                .AsNoTracking()
                .Where(i => !string.IsNullOrEmpty(i.UNIDAD_AFECTADA) && i.U_AFECTADA != null)
                .Select(i => new
                {
                    CODIGO = i.UNIDAD_AFECTADA,
                    SIGLA = i.U_AFECTADA!.SIGLA
                })
                .Distinct()
                .OrderBy(u => u.SIGLA)
                .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok Lista de Unidades de Incidentes", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new { listaUnidadesIncidentes } );
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


    }
}