using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class CuotaUfComisionEndpoint {
        public static IEndpointRouteBuilder MapCuotaUfComisionEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder cuotaUfComisionGroup = routes.MapGroup("/CuotaUfComision");
            
            cuotaUfComisionGroup.MapGet("/UltimaFechaAlguna", async (CuotaUfComisionDAO cuotaUfComisionDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    DateTime ultimaFecha = await cuotaUfComisionDAO.ObtenerUltimaFechaAlguna();

                    LambdaLogger.Log(
                        $"[GET] - [CuotaUfComision] - [UltimaFechaAlguna] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo la última fecha con algún valor cuota exitosamente.");

                    return Results.Ok(ultimaFecha);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [CuotaUfComision] - [UltimaFechaAlguna] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener la última fecha con algún valor cuota. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });

            return routes;
        }
    }
}
