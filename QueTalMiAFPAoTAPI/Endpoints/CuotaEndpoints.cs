using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class CuotaEndpoints {
        public static IEndpointRouteBuilder MapCuotaEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/Cuota");
            group.MapActualizacionMasivaEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapActualizacionMasivaEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/ActualizacionMasiva", async (EntActualizacionMasivaCuota cuotasExtraidas, CuotaDAO cuotaDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    int cantCuotasInsertadas = 0;
                    int cantCuotasActualizadas = 0;

                    foreach (Cuota cuota in cuotasExtraidas.Cuotas) {
                        Cuota? cuotaExistente = await cuotaDAO.ObtenerCuota(cuota.Afp, cuota.Fecha, cuota.Fondo);

                        if (cuotaExistente == null) {
                            await cuotaDAO.InsertarCuota(cuota);
                            cantCuotasInsertadas++;
                        } else if (cuotaExistente.Valor != cuota.Valor) {
                            await cuotaDAO.ActualizarCuota(new Cuota(
                                cuotaExistente.Id,
                                cuotaExistente.Afp,
                                cuotaExistente.Fecha,
                                cuotaExistente.Fondo,
                                cuota.Valor
                            ));
                            cantCuotasActualizadas++;
                        }
                    }

                    LambdaLogger.Log(
                        $"[POST] - [Cuota] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Actualización masiva de valores cuota exitosa: {cantCuotasInsertadas} insertadas y {cantCuotasActualizadas} actualizadas.");

                    return Results.Ok(new SalActualizacionMasivaCuota(cantCuotasInsertadas, cantCuotasActualizadas));
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Cuota] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la actualización masiva de valores cuota. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });

            return routes;
        }
    }
}
