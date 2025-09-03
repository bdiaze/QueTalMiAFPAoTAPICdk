using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class NotificacionDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<List<Notificacion>> ObtenerPorSub(string sub, short? vigente = null) {
            List<Notificacion> notificaciones = [];

            string queryString = " SELECT NT.\"ID\", NT.\"SUB\", NT.\"CORREO_NOTIFICACION\", NT.\"ID_TIPO_NOTIFICACION\", " +
                "NT.\"FECHA_CREACION\", NT.\"FECHA_ELIMINACION\", NT.\"VIGENTE\", NT.\"FECHA_HABILITACION\", NT.\"FECHA_DESHABILITACION\", NT.\"HABILITADO\" " +
                "FROM \"QueTalMiAFP\".\"NOTIFICACION\" NT " +
                "WHERE NT.\"SUB\" = @Sub " +
                (vigente != null ? "AND NT.\"VIGENTE\" = @Vigente;" : ";");

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Sub", sub);
            if (vigente != null) command.Parameters.AddWithValue("@Vigente", vigente);

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                notificaciones.Add(new() {
                    Id = reader.GetInt64(0),
                    Sub = reader.GetString(1),
                    CorreoNotificacion = reader.GetString(2),
                    IdTipoNotificacion = reader.GetInt16(3),
                    FechaCreacion = reader.GetDateTime(4),
                    FechaEliminacion = !await reader.IsDBNullAsync(5) ? reader.GetDateTime(5) : null,
                    Vigente = reader.GetInt16(6),
                    FechaHabilitacion = reader.GetDateTime(7),
                    FechaDeshabilitacion = !await reader.IsDBNullAsync(8) ? reader.GetDateTime(8) : null,
                    Habilitado = reader.GetInt16(9)
                });
            }
            await reader.CloseAsync();

            return notificaciones;
        }

        public async Task<List<Notificacion>> ObtenerPorTipoNotificacion(short idTipoNotificacion, short? habilitado = null) {
            List<Notificacion> notificaciones = [];

            string queryString = " SELECT NT.\"ID\", NT.\"SUB\", NT.\"CORREO_NOTIFICACION\", NT.\"ID_TIPO_NOTIFICACION\", " +
                "NT.\"FECHA_CREACION\", NT.\"FECHA_ELIMINACION\", NT.\"VIGENTE\", NT.\"FECHA_HABILITACION\", NT.\"FECHA_DESHABILITACION\", NT.\"HABILITADO\" " +
                "FROM \"QueTalMiAFP\".\"NOTIFICACION\" NT " +
                "WHERE NT.\"ID_TIPO_NOTIFICACION\" = @IdTipoNotificacion " +
                (habilitado != null ? "AND NT.\"HABILITADO\" = @Habilitado;" : ";");

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@IdTipoNotificacion", idTipoNotificacion);
            if (habilitado != null) command.Parameters.AddWithValue("@Habilitado", habilitado);

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                notificaciones.Add(new() {
                    Id = reader.GetInt64(0),
                    Sub = reader.GetString(1),
                    CorreoNotificacion = reader.GetString(2),
                    IdTipoNotificacion = reader.GetInt16(3),
                    FechaCreacion = reader.GetDateTime(4),
                    FechaEliminacion = !await reader.IsDBNullAsync(5) ? reader.GetDateTime(5) : null,
                    Vigente = reader.GetInt16(6),
                    FechaHabilitacion = reader.GetDateTime(7),
                    FechaDeshabilitacion = !await reader.IsDBNullAsync(8) ? reader.GetDateTime(8) : null,
                    Habilitado = reader.GetInt16(9)
                });
            }
            await reader.CloseAsync();

            return notificaciones;
        }

        public async Task<Notificacion> Ingresar(string sub, string correoNotificacion, short idTipoNotificacion, DateTimeOffset fechaCreacion, DateTimeOffset? fechaEliminacion, short vigente, DateTimeOffset fechaHabilitacion, DateTimeOffset? fechaDeshabilitacion, short habilitado) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"NOTIFICACION\"(" +
                "\"SUB\", " +
                "\"CORREO_NOTIFICACION\", " +
                "\"ID_TIPO_NOTIFICACION\", " +
                "\"FECHA_CREACION\", " +
                "\"FECHA_ELIMINACION\", " +
                "\"VIGENTE\", " +
                "\"FECHA_HABILITACION\", " +
                "\"FECHA_DESHABILITACION\", " +
                "\"HABILITADO\"" +
                ") VALUES (" +
                "@Sub, " +
                "@CorreoNotificacion, " +
                "@IdTipoNotificacion, " +
                "@FechaCreacion, " +
                "@FechaEliminacion, " +
                "@Vigente, " +
                "@FechaHabilitacion, " +
                "@FechaDeshabilitacion, " +
                "@Habilitado" +
                ") RETURNING \"ID\";";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Sub", sub);
            command.Parameters.AddWithValue("@CorreoNotificacion", correoNotificacion);
            command.Parameters.AddWithValue("@IdTipoNotificacion", idTipoNotificacion);
            command.Parameters.AddWithValue("@FechaCreacion", fechaCreacion.ToUniversalTime());
            if (fechaEliminacion != null) command.Parameters.AddWithValue("@FechaEliminacion", fechaEliminacion.Value.ToUniversalTime());
            else command.Parameters.AddWithValue("@FechaEliminacion", DBNull.Value);
            command.Parameters.AddWithValue("@Vigente", vigente);
            command.Parameters.AddWithValue("@FechaHabilitacion", fechaHabilitacion.ToUniversalTime());
            if (fechaDeshabilitacion != null) command.Parameters.AddWithValue("@FechaDeshabilitacion", fechaDeshabilitacion.Value.ToUniversalTime());
            else command.Parameters.AddWithValue("@FechaDeshabilitacion", DBNull.Value);
            command.Parameters.AddWithValue("@Habilitado", habilitado);

            long id = (long)(await command.ExecuteScalarAsync())!;

            return new Notificacion {
                Id = id,
                Sub = sub,
                CorreoNotificacion = correoNotificacion,
                IdTipoNotificacion = idTipoNotificacion,
                FechaCreacion = fechaCreacion,
                FechaEliminacion = fechaEliminacion,
                Vigente = vigente,
                FechaHabilitacion = fechaHabilitacion,
                FechaDeshabilitacion = fechaDeshabilitacion,
                Habilitado = habilitado
            };
        }

        public async Task<Notificacion> Modificar(Notificacion notificacion) {
            string queryString = "UPDATE \"QueTalMiAFP\".\"NOTIFICACION\" SET " +
                "\"SUB\" = @Sub, " +
                "\"CORREO_NOTIFICACION\" = @CorreoNotificacion, " +
                "\"ID_TIPO_NOTIFICACION\" = @IdTipoNotificacion, " +
                "\"FECHA_CREACION\" = @FechaCreacion, " +
                "\"FECHA_ELIMINACION\" = @FechaEliminacion, " +
                "\"VIGENTE\" = @Vigente, " +
                "\"FECHA_HABILITACION\" = @FechaHabilitacion, " +
                "\"FECHA_DESHABILITACION\" = @FechaDeshabilitacion, " +
                "\"HABILITADO\" = @Habilitado " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", notificacion.Id!);
            command.Parameters.AddWithValue("@Sub", notificacion.Sub);
            command.Parameters.AddWithValue("@CorreoNotificacion", notificacion.CorreoNotificacion);
            command.Parameters.AddWithValue("@IdTipoNotificacion", notificacion.IdTipoNotificacion);
            command.Parameters.AddWithValue("@FechaCreacion", notificacion.FechaCreacion.ToUniversalTime());
            if (notificacion.FechaEliminacion != null) command.Parameters.AddWithValue("@FechaEliminacion", notificacion.FechaEliminacion.Value.ToUniversalTime());
            else command.Parameters.AddWithValue("@FechaEliminacion", DBNull.Value);
            command.Parameters.AddWithValue("@Vigente", notificacion.Vigente);
            command.Parameters.AddWithValue("@FechaHabilitacion", notificacion.FechaHabilitacion.ToUniversalTime());
            if (notificacion.FechaDeshabilitacion != null) command.Parameters.AddWithValue("@FechaDeshabilitacion", notificacion.FechaDeshabilitacion.Value.ToUniversalTime());
            else command.Parameters.AddWithValue("@FechaDeshabilitacion", DBNull.Value);
            command.Parameters.AddWithValue("@Habilitado", notificacion.Habilitado);

            await command.ExecuteNonQueryAsync();

            return notificacion;
        }
    }
}
