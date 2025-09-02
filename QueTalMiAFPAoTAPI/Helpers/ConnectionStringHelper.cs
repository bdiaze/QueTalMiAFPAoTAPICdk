using Amazon.Util;
using QueTalMiAFPAoTAPI.Models;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QueTalMiAFPAoTAPI.Helpers {
    public class ConnectionStringHelper(IHostEnvironment env, IConfiguration config, VariableEntornoHelper variableEntorno, SecretManagerHelper secretManager) {

        private string? connectionString = null;

        public async Task<string> Obtener() {
            if (connectionString == null) {
                string secretArnConnectionString = variableEntorno.Obtener("SECRET_ARN_CONNECTION_STRING");
                string secretConnectionString = await secretManager.ObtenerSecreto(secretArnConnectionString);
                RDSSecret secret = JsonSerializer.Deserialize(secretConnectionString, AppJsonSerializerContext.Default.RDSSecret)!;

                string host = secret.Host;
                string port = secret.Port;
                string database = secret.QueTalMiAFPDatabase;
                string username = secret.QueTalMiAFPAppUsername;
                string password = secret.QueTalMiAFPAppPassword;
                
                // Como estamos en ambiente de desarrollo, se usan host, username y password de archivo appsettings.Development.json
                if (env.IsDevelopment()) {
                    host = config["Develop:Database:Host"] ?? throw new Exception("Debes agregar el atributo Develop > Database > Host en el archivo appsettings.Development.json para ejecutar localmente.");
                    username = config["Develop:Database:User"] ?? throw new Exception("Debes agregar el atributo Develop > Database > User en el archivo appsettings.Development.json para ejecutar localmente.");
                    password = config["Develop:Database:Pass"] ?? throw new Exception("Debes agregar el atributo Develop > Database > Pass en el archivo appsettings.Development.json para ejecutar localmente.");
                }                
                
                connectionString = 
                    $"Server={host};" +
                    $"Port={port};" +
                    $"Database={database};" +
                    $"User Id={username};" +
                    $"Password='{password}';";

                if (!env.IsDevelopment()) {
                    connectionString += "Ssl Mode=Require;";
                    connectionString += "Trust Server Certificate=true;";
                }
            }

            return connectionString;
        }
    }
}
