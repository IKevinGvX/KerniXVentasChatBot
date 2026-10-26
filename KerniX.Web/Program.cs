using KerniX.Web.Models;
using KerniX.Web.Services;
using OpenAI.Chat;


// ================================================================
// BUILDER
// ================================================================

var builder =
    WebApplication.CreateBuilder(args);


// ================================================================
// RAZOR PAGES
// ================================================================

builder.Services.AddRazorPages();


// ================================================================
// OPENAI
// ================================================================

string openAiApiKey =
    builder.Configuration["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException(
        "No se encontró OpenAI:ApiKey. Configúrala mediante User Secrets."
    );

string openAiModel =
    builder.Configuration["OpenAI:Model"]
    ?? "gpt-5-mini";


builder.Services.AddSingleton(
    new ChatClient(
        model: openAiModel,
        apiKey: openAiApiKey
    )
);


// ================================================================
// SERVICIOS KERNIX
// ================================================================

builder.Services.AddScoped<
    IKernixAiService,
    KernixAiService
>();


builder.Services.AddSingleton<
    IKernixConversationService,
    KernixConversationService
>();


builder.Services.AddSingleton<
    IKernixActionService,
    KernixActionService
>();


builder.Services.AddSingleton<
    IKernixProductCatalogService,
    KernixProductCatalogService
>();


builder.Services.AddSingleton<
    IKernixCotizacionTemporalService,
    KernixCotizacionTemporalService
>();


builder.Services.AddSingleton<
    IKernixCommercialExtractorService,
    KernixCommercialExtractorService
>();


builder.Services.AddSingleton<
    KernixCommercialRegistrationService
>();


// ================================================================
// BUILD
// ================================================================

var app =
    builder.Build();


// ================================================================
// PIPELINE
// ================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();


// ================================================================
// CREAR CONVERSACIÓN
// ================================================================

app.MapPost(
    "/api/kernix/conversations",
    (
        IKernixConversationService conversationService
    ) =>
    {
        KernixConversation conversation =
            conversationService.CreateConversation();


        return Results.Ok(
            new
            {
                success = true,

                conversationId =
                    conversation.Id,

                title =
                    conversation.Title,

                createdAt =
                    conversation.CreatedAt
            }
        );
    }
);


// ================================================================
// LISTAR CONVERSACIONES
// ================================================================

app.MapGet(
    "/api/kernix/conversations",
    (
        IKernixConversationService conversationService
    ) =>
    {
        var conversations =
            conversationService
                .GetConversations()
                .Select(
                    conversation =>
                        new
                        {
                            id =
                                conversation.Id,

                            title =
                                conversation.Title,

                            summary =
                                conversation.Summary,

                            createdAt =
                                conversation.CreatedAt,

                            updatedAt =
                                conversation.UpdatedAt,

                            messageCount =
                                conversation.Messages.Count
                        }
                )
                .ToList();


        return Results.Ok(
            new
            {
                success = true,
                conversations
            }
        );
    }
);


// ================================================================
// OBTENER CONVERSACIÓN
// ================================================================

app.MapGet(
    "/api/kernix/conversations/{id:guid}",
    (
        Guid id,
        IKernixConversationService conversationService
    ) =>
    {
        KernixConversation? conversation =
            conversationService.GetConversation(
                id
            );


        if (conversation == null)
        {
            return Results.NotFound(
                new
                {
                    success = false,

                    message =
                        "La conversación no existe."
                }
            );
        }


        IReadOnlyList<KernixConversationMessage> messages =
            conversationService.GetMessages(
                id
            );


        return Results.Ok(
            new
            {
                success = true,

                conversation =
                    new
                    {
                        id =
                            conversation.Id,

                        title =
                            conversation.Title,

                        summary =
                            conversation.Summary,

                        createdAt =
                            conversation.CreatedAt,

                        updatedAt =
                            conversation.UpdatedAt,

                        messages =
                            messages.Select(
                                message =>
                                    new
                                    {
                                        id =
                                            message.Id,

                                        role =
                                            message.Role,

                                        content =
                                            message.Content,

                                        createdAt =
                                            message.CreatedAt
                                    }
                            )
                    }
            }
        );
    }
);


// ================================================================
// ELIMINAR CONVERSACIÓN
// ================================================================

