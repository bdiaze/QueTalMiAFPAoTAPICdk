using Npgsql;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class CuotaDAO(DatabaseConnectionHelper connectionHelper) {

        public async Task<Cuota?> ObtenerCuota(string afp, DateTime fecha, string fondo) {
            Cuota? cuota = null;

            string queryString = "SELECT CU.\"ID\", CU.\"AFP\", CU.\"FECHA\", CU.\"FONDO\", CU.\"VALOR\" " +
                "FROM \"QueTalMiAFP\".\"CUOTA\" CU " +
                "WHERE CU.\"AFP\" = @Afp " +
                "AND CU.\"FECHA\" = @Fecha " +
                "AND CU.\"FONDO\" = @Fondo;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Afp", afp);
            command.Parameters.AddWithValue("@Fecha", fecha);
            command.Parameters.AddWithValue("@Fondo", fondo);

            DbDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                cuota = new Cuota(
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetDateTime(2),
                    reader.GetString(3),
                    reader.GetDecimal(4)
                );
            }
            await reader.CloseAsync();
            
            return cuota;
        }

        public async Task InsertarCuota(Cuota cuota) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"CUOTA\"(\"AFP\", " +
                "\"FECHA\", " +
                "\"FONDO\", " +
                "\"VALOR\"" +
                ") VALUES (" +
                "@Afp, " +
                "@Fecha, " +
                "@Fondo, " +
                "@Valor" +
                ") " +
                "RETURNING \"ID\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Afp", cuota.Afp);
            command.Parameters.AddWithValue("@Fecha", cuota.Fecha);
            command.Parameters.AddWithValue("@Fondo", cuota.Fondo);
            command.Parameters.AddWithValue("@Valor", cuota.Valor);

            _ = (long)(await command.ExecuteScalarAsync())!;
        }

        public async Task ActualizarCuota(Cuota cuota) {
            string queryString = "UPDATE \"QueTalMiAFP\".\"CUOTA\" " +
                "SET \"AFP\" = @Afp, " +
                "\"FECHA\" = @Fecha, " +
                "\"FONDO\" = @Fondo, " +
                "\"VALOR\" = @Valor " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", cuota.Id);
            command.Parameters.AddWithValue("@Afp", cuota.Afp);
            command.Parameters.AddWithValue("@Fecha", cuota.Fecha);
            command.Parameters.AddWithValue("@Fondo", cuota.Fondo);
            command.Parameters.AddWithValue("@Valor", cuota.Valor);

            await command.ExecuteNonQueryAsync();
        }

    }
}
