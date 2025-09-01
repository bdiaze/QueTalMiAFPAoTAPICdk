using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class TipoMensajeEndpoints {
        public static IEndpointRouteBuilder MapTipoMensajesEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/TipoMensaje");
            group.MapObtenerTipoMensajeEndpoint();
            group.MapObtenerVigentesEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerTipoMensajeEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerTipoMensaje", async (short idTipoMensaje, TipoMensajeDAO tipoMensajeDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    TipoMensaje? salida = await tipoMensajeDAO.ObtenerTipoMensaje(idTipoMensaje);

                    if (salida == null) {
                        LambdaLogger.Log(
                            $"[GET] - [TipoMensaje] - [ObtenerTipoMensaje] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status204NoContent}] - " +
                            $"No se encontró el tipo de mensaje - ID Tipo Mensaje: {idTipoMensaje}.");

                        return Results.NoContent();
                    } else {
                        LambdaLogger.Log(
                            $"[GET] - [TipoMensaje] - [ObtenerTipoMensaje] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                            $"Se obtuvo el tipo de mensaje exitosamente - ID Tipo Mensaje: {idTipoMensaje}.");

                        return Results.Ok(salida);
                    }
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [TipoMensaje] - [ObtenerTipoMensaje] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener el tipo de mensaje - ID Tipo Mensaje: {idTipoMensaje}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerVigentesEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerVigentes", async (TipoMensajeDAO tipoMensajeDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    List<TipoMensaje> salida = await tipoMensajeDAO.ObtenerTiposMensaje(1);

                    LambdaLogger.Log(
                        $"[GET] - [TipoMensaje] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo los tipos de mensaje vigentes exitosamente: {salida.Count} registros.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [TipoMensaje] - [ObtenerVigentes] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener los tipos de mensaje vigentes. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });

            return routes;
        }
    }
}
