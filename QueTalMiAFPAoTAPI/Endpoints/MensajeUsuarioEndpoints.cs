using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class MensajeUsuarioEndpoints {
        public static IEndpointRouteBuilder MapMensajeUsuarioEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/MensajeUsuario");
            group.MapObtenerMensajesEndpoint();
            group.MapIngresarMensajeEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerMensajesEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerMensajes", async (DateTime fechaDesde, DateTime fechaHasta, MensajeUsuarioDAO mensajeUsuarioDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    List<MensajeUsuario> salida = await mensajeUsuarioDAO.ObtenerMensajesUsuarios(fechaDesde, fechaHasta);

                    LambdaLogger.Log(
                        $"[GET] - [MensajeUsuario] - [ObtenerMensajes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo los mensajes exitosamente - Desde {fechaDesde:yyyy-MM-dd HH:mm:ss} - Hasta {fechaHasta:yyyy-MM-dd HH:mm:ss}: {salida.Count} registros.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [MensajeUsuario] - [ObtenerMensajes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener los mensajes - Desde {fechaDesde:yyyy-MM-dd HH:mm:ss} - Hasta {fechaHasta:yyyy-MM-dd HH:mm:ss}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }

        private static IEndpointRouteBuilder MapIngresarMensajeEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/IngresarMensaje", async (EntIngresarMensaje mensaje, MensajeUsuarioDAO mensajeUsuarioDAO, TipoMensajeDAO tipoMensajeDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    DateTime fechaActual = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneConverter.TZConvert.GetTimeZoneInfo("Pacific SA Standard Time"));
                    MensajeUsuario mensajeIngresado = await mensajeUsuarioDAO.IngresarMensajeUsuario(mensaje.IdTipoMensaje, fechaActual, mensaje.Nombre, mensaje.Correo, mensaje.Mensaje);
                    mensajeIngresado.TipoMensaje = await tipoMensajeDAO.ObtenerTipoMensaje(mensajeIngresado.IdTipoMensaje);

                    LambdaLogger.Log(
                        $"[POST] - [MensajeUsuario] - [IngresarMensaje] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se insertó el mensaje exitosamente - ID Mensaje: {mensajeIngresado.IdMensaje}.");

                    return Results.Ok(mensajeIngresado);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [MensajeUsuario] - [IngresarMensaje] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al insertar el mensaje. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }
    }
}
