using Npgsql;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class ComisionDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<Comision?> ObtenerComision(byte tipoComision, string afp, DateTime fecha) {
            Comision? comision = null;

            string queryString = "SELECT CO.\"ID\", CO.\"AFP\", CO.\"FECHA\", CO.\"VALOR\", CO.\"TIPO_COMISION\", CO.\"TIPO_VALOR\" " +
                    "FROM \"QueTalMiAFP\".\"COMISION\" CO " +
                    "WHERE CO.\"TIPO_COMISION\" = @TipoComision " +
                    "AND CO.\"AFP\" = @Afp " +
                    "AND CO.\"FECHA\" = @Fecha;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@TipoComision", tipoComision);
            command.Parameters.AddWithValue("@Afp", afp);
            command.Parameters.AddWithValue("@Fecha", fecha);

            DbDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                comision = new Comision(
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetDateTime(2),
                    reader.GetDecimal(3),
                    reader.GetByte(4),
                    reader.GetByte(5)
                );
            }
            await reader.CloseAsync();
            
            return comision;
        }
        public async Task InsertarComision(Comision comision) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"COMISION\"(" +
                "\"AFP\", " +
                "\"FECHA\", " +
                "\"VALOR\", " +
                "\"TIPO_COMISION\", " +
                "\"TIPO_VALOR\"" +
                ") VALUES (" +
                "@Afp, " +
                "@Fecha, " +
                "@Valor, " +
                "@TipoComision, " +
                "@TipoValor" +
                ") " +
                "RETURNING \"ID\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Afp", comision.Afp);
            command.Parameters.AddWithValue("@Fecha", comision.Fecha);
            command.Parameters.AddWithValue("@Valor", comision.Valor);
            command.Parameters.AddWithValue("@TipoComision", comision.TipoComision);
            command.Parameters.AddWithValue("@TipoValor", comision.TipoValor);

            _ = (long)(await command.ExecuteScalarAsync())!;
        }

        public async Task ActualizarComision(Comision comision) {
            string queryString = "UPDATE \"QueTalMiAFP\".\"COMISION\" " +
                "SET \"AFP\" = @Afp, " +
                "\"FECHA\" = @Fecha, " +
                "\"VALOR\" = @Valor, " +
                "\"TIPO_COMISION\" = @TipoComision, " +
                "\"TIPO_VALOR\" = @TipoValor " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", comision.Id);
            command.Parameters.AddWithValue("@Afp", comision.Afp);
            command.Parameters.AddWithValue("@Fecha", comision.Fecha);
            command.Parameters.AddWithValue("@Valor", comision.Valor);
            command.Parameters.AddWithValue("@TipoComision", comision.TipoComision);
            command.Parameters.AddWithValue("@TipoValor", comision.TipoValor);

            await command.ExecuteNonQueryAsync();
        }
    }
}
