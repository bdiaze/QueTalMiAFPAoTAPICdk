using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>());

builder.Services.AddSingleton<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>();
builder.Services.AddSingleton<IAmazonSecretsManager, AmazonSecretsManagerClient>();
builder.Services.AddSingleton<IAmazonS3, AmazonS3Client>();
builder.Services.AddSingleton<VariableEntorno>();
builder.Services.AddSingleton<ParameterStoreHelper>();
builder.Services.AddSingleton<SecretManagerHelper>();
builder.Services.AddSingleton<ConnectionStringHelper>();
builder.Services.AddSingleton<DatabaseConnectionHelper>();
builder.Services.AddSingleton<CuotaUfComisionDAO>();

var app = builder.Build();

app.UseHttpsRedirection();

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

RouteGroupBuilder todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

#region /CuotaUfComision
RouteGroupBuilder cuotaUfComisionGroup = app.MapGroup("/CuotaUfComision");
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
        return Results.Problem();
    }
});
#endregion

app.Run();