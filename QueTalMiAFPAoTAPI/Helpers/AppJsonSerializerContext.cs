using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Models;
using System.Text.Json.Serialization;

namespace QueTalMiAFPAoTAPI.Helpers {

    [JsonSerializable(typeof(APIGatewayProxyRequest))]
    [JsonSerializable(typeof(APIGatewayProxyResponse))]
    [JsonSerializable(typeof(RDSSecret))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(List<CuotaUf>))]
    [JsonSerializable(typeof(SalObtenerCuotas))]
    [JsonSerializable(typeof(EntObtenerUltimaCuota))]
    [JsonSerializable(typeof(List<SalObtenerUltimaCuota>))]
    [JsonSerializable(typeof(List<RentabilidadReal>))]
    [JsonSerializable(typeof(List<TipoMensaje>))]
    [JsonSerializable(typeof(TipoMensaje))]
    [JsonSerializable(typeof(List<MensajeUsuario>))]
    [JsonSerializable(typeof(MensajeUsuario))]
    [JsonSerializable(typeof(EntIngresarMensaje))]
    [JsonSerializable(typeof(EntActualizacionMasivaUf))]
    [JsonSerializable(typeof(SalActualizacionMasivaUf))]
    [JsonSerializable(typeof(EntActualizacionMasivaCuota))]
    [JsonSerializable(typeof(SalActualizacionMasivaCuota))]
    [JsonSerializable(typeof(EntActualizacionMasivaComision))]
    [JsonSerializable(typeof(SalActualizacionMasivaComision))]
    [JsonSerializable(typeof(ProblemDetails))]
    [JsonSerializable(typeof(List<TipoPeriodicidad>))]
    [JsonSerializable(typeof(TipoPeriodicidad))]
    internal partial class AppJsonSerializerContext: JsonSerializerContext {
    
    }
}
