using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class TipoPeriodicidadDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<TipoPeriodicidad?> ObtenerUna(short id) {
            TipoPeriodicidad? tipoPeriodicidad = null;

            string queryString = " SELECT TP.\"ID\", TP.\"NOMBRE\", TP.\"CRON\" " +
                "FROM \"QueTalMiAFP\".\"TIPO_PERIODICIDAD\" TP " +
                "WHERE TP.\"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", id);

            DbDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                tipoPeriodicidad = new() {
                    Id = reader.GetInt16(0),
                    Nombre = reader.GetString(1),
                    Cron = reader.GetString(2)
                };
            }
            await reader.CloseAsync();

            return tipoPeriodicidad;
        }

        public async Task<List<TipoPeriodicidad>> ObtenerTodas() {
            List<TipoPeriodicidad> tiposPeriodicidad = [];

            string queryString = " SELECT TP.\"ID\", TP.\"NOMBRE\", TP.\"CRON\" " +
                "FROM \"QueTalMiAFP\".\"TIPO_PERIODICIDAD\" TP;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                tiposPeriodicidad.Add(new() {
                    Id = reader.GetInt16(0),
                    Nombre = reader.GetString(1),
                    Cron = reader.GetString(2)
                });
            }
            await reader.CloseAsync();

            return tiposPeriodicidad;
        }

        public async Task<TipoPeriodicidad> Ingresar(short id, string nombre, string cron) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"TIPO_PERIODICIDAD\"(" +
                "\"ID\", " +
                "\"NOMBRE\", " +
                "\"CRON\" " +
                ") VALUES (" +
                "@Id, " +
                "@Nombre, " +
                "@Cron " +
                ");";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Nombre", nombre);
            command.Parameters.AddWithValue("@Cron", cron);

            await command.ExecuteNonQueryAsync();

            return new TipoPeriodicidad {
                Id = id,
                Nombre = nombre,
                Cron = cron
            };
        }

        public async Task<TipoPeriodicidad> Modificar(TipoPeriodicidad tipoPeriodicidad) {
            string queryString = "UPDATE \"QueTalMiAFP\".\"TIPO_PERIODICIDAD\" SET " +
                "\"NOMBRE\" = @Nombre, " +
                "\"CRON\" = @Cron " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", tipoPeriodicidad.Id);
            command.Parameters.AddWithValue("@Nombre", tipoPeriodicidad.Nombre);
            command.Parameters.AddWithValue("@Cron", tipoPeriodicidad.Cron);

            await command.ExecuteNonQueryAsync();

            return tipoPeriodicidad;
        }

        public async Task Eliminar(short id) {
            string queryString = "DELETE FROM \"QueTalMiAFP\".\"TIPO_PERIODICIDAD\" " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", id);

            await command.ExecuteNonQueryAsync();
        }
    }
}