app.MapDelete(
    "/api/kernix/conversations/{id:guid}",
    (
        Guid id,
        IKernixConversationService conversationService,
        IKernixCotizacionTemporalService cotizacionService
    ) =>
    {
        bool deleted =
            conversationService.DeleteConversation(
                id
            );


        // También eliminamos cualquier cotización temporal
        // asociada con esta conversación.
        cotizacionService.Delete(
            id
        );


        if (!deleted)
        {
            return Results.NotFound(
                new
                {
                    success = false,

                    message =
                        "La conversación no existe."
                }
            );
        }


        return Results.Ok(
            new
            {
                success = true,

                message =
                    "Conversación eliminada correctamente."
            }
        );
    }
);


// ================================================================
// KERNIX IA - ENDPOINT NORMAL
// ================================================================

app.MapPost(
    "/api/kernix/chat",
    async (
        KernixChatRequest request,
        IKernixAiService aiService,
        CancellationToken cancellationToken
    ) =>
    {
        // ============================================================
        // VALIDACIÓN
        // ============================================================

        if (
            request == null ||
            string.IsNullOrWhiteSpace(
                request.Message
            ))
        {
            return Results.BadRequest(
                new
                {
                    success = false,

                    message =
                        "Debes escribir un mensaje."
                }
            );
        }


        int maxCharacters =
            app.Configuration.GetValue<int?>(
                "OpenAI:MaxMessageCharacters"
            ) ?? 4000;


        if (
            request.Message.Length >
            maxCharacters)
        {
            return Results.BadRequest(
                new
                {
                    success = false,

                    message =
                        $"El mensaje no puede superar {maxCharacters} caracteres."
                }
            );
        }


        // ============================================================
        // OPENAI
        // ============================================================

        try
        {
            KernixChatResponse response =
                await aiService.SendMessageAsync(
                    request,
                    cancellationToken
                );


            return Results.Ok(
                response
            );
        }

        catch (OperationCanceledException)
        {
            return Results.Json(
                new
                {
                    success = false,

                    message =
                        "La solicitud fue cancelada."
                },
                statusCode: 499
            );
        }

        catch (ArgumentException ex)
        {
            return Results.BadRequest(
                new
                {
                    success = false,
                    message = ex.Message
                }
            );
        }

        catch (Exception ex)
        {
            app.Logger.LogError(
                ex,
                "Error procesando KerniX IA."
            );


            return Results.Json(
                new
                {
                    success = false,

                    message =
                        app.Environment.IsDevelopment()
                            ? $"OPENAI ERROR: {ex.Message}"
                            : "KerniX IA no está disponible temporalmente."
                },
                statusCode: 500
            );
        }
    }
);


// ================================================================
// KERNIX IA - STREAM + CONVERSACIÓN + REGISTRO COMERCIAL
// ================================================================

