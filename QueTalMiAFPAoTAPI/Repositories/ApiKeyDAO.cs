using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class ApiKeyDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<ApiKey?> Obtener(long id) {
            string queryString = "SELECT AK.\"ID\", AK.\"REQUEST_ID\", AK.\"SUB\", AK.\"API_KEY_PUBLIC_ID\", AK.\"API_KEY_HASH\", " +
                "AK.\"FECHA_CREACION\", AK.\"FECHA_ELIMINACION\", AK.\"VIGENTE\", AK.\"FECHA_ULTIMO_USO\" " +
                "FROM \"QueTalMiAFP\".\"API_KEY\" AK " +
                "WHERE AK.\"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", id);

            ApiKey? apiKey = null;

            DbDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                apiKey = new() {
                    Id = reader.GetInt64(0),
                    RequestId = reader.GetString(1),
                    Sub = reader.GetString(2),
                    ApiKeyPublicId = reader.GetString(3),
                    ApiKeyHash = reader.GetString(4),
                    FechaCreacion = reader.GetDateTime(5),
                    FechaEliminacion = !await reader.IsDBNullAsync(6) ? reader.GetDateTime(6) : null,
                    Vigente = reader.GetInt16(7),
                    FechaUltimoUso = !await reader.IsDBNullAsync(8) ? reader.GetDateTime(8) : null,
                };
            }
            await reader.CloseAsync();

            return apiKey;
        }

        public async Task<List<ApiKey>> ObtenerPorSub(string sub, short? vigente = null) {
            List<ApiKey> apiKeys = [];

            string queryString = "SELECT AK.\"ID\", AK.\"REQUEST_ID\", AK.\"SUB\", AK.\"API_KEY_PUBLIC_ID\", AK.\"API_KEY_HASH\", " +
                "AK.\"FECHA_CREACION\", AK.\"FECHA_ELIMINACION\", AK.\"VIGENTE\", AK.\"FECHA_ULTIMO_USO\" " +
                "FROM \"QueTalMiAFP\".\"API_KEY\" AK " +
                "WHERE AK.\"SUB\" = @Sub " +
                (vigente != null ? "AND AK.\"VIGENTE\" = @Vigente;" : ";");

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Sub", sub);
            if (vigente != null) command.Parameters.AddWithValue("@Vigente", vigente);

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                apiKeys.Add(new() {
                    Id = reader.GetInt64(0),
                    RequestId = reader.GetString(1),
                    Sub = reader.GetString(2),
                    ApiKeyPublicId = reader.GetString(3),
                    ApiKeyHash = reader.GetString(4),
                    FechaCreacion = reader.GetDateTime(5),
                    FechaEliminacion = !await reader.IsDBNullAsync(6) ? reader.GetDateTime(6) : null,
                    Vigente = reader.GetInt16(7),
                    FechaUltimoUso = !await reader.IsDBNullAsync(8) ? reader.GetDateTime(8) : null,
                });
            }
            await reader.CloseAsync();

            return apiKeys;
        }

        public async Task<ApiKey?> ObtenerPorApiKeyPublicId(string apiKeyPublicId) {
            string queryString = "SELECT AK.\"ID\", AK.\"REQUEST_ID\", AK.\"SUB\", AK.\"API_KEY_PUBLIC_ID\", AK.\"API_KEY_HASH\", " +
                "AK.\"FECHA_CREACION\", AK.\"FECHA_ELIMINACION\", AK.\"VIGENTE\", AK.\"FECHA_ULTIMO_USO\" " +
                "FROM \"QueTalMiAFP\".\"API_KEY\" AK " +
                "WHERE AK.\"API_KEY_PUBLIC_ID\" = @ApiKeyPublicId;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@ApiKeyPublicId", apiKeyPublicId);

            ApiKey? apiKey = null;

            DbDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                apiKey = new() {
                    Id = reader.GetInt64(0),
                    RequestId = reader.GetString(1),
                    Sub = reader.GetString(2),
                    ApiKeyPublicId = reader.GetString(3),
                    ApiKeyHash = reader.GetString(4),
                    FechaCreacion = reader.GetDateTime(5),
                    FechaEliminacion = !await reader.IsDBNullAsync(6) ? reader.GetDateTime(6) : null,
                    Vigente = reader.GetInt16(7),
                    FechaUltimoUso = !await reader.IsDBNullAsync(8) ? reader.GetDateTime(8) : null,
                };
            }
            await reader.CloseAsync();

            return apiKey;
        }

        public async Task<ApiKey?> ObtenerPorRequestId(string requestId) {
            string queryString = "SELECT AK.\"ID\", AK.\"REQUEST_ID\", AK.\"SUB\", AK.\"API_KEY_PUBLIC_ID\", AK.\"API_KEY_HASH\", " +
                "AK.\"FECHA_CREACION\", AK.\"FECHA_ELIMINACION\", AK.\"VIGENTE\", AK.\"FECHA_ULTIMO_USO\" " +
                "FROM \"QueTalMiAFP\".\"API_KEY\" AK " +
                "WHERE AK.\"REQUEST_ID\" = @RequestId;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@RequestId", requestId);

            ApiKey? apiKey = null;

            DbDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                apiKey = new() {
                    Id = reader.GetInt64(0),
                    RequestId = reader.GetString(1),
                    Sub = reader.GetString(2),
                    ApiKeyPublicId = reader.GetString(3),
                    ApiKeyHash = reader.GetString(4),
                    FechaCreacion = reader.GetDateTime(5),
                    FechaEliminacion = !await reader.IsDBNullAsync(6) ? reader.GetDateTime(6) : null,
                    Vigente = reader.GetInt16(7),
                    FechaUltimoUso = !await reader.IsDBNullAsync(8) ? reader.GetDateTime(8) : null,
                };
            }
            await reader.CloseAsync();

            return apiKey;
        }

        public async Task<ApiKey> Ingresar(string requestId, string sub, string apiKeyPublicId ,string apiKeyHash, DateTimeOffset fechaCreacion, DateTimeOffset? fechaEliminacion, short vigente, DateTimeOffset? fechaUltimoUso) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"API_KEY\"(" +
                "\"REQUEST_ID\", " +
                "\"SUB\", " +
                "\"API_KEY_PUBLIC_ID\", " +
                "\"API_KEY_HASH\", " +
                "\"FECHA_CREACION\", " +
                "\"FECHA_ELIMINACION\", " +
                "\"VIGENTE\", " +
                "\"FECHA_ULTIMO_USO\"" +
                ") VALUES (" +
                "@RequestId, " +
                "@Sub, " +
                "@ApiKeyPublicId, " +
                "@ApiKeyHash, " +
                "@FechaCreacion, " +
                "@FechaEliminacion, " +
                "@Vigente," +
                "@FechaUltimoUso" +
                ") RETURNING \"ID\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@RequestId", requestId);
            command.Parameters.AddWithValue("@Sub", sub);
            command.Parameters.AddWithValue("@ApiKeyPublicId", apiKeyPublicId);
            command.Parameters.AddWithValue("@ApiKeyHash", apiKeyHash);
            command.Parameters.AddWithValue("@FechaCreacion", fechaCreacion.ToUniversalTime());
            if (fechaEliminacion != null) command.Parameters.AddWithValue("@FechaEliminacion", fechaEliminacion.Value.ToUniversalTime());
            else command.Parameters.AddWithValue("@FechaEliminacion", DBNull.Value);
            command.Parameters.AddWithValue("@Vigente", vigente);
            if (fechaUltimoUso != null) command.Parameters.AddWithValue("@FechaUltimoUso", fechaUltimoUso.Value.ToUniversalTime());
            else command.Parameters.AddWithValue("@FechaUltimoUso", DBNull.Value);

            long id = (long)(await command.ExecuteScalarAsync())!;

            return new ApiKey {
                Id = id,
                RequestId = requestId,
                Sub = sub,
                ApiKeyPublicId = apiKeyPublicId,
                ApiKeyHash = apiKeyHash,
                FechaCreacion = fechaCreacion,
                FechaEliminacion = fechaEliminacion,
                Vigente = vigente,
                FechaUltimoUso = fechaUltimoUso
            };
        }

        public async Task <ApiKey> Modificar(ApiKey apiKey) {
            string queryString = "UPDATE \"QueTalMiAFP\".\"API_KEY\" SET " +
                "\"REQUEST_ID\" = @RequestId, " +
                "\"SUB\" = @Sub, " +
                "\"API_KEY_PUBLIC_ID\" = @ApiKeyPublicId, " +
                "\"API_KEY_HASH\" = @ApiKeyHash, " +
                "\"FECHA_CREACION\" = @FechaCreacion, " +
                "\"FECHA_ELIMINACION\" = @FechaEliminacion, " +
                "\"VIGENTE\" = @Vigente, " +
                "\"FECHA_ULTIMO_USO\" = @FechaUltimoUso " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", apiKey.Id!);
            command.Parameters.AddWithValue("@RequestId", apiKey.RequestId);
            command.Parameters.AddWithValue("@Sub", apiKey.Sub);
            command.Parameters.AddWithValue("@ApiKeyPublicId", apiKey.ApiKeyPublicId);
            command.Parameters.AddWithValue("@ApiKeyHash", apiKey.ApiKeyHash);
            command.Parameters.AddWithValue("@FechaCreacion", apiKey.FechaCreacion.ToUniversalTime());
            if (apiKey.FechaEliminacion != null) command.Parameters.AddWithValue("@FechaEliminacion", apiKey.FechaEliminacion.Value.ToUniversalTime());
            else command.Parameters.AddWithValue("@FechaEliminacion", DBNull.Value);
            command.Parameters.AddWithValue("@Vigente", apiKey.Vigente);
            if (apiKey.FechaUltimoUso != null) command.Parameters.AddWithValue("@FechaUltimoUso", apiKey.FechaUltimoUso.Value.ToUniversalTime());
            else command.Parameters.AddWithValue("@FechaUltimoUso", DBNull.Value);

            await command.ExecuteNonQueryAsync();

            return apiKey;
        }
    }
}
