using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class ComisionEndpoints {

        public static IEndpointRouteBuilder MapComisionEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/Comision");
            group.MapActualizacionMasivaEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapActualizacionMasivaEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/ActualizacionMasiva", async (EntActualizacionMasivaComision comisionesExtraidas, ComisionDAO comisionDAO, Repositories.DynamoDB.ComisionDAO dynamoComisionDao) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    SalActualizacionMasivaComision retorno = new() {
                        CantComisionesInsertadas = 0,
                        CantComisionesActualizadas = 0
                    };

                    foreach (Comision comision in comisionesExtraidas.Comisiones) {
                        Comision? comisionExistente = await comisionDAO.ObtenerComision(comision.TipoComision, comision.Afp, comision.Fecha);

                        if (comisionExistente == null) {
                            await comisionDAO.InsertarComision(comision);
                            retorno.CantComisionesInsertadas++;
                        } else if (comisionExistente.Valor != comision.Valor || comisionExistente.TipoValor != comision.TipoValor) {
                            comisionExistente.Valor = comision.Valor;
                            comisionExistente.TipoValor = comision.TipoValor;
                            await comisionDAO.ActualizarComision(comisionExistente);
                            retorno.CantComisionesActualizadas++;
                        }
                    }

                    // Se insertan o actualizan los valores en DynamoDB...
                    Dictionary<string, Dictionary<byte, Dictionary<DateOnly, Entities.DynamoDB.Comision>>> comisionesExistentesDynamo =  await dynamoComisionDao.ObtenerVarias([.. comisionesExtraidas.Comisiones.Select(c => (c.Afp, c.TipoComision, DateOnly.FromDateTime(c.Fecha)))], true);
                    HashSet<Entities.DynamoDB.Comision> comisionesInsertarOActualizar = [];
                    foreach (Comision comision in comisionesExtraidas.Comisiones) {
                        if (comisionesExistentesDynamo.TryGetValue(comision.Afp, out Dictionary<byte, Dictionary<DateOnly, Entities.DynamoDB.Comision>>? dictTipoComision) && 
                            dictTipoComision.TryGetValue(comision.TipoComision, out Dictionary<DateOnly, Entities.DynamoDB.Comision>? dictFechas) && 
                            dictFechas.TryGetValue(DateOnly.FromDateTime(comision.Fecha), out Entities.DynamoDB.Comision? comisionExistenteDynamo)
                        ) {
                            if (comisionExistenteDynamo.Valor != comision.Valor || comisionExistenteDynamo.TipoValor != comision.TipoValor) {
                                comisionExistenteDynamo.Valor = comision.Valor;
                                comisionExistenteDynamo.TipoValor = comision.TipoValor;
                                comisionExistenteDynamo.FechaModificacion = DateTimeOffset.UtcNow;
                                comisionesInsertarOActualizar.Add(comisionExistenteDynamo);
                            }
                        } else {
                            comisionesInsertarOActualizar.Add(new Entities.DynamoDB.Comision { 
                                Afp = comision.Afp,
                                TipoComision = comision.TipoComision,
                                Fecha = DateOnly.FromDateTime(comision.Fecha),
                                Valor = comision.Valor,
                                TipoValor = comision.TipoValor,
                                FechaCreacion = DateTimeOffset.UtcNow
                            });
                        }
                    }
                    if (comisionesInsertarOActualizar.Count > 0) {
                        await dynamoComisionDao.InsertarOActualizarVarias(comisionesInsertarOActualizar);
                    }

                    LambdaLogger.Log(
                        $"[POST] - [Comision] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Actualización masiva de comisiones exitosa: {retorno.CantComisionesInsertadas} insertadas y {retorno.CantComisionesActualizadas} actualizadas.");

                    return Results.Ok(retorno);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Comision] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la actualización masiva de comisiones. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }
    }
}
