using Npgsql;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using System.Data.Common;

namespace QueTalMiAFPAoTAPI.Repositories {
    public class TipoMensajeDAO(DatabaseConnectionHelper connectionHelper) {
        public async Task<List<TipoMensaje>> ObtenerTiposMensaje(byte vigencia = 1) {
            List<TipoMensaje> tiposMensaje = [];

            string queryString = "SELECT TM.\"ID_TIPO_MENSAJE\", TM.\"DESCRIPCION_CORTA\", TM.\"DESCRIPCION_LARGA\", TM.\"VIGENCIA\" " +
                    "FROM \"QueTalMiAFP\".\"TIPO_MENSAJE\" TM " +
                    "WHERE TM.\"VIGENCIA\" = @Vigencia " +
                    "ORDER BY TM.\"ID_TIPO_MENSAJE\" DESC;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);

            command.Parameters.AddWithValue("@Vigencia", vigencia);

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                tiposMensaje.Add(new TipoMensaje {
                    IdTipoMensaje = reader.GetInt16(0),
                    DescripcionCorta = reader.GetString(1),
                    DescripcionLarga = reader.GetString(2),
                    Vigencia = reader.GetByte(3)
                });
            }
            await reader.CloseAsync();

            return tiposMensaje;
        }

        public async Task<TipoMensaje?> ObtenerTipoMensaje(short idTipoMensaje) {
            TipoMensaje? tipoMensaje = null;

            string queryString = "SELECT TM.\"ID_TIPO_MENSAJE\", TM.\"DESCRIPCION_CORTA\", TM.\"DESCRIPCION_LARGA\", TM.\"VIGENCIA\" " +
                    "FROM \"QueTalMiAFP\".\"TIPO_MENSAJE\" TM " +
                    "WHERE TM.\"ID_TIPO_MENSAJE\" = @IdTipoMensaje;";

            await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
            await using NpgsqlCommand command = new(queryString, connection);
            
            command.Parameters.AddWithValue("@IdTipoMensaje", idTipoMensaje);

            DbDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                tipoMensaje = new TipoMensaje { 
                    IdTipoMensaje = reader.GetInt16(0),
                    DescripcionCorta = reader.GetString(1),
                    DescripcionLarga = reader.GetString(2),
                    Vigencia = reader.GetByte(3)
                };
            }
            await reader.CloseAsync();

            return tipoMensaje;
        }
    }
}
