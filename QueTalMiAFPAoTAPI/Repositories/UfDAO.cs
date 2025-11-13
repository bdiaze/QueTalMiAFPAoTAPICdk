using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;
using System.Globalization;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class UfDAO(DatabaseConnectionHelper connectionHelper) {
        
        public async Task<Uf?> ObtenerUf(DateOnly fecha) {
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
                    Fecha = DateOnly.FromDateTime(reader.GetDateTime(1)),
                    Valor = reader.GetDecimal(2)
                };
            }
            await reader.CloseAsync();
            

            return uf;
        }

        public async Task<SortedDictionary<DateOnly, Uf>> ObtenerVariasUf(DateOnly[] fechas) {
			SortedDictionary<DateOnly, Uf> retorno = [];

			string queryString = "SELECT UF.\"ID\", UF.\"FECHA\", UF.\"VALOR\" " +
				"FROM \"QueTalMiAFP\".\"UF\" UF " +
				"WHERE UF.\"FECHA\" IN (" +
				"SELECT TO_DATE(\"STT_FECHA\", 'YYYY-MM-DD') AS \"FECHA\" " +
				"FROM STRING_TO_TABLE(@Fechas, ',') AS \"STT_FECHA\");";

			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await using NpgsqlCommand command = new(queryString, connection);

			command.Parameters.AddWithValue("@Fechas", string.Join(",", fechas.Select(f => f.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))));

			DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
				Uf uf = new() {
					Id = reader.GetInt64(0),
					Fecha = DateOnly.FromDateTime(reader.GetDateTime(1)),
					Valor = reader.GetDecimal(2)
				};
                retorno.Add(uf.Fecha, uf);
			}
			await reader.CloseAsync();

			return retorno;
        }

        public async Task InsertarUf(Uf uf) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"UF\"(" +
                "\"FECHA\", " +
                "\"VALOR\", " +
                "\"FECHA_CREACION\"" +
                ") VALUES (" +
                "@Fecha, " +
                "@Valor, " +
                "@FechaCreacion" +
                ") " +
                "RETURNING \"ID\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Fecha", uf.Fecha);
            command.Parameters.AddWithValue("@Valor", uf.Valor);
            command.Parameters.AddWithValue("@FechaCreacion", DateTimeOffset.UtcNow);

            _ = (long)(await command.ExecuteScalarAsync())!;
        }

        public async Task ActualizarUf(Uf uf) {
            string queryString = "UPDATE \"QueTalMiAFP\".\"UF\" " +
                "SET \"FECHA\" = @Fecha, " +
                "\"VALOR\" = @Valor, " +
                "\"FECHA_MODIFICACION\" = @FechaModificacion " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", uf.Id!);
            command.Parameters.AddWithValue("@Fecha", uf.Fecha);
            command.Parameters.AddWithValue("@Valor", uf.Valor);
            command.Parameters.AddWithValue("@FechaModificacion", DateTimeOffset.UtcNow);

            await command.ExecuteNonQueryAsync();
        }
    }
}
