﻿using QueTalMiAFPAoTAPI.Models;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace QueTalMiAFPAoTAPI.Helpers {
    public class KairosHelper(VariableEntornoHelper variableEntorno, ParameterStoreHelper parameterStore, ApiKeyHelper apiKey) {
        private readonly string _kairosBaseUrl = parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_KAIROS_API_URL")).Result;
        private readonly string _kairosApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_KAIROS_API_KEY_ID")).Result).Result;

        public async Task<SalKairosIngresarProceso> IngresarProceso(EntKairosIngresarProceso proceso) {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("x-api-key", _kairosApiKey);

            HttpResponseMessage response = await client.PostAsync(_kairosBaseUrl + "Procesos/", new StringContent(JsonSerializer.Serialize(proceso, AppJsonSerializerContext.Default.EntKairosIngresarProceso), Encoding.UTF8, "application/json"));
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al ingresar el proceso. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }

            return JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.SalKairosIngresarProceso)!;
        }

        public async Task EliminarProceso(string idProceso) {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("x-api-key", _kairosApiKey);

            HttpResponseMessage response = await client.DeleteAsync(_kairosBaseUrl + $"Procesos/{Uri.EscapeDataString(idProceso)}");
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al eliminar el proceso. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