app.MapPost(
    "/api/kernix/chat/stream",
    async (
        KernixChatRequest request,
        IKernixAiService aiService,
        IKernixConversationService conversationService,
        KernixCommercialRegistrationService commercialRegistrationService,
        HttpContext context,
        CancellationToken cancellationToken
    ) =>
    {
        // ============================================================
        // VALIDACIÓN
        // ============================================================

        if (
            request == null ||
            string.IsNullOrWhiteSpace(
                request.Message
            ))
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;


            context.Response.ContentType =
                "text/plain; charset=utf-8";


            await context.Response.WriteAsync(
                "Debes escribir un mensaje.",
                cancellationToken
            );


            return;
        }


        int maxCharacters =
            app.Configuration.GetValue<int?>(
                "OpenAI:MaxMessageCharacters"
            ) ?? 4000;


        if (
            request.Message.Length >
            maxCharacters)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;


            context.Response.ContentType =
                "text/plain; charset=utf-8";


            await context.Response.WriteAsync(
                $"El mensaje no puede superar {maxCharacters} caracteres.",
                cancellationToken
            );


            return;
        }


        // ============================================================
        // CONVERSATION ID
        // ============================================================

        Guid conversationId;


        string? conversationHeader =
            context.Request.Headers[
                "X-Kernix-Conversation-Id"
            ].FirstOrDefault();


        if (
            !Guid.TryParse(
                conversationHeader,
                out conversationId
            ))
        {
            KernixConversation newConversation =
                conversationService.CreateConversation();


            conversationId =
                newConversation.Id;
        }
        else
        {
            KernixConversation? existingConversation =
                conversationService.GetConversation(
                    conversationId
                );


            if (existingConversation == null)
            {
                KernixConversation newConversation =
                    conversationService.CreateConversation();


                conversationId =
                    newConversation.Id;
            }
        }


        // ============================================================
        // DEVOLVER CONVERSATION ID
        // ============================================================

        context.Response.Headers[
            "X-Kernix-Conversation-Id"
        ] = conversationId.ToString();


        // ============================================================
        // LOG
        // ============================================================

        app.Logger.LogInformation(
            "KerniX Chat | ConversationId: {ConversationId}",
            conversationId
        );


        // ============================================================
        // GUARDAR MENSAJE DEL USUARIO
        // ============================================================

        conversationService.AddMessage(
            conversationId,
            "user",
            request.Message
        );


        // ============================================================
        // REGISTRO COMERCIAL AUTOMÁTICO
        // ============================================================

        KernixCotizacionTemporal? cotizacionTemporal =
            commercialRegistrationService.ProcessMessage(
                conversationId,
                request.Message
            );


        if (cotizacionTemporal != null)
        {
            app.Logger.LogInformation(
                "Cotización temporal actualizada | Id: {CotizacionId} | Cliente: {Cliente} | Teléfono: {Telefono} | Presupuesto: {Presupuesto} | Productos: {Productos} | Estado: {Estado}",
                cotizacionTemporal.Id,
                cotizacionTemporal.NombreCliente,
                cotizacionTemporal.Telefono,
                cotizacionTemporal.Presupuesto,
                cotizacionTemporal.Productos.Count,
                cotizacionTemporal.Estado
            );
        }
        else
        {
            app.Logger.LogInformation(
                "El mensaje no produjo cambios comerciales. ConversationId: {ConversationId}",
                conversationId
            );
        }


        // ============================================================
        // HISTORIAL
        // ============================================================

        IReadOnlyList<KernixConversationMessage>
            storedMessages =
                conversationService.GetMessages(
                    conversationId
                );


        List<KernixHistoryMessage> serverHistory =
            storedMessages
                .Take(
                    Math.Max(
                        0,
                        storedMessages.Count - 1
                    )
                )
                .Select(
                    message =>
                        new KernixHistoryMessage
                        {
                            Role =
                                message.Role,

                            Content =
                                message.Content
                        }
                )
                .ToList();


        request.History =
            serverHistory;


        // ============================================================
        // GENERAR RESPUESTA
        // ============================================================

        try
        {
            KernixChatResponse aiResponse =
                await aiService.SendMessageAsync(
                    request,
                    cancellationToken
                );


            string finalResponse =
                aiResponse.Message?.Trim()
                ?? string.Empty;


            // ========================================================
            // VALIDACIÓN
            // ========================================================

            if (
                string.IsNullOrWhiteSpace(
                    finalResponse
                ))
            {
                throw new InvalidOperationException(
                    "KerniX IA devolvió una respuesta vacía."
                );
            }


            cancellationToken
                .ThrowIfCancellationRequested();


            // ========================================================
            // GUARDAR RESPUESTA COMPLETA
            // ========================================================

            conversationService.AddMessage(
                conversationId,
                "assistant",
                finalResponse
            );


            // ========================================================
            // HEADERS
            // ========================================================

            context.Response.StatusCode =
                StatusCodes.Status200OK;


            context.Response.ContentType =
                "text/plain; charset=utf-8";


            context.Response.Headers.CacheControl =
                "no-cache";


            context.Response.Headers[
                "X-Accel-Buffering"
            ] = "no";


            // ========================================================
            // ENVIAR POR BLOQUES
            // ========================================================

            const int chunkSize =
                48;


            for (
                int index = 0;
                index < finalResponse.Length;
                index += chunkSize)
            {
                cancellationToken
                    .ThrowIfCancellationRequested();


                int length =
                    Math.Min(
                        chunkSize,
                        finalResponse.Length - index
                    );


                string chunk =
                    finalResponse.Substring(
                        index,
                        length
                    );


                await context.Response.WriteAsync(
                    chunk,
                    cancellationToken
                );


                await context.Response.Body.FlushAsync(
                    cancellationToken
                );
            }


            app.Logger.LogInformation(
                "Respuesta KerniX IA completada. ConversationId: {ConversationId}. Caracteres: {Characters}.",
                conversationId,
                finalResponse.Length
            );
        }

        catch (OperationCanceledException)
        {
            app.Logger.LogInformation(
                "Solicitud KerniX IA cancelada. ConversationId: {ConversationId}.",
                conversationId
            );


            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode =
                    499;
            }
        }

        catch (Exception ex)
        {
            app.Logger.LogError(
                ex,
                "Error KerniX IA. ConversationId: {ConversationId}.",
                conversationId
            );


            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode =
                    StatusCodes.Status500InternalServerError;


                context.Response.ContentType =
                    "text/plain; charset=utf-8";


                await context.Response.WriteAsync(
                    app.Environment.IsDevelopment()
                        ? $"OPENAI ERROR: {ex.Message}"
                        : "KerniX IA no está disponible temporalmente."
                );
            }
        }
    }
);


