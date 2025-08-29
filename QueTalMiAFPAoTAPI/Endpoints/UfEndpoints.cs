using Amazon.Lambda.Core;
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
                    int cantUfsInsertadas = 0;
                    int cantUfsActualizadas = 0;

                    foreach (Uf uf in ufsExtraidas.Ufs) {
                        Uf? ufExistente = await ufDAO.ObtenerUf(uf.Fecha);
                        if (ufExistente == null) {
                            await ufDAO.InsertarUf(uf);
                            cantUfsInsertadas++;
                        } else if (ufExistente.Valor != uf.Valor) {
                            await ufDAO.ActualizarUf(new Uf(
                                ufExistente.Id,
                                ufExistente.Fecha,
                                uf.Valor
                            ));
                            cantUfsActualizadas++;
                        }
                    }

                    LambdaLogger.Log(
                        $"[POST] - [Uf] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Actualización masiva de UF exitosa: {cantUfsInsertadas} insertadas y {cantUfsActualizadas} actualizadas.");

                    return Results.Ok(new SalActualizacionMasivaUf(cantUfsInsertadas, cantUfsActualizadas));
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
