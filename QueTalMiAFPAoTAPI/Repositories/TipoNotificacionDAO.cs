using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class TipoNotificacionDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<TipoNotificacion?> ObtenerUna(short id) {
            TipoNotificacion? tipoNotificacion = null;

            string queryString = " SELECT TN.\"ID\", TN.\"NOMBRE\", TN.\"DESCRIPCION\", TN.\"ID_TIPO_PERIODICIDAD\", TN.\"HABILITADO\", TN.\"ID_PROCESO\" " +
                "FROM \"QueTalMiAFP\".\"TIPO_NOTIFICACION\" TN " +
                "WHERE TN.\"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", id);

            DbDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                tipoNotificacion = new() {
                    Id = reader.GetInt16(0),
                    Nombre = reader.GetString(1),
                    Descripcion = reader.GetString(2),
                    IdTipoPeriodicidad = reader.GetInt16(3),
                    Habilitado = reader.GetInt16(4),
                    IdProceso = !await reader.IsDBNullAsync(5) ? reader.GetString(5) : null,
                };
            }
            await reader.CloseAsync();

            return tipoNotificacion;
        }

        public async Task<List<TipoNotificacion>> ObtenerPorTipoPeriodicidad(short idTipoPeriodicidad, short? habilitado = null) {
            List<TipoNotificacion> tiposNotificaciones = [];

            string queryString = " SELECT TN.\"ID\", TN.\"NOMBRE\", TN.\"DESCRIPCION\", TN.\"ID_TIPO_PERIODICIDAD\", TN.\"HABILITADO\", TN.\"ID_PROCESO\" " +
                "FROM \"QueTalMiAFP\".\"TIPO_NOTIFICACION\" TN " +
                "WHERE TN.\"ID_TIPO_PERIODICIDAD\" = @IdTipoPeriodicidad " +
                (habilitado != null ? "AND TN.\"HABILITADO\" = @Habilitado;" : ";");

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@IdTipoPeriodicidad", idTipoPeriodicidad);
            if (habilitado != null) command.Parameters.AddWithValue("@Habilitado", habilitado);

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                tiposNotificaciones.Add(new() {
                    Id = reader.GetInt16(0),
                    Nombre = reader.GetString(1),
                    Descripcion = reader.GetString(2),
                    IdTipoPeriodicidad = reader.GetInt16(3),
                    Habilitado = reader.GetInt16(4),
                    IdProceso = !await reader.IsDBNullAsync(5) ? reader.GetString(5) : null,
                });
            }
            await reader.CloseAsync();

            return tiposNotificaciones;
        }

        public async Task<List<TipoNotificacion>> ObtenerTodas() {
            List<TipoNotificacion> tiposNotificaciones = [];

            string queryString = " SELECT TN.\"ID\", TN.\"NOMBRE\", TN.\"DESCRIPCION\", TN.\"ID_TIPO_PERIODICIDAD\", TN.\"HABILITADO\", TN.\"ID_PROCESO\" " +
                "FROM \"QueTalMiAFP\".\"TIPO_NOTIFICACION\" TN;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                tiposNotificaciones.Add(new() {
                    Id = reader.GetInt16(0),
                    Nombre = reader.GetString(1),
                    Descripcion = reader.GetString(2),
                    IdTipoPeriodicidad = reader.GetInt16(3),
                    Habilitado = reader.GetInt16(4),
                    IdProceso = !await reader.IsDBNullAsync(5) ? reader.GetString(5) : null,
                });
            }
            await reader.CloseAsync();

            return tiposNotificaciones;
        }

        public async Task<TipoNotificacion> Ingresar(short id, string nombre, string descripcion, short idTipoPeriodicidad, short habilitado, string? idProceso = null) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"TIPO_NOTIFICACION\"(" +
                "\"ID\", " +
                "\"NOMBRE\", " +
                "\"DESCRIPCION\", " +
                "\"ID_TIPO_PERIODICIDAD\"," +
                "\"HABILITADO\"," +
                "\"ID_PROCESO\"" +
                ") VALUES (" +
                "@Id, " +
                "@Nombre, " +
                "@Descripcion, " +
                "@IdTipoPeriodicidad, " +
                "@Habilitado, " +
                "@IdProceso" +
                ");";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Nombre", nombre);
            command.Parameters.AddWithValue("@Descripcion", descripcion);
            command.Parameters.AddWithValue("@IdTipoPeriodicidad", idTipoPeriodicidad);
            command.Parameters.AddWithValue("@Habilitado", habilitado);
            if (idProceso != null) command.Parameters.AddWithValue("@IdProceso", idProceso);
            else command.Parameters.AddWithValue("@IdProceso", DBNull.Value);

            await command.ExecuteNonQueryAsync();

            return new TipoNotificacion {
                Id = id,
                Nombre = nombre,
                Descripcion = descripcion,
                IdTipoPeriodicidad = idTipoPeriodicidad,
                Habilitado = habilitado
            };
        }

        public async Task<TipoNotificacion> Modificar(TipoNotificacion tipoNotificacion) {
            string queryString = "UPDATE \"QueTalMiAFP\".\"TIPO_NOTIFICACION\" SET " +
                "\"NOMBRE\" = @Nombre, " +
                "\"DESCRIPCION\" = @Descripcion, " +
                "\"ID_TIPO_PERIODICIDAD\" = @IdTipoPeriodicidad, " +
                "\"HABILITADO\" = @Habilitado, " +
                "\"ID_PROCESO\" = @IdProceso " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", tipoNotificacion.Id);
            command.Parameters.AddWithValue("@Nombre", tipoNotificacion.Nombre);
            command.Parameters.AddWithValue("@Descripcion", tipoNotificacion.Descripcion);
            command.Parameters.AddWithValue("@IdTipoPeriodicidad", tipoNotificacion.IdTipoPeriodicidad);
            command.Parameters.AddWithValue("@Habilitado", tipoNotificacion.Habilitado);
            if (tipoNotificacion.IdProceso != null) command.Parameters.AddWithValue("@IdProceso", tipoNotificacion.IdProceso);
            else command.Parameters.AddWithValue("@IdProceso", DBNull.Value);

            await command.ExecuteNonQueryAsync();

            return tipoNotificacion;
        }

        public async Task Eliminar(short id) {
            string queryString = "DELETE FROM \"QueTalMiAFP\".\"TIPO_NOTIFICACION\" " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", id);

            await command.ExecuteNonQueryAsync();
        }
    }
}
