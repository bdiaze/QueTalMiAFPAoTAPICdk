using Amazon.Lambda.APIGatewayEvents;
using QueTalMiAFPAoTAPI.Models;
using System.Text.Json.Serialization;

namespace QueTalMiAFPAoTAPI.Helpers {
    
    [JsonSerializable(typeof(APIGatewayProxyRequest))]
    [JsonSerializable(typeof(APIGatewayProxyResponse))]
    [JsonSerializable(typeof(RDSSecret))]
    [JsonSerializable(typeof(Todo[]))]
    internal partial class AppJsonSerializerContext: JsonSerializerContext {
    
    }
}
