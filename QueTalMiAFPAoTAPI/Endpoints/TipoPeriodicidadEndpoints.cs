using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class TipoPeriodicidadEndpoints {
        public static IEndpointRouteBuilder MapTipoPeriodicidadEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/TipoPeriodicidad");
            group.MapObtenerUnaEndpoint();
            group.MapObtenerTodasEndpoint();
            group.MapIngresarEndpoint();
            group.MapModificarEndpoint();
            group.MapEliminarEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerUnaEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerUna", async (short idTipoPeriodicidad, TipoPeriodicidadDAO tipoPeriodicidadDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    TipoPeriodicidad? salida = await tipoPeriodicidadDAO.ObtenerUna(idTipoPeriodicidad);

                    LambdaLogger.Log(
                        $"[GET] - [TipoPeriodicidad] - [ObtenerUna] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo el tipo de periodicidad exitosamente - ID Tipo Periodicidad: {salida?.Id}.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [TipoPeriodicidad] - [ObtenerUna] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener el tipo de periodicidad - ID Tipo Periodicidad: {idTipoPeriodicidad}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerTodasEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerTodas", async (TipoPeriodicidadDAO tipoPeriodicidadDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    List<TipoPeriodicidad> salida = await tipoPeriodicidadDAO.ObtenerTodas();

                    LambdaLogger.Log(
                        $"[GET] - [TipoPeriodicidad] - [ObtenerTodas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvieron exitosamente todos los tipos de periodicidad - Cantidad: {salida.Count}.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [TipoPeriodicidad] - [ObtenerTodas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener todos los tipos de periodicidad. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }

        private static IEndpointRouteBuilder MapIngresarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/Ingresar", async (short id, string nombre, string cron, TipoPeriodicidadDAO tipoPeriodicidadDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    TipoPeriodicidad? salida = await tipoPeriodicidadDAO.ObtenerUna(id);
                    salida ??= await tipoPeriodicidadDAO.Ingresar(id, nombre, cron);

                    LambdaLogger.Log(
                        $"[POST] - [TipoPeriodicidad] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se ingresó exitosamente el tipo de periodicidad - ID: {salida.Id}");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [TipoPeriodicidad] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al ingresar el tipo de periodicidad - ID: {id}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }

        private static IEndpointRouteBuilder MapModificarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPut("/Modificar", async (TipoPeriodicidad tipoPeriodicidad, TipoPeriodicidadDAO tipoPeriodicidadDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    TipoPeriodicidad? salida = await tipoPeriodicidadDAO.ObtenerUna(tipoPeriodicidad.Id) ?? throw new Exception("El tipo de periodicidad no existe.");
                    salida = await tipoPeriodicidadDAO.Modificar(tipoPeriodicidad);

                    LambdaLogger.Log(
                        $"[PUT] - [TipoPeriodicidad] - [Modificar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se modificó exitosamente el tipo de periodicidad - ID: {salida.Id}");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[PUT] - [TipoPeriodicidad] - [Modificar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al modificar el tipo de periodicidad - ID: {tipoPeriodicidad.Id}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }

        private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapDelete("/Eliminar", async (short id, TipoPeriodicidadDAO tipoPeriodicidadDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    await tipoPeriodicidadDAO.Eliminar(id);

                    LambdaLogger.Log(
                        $"[DELETE] - [TipoPeriodicidad] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se eliminó exitosamente el tipo de periodicidad - ID: {id}");

                    return Results.Ok();
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[DELETE] - [TipoPeriodicidad] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al eliminar el tipo de periodicidad - ID: {id}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }
    }
}
