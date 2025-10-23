using Amazon.Lambda.Core;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class HistorialUsoApiKeyEndpoints {
        public static IEndpointRouteBuilder MapHistorialUsoApiKeyEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/HistorialUsoApiKey");
            group.MapIngresarEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapIngresarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/Ingresar", async (EntIngresarHistorialUsoApiKey entrada, HistorialUsoApiKeyDAO historialUsoApiKeyDAO, ApiKeyDAO apiKeyDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    string publicId = entrada.ApiKey[..10];
                    string apiKey = entrada.ApiKey[10..];

                    // Se valida que exista el API Key...
                    ApiKey apiKeyExistente = await apiKeyDAO.ObtenerPorApiKeyPublicId(publicId) ?? throw new Exception($"No existe la API key con Public ID {publicId}.");

                   HistorialUsoApiKey salida = await historialUsoApiKeyDAO.Ingresar(
                       apiKeyExistente.Id!.Value, 
                       entrada.FechaUso, 
                       entrada.Ruta, 
                       entrada.ParametrosEntrada, 
                       entrada.CodigoRetorno, 
                       entrada.CantRegistrosRetorno
                    );

                    apiKeyExistente.FechaUltimoUso = entrada.FechaUso;
                    await apiKeyDAO.Modificar(apiKeyExistente);

                    LambdaLogger.Log(
                        $"[POST] - [HistorialUsoApiKey] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se ingresó exitosamente el historial de uso de API key - ID: {salida.Id}");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    string publicKey = entrada.ApiKey != null && entrada.ApiKey.Length >= 10 ? entrada.ApiKey[..10] : "";

                    LambdaLogger.Log(
                        $"[POST] - [HistorialUsoApiKey] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al ingresar el historial de uso de API key - Public ID: {publicKey} - Fecha Uso: {entrada.FechaUso:yyyy-MM-dd HH:mm:ss} " +
                    $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }
    }
}
