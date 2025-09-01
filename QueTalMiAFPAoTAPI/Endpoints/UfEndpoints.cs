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
            routes.MapPost("/ActualizacionMasiva", async (EntActualizacionMasivaUf ufsExtraidas, UfDAO ufDAO) => {
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
            });

            return routes;
        }
    }
}
