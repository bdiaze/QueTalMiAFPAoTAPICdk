using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class HistorialNotificacionesDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<List<HistorialNotificaciones>> ObtenerPorNotificacion(long idNotificacion) {
            List<HistorialNotificaciones> historialNotificaciones = [];

            string queryString = " SELECT HN.\"ID\", HN.\"ID_NOTIFICACION\", HN.\"FECHA_NOTIFICACION\", HN.\"ESTADO\" " +
                "FROM \"QueTalMiAFP\".\"HISTORIAL_NOTIFICACIONES\" HN " +
                "WHERE HN.\"ID_NOTIFICACION\" = @IdNotificacion; ";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@IdNotificacion", idNotificacion);

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                historialNotificaciones.Add(new() {
                    Id = reader.GetInt64(0),
                    IdNotificacion = reader.GetInt64(1),
                    FechaNotificacion = reader.GetDateTime(2),
                    Estado = reader.GetInt16(3)
                });
            }
            await reader.CloseAsync();

            return historialNotificaciones;
        }

        public async Task<List<HistorialNotificaciones>> ObtenerPorRangoFechas(DateTimeOffset fechaDesde, DateTimeOffset fechaHasta) {
            List<HistorialNotificaciones> historialNotificaciones = [];

            string queryString = " SELECT HN.\"ID\", HN.\"ID_NOTIFICACION\", HN.\"FECHA_NOTIFICACION\", HN.\"ESTADO\" " +
                "FROM \"QueTalMiAFP\".\"HISTORIAL_NOTIFICACIONES\" HN " +
                "WHERE HN.\"FECHA_NOTIFICACION\" >= @FechaDesde " +
                "AND HN.\"FECHA_NOTIFICACION\" <= @FechaHasta;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@FechaDesde", fechaDesde);
            command.Parameters.AddWithValue("@FechaHasta", fechaHasta);

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                historialNotificaciones.Add(new() {
                    Id = reader.GetInt64(0),
                    IdNotificacion = reader.GetInt64(1),
                    FechaNotificacion = reader.GetDateTime(2),
                    Estado = reader.GetInt16(3)
                });
            }
            await reader.CloseAsync();

            return historialNotificaciones;
        }

        public async Task<HistorialNotificaciones> Ingresar(long idNotificacion, DateTimeOffset fechaNotificacion, short estado) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"HISTORIAL_NOTIFICACIONES\"(" +
                "\"ID_NOTIFICACION\", " +
                "\"FECHA_NOTIFICACION\", " +
                "\"ESTADO\"" +
                ") VALUES (" +
                "@IdNotificacion, " +
                "@FechaNotificacion, " +
                "@Estado" +
                ") RETURNING \"ID\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@IdNotificacion", idNotificacion);
            command.Parameters.AddWithValue("@FechaNotificacion", fechaNotificacion);
            command.Parameters.AddWithValue("@Estado", estado);

            long id = (long)(await command.ExecuteScalarAsync())!;

            return new HistorialNotificaciones {
                Id = id,
                IdNotificacion = idNotificacion,
                FechaNotificacion = fechaNotificacion,
                Estado = estado
            };
        }
    }
}
