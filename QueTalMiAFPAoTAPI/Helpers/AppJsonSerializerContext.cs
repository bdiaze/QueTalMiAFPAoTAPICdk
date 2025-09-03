using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Mvc;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Models;
using System.Text.Json.Serialization;

namespace QueTalMiAFPAoTAPI.Helpers {

    [JsonSerializable(typeof(APIGatewayProxyRequest))]
    [JsonSerializable(typeof(APIGatewayProxyResponse))]
    [JsonSerializable(typeof(ProblemDetails))]
    [JsonSerializable(typeof(RDSSecret))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(List<CuotaUf>))]
    [JsonSerializable(typeof(SalObtenerCuotas))]
    [JsonSerializable(typeof(EntObtenerUltimaCuota))]
    [JsonSerializable(typeof(List<SalObtenerUltimaCuota>))]
    [JsonSerializable(typeof(List<RentabilidadReal>))]
    [JsonSerializable(typeof(TipoMensaje))]
    [JsonSerializable(typeof(List<TipoMensaje>))]
    [JsonSerializable(typeof(MensajeUsuario))]
    [JsonSerializable(typeof(List<MensajeUsuario>))]
    [JsonSerializable(typeof(EntIngresarMensaje))]
    [JsonSerializable(typeof(EntActualizacionMasivaUf))]
    [JsonSerializable(typeof(SalActualizacionMasivaUf))]
    [JsonSerializable(typeof(EntActualizacionMasivaCuota))]
    [JsonSerializable(typeof(SalActualizacionMasivaCuota))]
    [JsonSerializable(typeof(EntActualizacionMasivaComision))]
    [JsonSerializable(typeof(SalActualizacionMasivaComision))]
    [JsonSerializable(typeof(TipoPeriodicidad))]
    [JsonSerializable(typeof(List<TipoPeriodicidad>))]
    [JsonSerializable(typeof(EntIngresarTipoPeriodicidad))]
    [JsonSerializable(typeof(TipoNotificacion))]
    [JsonSerializable(typeof(List<TipoNotificacion>))]
    [JsonSerializable(typeof(EntIngresarTipoNotificacion))]
    internal partial class AppJsonSerializerContext: JsonSerializerContext {
    
    }
}
