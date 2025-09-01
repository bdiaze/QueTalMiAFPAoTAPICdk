using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class UfDAO(DatabaseConnectionHelper connectionHelper) {
        
        public async Task<Uf?> ObtenerUf(DateTime fecha) {
            Uf? uf = null;

            string queryString = "SELECT UF.\"ID\", UF.\"FECHA\", UF.\"VALOR\" " +
                "FROM \"QueTalMiAFP\".\"UF\" UF " +
                "WHERE UF.\"FECHA\" = @Fecha;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Fecha", fecha);

            DbDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                uf = new Uf { 
                    Id = reader.GetInt64(0),
                    Fecha = reader.GetDateTime(1),
                    Valor = reader.GetDecimal(2)
                };
            }
            await reader.CloseAsync();
            

            return uf;
        }

        public async Task InsertarUf(Uf uf) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"UF\"(" +
                "\"FECHA\", " +
                "\"VALOR\"" +
                ") VALUES (" +
                "@Fecha, " +
                "@Valor" +
                ") " +
                "RETURNING \"ID\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Fecha", uf.Fecha);
            command.Parameters.AddWithValue("@Valor", uf.Valor);

            _ = (long)(await command.ExecuteScalarAsync())!;
        }

        public async Task ActualizarUf(Uf uf) {
            string queryString = "UPDATE \"QueTalMiAFP\".\"UF\" " +
                "SET \"FECHA\" = @Fecha, " +
                "\"VALOR\" = @Valor " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", uf.Id!);
            command.Parameters.AddWithValue("@Fecha", uf.Fecha);
            command.Parameters.AddWithValue("@Valor", uf.Valor);

            await command.ExecuteNonQueryAsync();
        }
    }
}
