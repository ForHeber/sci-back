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
    //[Authorize(Roles = "541, 542, 543, 545")]
    [Route("api/[controller]")]
    [ApiController]
    public class RegistroEventosController : ControllerBase
    {
        private readonly DbContextSci _sciContext;
        private readonly DbContextUsuarios _usuariosContext;

        public RegistroEventosController(
            DbContextSci sciContext,
            DbContextUsuarios usuariosContext
        ){
            _sciContext = sciContext;
            _usuariosContext = usuariosContext;
        }

        // Retorna la cantidad de eventos por el TIPO_ANTECEDENTE y el Registro del Evento mas reciente.
        // En el caso de TIPO_ANTECEDENTE "PRE" ademas de agrupar por CONCEPTO (Atributo Modificado)
        // retornando la cantidad y el registro más reciente del CONCEPTO modificado.

        [HttpGet("[action]/{preliminaresId}")]
        public async Task<ActionResult<List<registro_eventos>>> GetListRegistrosEventosInformesPreliminares(
            [FromRoute] int preliminaresId
        ) 
        {
            try
            {
                var tiposAntecedente = new[] { "PRE", "INV", "JUD", "POL", "ARC" };

                List<registro_eventos> listaDeEventosDelInforme = await _sciContext.RegistroEventos
                    .Where(re => re.ESTADO == 1 && re.TIPO_INFORME == "P" && re.TIPO_INFORME_ID == preliminaresId && tiposAntecedente.Contains(re.TIPO_ANTECEDENTE))
                    .OrderByDescending(re => re.TIPO_INFORME)
                    .ThenByDescending(re => re.TIPO_INFORME_ID)
                    .ThenByDescending(re => re.TIPO_ANTECEDENTE)
                    .ThenByDescending(re => re.CREACION)
                    .ToListAsync();

                // Agrupar los eventos por tipo de antecedente

                var eventosPorTipo = listaDeEventosDelInforme
                    .ToLookup(re => re.TIPO_ANTECEDENTE);

                // Agrupar los eventos por concepto el caso de "PRE"

                var eventosPRELIMINARESPorConcepto = eventosPorTipo["PRE"]
                    .GroupBy(evento => evento.CONCEPTO)
                    .OrderBy(grupo => grupo.Key)
                    .Select(grupo => new
                    {
                        concepto = grupo.Key,
                        cantidad = grupo.Count(),
                        primerRegistro = grupo
                            .OrderByDescending(evento => evento.CREACION)
                            .FirstOrDefault()
                    })
                    .ToList();

                int cantidadEventosINVOLUCRADOS = eventosPorTipo["INV"].Count();
                int cantidadEventosJUDICIALES = eventosPorTipo["JUD"].Count();
                int cantidadEventosPOLICIALES = eventosPorTipo["POL"].Count();
                int cantidadEventosARCHIVOS = eventosPorTipo["ARC"].Count();

                registro_eventos? eventosINVOLUCRADOS = eventosPorTipo["INV"].FirstOrDefault();
                registro_eventos? eventosJUDICIALES = eventosPorTipo["JUD"].FirstOrDefault();
                registro_eventos? eventosPOLICIALES = eventosPorTipo["POL"].FirstOrDefault();
                registro_eventos? eventosARCHIVOS = eventosPorTipo["ARC"].FirstOrDefault();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {
                    eventosPRELIMINARESPorConcepto,
                    cantidadEventosINVOLUCRADOS,
                    cantidadEventosJUDICIALES,
                    cantidadEventosPOLICIALES,
                    cantidadEventosARCHIVOS,
                    eventosINVOLUCRADOS,
                    eventosJUDICIALES,
                    eventosPOLICIALES,
                    eventosARCHIVOS
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

        // Retorna la cantidad de eventos por el TIPO_ANTECEDENTE y el Registro del Evento mas reciente.
        // En el caso de TIPO_ANTECEDENTE "INC" ademas de agrupar por CONCEPTO (Atributo Modificado)
        // retornando la cantidad y el registro más reciente del CONCEPTO modificado.

        [HttpGet("[action]/{incidentesId}")]
        public async Task<ActionResult<List<registro_eventos>>> GetListRegistrosEventosInformesIncidentes(
            [FromRoute] int incidentesId) {
            try
            {
                var tiposAntecedente = new[] { "INC", "INV", "JUD", "POL", "ARC", "AMP" };

                List<registro_eventos> listaDeEventosDelInforme = await _sciContext.RegistroEventos
                    .Where(re => re.ESTADO == 1 && re.TIPO_INFORME == "I" && re.TIPO_INFORME_ID == incidentesId && tiposAntecedente.Contains(re.TIPO_ANTECEDENTE))
                    .OrderByDescending(re => re.TIPO_INFORME)
                    .ThenByDescending(re => re.TIPO_INFORME_ID)
                    .ThenByDescending(re => re.TIPO_ANTECEDENTE)
                    .ThenByDescending(re => re.CREACION)
                    .ToListAsync();

                // Agrupar los eventos por tipo de antecedente

                var eventosPorTipo = listaDeEventosDelInforme
                    .ToLookup(re => re.TIPO_ANTECEDENTE);

                // Agrupar los eventos por concepto en el caso de "INC"

                var eventosINCIDENTESPorConcepto = eventosPorTipo["INC"]
                    .GroupBy(evento => evento.CONCEPTO)
                    .OrderBy(grupo => grupo.Key)
                    .Select(grupo => new
                    {
                        concepto = grupo.Key,
                        cantidad = grupo.Count(),
                        primerRegistro = grupo
                            .OrderByDescending(evento => evento.CREACION)
                            .FirstOrDefault()
                    })
                    .ToList();

                int cantidadEventosINVOLUCRADOS = eventosPorTipo["INV"].Count();
                int cantidadEventosJUDICIALES = eventosPorTipo["JUD"].Count();
                int cantidadEventosPOLICIALES = eventosPorTipo["POL"].Count();
                int cantidadEventosARCHIVOS = eventosPorTipo["ARC"].Count();
                int cantidadEventosAMPLIACIONES = eventosPorTipo["AMP"].Count();

                registro_eventos? eventosINVOLUCRADOS = eventosPorTipo["INV"].FirstOrDefault();
                registro_eventos? eventosJUDICIALES = eventosPorTipo["JUD"].FirstOrDefault();
                registro_eventos? eventosPOLICIALES = eventosPorTipo["POL"].FirstOrDefault();
                registro_eventos? eventosARCHIVOS = eventosPorTipo["ARC"].FirstOrDefault();
                registro_eventos? eventosAMPLIACIONES = eventosPorTipo["AMP"].FirstOrDefault();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {
                    eventosINCIDENTESPorConcepto,
                    cantidadEventosINVOLUCRADOS,
                    cantidadEventosJUDICIALES,
                    cantidadEventosPOLICIALES,
                    cantidadEventosARCHIVOS,
                    cantidadEventosAMPLIACIONES,
                    eventosINVOLUCRADOS,
                    eventosJUDICIALES,
                    eventosPOLICIALES,
                    eventosARCHIVOS,
                    eventosAMPLIACIONES
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

        // Retorna la cantidad de eventos por el TIPO_ANTECEDENTE y el Registro del Evento mas reciente.
        // En el caso de TIPO_ANTECEDENTE "FIN" ademas de agrupar por CONCEPTO (Atributo Modificado)
        // retornando la cantidad y el registro más reciente del CONCEPTO modificado.

        [HttpGet("[action]/{finalesId}")]
        public async Task<ActionResult<List<registro_eventos>>> GetListRegistrosEventosInformesFinales(
            [FromRoute] int finalesId) {
            try
            {
                var tiposAntecedente = new[] { "FIN", "INV", "JUD", "POL", "ARC", "AMP" };

                List<registro_eventos> listaDeEventosDelInforme = await _sciContext.RegistroEventos
                    .Where(re => re.ESTADO == 1 && re.TIPO_INFORME == "F" && re.TIPO_INFORME_ID == finalesId && tiposAntecedente.Contains(re.TIPO_ANTECEDENTE))
                    .OrderByDescending(re => re.TIPO_INFORME)
                    .ThenByDescending(re => re.TIPO_INFORME_ID)
                    .ThenByDescending(re => re.TIPO_ANTECEDENTE)
                    .ThenByDescending(re => re.CREACION)
                    .ToListAsync();

                // Agrupar los eventos por tipo de antecedente

                var eventosPorTipo = listaDeEventosDelInforme
                    .ToLookup(re => re.TIPO_ANTECEDENTE);

                // Agrupar los eventos por concepto el caso de "FIN"

                var eventosFINALESPorConcepto = eventosPorTipo["FIN"]
                    .GroupBy(evento => evento.CONCEPTO)
                    .OrderBy(grupo => grupo.Key)
                    .Select(grupo => new
                    {
                        concepto = grupo.Key,
                        cantidad = grupo.Count(),
                        primerRegistro = grupo
                            .OrderByDescending(evento => evento.CREACION)
                            .FirstOrDefault()
                    })
                    .ToList();

                int cantidadEventosINVOLUCRADOS = eventosPorTipo["INV"].Count();
                int cantidadEventosJUDICIALES = eventosPorTipo["JUD"].Count();
                int cantidadEventosPOLICIALES = eventosPorTipo["POL"].Count();
                int cantidadEventosARCHIVOS = eventosPorTipo["ARC"].Count();
                int cantidadEventosAMPLIACIONES = eventosPorTipo["AMP"].Count();

                registro_eventos? eventosINVOLUCRADOS = eventosPorTipo["INV"].FirstOrDefault();
                registro_eventos? eventosJUDICIALES = eventosPorTipo["JUD"].FirstOrDefault();
                registro_eventos? eventosPOLICIALES = eventosPorTipo["POL"].FirstOrDefault();
                registro_eventos? eventosARCHIVOS = eventosPorTipo["ARC"].FirstOrDefault();
                registro_eventos? eventosAMPLIACIONES = eventosPorTipo["AMP"].FirstOrDefault();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new {
                    eventosFINALESPorConcepto,
                    cantidadEventosINVOLUCRADOS,
                    cantidadEventosJUDICIALES,
                    cantidadEventosPOLICIALES,
                    cantidadEventosARCHIVOS,
                    cantidadEventosAMPLIACIONES,
                    eventosINVOLUCRADOS,
                    eventosJUDICIALES,
                    eventosPOLICIALES,
                    eventosARCHIVOS,
                    eventosAMPLIACIONES
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

        // Lista Todos los Eventos del TIPO_INFORME ("P","I","F"), del TIPO_INFORME_ID (Id de Informe),
        // TIPO_ANTECEDENTE ("PRE", "INC", "FIN", "INV", "JUD", "POL", "ARC", "AMP") segun corresponda
        // y por CONCEPTO si es del TIPO_ANTECEDENTE "PRE", "INC" o "FIN".
        // si CONCEPTO enviado es "000", se deben incluir todos los conceptos.

        [HttpGet("[action]/{tipoInforme},{tipoInformeId},{tipoAntecedente},{concepto}")]
        public async Task<ActionResult<List<registro_eventos>>> GetListDetalleRegistrosEventosInformesOLD(
            [FromRoute] string tipoInforme,
            [FromRoute] int tipoInformeId,
            [FromRoute] string tipoAntecedente,
            [FromRoute] string concepto
        ) 
        {
            try
            {
                List<registro_eventos> listaDetalleDeEventos = await _sciContext.RegistroEventos
                    .Where(re =>    re.ESTADO == 1 && 
                                    re.TIPO_INFORME == tipoInforme && 
                                    re.TIPO_INFORME_ID == tipoInformeId && 
                                    re.TIPO_ANTECEDENTE == tipoAntecedente &&
                                    (concepto == "000" || re.CONCEPTO == concepto))
                    .OrderByDescending(re => re.CREACION)
                    .ToListAsync();

                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new { listaDetalleDeEventos });
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


        [HttpGet("[action]/{tipoInforme},{tipoInformeId},{tipoAntecedente},{concepto}")]
        public async Task<ActionResult<List<registro_eventos>>> GetListDetalleRegistrosEventosInformes(
            [FromRoute] string tipoInforme,
            [FromRoute] int tipoInformeId,
            [FromRoute] string tipoAntecedente,
            [FromRoute] string concepto
        ) 
        {
            try
            {
                var listaDetalleDeEventos = await _sciContext.RegistroEventos
                    .Where(re =>    re.ESTADO == 1 && 
                                    re.TIPO_INFORME == tipoInforme && 
                                    re.TIPO_INFORME_ID == tipoInformeId && 
                                    re.TIPO_ANTECEDENTE == tipoAntecedente &&
                                    (concepto == "000" || re.CONCEPTO == concepto))
                    .OrderByDescending(re => re.CREACION)
                    .ToListAsync();

                // Reúne los valores anterior y nuevo de los eventos de un concepto.

                IEnumerable<string?> ValoresDe(string conceptoBuscado) => listaDetalleDeEventos
                        .Where(e => e.CONCEPTO?.Trim() == conceptoBuscado)
                        .SelectMany(e => new[] { e.VALOR_ANTERIOR, e.VALOR_NUEVO });

                // Extrae los IDs numéricos válidos para consultar los catálogos.

                int[] IdsDe(string conceptoBuscado) => ValoresDe(conceptoBuscado)
                        .Select(valor => int.TryParse(valor?.Trim(), out var id) ? (int?)id : null)
                        .Where(id => id.HasValue)
                        .Select(id => id.GetValueOrDefault())
                        .Distinct()
                        .ToArray();

                string? NormalizarCodigoUnidad(string? codigo)
                {
                    if (string.IsNullOrWhiteSpace(codigo))
                        return null;

                    var codigoNormalizado = codigo.Trim().Trim('"').Trim();
                        return string.IsNullOrWhiteSpace(codigoNormalizado)
                        ? null
                        : codigoNormalizado.ToUpperInvariant();
                }

                // Los valores de unidad guardan CODIGO y pueden contener espacios.

                var codigosUnidad = ValoresDe("UNIDAD_INFORMANTE")
                    .Concat(ValoresDe("UNIDAD_AFECTADA"))
                    .Select(NormalizarCodigoUnidad)
                    .Where(codigo => codigo != null)
                    .Select(codigo => codigo!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                // Carga cada catálogo una sola vez.

                var idsTipoIngesta = IdsDe("TIPO_INGESTAID");
                var idsTipoIncidente = IdsDe("TIPO_INCIDENTEID");
                var idsTipoIdentificacion = IdsDe("TIPO_IDENTIFICACIONID");
                var idsTipoArmamento = IdsDe("TIPO_ARMAMENTOID");
                var idsTipoInvestigacion = IdsDe("TIPO_INVESTIGACIONID");

                // Busca las unidades correspondientes a los códigos normalizados.

                var unidadesEncontradas = await _sciContext.Unidades
                    .AsNoTracking()
                    .Where(u => u.CODIGO != null &&
                                codigosUnidad.Contains(u.CODIGO.Trim().ToUpper()))
                    .Select(u => new
                    {
                        u.CODIGO,
                        u.DESCRIPCION
                    })
                    .ToListAsync();

                // Normaliza los códigos de las unidades encontradas y las agrupa por código.

                var unidades = unidadesEncontradas
                    .Select(u => new
                    {
                        Codigo = NormalizarCodigoUnidad(u.CODIGO),
                        u.DESCRIPCION
                    })
                    .Where(u => u.Codigo != null)
                    .GroupBy(u => u.Codigo!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        grupo => grupo.Key,
                        grupo => grupo.First().DESCRIPCION ?? "S/I",
                        StringComparer.OrdinalIgnoreCase);

                // Carga los catálogos de tipos correspondientes a los IDs encontrados.
                
                var tiposIngesta = await _sciContext.TipoIngesta
                    .Where(t => idsTipoIngesta.Contains(t.ID))
                    .ToDictionaryAsync(t => t.ID, t => t.DESCRIPCION ?? "S/I");

                var tiposIncidente = await _sciContext.TipoIncidente
                    .Where(t => idsTipoIncidente.Contains(t.ID))
                    .ToDictionaryAsync(t => t.ID, t => t.DESCRIPCION ?? "S/I");

                var tiposIdentificacion = await _sciContext.TipoIdentificacion
                    .Where(t => idsTipoIdentificacion.Contains(t.ID))
                    .ToDictionaryAsync(t => t.ID, t => t.DESCRIPCION ?? "S/I");

                var tiposArmamento = await _sciContext.TipoArmamento
                    .Where(t => idsTipoArmamento.Contains(t.ID))
                    .ToDictionaryAsync(t => t.ID, t => t.DESCRIPCION ?? "S/I");

                var tiposInvestigacion = await _sciContext.TipoInvestigacion
                    .Where(t => idsTipoInvestigacion.Contains(t.ID))
                    .ToDictionaryAsync(t => t.ID, t => t.DESCRIPCION ?? "S/I");

                // Formatea los valores de los conceptos de evento según su tipo.

                string FormatearId(string? valor, IReadOnlyDictionary<int, string> catalogo)
                {
                    if (!int.TryParse(valor?.Trim(), out var id) ||
                        !catalogo.TryGetValue(id, out var descripcion))
                    {
                        return "S/I";
                    }
                    return $"{id} - {descripcion}";
                }

                // Formatea los valores de los conceptos de evento que representan unidades.

                string FormatearUnidad(string? codigo)
                {
                    var codigoNormalizado = NormalizarCodigoUnidad(codigo);
                    if (codigoNormalizado == null)
                        return "S/I";

                    if (!unidades.TryGetValue(codigoNormalizado, out var descripcion))
                        return "S/I";

                    return $"{codigoNormalizado} - {descripcion}";
                }

                // Traduce los valores de los conceptos de evento según su tipo, utilizando las funciones de formateo definidas anteriormente.

                string? TraducirValor(string? conceptoEvento, string? valor) =>
                    conceptoEvento?.Trim() switch
                    {
                        "UNIDAD_INFORMANTE" or "UNIDAD_AFECTADA" => FormatearUnidad(valor),
                        "TIPO_INGESTAID" => FormatearId(valor, tiposIngesta),
                        "TIPO_INCIDENTEID" => FormatearId(valor, tiposIncidente),
                        "TIPO_IDENTIFICACIONID" => FormatearId(valor, tiposIdentificacion),
                        "TIPO_ARMAMENTOID" => FormatearId(valor, tiposArmamento),
                        "TIPO_INVESTIGACIONID" => FormatearId(valor, tiposInvestigacion),

                        // Los conceptos no asociados a estos catálogos conservan su valor original.
                        _ => valor
                    };

                // Traduce los valores de los detalles de eventos utilizando la función TraducirValor.

                var respuesta = listaDetalleDeEventos
                    .Select(e => new
                    {
                        e.ID,
                        e.TIPO_INFORME,
                        e.TIPO_INFORME_ID,
                        e.TIPO_ANTECEDENTE,
                        e.TIPO_EVENTO,
                        e.MODIFICADO_POR,
                        e.MODIFICADO_FECHA,
                        e.CONCEPTO,
                        VALOR_ANTERIOR = TraducirValor(e.CONCEPTO, e.VALOR_ANTERIOR),
                        VALOR_NUEVO = TraducirValor(e.CONCEPTO, e.VALOR_NUEVO),
                        e.PROPIETARIO,
                        e.CREACION,
                        e.USUARIO,
                        e.ACTUALIZACION
                    })
                    .ToList();



                Auditor.registroAuditor(
                    "Get", 200, User.GetClaimValue("IP"), int.Parse(User.GetClaimValue("Run")), int.Parse(User.GetClaimValue("Rol")),
                    Util.GetActionName(ControllerContext), "Ok", JsonConvert.SerializeObject(""), _usuariosContext
                );

                return Ok(new { listaDetalleDeEventos = respuesta });
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
// 19-10-2026 13:59.