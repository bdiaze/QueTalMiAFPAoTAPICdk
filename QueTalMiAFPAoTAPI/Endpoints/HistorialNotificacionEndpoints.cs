using Amazon.Lambda.Core;
using Microsoft.AspNetCore.SignalR;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class HistorialNotificacionEndpoints {
        public static IEndpointRouteBuilder MapHistorialNotificacionEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/HistorialNotificacion");
            group.MapObtenerPorNotificacionEndpoint();
            group.MapObtenerPorRangoFechasEndpoint();
            group.MapIngresarEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerPorNotificacionEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerPorNotificacion", async (long idNotificacion, HistorialNotificacionDAO historialNotificacionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    List<HistorialNotificacion> salida = await historialNotificacionDAO.ObtenerPorNotificacion(idNotificacion);

                    LambdaLogger.Log(
                        $"[GET] - [HistorialNotificacion] - [ObtenerPorNotificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo exitosamente el historial de notificaciones para la notificación ID: {idNotificacion} - Cantidad: {salida.Count}.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [HistorialNotificacion] - [ObtenerPorNotificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener el historial de notificaciones - ID notificación: {idNotificacion}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerPorRangoFechasEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerPorRangoFechas", async (DateTimeOffset fechaDesde, DateTimeOffset fechaHasta, HistorialNotificacionDAO historialNotificacionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    List<HistorialNotificacion> salida = await historialNotificacionDAO.ObtenerPorRangoFechas(fechaDesde, fechaHasta);

                    LambdaLogger.Log(
                        $"[GET] - [HistorialNotificacion] - [ObtenerPorRangoFechas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo exitosamente el historial de notificaciones para el rango: {fechaDesde:yyyy-MM-dd HH:mm:ss} a {fechaHasta:yyyy-MM-dd HH:mm:ss} - Cantidad: {salida.Count}.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [HistorialNotificacion] - [ObtenerPorRangoFechas] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener el historial de notificaciones para el rango: {fechaDesde:yyyy-MM-dd HH:mm:ss} a {fechaHasta:yyyy-MM-dd HH:mm:ss}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }

        private static IEndpointRouteBuilder MapIngresarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/Ingresar", async (EntIngresarHistorialNotificacion entrada, HistorialNotificacionDAO historialNotificacionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    // Se valida que no haya ya un registro de notificación a la misma hora...
                    HistorialNotificacion? salida = (await historialNotificacionDAO.ObtenerPorNotificacion(entrada.IdNotificacion)).FirstOrDefault(hn => hn.FechaNotificacion == entrada.FechaNotificacion);
                    salida ??= await historialNotificacionDAO.Ingresar(entrada.IdNotificacion, entrada.FechaNotificacion, entrada.Estado);

                    LambdaLogger.Log(
                        $"[POST] - [HistorialNotificacion] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se ingresó exitosamente el historial de notificación - ID: {salida.Id}");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [HistorialNotificacion] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al ingresar el historial de notificación - Fecha Notificación: {entrada.FechaNotificacion:yyyy-MM-dd HH:mm:ss}. " +
                    $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }
    }
}
