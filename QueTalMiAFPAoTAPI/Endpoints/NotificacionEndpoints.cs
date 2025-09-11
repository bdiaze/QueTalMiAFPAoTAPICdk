using Amazon.Lambda.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Models;
using QueTalMiAFPAoTAPI.Entities;
using QueTalMiAFPAoTAPI.Helpers;
using QueTalMiAFPAoTAPI.Models;
using QueTalMiAFPAoTAPI.Repositories;
using System.Diagnostics;
using System.Text.Json;

namespace QueTalMiAFPAoTAPI.Endpoints {
    public static class NotificacionEndpoints {
        public static IEndpointRouteBuilder MapNotificacionEndpoints(this IEndpointRouteBuilder routes) {
            RouteGroupBuilder group = routes.MapGroup("/Notificacion");
            group.MapObtenerPorSubEndpoint();
            group.MapObtenerPorTipoNotificacionEndpoint();
            group.MapIngresarEndpoint();
            group.MapModificarEndpoint();

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerPorSubEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerPorSub", async (string sub, NotificacionDAO notificacionDAO, short? vigente = null) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    List<Notificacion> salida = await notificacionDAO.ObtenerPorSub(sub, vigente);

                    LambdaLogger.Log(
                        $"[GET] - [Notificacion] - [ObtenerPorSub] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo exitosamente las notificaciones para el sub: {sub} - Cantidad: {salida.Count}.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [Notificacion] - [ObtenerPorSub] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener las notificaciones para el sub: {sub}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }

        private static IEndpointRouteBuilder MapObtenerPorTipoNotificacionEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapGet("/ObtenerPorTipoNotificacion", async (short idTipoNotificacion, NotificacionDAO notificacionDAO, short? habilitado = null) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    List<Notificacion> salida = await notificacionDAO.ObtenerPorTipoNotificacion(idTipoNotificacion, habilitado);

                    LambdaLogger.Log(
                        $"[GET] - [Notificacion] - [ObtenerPorTipoNotificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se obtuvo exitosamente las notificaciones para el tipo de notificación ID: {idTipoNotificacion} - Cantidad: {salida.Count}.");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[GET] - [Notificacion] - [ObtenerPorTipoNotificacion] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al obtener las notificaciones para el tipo de notificación ID: {idTipoNotificacion}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }

        private static IEndpointRouteBuilder MapIngresarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPost("/Ingresar", async (EntIngresarNotificacion entrada, VariableEntornoHelper variableEntorno, ParameterStoreHelper parameterStore,  NotificacionDAO notificacionDAO, TipoNotificacionDAO tipoNotificacionDAO, TipoPeriodicidadDAO tipoPeriodicidadDAO, KairosHelper kairos) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    // Se valida que no haya ya un registro de notificación aún vigente...
                    Notificacion? salida = (await notificacionDAO.ObtenerPorSub(entrada.Sub, 1 /* Vigente */)).FirstOrDefault(n => n.IdTipoNotificacion == entrada.IdTipoNotificacion);
                    
                    if (salida == null) {
                        // Se obtiene el tipo de notificación...
                        TipoNotificacion? tipoNotificacion = await tipoNotificacionDAO.ObtenerUna(entrada.IdTipoNotificacion) ?? throw new Exception($"No existe el tipo de notificación ID: {entrada.IdTipoNotificacion}");
                        TipoPeriodicidad? tipoPeriodicidad = await tipoPeriodicidadDAO.ObtenerUna(tipoNotificacion.IdTipoPeriodicidad) ?? throw new Exception($"No existe el tipo de periodicidad ID: {tipoNotificacion.IdTipoPeriodicidad}");
                        
                        // Nos aseguramos que exista el proceso de notificación para el cron deseado...
                        SalKairosIngresarProceso kairosProceso = await kairos.IngresarProceso(new EntKairosIngresarProceso() { 
                            Nombre = $"Notificación {entrada.IdTipoNotificacion} - {variableEntorno.Obtener("APP_NAME")}",
                            Cron = tipoPeriodicidad.Cron,
                            ArnProceso = await parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_NOTIFICACIONES_LAMBDA_ARN")),
                            ArnRol = await parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_NOTIFICACIONES_EJECUCION_ROLE_ARN")),
                            Parametros = JsonSerializer.Serialize(new ParametroNotificacion { IdTipoNotificacion = entrada.IdTipoNotificacion }, AppJsonSerializerContext.Default.ParametroNotificacion),
                            Habilitado = true,
                        });

                        // Si no tenemos registrado el ID Proceso se actualiza... 
                        if (tipoNotificacion.IdProceso != kairosProceso.IdProceso) {
                            tipoNotificacion.IdProceso = kairosProceso.IdProceso;
                            tipoNotificacion = await tipoNotificacionDAO.Modificar(tipoNotificacion);
                        }

                        salida = await notificacionDAO.Ingresar(
                            entrada.Sub,
                            entrada.CorreoNotificacion,
                            tipoNotificacion.Id,
                            DateTimeOffset.Now,
                            null,
                            1 /* Vigente */,
                            DateTimeOffset.Now,
                            null,
                            1 /* Habilitado */
                        );
                    }

                    LambdaLogger.Log(
                        $"[POST] - [Notificacion] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se ingresó exitosamente la notificación - ID: {salida.Id}");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[POST] - [Notificacion] - [Ingresar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al ingresar la notificación - Sub: {entrada.Sub} - ID Tipo Notificación: {entrada.IdTipoNotificacion}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }

        private static IEndpointRouteBuilder MapModificarEndpoint(this IEndpointRouteBuilder routes) {
            routes.MapPut("/Modificar", async (Notificacion notificacion, VariableEntornoHelper variableEntorno, ParameterStoreHelper parameterStore, NotificacionDAO notificacionDAO, TipoNotificacionDAO tipoNotificacionDAO, TipoPeriodicidadDAO tipoPeriodicidadDAO, KairosHelper kairos) => {
                Stopwatch stopwatch = Stopwatch.StartNew();

                try {
                    Notificacion? salida = (await notificacionDAO.ObtenerPorSub(notificacion.Sub, 1)).FirstOrDefault(n => n.Id == notificacion.Id) ?? throw new Exception("La notificación no existe.");

                    // Si se está eliminando, también se deja deshabilitado...
                    if (notificacion.Vigente == 0) {
                        notificacion.Habilitado = 0;
                    }

                    // Si se está eliminando, se registra la fecha de eliminación...
                    if (notificacion.Vigente == 0 && salida.Vigente == 1) {
                        salida.Vigente = 0;
                        salida.FechaEliminacion = DateTimeOffset.Now;
                    }

                    // Si se está deshabilitando, se registra la fecha de deshabilitación...
                    if (notificacion.Habilitado == 0 && salida.Habilitado == 1) {
                        salida.Habilitado = 0;
                        salida.FechaDeshabilitacion = DateTimeOffset.Now;

                        // Además, se desprograma proceso de notificación si no existen otras notificaciones habilitadas...
                        int cantOtrasNotif = (await notificacionDAO.ObtenerPorTipoNotificacion(salida.IdTipoNotificacion, 1)).Where(n => n.Id != salida.Id).Count();
                        if (cantOtrasNotif == 0) {
                            TipoNotificacion? tipoNotificacion = await tipoNotificacionDAO.ObtenerUna(salida.IdTipoNotificacion) ?? throw new Exception($"No existe el tipo de notificación ID: {salida.IdTipoNotificacion}");
                            if (tipoNotificacion.IdProceso != null) await kairos.EliminarProceso(tipoNotificacion.IdProceso);
                        }

                    // Si se está habilitando, se registra la nueva fecha de habilitación y se quita fecha de deshabilitación (solo si está vigente)...
                    } else if (notificacion.Habilitado == 1 && salida.Habilitado == 0 && salida.Vigente == 1) {
                        salida.Habilitado = 1;
                        salida.FechaHabilitacion = DateTimeOffset.Now;
                        salida.FechaDeshabilitacion = null;

                        // Además, se programa proceso de notificación si no existen otras notificaciones habilitadas...
                        int cantOtrasNotif = (await notificacionDAO.ObtenerPorTipoNotificacion(salida.IdTipoNotificacion, 1)).Where(n => n.Id != salida.Id).Count();
                        if (cantOtrasNotif == 0) {
                            TipoNotificacion? tipoNotificacion = await tipoNotificacionDAO.ObtenerUna(salida.IdTipoNotificacion) ?? throw new Exception($"No existe el tipo de notificación ID: {salida.IdTipoNotificacion}");
                            TipoPeriodicidad? tipoPeriodicidad = await tipoPeriodicidadDAO.ObtenerUna(tipoNotificacion.IdTipoPeriodicidad) ?? throw new Exception($"No existe el tipo de periodicidad ID: {tipoNotificacion.IdTipoPeriodicidad}");

                            SalKairosIngresarProceso kairosProceso = await kairos.IngresarProceso(new EntKairosIngresarProceso() {
                                Nombre = $"Notificación {salida.IdTipoNotificacion} - {variableEntorno.Obtener("APP_NAME")}",
                                Cron = tipoPeriodicidad.Cron,
                                ArnProceso = await parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_NOTIFICACIONES_LAMBDA_ARN")),
                                ArnRol = await parameterStore.ObtenerParametro(variableEntorno.Obtener("ARN_PARAMETER_NOTIFICACIONES_EJECUCION_ROLE_ARN")),
                                Parametros = JsonSerializer.Serialize(new ParametroNotificacion { IdTipoNotificacion = salida.IdTipoNotificacion }, AppJsonSerializerContext.Default.ParametroNotificacion),
                                Habilitado = true,
                            });

                            // Si no tenemos registrado el ID Proceso se actualiza... 
                            if (tipoNotificacion.IdProceso != kairosProceso.IdProceso) {
                                tipoNotificacion.IdProceso = kairosProceso.IdProceso;
                                tipoNotificacion = await tipoNotificacionDAO.Modificar(tipoNotificacion);
                            }
                        }

                    }

                    salida = await notificacionDAO.Modificar(salida);

                    LambdaLogger.Log(
                        $"[PUT] - [Notificacion] - [Modificar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status200OK}] - " +
                        $"Se modificó exitosamente la notificación - ID: {salida.Id}");

                    return Results.Ok(salida);
                } catch (Exception ex) {
                    LambdaLogger.Log(
                        $"[PUT] - [Notificacion] - [Modificar] - [{stopwatch.ElapsedMilliseconds} ms] - [{StatusCodes.Status500InternalServerError}] - " +
                        $"Ocurrió un error al modificar la notificación - ID: {notificacion.Id}. " +
                        $"{ex}");
                    return Results.Problem("Ocurrió un error al procesar su solicitud.");
                }
            }).WithOpenApi();

            return routes;
        }
    }
}
