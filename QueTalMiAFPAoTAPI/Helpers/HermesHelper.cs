using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace QueTalMiAFPAoTAPI.Helpers {
    public class HermesHelper(VariableEntornoHelper variableEntorno, ParameterStoreHelper parameterStore, ApiKeyHelper apiKey) {
        private readonly string _hermesBaseUrl = parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_HERMES_API_URL")).Result;
        private readonly string _hermesApiKey = apiKey.ObtenerApiKey(parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_HERMES_API_KEY_ID")).Result).Result;

        public async Task<SalHermesCorreoEnviar> EnviarCorreo(EntHermesCorreoEnviar correo) {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("x-api-key", _hermesApiKey);

            HttpResponseMessage response = await client.PostAsync(_hermesBaseUrl + "Correo/Enviar", new StringContent(JsonSerializer.Serialize(correo, AppJsonSerializerContext.Default.EntHermesCorreoEnviar), Encoding.UTF8, "application/json"));
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception($"Ocurrió un error al enviar el correo. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
            }
            
            return JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.SalHermesCorreoEnviar)!;

        }
    }
}
