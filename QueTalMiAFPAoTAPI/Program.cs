using Amazon.APIGateway;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.OpenApi.Models;
using QueTalMiAFPAoTAPI.Endpoints;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.Configure<RouteOptions>(options => {
    options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "API ¿Qué tal mi AFP? - Minimal API AoT",
        Version = "v1"
    });
});

#region Singleton AWS Services
builder.Services.AddSingleton<IAmazonSimpleSystemsManagement>(sp => {
    AmazonSimpleSystemsManagementConfig config = new() {
        ConnectTimeout = TimeSpan.FromSeconds(5),
        Timeout = TimeSpan.FromSeconds(25)
    };
    return new AmazonSimpleSystemsManagementClient(config);
});
builder.Services.AddSingleton<IAmazonSecretsManager>(sp => {
    AmazonSecretsManagerConfig config = new() {
        ConnectTimeout = TimeSpan.FromSeconds(5),
        Timeout = TimeSpan.FromSeconds(25)
    };
    return new AmazonSecretsManagerClient(config);
});
builder.Services.AddSingleton<IAmazonS3>(sp => {
    AmazonS3Config config = new() {
        ConnectTimeout = TimeSpan.FromSeconds(5),
        Timeout = TimeSpan.FromSeconds(25)
    };
    return new AmazonS3Client(config);
});
builder.Services.AddSingleton<IAmazonAPIGateway>(sp => {
    AmazonAPIGatewayConfig config = new() {
        ConnectTimeout = TimeSpan.FromSeconds(5),
        Timeout = TimeSpan.FromSeconds(25)
    };
    return new AmazonAPIGatewayClient(config);
});
builder.Services.AddSingleton<IAmazonDynamoDB>(sp => {
    AmazonDynamoDBConfig config = new() {
        ConnectTimeout = TimeSpan.FromSeconds(5),
        Timeout = TimeSpan.FromSeconds(25)
    };
    return new AmazonDynamoDBClient(config);
});
#endregion

#region Singleton Helpers
builder.Services.AddSingleton<VariableEntornoHelper>();
builder.Services.AddSingleton<ParameterStoreHelper>();
builder.Services.AddSingleton<SecretManagerHelper>();
builder.Services.AddSingleton<ApiKeyHelper>();
builder.Services.AddSingleton<S3Helper>();
builder.Services.AddSingleton<ConnectionStringHelper>();
builder.Services.AddSingleton<DatabaseConnectionHelper>();
builder.Services.AddSingleton<HermesHelper>();
builder.Services.AddSingleton<KairosHelper>();
#endregion

#region Singleton Repositories DAO
builder.Services.AddSingleton<CuotaUfComisionDAO>();
builder.Services.AddSingleton<MensajeUsuarioDAO>();
builder.Services.AddSingleton<TipoMensajeDAO>();
builder.Services.AddSingleton<CuotaDAO>();
builder.Services.AddSingleton<UfDAO>();
builder.Services.AddSingleton<ComisionDAO>();
builder.Services.AddSingleton<HistorialNotificacionDAO>();
builder.Services.AddSingleton<NotificacionDAO>();
builder.Services.AddSingleton<TipoNotificacionDAO>();
builder.Services.AddSingleton<TipoPeriodicidadDAO>();
builder.Services.AddSingleton<ApiKeyDAO>();

// builder.Services.AddSingleton<QueTalMiAFPAoTAPI.Repositories.DynamoDB.UfDAO>();
// builder.Services.AddSingleton<QueTalMiAFPAoTAPI.Repositories.DynamoDB.ComisionDAO>();
// builder.Services.AddSingleton<QueTalMiAFPAoTAPI.Repositories.DynamoDB.CuotaDAO>();
#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapCuotaUfComisionEndpoints();
app.MapMensajeUsuarioEndpoints();
app.MapTipoMensajesEndpoints();
app.MapCuotaEndpoints();
app.MapUfEndpoints();
app.MapComisionEndpoints();
app.MapTipoPeriodicidadEndpoints();
app.MapTipoNotificacionEndpoints();
app.MapNotificacionEndpoints();
app.MapHistorialNotificacionEndpoints();
app.MapApiKeyEndpoints();

app.Run();