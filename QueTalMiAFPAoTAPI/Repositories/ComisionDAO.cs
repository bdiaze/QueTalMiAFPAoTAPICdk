using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class ComisionDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<Comision?> ObtenerComision(byte tipoComision, string afp, DateOnly fecha) {
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
                comision = new Comision { 
                    Id = reader.GetInt64(0),
                    Afp = reader.GetString(1),
                    Fecha = DateOnly.FromDateTime(reader.GetDateTime(2)),
                    Valor = reader.GetDecimal(3),
                    TipoComision = reader.GetByte(4),
                    TipoValor = reader.GetByte(5)
                };
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
                "\"TIPO_VALOR\", " +
                "\"FECHA_CREACION\"" +
                ") VALUES (" +
                "@Afp, " +
                "@Fecha, " +
                "@Valor, " +
                "@TipoComision, " +
                "@TipoValor, " +
                "@FechaCreacion" +
                ") " +
                "RETURNING \"ID\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Afp", comision.Afp);
            command.Parameters.AddWithValue("@Fecha", comision.Fecha);
            command.Parameters.AddWithValue("@Valor", comision.Valor);
            command.Parameters.AddWithValue("@TipoComision", comision.TipoComision);
            command.Parameters.AddWithValue("@TipoValor", comision.TipoValor);
            command.Parameters.AddWithValue("@FechaCreacion", DateTimeOffset.UtcNow);

            _ = (long)(await command.ExecuteScalarAsync())!;
        }

        public async Task ActualizarComision(Comision comision) {
            string queryString = "UPDATE \"QueTalMiAFP\".\"COMISION\" " +
                "SET \"AFP\" = @Afp, " +
                "\"FECHA\" = @Fecha, " +
                "\"VALOR\" = @Valor, " +
                "\"TIPO_COMISION\" = @TipoComision, " +
                "\"TIPO_VALOR\" = @TipoValor, " +
                "\"FECHA_MODIFICACION\" = @FechaModificacion " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", comision.Id!);
            command.Parameters.AddWithValue("@Afp", comision.Afp);
            command.Parameters.AddWithValue("@Fecha", comision.Fecha);
            command.Parameters.AddWithValue("@Valor", comision.Valor);
            command.Parameters.AddWithValue("@TipoComision", comision.TipoComision);
            command.Parameters.AddWithValue("@TipoValor", comision.TipoValor);
            command.Parameters.AddWithValue("@FechaModificacion", DateTimeOffset.UtcNow);

            await command.ExecuteNonQueryAsync();
        }
    }
}
