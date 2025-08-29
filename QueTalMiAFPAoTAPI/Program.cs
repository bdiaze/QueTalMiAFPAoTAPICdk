using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
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

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>());

builder.Services.AddSingleton<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>();
builder.Services.AddSingleton<IAmazonSecretsManager, AmazonSecretsManagerClient>();
builder.Services.AddSingleton<IAmazonS3, AmazonS3Client>();
builder.Services.AddSingleton<VariableEntorno>();
builder.Services.AddSingleton<ParameterStoreHelper>();
builder.Services.AddSingleton<SecretManagerHelper>();
builder.Services.AddSingleton<S3Helper>();
builder.Services.AddSingleton<ConnectionStringHelper>();
builder.Services.AddSingleton<DatabaseConnectionHelper>();
builder.Services.AddSingleton<CuotaUfComisionDAO>();
builder.Services.AddSingleton<MensajeUsuarioDAO>();
builder.Services.AddSingleton<TipoMensajeDAO>();
builder.Services.AddSingleton<CuotaDAO>();
builder.Services.AddSingleton<UfDAO>();
builder.Services.AddSingleton<ComisionDAO>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapCuotaUfComisionEndpoints();
app.MapMensajeUsuarioEndpoints();
app.MapTipoMensajesEndpoints();
app.MapCuotaEndpoints();
app.MapUfEndpoints();
app.MapComisionEndpoints();

app.Run();