// ================================================================
// ACCIONES INTELIGENTES
// ================================================================

app.MapPost(
    "/api/kernix/actions",
    (
        KernixChatRequest request,
        IKernixActionService actionService,
        IKernixCotizacionTemporalService cotizacionService,
        HttpContext context
    ) =>
    {
        // ============================================================
        // VALIDACIÓN
        // ============================================================

        if (
            request == null ||
            string.IsNullOrWhiteSpace(
                request.Message
            ))
        {
            return Results.BadRequest(
                new
                {
                    success = false,

                    message =
                        "Debes escribir un mensaje."
                }
            );
        }


        // ============================================================
        // DETECTAR ACCIONES
        // ============================================================

        KernixActionResponse response =
            actionService.DetectActions(
                request.Message
            );


        // ============================================================
        // CONVERSATION ID
        // ============================================================

        string? conversationHeader =
            context.Request.Headers[
                "X-Kernix-Conversation-Id"
            ].FirstOrDefault();


        if (
            !Guid.TryParse(
                conversationHeader,
                out Guid conversationId
            ))
        {
            app.Logger.LogWarning(
                "KerniX Actions: no llegó un ConversationId válido."
            );


            // Si no existe ID dejamos el fallback
            // de KernixActionService.
            return Results.Ok(
                response
            );
        }


        app.Logger.LogInformation(
            "KerniX Actions | ConversationId: {ConversationId} | Intent: {Intent}",
            conversationId,
            response.Intent
        );


        // ============================================================
        // BUSCAR COTIZACIÓN
        // ============================================================

        KernixCotizacionTemporal? cotizacion =
            cotizacionService.Get(
                conversationId
            );


        if (cotizacion == null)
        {
            app.Logger.LogWarning(
                "KerniX Actions: no existe CotizacionTemporal para {ConversationId}.",
                conversationId
            );


            return Results.Ok(
                response
            );
        }


        // ============================================================
        // LOG DE COTIZACIÓN
        // ============================================================

        app.Logger.LogInformation(
            "Cotización encontrada | Cliente: {Cliente} | Teléfono: {Telefono} | Presupuesto: {Presupuesto} | Productos: {Productos} | Uso: {Uso} | Estado: {Estado}",
            cotizacion.NombreCliente,
            cotizacion.Telefono,
            cotizacion.Presupuesto,
            cotizacion.Productos.Count,
            cotizacion.Uso,
            cotizacion.Estado
        );


        // ============================================================
        // CAMPOS FALTANTES
        // ============================================================

        IReadOnlyList<string> missingFields =
            cotizacionService.GetMissingFields(
                conversationId
            );


        if (missingFields.Count > 0)
        {
            app.Logger.LogWarning(
                "Cotización incompleta | ConversationId: {ConversationId} | Faltan: {Missing}",
                conversationId,
                string.Join(
                    ", ",
                    missingFields
                )
            );
        }
        else
        {
            app.Logger.LogInformation(
                "Cotización completa | ConversationId: {ConversationId}",
                conversationId
            );
        }


        // ============================================================
        // PERSONALIZAR ACCIÓN WHATSAPP
        // ============================================================

        foreach (
            KernixAiAction action
            in response.Actions)
        {
            // Solo modificamos acciones seller.
            if (
                !string.Equals(
                    action.Type,
                    "seller",
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                continue;
            }


            try
            {
                // ====================================================
                // VENDEDOR 01
                // ====================================================

                action.Url =
                    cotizacionService.BuildWhatsAppUrl(
                        conversationId,
                        "1"
                    );


                // ====================================================
                // CAMBIAR BOTÓN
                // ====================================================

                action.Label =
                    "Continuar por WhatsApp";


                action.External =
                    true;


                app.Logger.LogInformation(
                    "WhatsApp personalizado generado | ConversationId: {ConversationId}",
                    conversationId
                );
            }

            catch (Exception ex)
            {
                // Si todavía falta información,
                // dejamos el botón genérico funcionando.

                app.Logger.LogWarning(
                    ex,
                    "No se pudo generar WhatsApp personalizado. ConversationId: {ConversationId}",
                    conversationId
                );
            }
        }


        // ============================================================
        // RESPUESTA
        // ============================================================

        return Results.Ok(
            response
        );
    }
);


// ================================================================
// RAZOR PAGES
// ================================================================

app.MapRazorPages()
    .WithStaticAssets();


// ================================================================
// START
// ================================================================

app.Run();