using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using System.Diagnostics;
using System.Net;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class ApiKeyEndpoints {
        public static IEndpointRouteBuilder MapApiKeyEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/ApiKey");
            group.MapCrearEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapCrearEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/Crear", async (EntCrearApiKey entrada, IAmazonAPIGateway apiGateway, VariableEntornoHelper variableEntorno, ParameterStoreHelper parameterStore) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    CreateApiKeyResponse apiKeyResponse = await apiGateway.CreateApiKeyAsync(new CreateApiKeyRequest {
                        Name = $"{variableEntorno.Obtener("APP_NAME")}/{entrada.Sub}",
                        Description = $"API Key del usuario {entrada.Sub} para la aplicacion {variableEntorno.Obtener("APP_NAME")}",
                        Enabled = true,
                        GenerateDistinctId = true,
                        Tags = new Dictionary<string, string> {
                            { "AppName", $"{variableEntorno.Obtener("APP_NAME")}" }
                        }
                    });

                    if (apiKeyResponse.HttpStatusCode != HttpStatusCode.OK) {
                        throw new Exception($"No se pudo crear la API Key - HttpStatusCode: {apiKeyResponse.HttpStatusCode}");
                    }

                    CreateUsagePlanResponse usagePlanResponse = await apiGateway.CreateUsagePlanAsync(new CreateUsagePlanRequest {
                        Name = $"{variableEntorno.Obtener("APP_NAME")}/{entrada.Sub}",
                        Description = $"Usage Plan del usuario {entrada.Sub} para la aplicación {variableEntorno.Obtener("APP_NAME")}",
                        ApiStages = [
                            new ApiStage {
                                ApiId = await parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_APIGATEWAY_API_ID")),
                                Stage = await parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_APIGATEWAY_API_STAGE"))
                            }
                        ],
                        Tags = new Dictionary<string, string> {
                            { "AppName", $"{variableEntorno.Obtener("APP_NAME")}" }
                        },
                    });

                    if (usagePlanResponse.HttpStatusCode != HttpStatusCode.OK) {
                        throw new Exception($"No se pudo crear el Usage Plan - HttpStatusCode: {usagePlanResponse.HttpStatusCode}");
                    }

                    CreateUsagePlanKeyResponse usagePlanKeyResponse = await apiGateway.CreateUsagePlanKeyAsync(new CreateUsagePlanKeyRequest {
                        KeyType = "API_KEY",
                        KeyId = apiKeyResponse.Id,
                        UsagePlanId = usagePlanResponse.Id
                    });

                    if (usagePlanKeyResponse.HttpStatusCode != HttpStatusCode.OK) {
                        throw new Exception($"No se pudo crear el Usage Plan - HttpStatusCode: {usagePlanResponse.HttpStatusCode}");
                    }

                    LambdaLogger.Log(
                        $"[POST] - [ApiKey] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Creación exitosa del API Key del usuario {entrada.Sub}.");

                    return Results.Ok(new SalCrearApiKey {
                        ApiKeyId = apiKeyResponse.Id,
                        ApiKeyValue = apiKeyResponse.Value,
                        UsagePlanId = usagePlanResponse.Id,
                    });
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [ApiKey] - [Crear] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error en la creación del API Key para el usuario {entrada.Sub}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }
    }
}
