using Microsoft.AspNetCore.SignalR;
using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class HistorialUsoApiKeyDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<HistorialUsoApiKey> Ingresar(long idApiKey, DateTimeOffset fechaUso, string ruta, string parametrosEntrada, short codigoRetorno, int cantRegistrosRetorno) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"HISTORIAL_USO_API_KEY\"(" +
                "\"ID_API_KEY\", " +
                "\"FECHA_USO\", " +
                "\"RUTA\", " +
                "\"PARAMETROS_ENTRADA\", " +
                "\"CODIGO_RETORNO\", " +
                "\"CANT_REGISTROS_RETORNO\"" +
                ") VALUES (" +
                "@IdApiKey, " +
                "@FechaUso, " +
                "@Ruta, " +
                "@ParametrosEntrada, " +
                "@CodigoRetorno, " +
                "@CantRegistrosRetorno" +
                ") RETURNING \"ID\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@IdApiKey", idApiKey);
            command.Parameters.AddWithValue("@FechaUso", fechaUso);
            command.Parameters.AddWithValue("@Ruta", ruta);
            command.Parameters.AddWithValue("@ParametrosEntrada", NpgsqlTypes.NpgsqlDbType.Jsonb, parametrosEntrada);
            command.Parameters.AddWithValue("@CodigoRetorno", codigoRetorno);
            command.Parameters.AddWithValue("@CantRegistrosRetorno", cantRegistrosRetorno);

            long id = (long)(await command.ExecuteScalarAsync())!;

            return new HistorialUsoApiKey { 
                Id = id,
                IdApiKey = idApiKey,
                FechaUso = fechaUso,
                Ruta = ruta,
                ParametrosEntrada = parametrosEntrada,
                CodigoRetorno = codigoRetorno,
                CantRegistrosRetorno = cantRegistrosRetorno
            };
        }
    }
}
