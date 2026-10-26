using System.Collections.Concurrent;
using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public class KernixConversationService
        : IKernixConversationService
    {
        // ============================================================
        // ALMACENAMIENTO TEMPORAL
        // ============================================================

        private readonly ConcurrentDictionary<Guid, KernixConversation>
            _conversations = new();

        private readonly ILogger<KernixConversationService>
            _logger;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public KernixConversationService(
            ILogger<KernixConversationService> logger)
        {
            _logger = logger;
        }

        // ============================================================
        // CREAR CONVERSACIÓN
        // ============================================================

        public KernixConversation CreateConversation()
        {
            KernixConversation conversation = new()
            {
                Id = Guid.NewGuid(),

                Title = "Nueva conversación",

                Summary = string.Empty,

                CreatedAt = DateTime.UtcNow,

                UpdatedAt = DateTime.UtcNow,

                Messages = new List<KernixConversationMessage>()
            };

            _conversations[conversation.Id] =
                conversation;

            _logger.LogInformation(
                "Conversación KerniX creada: {ConversationId}",
                conversation.Id
            );

            return conversation;
        }

        // ============================================================
        // OBTENER CONVERSACIÓN
        // ============================================================

        public KernixConversation? GetConversation(
            Guid conversationId)
        {
            _conversations.TryGetValue(
                conversationId,
                out KernixConversation? conversation
            );

            return conversation;
        }

        // ============================================================
        // LISTAR CONVERSACIONES
        // ============================================================

        public IReadOnlyCollection<KernixConversation>
            GetConversations()
        {
            return _conversations
                .Values
                .OrderByDescending(
                    conversation =>
                        conversation.UpdatedAt
                )
                .ToList();
        }

        // ============================================================
        // AGREGAR MENSAJE
        // ============================================================

        public KernixConversationMessage AddMessage(
            Guid conversationId,
            string role,
            string content)
        {
            // --------------------------------------------------------
            // VALIDACIONES
            // --------------------------------------------------------

            if (conversationId == Guid.Empty)
            {
                throw new ArgumentException(
                    "El identificador de conversación no es válido.",
                    nameof(conversationId)
                );
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException(
                    "El rol del mensaje es obligatorio.",
                    nameof(role)
                );
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(
                    "El contenido del mensaje es obligatorio.",
                    nameof(content)
                );
            }

            role = role
                .Trim()
                .ToLowerInvariant();

            if (role != "user" &&
                role != "assistant")
            {
                throw new ArgumentException(
                    "El rol debe ser 'user' o 'assistant'.",
                    nameof(role)
                );
            }

            // --------------------------------------------------------
            // BUSCAR CONVERSACIÓN
            // --------------------------------------------------------

            KernixConversation? conversation =
                GetConversation(conversationId);

            if (conversation == null)
            {
                throw new KeyNotFoundException(
                    $"No existe la conversación {conversationId}."
                );
            }

            // --------------------------------------------------------
            // CREAR MENSAJE
            // --------------------------------------------------------

            KernixConversationMessage message = new()
            {
                Id = Guid.NewGuid(),

                ConversationId =
                    conversationId,

                Role = role,

                Content =
                    content.Trim(),

                CreatedAt =
                    DateTime.UtcNow
            };

            // --------------------------------------------------------
            // GUARDAR
            // --------------------------------------------------------

            lock (conversation.Messages)
            {
                conversation.Messages.Add(
                    message
                );

                conversation.UpdatedAt =
                    DateTime.UtcNow;

                // El primer mensaje del usuario
                // genera un título provisional.
                if (
                    conversation.Title ==
                        "Nueva conversación"
                    &&
                    role == "user"
                )
                {
                    conversation.Title =
                        GenerateTitle(
                            message.Content
                        );
                }
            }

            _logger.LogDebug(
                "Mensaje agregado a conversación {ConversationId}. Rol: {Role}",
                conversationId,
                role
            );

            return message;
        }

        // ============================================================
        // OBTENER MENSAJES
        // ============================================================

        public IReadOnlyList<KernixConversationMessage>
            GetMessages(
                Guid conversationId)
        {
            KernixConversation? conversation =
                GetConversation(
                    conversationId
                );

            if (conversation == null)
            {
                return Array.Empty<
                    KernixConversationMessage
                >();
            }

            lock (conversation.Messages)
            {
                return conversation.Messages
                    .OrderBy(
                        message =>
                            message.CreatedAt
                    )
                    .ToList();
            }
        }

        // ============================================================
        // ACTUALIZAR RESUMEN
        // ============================================================

        public void UpdateSummary(
            Guid conversationId,
            string summary)
        {
            KernixConversation? conversation =
                GetConversation(
                    conversationId
                );

            if (conversation == null)
            {
                throw new KeyNotFoundException(
                    $"No existe la conversación {conversationId}."
                );
            }

            conversation.Summary =
                summary?.Trim()
                ?? string.Empty;

            conversation.UpdatedAt =
                DateTime.UtcNow;
        }

        // ============================================================
        // ELIMINAR CONVERSACIÓN
        // ============================================================

        public bool DeleteConversation(
            Guid conversationId)
        {
            bool removed =
                _conversations.TryRemove(
                    conversationId,
                    out _
                );

            if (removed)
            {
                _logger.LogInformation(
                    "Conversación KerniX eliminada: {ConversationId}",
                    conversationId
                );
            }

            return removed;
        }

        // ============================================================
        // LIMPIAR TODAS
        // ============================================================

        public void Clear()
        {
            _conversations.Clear();

            _logger.LogInformation(
                "Se limpiaron las conversaciones KerniX."
            );
        }

        // ============================================================
        // GENERAR TÍTULO PROVISIONAL
        // ============================================================

        private static string GenerateTitle(
            string content)
        {
            string cleanContent =
                content
                    .Replace("\r", " ")
                    .Replace("\n", " ")
                    .Trim();

            const int maxLength = 45;

            if (cleanContent.Length <= maxLength)
            {
                return cleanContent;
            }

            return cleanContent[..maxLength]
                .Trim()
                + "...";
        }
    }
}