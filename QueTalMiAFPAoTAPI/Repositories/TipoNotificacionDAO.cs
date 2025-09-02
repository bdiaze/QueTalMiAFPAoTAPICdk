using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class TipoNotificacionDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<TipoNotificacion?> ObtenerUna(short id) {
            TipoNotificacion? tipoNotificacion = null;

            string queryString = " SELECT TN.\"ID\", TN.\"NOMBRE\", TN.\"DESCRIPCION\", TN.\"ID_TIPO_PERIODICIDAD\", TN.\"HABILITADO\" " +
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
                    Habilitado = reader.GetInt16(4)
                };
            }
            await reader.CloseAsync();

            return tipoNotificacion;
        }

        public async Task<List<TipoNotificacion>> ObtenerPorTipoPeriodicidad(short idTipoPeriodicidad, short? habilitado = null) {
            List<TipoNotificacion> tiposNotificaciones = [];

            string queryString = " SELECT TN.\"ID\", TN.\"NOMBRE\", TN.\"DESCRIPCION\", TN.\"ID_TIPO_PERIODICIDAD\", TN.\"HABILITADO\" " +
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
                    Habilitado = reader.GetInt16(4)
                });
            }
            await reader.CloseAsync();

            return tiposNotificaciones;
        }

        public async Task<List<TipoNotificacion>> ObtenerTodas() {
            List<TipoNotificacion> tiposNotificaciones = [];

            string queryString = " SELECT TN.\"ID\", TN.\"NOMBRE\", TN.\"DESCRIPCION\", TN.\"ID_TIPO_PERIODICIDAD\", TN.\"HABILITADO\" " +
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
                    Habilitado = reader.GetInt16(4)
                });
            }
            await reader.CloseAsync();

            return tiposNotificaciones;
        }

        public async Task<TipoNotificacion> Ingresar(short id, string nombre, string descripcion, short idTipoPeriodicidad, short habilitado) {
            string queryString = "INSERT INTO \"QueTalMiAFP\".\"TIPO_NOTIFICACION\"(" +
                "\"ID\", " +
                "\"NOMBRE\", " +
                "\"DESCRIPCION\", " +
                "\"ID_TIPO_PERIODICIDAD\"," +
                "\"HABILITADO\"" +
                ") VALUES (" +
                "@Id, " +
                "@Nombre, " +
                "@Descripcion, " +
                "@IdTipoPeriodicidad, " +
                "@Habilitado" +
                ");";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Nombre", nombre);
            command.Parameters.AddWithValue("@Descripcion", descripcion);
            command.Parameters.AddWithValue("@IdTipoPeriodicidad", idTipoPeriodicidad);
            command.Parameters.AddWithValue("@Habilitado", habilitado);

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
                "\"HABILITADO\" = @Habilitado " +
                "WHERE \"ID\" = @Id;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Id", tipoNotificacion.Id);
            command.Parameters.AddWithValue("@Nombre", tipoNotificacion.Nombre);
            command.Parameters.AddWithValue("@Descripcion", tipoNotificacion.Descripcion);
            command.Parameters.AddWithValue("@IdTipoPeriodicidad", tipoNotificacion.IdTipoPeriodicidad);
            command.Parameters.AddWithValue("@Habilitado", tipoNotificacion.Habilitado);

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
