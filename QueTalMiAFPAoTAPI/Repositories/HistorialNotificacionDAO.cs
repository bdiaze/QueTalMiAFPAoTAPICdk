using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class HistorialNotificacionDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<List<HistorialNotificacion>> ObtenerPorNotificacion(long idNotificacion) {
            List<HistorialNotificacion> historialNotificaciones = [];

            string queryString = " SELECT HN.\"ID\", HN.\"ID_NOTIFICACION\", HN.\"FECHA_NOTIFICACION\", HN.\"ESTADO\" " +
                "FROM \"QueTalMiAFP\".\"HISTORIAL_NOTIFICACION\" HN " +
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

        public async Task<List<HistorialNotificacion>> ObtenerPorRangoFechas(DateTimeOffset fechaDesde, DateTimeOffset fechaHasta) {
            List<HistorialNotificacion> historialNotificaciones = [];

            string queryString = " SELECT HN.\"ID\", HN.\"ID_NOTIFICACION\", HN.\"FECHA_NOTIFICACION\", HN.\"ESTADO\" " +
                "FROM \"QueTalMiAFP\".\"HISTORIAL_NOTIFICACION\" HN " +
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

        public async Task<HistorialNotificacion> Ingresar(long idNotificacion, DateTimeOffset fechaNotificacion, short estado) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"HISTORIAL_NOTIFICACION\"(" +
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

            return new HistorialNotificacion {
                Id = id,
                IdNotificacion = idNotificacion,
                FechaNotificacion = fechaNotificacion,
                Estado = estado
            };
        }
    }
}
