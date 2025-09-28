using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Entities;
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
            routes.MapPost("/ActualizacionMasiva", async (EntActualizacionMasivaCuota cuotasExtraidas, CuotaDAO cuotaDAO, Repositories.DynamoDB.CuotaDAO dynamoCuotaDao) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    SalActualizacionMasivaCuota retorno = new() { 
                        CantCuotasInsertadas = 0,
                        CantCuotasActualizadas = 0
                    };

                    foreach (Cuota cuota in cuotasExtraidas.Cuotas) {
                        Cuota? cuotaExistente = await cuotaDAO.ObtenerCuota(cuota.Afp, cuota.Fecha, cuota.Fondo);

                        if (cuotaExistente == null) {
                            await cuotaDAO.InsertarCuota(cuota);
                            retorno.CantCuotasInsertadas++;
                        } else if (cuotaExistente.Valor != cuota.Valor) {
                            cuotaExistente.Valor = cuota.Valor;
                            await cuotaDAO.ActualizarCuota(cuotaExistente);
                            retorno.CantCuotasActualizadas++;
                        }
                    }

                    // Se insertan o actualizan los valores en DynamoDB...
                    Dictionary<string, Dictionary<string, Dictionary<DateOnly, Entities.DynamoDB.Cuota>>> cuotasExistentesDynamo = await dynamoCuotaDao.ObtenerVarias([.. cuotasExtraidas.Cuotas.Select(c => (c.Afp, c.Fondo, DateOnly.FromDateTime(c.Fecha)))], true);
                    HashSet<Entities.DynamoDB.Cuota> cuotasInsertarOActualizar = [];
                    foreach (Cuota cuota in cuotasExtraidas.Cuotas) {
                        if (cuotasExistentesDynamo.TryGetValue(cuota.Afp, out Dictionary<string, Dictionary<DateOnly, Entities.DynamoDB.Cuota>>? dictFondos) &&
                            dictFondos.TryGetValue(cuota.Fondo, out Dictionary<DateOnly, Entities.DynamoDB.Cuota>? dictFechas) &&
                            dictFechas.TryGetValue(DateOnly.FromDateTime(cuota.Fecha), out Entities.DynamoDB.Cuota? cuotaExistenteDynamo)
                        ) {
                            if (cuotaExistenteDynamo.Valor != cuota.Valor) {
                                cuotaExistenteDynamo.Valor = cuota.Valor;
                                cuotaExistenteDynamo.FechaModificacion = DateTimeOffset.UtcNow;
                                cuotasInsertarOActualizar.Add(cuotaExistenteDynamo);
                            }
                        } else {
                            cuotasInsertarOActualizar.Add(new Entities.DynamoDB.Cuota { 
                                Afp = cuota.Afp,
                                Fondo = cuota.Fondo,
                                Fecha = DateOnly.FromDateTime(cuota.Fecha),
                                Valor = cuota.Valor,
                                FechaCreacion = DateTimeOffset.UtcNow
                            });
                        }
                    }
                    if (cuotasInsertarOActualizar.Count > 0) {
                        await dynamoCuotaDao.InsertarOActualizarVarias(cuotasInsertarOActualizar);
                    }

                    LambdaLogger.Log(
                        $"[POST] - [Cuota] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Actualización masiva de valores cuota exitosa: {retorno.CantCuotasInsertadas} insertadas y {retorno.CantCuotasActualizadas} actualizadas.");

                    return Results.Ok(retorno);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Cuota] - [ActualizacionMasiva] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la actualización masiva de valores cuota. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }
    }
}
