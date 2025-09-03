using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class TipoNotificacionEndpoints {
        public static IEndpointRouteBuilder MapTipoNotificacionEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/TipoNotificacion");
            group.MapObtenerUnaEndpoint();
            group.MapObtenerPorPeriodicidadEndpoint();
            group.MapObtenerTodasEndpoint();
            group.MapIngresarEndpoint();
            group.MapModificarEndpoint();
            group.MapEliminarEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerUnaEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerUna", async (short id, TipoNotificacionDAO tipoNotificacionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    TipoNotificacion? salida = await tipoNotificacionDAO.ObtenerUna(id);

                    LambdaLogger.Log(
                        $"[GET] - [TipoNotificacion] - [ObtenerUna] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo el tipo de notificación exitosamente - ID: {salida?.Id}.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [TipoNotificacion] - [ObtenerUna] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                         $"Ocurrió un error al obtener el tipo de notificación - ID: {id}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerPorPeriodicidadEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerPorPeriodicidad", async (short idTipoPeriodicidad, TipoNotificacionDAO tipoNotificacionDAO, short? habilitado = null) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    List<TipoNotificacion> salida = await tipoNotificacionDAO.ObtenerPorTipoPeriodicidad(idTipoPeriodicidad, habilitado);

                    LambdaLogger.Log(
                        $"[GET] - [TipoNotificacion] - [ObtenerPorPeriodicidad] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvieron exitosamente los tipos de notificaciones para el tipo de periodicidad ID: {idTipoPeriodicidad} - Cantidad: {salida.Count}.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [TipoNotificacion] - [ObtenerPorPeriodicidad] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener los tipos de notificaciones - ID tipo de periodicidad: {idTipoPeriodicidad}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerTodasEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerTodas", async (TipoNotificacionDAO tipoNotificacionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    List<TipoNotificacion> salida = await tipoNotificacionDAO.ObtenerTodas();

                    LambdaLogger.Log(
                        $"[GET] - [TipoNotificacion] - [ObtenerTodas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvieron exitosamente todos los tipos de notificaciones - Cantidad: {salida.Count}.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [TipoNotificacion] - [ObtenerTodas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener todos los tipos de notificaciones. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }

        private static IEndpointRouteBuilder MapIngresarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/Ingresar", async (EntIngresarTipoNotificacion entrada, TipoNotificacionDAO tipoNotificacionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    TipoNotificacion? salida = await tipoNotificacionDAO.ObtenerUna(entrada.Id);
                    salida ??= await tipoNotificacionDAO.Ingresar(entrada.Id, entrada.Nombre, entrada.Descripcion, entrada.IdTipoPeriodicidad, entrada.Habilitado);

                    LambdaLogger.Log(
                        $"[POST] - [TipoNotificacion] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se ingresó exitosamente el tipo de notificación - ID: {salida.Id}");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [TipoNotificacion] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al ingresar el tipo de notificación - ID: {entrada.Id}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }

        private static IEndpointRouteBuilder MapModificarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPut("/Modificar", async (TipoNotificacion tipoNotificacion, TipoNotificacionDAO tipoNotificacionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    TipoNotificacion? salida = await tipoNotificacionDAO.ObtenerUna(tipoNotificacion.Id) ?? throw new Exception("El tipo de notificación no existe.");
                    salida = await tipoNotificacionDAO.Modificar(tipoNotificacion);

                    LambdaLogger.Log(
                        $"[PUT] - [TipoNotificacion] - [Modificar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se modificó exitosamente el tipo de notificación - ID: {salida.Id}");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[PUT] - [TipoNotificacion] - [Modificar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al modificar el tipo de notificación - ID: {tipoNotificacion.Id}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }

        private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapDelete("/Eliminar", async (short id, TipoNotificacionDAO tipoNotificacionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    await tipoNotificacionDAO.Eliminar(id);

                    LambdaLogger.Log(
                        $"[DELETE] - [TipoNotificacion] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se eliminó exitosamente el tipo de notificación - ID: {id}");

                    return Results.Ok();
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[DELETE] - [TipoNotificacion] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al eliminar el tipo de notificación - ID: {id}. " +
                    $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });
            return routes;
        }
    }
}
