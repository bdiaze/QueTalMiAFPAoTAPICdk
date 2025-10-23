using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;
using System.Net;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class ApiKeyEndpoints {
        public static IEndpointRouteBuilder MapApiKeyEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/ApiKey");
            group.MapObtenerPorSubEndpoint();
            group.MapCrearEndpoint();
            group.MapValidarEndpoint();
            group.MapEliminarEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerPorSubEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerPorSub", async (string sub, ApiKeyDAO apiKeyDAO, short? vigente = null) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    List<Entities.ApiKey> salida = await apiKeyDAO.ObtenerPorSub(sub, vigente);

                    LambdaLogger.Log(
                        $"[GET] - [ApiKey] - [ObtenerPorSub] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo exitosamente las API keys para el sub: {sub} - Cantidad: {salida.Count}.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [ApiKey] - [ObtenerPorSub] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener las API keys para el sub: {sub}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }

        private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/Crear", async (EntCrearApiKey entrada, ApiKeyDAO apiKeyDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    // Se valida que no exista api key con mismo request ID, para mantener idempotencia...
                    Entities.ApiKey? apiExistente = await apiKeyDAO.ObtenerPorRequestId(entrada.RequestId);
                    if (apiExistente != null) {
                        LambdaLogger.Log(
                            $"[POST] - [ApiKey] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                            $"La API key del usuario {entrada.Sub} ya fue creada anteriormente.");

                        return Results.Ok(new SalCrearApiKey {
                            Id = apiExistente.Id!.Value,
                        });
                    }

                    // Se valida que el usuario no tenga otra api key vigente...
                    List<Entities.ApiKey> apiKeys = await apiKeyDAO.ObtenerPorSub(entrada.Sub, 1);
                    if (apiKeys.Count > 0) {
                        throw new Exception("El usuario ya tiene una API key vigente.");
                    }

                    string publicId;
                    do {
                        publicId = Guid.NewGuid().ToString("N")[..10];
                    } while (await apiKeyDAO.ObtenerPorApiKeyPublicId(publicId) != null);

                    string apiKeyValue = Guid.NewGuid().ToString("N");

                    Entities.ApiKey apiKey = await apiKeyDAO.Ingresar(
                        entrada.RequestId,
                        entrada.Sub,
                        publicId,
                        CryptoHelper.Hash(apiKeyValue),
                        DateTimeOffset.UtcNow,
                        null,
                        1,
                        null
                    );

                    LambdaLogger.Log(
                        $"[POST] - [ApiKey] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Creación exitosa del API key del usuario {entrada.Sub}.");

                    return Results.Ok(new SalCrearApiKey {
                        Id = apiKey.Id!.Value,
                        ApiKey = $"{apiKey.ApiKeyPublicId}{apiKeyValue}",
                    });
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [ApiKey] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la creación del API key para el usuario {entrada.Sub}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }

        private static IEndpointRouteBuilder MapValidarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/Validar", async (EntValidarApiKey entrada, ApiKeyDAO apiKeyDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    // Se valida el formato del API Key ingresado...
                    if (entrada.ApiKey.Length != 42) {
                        LambdaLogger.Log(
                            $"[POST] - [ApiKey] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status401Unauthorized}] - " +
                            $"La validación del API key no fue exitosa, la API key no tiene el largo necesario.");

                        return Results.Unauthorized();
                    }

                    string publicId = entrada.ApiKey[..10];
                    string apiKey = entrada.ApiKey[10..];

                    // Se valida que exista el API Key...
                    Entities.ApiKey? apiKeyExistente = await apiKeyDAO.ObtenerPorApiKeyPublicId(publicId);
                    if (apiKeyExistente == null) {
                        LambdaLogger.Log(
                            $"[POST] - [ApiKey] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status401Unauthorized}] - " +
                            $"La validación del API key no fue exitosa, la API key con Public ID {publicId} no existe.");

                        return Results.Unauthorized();
                    }

                    // Se valida que la API Key esté vigente...
                    if (apiKeyExistente.Vigente != 1) {
                        LambdaLogger.Log(
                            $"[POST] - [ApiKey] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status401Unauthorized}] - " +
                            $"La validación del API key no fue exitosa, la API key con Public ID {publicId} no esta vigente.");

                        return Results.Unauthorized();
                    }

                    // Se valida que el API Key ingresado coincida con el hash generado originalmente...
                    if (!CryptoHelper.Verify(apiKey, apiKeyExistente.ApiKeyHash)) {
                        LambdaLogger.Log(
                            $"[POST] - [ApiKey] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status401Unauthorized}] - " +
                            $"La validación del API key no fue exitosa, el hash de la API Key no coincide - Public ID: {publicId}.");

                        return Results.Unauthorized();
                    }

                    LambdaLogger.Log(
                        $"[POST] - [ApiKey] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Validación exitosa del API key - Public ID: {publicId}.");

                    return Results.Ok();
                } catch (Exception ex) {
                    string publicKey = entrada.ApiKey != null && entrada.ApiKey.Length >= 10? entrada.ApiKey[..10] : "";

                    LambdaLogger.Log(
                        $"[POST] - [ApiKey] - [Validar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la validación del API key - Public ID: {publicKey}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }

        private static IEndpointRouteBuilder MapEliminarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapDelete("/Eliminar", async (long id, ApiKeyDAO apiKeyDAO) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    Entities.ApiKey? apiKeyExistente = await apiKeyDAO.Obtener(id) ?? throw new Exception("La API key no existe.");

                    if (apiKeyExistente.Vigente == 1) { 
                        apiKeyExistente.Vigente = 0;
                        apiKeyExistente.FechaEliminacion = DateTimeOffset.UtcNow;
                        apiKeyExistente = await apiKeyDAO.Modificar(apiKeyExistente);
                    }

                    LambdaLogger.Log(
                        $"[DELETE] - [ApiKey] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se eliminó exitosamente la API key - ID: {apiKeyExistente.Id}.");

                    return Results.Ok(apiKeyExistente);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[DELETE] - [ApiKey] - [Eliminar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al eliminar la API key - ID: {id}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            });

            return routes;
        }
    }
}
