using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class UfEndpoints {
        public static IEndpointRouteBuilder MapUfEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/Uf");
            group.MapActualizacionMasivaEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapActualizacionMasivaEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/ActualizacionMasiva", async (EntActualizacionMasivaUf ufsExtraidas, UfDAO ufDAO/*, Repositories.DynamoDB.UfDAO dynamoUfDao*/) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    SalActualizacionMasivaUf retorno = new() { 
                        CantUfsInsertadas = 0,
                        CantUfsActualizadas = 0                    
                    };

                    foreach (Uf uf in ufsExtraidas.Ufs) {
                        Uf? ufExistente = await ufDAO.ObtenerUf(uf.Fecha);
                        if (ufExistente == null) {
                            await ufDAO.InsertarUf(uf);
                            retorno.CantUfsInsertadas++;
                        } else if (ufExistente.Valor != uf.Valor) {
                            ufExistente.Valor = uf.Valor;
                            await ufDAO.ActualizarUf(ufExistente);
                            retorno.CantUfsActualizadas++;
                        }
                    }

                    // Se insertan o actualizan los valores en DynamoDB...
                    /*
                    Dictionary<DateOnly, Entities.DynamoDB.Uf> ufsExistentesDynamo = await dynamoUfDao.ObtenerVarias([.. ufsExtraidas.Ufs.Select(u => DateOnly.FromDateTime(u.Fecha))], true);
                    HashSet<Entities.DynamoDB.Uf> ufsInsertarOActualizar = [];
                    foreach (Uf uf in ufsExtraidas.Ufs) {
                        if (ufsExistentesDynamo.TryGetValue(DateOnly.FromDateTime(uf.Fecha), out Entities.DynamoDB.Uf? ufExistenteDynamo)) {
                            if (ufExistenteDynamo.Valor != uf.Valor) {
                                ufExistenteDynamo.Valor = uf.Valor;
                                ufExistenteDynamo.FechaModificacion = DateTimeOffset.UtcNow;
                                ufsInsertarOActualizar.Add(ufExistenteDynamo);
                            }
                        } else {
                            ufsInsertarOActualizar.Add(new Entities.DynamoDB.Uf { 
                                Fecha = DateOnly.FromDateTime(uf.Fecha), 
                                Valor = uf.Valor,
                                FechaCreacion = DateTimeOffset.UtcNow
                            });
                        }
                    }
                    if (ufsInsertarOActualizar.Count > 0) {
                        await dynamoUfDao.InsertarOActualizarVarias(ufsInsertarOActualizar);
                    }
                    */

                    LambdaLogger.Log(
                        $"[POST] - [Uf] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Actualización masiva de UF exitosa: {retorno.CantUfsInsertadas} insertadas y {retorno.CantUfsActualizadas} actualizadas.");

                    return Results.Ok(retorno);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Uf] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la actualización masiva de UF. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }
    }
}
