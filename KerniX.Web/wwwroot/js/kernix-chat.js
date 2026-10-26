/* ============================================================
   KERNIX BUSINESS AI
   Cliente conversacional OpenAI
   ConversationId + Streaming + Persistencia local
============================================================ */

let kernixRequestInProgress = false;

const KERNIX_CONVERSATION_KEY =
    "kernixConversationId";


/* ============================================================
   CONVERSATION ID
============================================================ */

function getKernixConversationId() {

    return localStorage.getItem(
        KERNIX_CONVERSATION_KEY
    );
}


function saveKernixConversationId(
    conversationId
) {

    if (!conversationId)
        return;

    localStorage.setItem(
        KERNIX_CONVERSATION_KEY,
        conversationId
    );

    console.log(
        "KerniX IA: conversación activa:",
        conversationId
    );
}


function removeKernixConversationId() {

    localStorage.removeItem(
        KERNIX_CONVERSATION_KEY
    );
}


/* ============================================================
   CREAR CONVERSACIÓN
============================================================ */

async function createKernixConversation() {

    const response =
        await fetch(
            "/api/kernix/conversations",
            {
                method: "POST",
                headers: {
                    "Accept": "application/json"
                }
            }
        );

    if (!response.ok) {

        throw new Error(
            "No fue posible crear la conversación."
        );
    }

    const data =
        await response.json();

    if (!data?.conversationId) {

        throw new Error(
            "El servidor no devolvió ConversationId."
        );
    }

    saveKernixConversationId(
        data.conversationId
    );

    return data.conversationId;
}


/* ============================================================
   OBTENER / CREAR CONVERSACIÓN
============================================================ */

async function ensureKernixConversation() {

    let conversationId =
        getKernixConversationId();

    if (conversationId) {

        try {

            const response =
                await fetch(
                    `/api/kernix/conversations/${conversationId}`,
                    {
                        method: "GET",
                        headers: {
                            "Accept":
                                "application/json"
                        }
                    }
                );

            if (response.ok) {

                return conversationId;
            }

            /*
             * Si el servidor reinició,
             * la conversación en memoria ya no existe.
             */

            removeKernixConversationId();

            conversationId = null;
        }
        catch (error) {

            console.warn(
                "KerniX IA: no se pudo validar la conversación.",
                error
            );
        }
    }

    return await createKernixConversation();
}


/* ============================================================
   OBTENER VENTANA
============================================================ */

function getKernixChatWindow() {

    return document.getElementById(
        "kernix-chat-window"
    );
}


/* ============================================================
   ABRIR / CERRAR
============================================================ */

function toggleKernixChat() {

    const chat =
        getKernixChatWindow();

    if (!chat) {

        console.error(
            "KerniX IA: no se encontró #kernix-chat-window"
        );

        return;
    }

    chat.classList.toggle("open");

    if (chat.classList.contains("open")) {

        setTimeout(() => {

            document
                .getElementById(
                    "kernix-chat-message"
                )
                ?.focus();

        }, 300);
    }
}


function openKernixChat() {

    const chat =
        getKernixChatWindow();

    if (!chat)
        return;

    chat.classList.add("open");

    setTimeout(() => {

        document
            .getElementById(
                "kernix-chat-message"
            )
            ?.focus();

    }, 300);
}


function closeKernixChat() {

    const chat =
        getKernixChatWindow();

    if (!chat)
        return;

    chat.classList.remove("open");
}


/* ============================================================
   SEGURIDAD HTML
============================================================ */

function escapeKernixHtml(value) {

    const div =
        document.createElement("div");

    div.textContent =
        value ?? "";

    return div.innerHTML;
}


/* ============================================================
   FORMATO IA
============================================================ */

function formatKernixAIMessage(message) {

    const safe =
        escapeKernixHtml(message);

    return safe
        .replace(
            /\*\*(.*?)\*\*/g,
            "<strong>$1</strong>"
        )
        .replace(
            /\n/g,
            "<br>"
        );
}


/* ============================================================
   SCROLL
============================================================ */

function scrollKernixChat() {

    const body =
        document.getElementById(
            "kernix-chat-body"
        );

    if (!body)
        return;

    requestAnimationFrame(() => {

        body.scrollTo({
            top: body.scrollHeight,
            behavior: "smooth"
        });
    });
}


/* ============================================================
   MENSAJE USUARIO
============================================================ */

function addKernixUserMessage(message) {

    const body =
        document.getElementById(
            "kernix-chat-body"
        );

    if (!body)
        return;

    const wrapper =
        document.createElement("div");

    wrapper.className =
        "kxc-message kxc-message-user kxc-new-message";

    wrapper.innerHTML = `
        <div class="kxc-message-wrapper">

            <div class="kxc-message-name">
                Tú
            </div>

            <div class="kxc-bubble">
                <p>
                    ${escapeKernixHtml(message)}
                </p>
            </div>

            <small class="kxc-message-time">
                Ahora
            </small>

        </div>
    `;

    body.appendChild(wrapper);

    scrollKernixChat();
}


/* ============================================================
   MENSAJE IA NORMAL
============================================================ */

function addKernixAIMessage(message) {

    const body =
        document.getElementById(
            "kernix-chat-body"
        );

    if (!body)
        return;

    const wrapper =
        document.createElement("div");

    wrapper.className =
        "kxc-message kxc-message-ai kxc-new-message";

    wrapper.innerHTML = `
        <div class="kxc-message-avatar">

            <img
                src="/images/kernix_icon.png"
                alt="KerniX IA"
            />

        </div>

        <div class="kxc-message-wrapper">

            <div class="kxc-message-name">

                KerniX IA

                <span>
                    ASISTENTE
                </span>

            </div>

            <div class="kxc-bubble">

                <p>
                    ${formatKernixAIMessage(message)}
                </p>

            </div>

            <small class="kxc-message-time">
                Ahora
            </small>

        </div>
    `;

    body.appendChild(wrapper);

    scrollKernixChat();
}


/* ============================================================
   MENSAJE STREAMING
============================================================ */

function createKernixStreamingMessage() {

    const body =
        document.getElementById(
            "kernix-chat-body"
        );

    if (!body)
        return null;

    const wrapper =
        document.createElement("div");

    wrapper.className =
        "kxc-message kxc-message-ai kxc-new-message";

    wrapper.innerHTML = `
        <div class="kxc-message-avatar">

            <img
                src="/images/kernix_icon.png"
                alt="KerniX IA"
            />

        </div>

        <div class="kxc-message-wrapper">

            <div class="kxc-message-name">

                KerniX IA

                <span>
                    ASISTENTE
                </span>

            </div>

            <div class="kxc-bubble kxc-streaming">

                <p class="kxc-stream-content"></p>

                <span
                    class="kxc-stream-cursor">
                </span>

            </div>

            <small class="kxc-message-time">
                Ahora
            </small>

        </div>
    `;

    body.appendChild(wrapper);

    scrollKernixChat();

    return wrapper;
}


function updateKernixStreamingMessage(
    element,
    text
) {

    if (!element)
        return;

    const content =
        element.querySelector(
            ".kxc-stream-content"
        );

    if (!content)
        return;

    content.innerHTML =
        formatKernixAIMessage(text);

    scrollKernixChat();
}


function finishKernixStreamingMessage(
    element
) {

    if (!element)
        return;

    const cursor =
        element.querySelector(
            ".kxc-stream-cursor"
        );

    if (cursor)
        cursor.remove();
}


/* ============================================================
   TYPING
============================================================ */

function showKernixTyping() {

    const typing =
        document.getElementById(
            "kernix-chat-typing"
        );

    if (typing) {

        typing.classList.add(
            "active"
        );
    }

    scrollKernixChat();
}


function hideKernixTyping() {

    const typing =
        document.getElementById(
            "kernix-chat-typing"
        );

    if (typing) {

        typing.classList.remove(
            "active"
        );
    }
}


/* ============================================================
   LOADING
============================================================ */

function setKernixLoading(loading) {

    kernixRequestInProgress =
        loading;

    const send =
        document.getElementById(
            "kernix-send"
        );

    const input =
        document.getElementById(
            "kernix-chat-message"
        );

    if (send) {

        send.disabled =
            loading;

        send.classList.toggle(
            "loading",
            loading
        );
    }

    if (input) {

        input.disabled =
            loading;
    }
}


/* ============================================================
   STREAM OPENAI
============================================================ */

async function requestKernixAI(
    message,
    conversationId
) {

    const response =
        await fetch(
            "/api/kernix/chat/stream",
            {
                method: "POST",

                headers: {

                    "Content-Type":
                        "application/json",

                    "Accept":
                        "text/plain",

                    "X-Kernix-Conversation-Id":
                        conversationId
                },

                body: JSON.stringify({

                    message: message,

                    /*
                     * El historial ahora lo administra
                     * exclusivamente el backend.
                     */

                    history: []
                })
            }
        );

    /*
     * El servidor puede devolver un ID nuevo
     * si la conversación anterior dejó de existir.
     */

    const serverConversationId =
        response.headers.get(
            "X-Kernix-Conversation-Id"
        );

    if (serverConversationId) {

        saveKernixConversationId(
            serverConversationId
        );
    }

    if (!response.ok) {

        const errorText =
            await response.text();

        throw new Error(
            errorText ||
            "No fue posible comunicarse con KerniX IA."
        );
    }

    if (!response.body) {

        throw new Error(
            "El navegador no pudo iniciar el streaming."
        );
    }

    return response.body;
}


/* ============================================================
   ENVIAR MENSAJE
============================================================ */

async function sendKernixMessage() {

    if (kernixRequestInProgress)
        return;

    const input =
        document.getElementById(
            "kernix-chat-message"
        );

    if (!input)
        return;

    const message =
        input.value.trim();

    if (!message)
        return;

    input.value = "";

    addKernixUserMessage(
        message
    );

    setKernixLoading(true);

    showKernixTyping();

    let assistantElement = null;

    let assistantText = "";

    try {

        /* ========================================================
           CONVERSACIÓN
        ======================================================== */

        const conversationId =
            await ensureKernixConversation();


        /* ========================================================
           OPENAI
        ======================================================== */

        const stream =
            await requestKernixAI(
                message,
                conversationId
            );


        /* ========================================================
           COMENZAR RESPUESTA VISUAL
        ======================================================== */

        hideKernixTyping();

        assistantElement =
            createKernixStreamingMessage();


        /* ========================================================
           LEER STREAM
        ======================================================== */

        const reader =
            stream.getReader();

        const decoder =
            new TextDecoder(
                "utf-8"
            );

        while (true) {

            const {
                value,
                done
            } =
                await reader.read();

            if (done)
                break;

            const chunk =
                decoder.decode(
                    value,
                    {
                        stream: true
                    }
                );

            if (!chunk)
                continue;

            assistantText +=
                chunk;

            updateKernixStreamingMessage(
                assistantElement,
                assistantText
            );
        }


        /* ========================================================
           ÚLTIMO FRAGMENTO
        ======================================================== */

        const finalChunk =
            decoder.decode();

        if (finalChunk) {

            assistantText +=
                finalChunk;

            updateKernixStreamingMessage(
                assistantElement,
                assistantText
            );
        }


        /* ========================================================
           RESPUESTA VACÍA
        ======================================================== */

        if (!assistantText.trim()) {

            assistantText =
                "No pude generar una respuesta en este momento.";

            updateKernixStreamingMessage(
                assistantElement,
                assistantText
            );
        }


        /* ========================================================
           FINALIZAR
        ======================================================== */

        finishKernixStreamingMessage(
            assistantElement
        );
        await processKernixActions(
            message,
            assistantElement
        );

    }
    catch (error) {

        console.error(
            "KerniX IA:",
            error
        );

        hideKernixTyping();

        if (
            assistantElement &&
            assistantText
        ) {

            assistantText +=
                "\n\nLa respuesta fue interrumpida.";

            updateKernixStreamingMessage(
                assistantElement,
                assistantText
            );

            finishKernixStreamingMessage(
                assistantElement
            );
        }
        else {

            addKernixAIMessage(
                error?.message ||
                "Tuve un problema al procesar el mensaje. Inténtalo nuevamente."
            );
        }
    }
    finally {

        hideKernixTyping();

        setKernixLoading(false);

        setTimeout(() => {

            input.focus();

        }, 100);
    }
}


/* ============================================================
   ACCIONES RÁPIDAS
============================================================ */

function handleKernixAction(action) {

    const input =
        document.getElementById(
            "kernix-chat-message"
        );

    if (!input)
        return;

    const messages = {

        productos:
            "Estoy buscando productos para mi empresa. ¿Puedes ayudarme?",

        inventario:
            "Quiero consultar la disponibilidad de un producto.",

        cotizacion:
            "Quiero preparar una cotización para mi empresa.",

        promociones:
            "Quiero conocer las promociones u oportunidades disponibles."
    };

    input.value =
        messages[action] || "";

    sendKernixMessage();
}


/* ============================================================
   NUEVA CONVERSACIÓN
============================================================ */

async function resetKernixConversation() {

    if (kernixRequestInProgress)
        return;

    /*
     * No borramos la conversación anterior.
     *
     * Simplemente dejamos de utilizar su ID.
     * Después podremos mostrarla en Historial.
     */

    removeKernixConversationId();

    const body =
        document.getElementById(
            "kernix-chat-body"
        );

    if (body) {

        body
            .querySelectorAll(
                ".kxc-new-message"
            )
            .forEach(
                message =>
                    message.remove()
            );
    }

    try {

        await createKernixConversation();

        addKernixAIMessage(
            "Nueva conversación iniciada. ¿En qué puedo ayudarte?"
        );

    }
    catch (error) {

        console.error(
            "KerniX IA:",
            error
        );

        addKernixAIMessage(
            "No pude iniciar una nueva conversación."
        );
    }
}


/* ============================================================
   CARGAR HISTORIAL VISUAL
============================================================ */

async function loadKernixConversation() {

    const conversationId =
        getKernixConversationId();

    if (!conversationId)
        return;

    try {

        const response =
            await fetch(
                `/api/kernix/conversations/${conversationId}`,
                {
                    method: "GET",

                    headers: {
                        "Accept":
                            "application/json"
                    }
                }
            );

        if (!response.ok) {

            /*
             * El servidor probablemente fue reiniciado.
             */

            removeKernixConversationId();

            return;
        }

        const data =
            await response.json();

        const messages =
            data?.conversation?.messages;

        if (!Array.isArray(messages))
            return;

        const body =
            document.getElementById(
                "kernix-chat-body"
            );

        if (!body)
            return;

        /*
         * Limpiamos mensajes dinámicos anteriores
         * antes de reconstruir el historial.
         */

        body
            .querySelectorAll(
                ".kxc-new-message"
            )
            .forEach(
                message =>
                    message.remove()
            );

        for (
            const message
            of messages
        ) {

            if (
                message.role ===
                "user"
            ) {

                addKernixUserMessage(
                    message.content
                );

            }
            else if (
                message.role ===
                "assistant"
            ) {

                addKernixAIMessage(
                    message.content
                );
            }
        }

        console.log(
            "KerniX IA: historial recuperado:",
            messages.length,
            "mensajes"
        );

    }
    catch (error) {

        console.warn(
            "KerniX IA: no se pudo recuperar el historial.",
            error
        );
    }
}


/* ============================================================
   EVENTOS
============================================================ */

document.addEventListener(
    "DOMContentLoaded",
    async function () {

        const input =
            document.getElementById(
                "kernix-chat-message"
            );

        const send =
            document.getElementById(
                "kernix-send"
            );


        /* ========================================================
           BOTÓN ENVIAR
        ======================================================== */

        if (send) {

            send.addEventListener(
                "click",
                function () {

                    sendKernixMessage();
                }
            );
        }


        /* ========================================================
           ENTER
        ======================================================== */

        if (input) {

            input.addEventListener(
                "keydown",
                function (event) {

                    if (
                        event.key === "Enter" &&
                        !event.shiftKey
                    ) {

                        event.preventDefault();

                        sendKernixMessage();
                    }
                }
            );
        }


        /* ========================================================
           ACCIONES RÁPIDAS
        ======================================================== */

        document
            .querySelectorAll(
                "[data-kernix-action]"
            )
            .forEach(
                button => {

                    button.addEventListener(
                        "click",
                        function () {

                            handleKernixAction(
                                this.dataset.kernixAction
                            );
                        }
                    );
                }
            );


        /* ========================================================
           RECUPERAR CONVERSACIÓN
        ======================================================== */

        await loadKernixConversation();


        console.log(
            "KerniX IA: interfaz + conversaciones cargadas correctamente."
        );
    }
);/* ============================================================
   ACCIONES INTELIGENTES DE KERNIX IA
============================================================ */
async function requestKernixActions(
    message
) {

    try {

        // ============================================================
        // CONVERSATION ID
        // ============================================================

        const conversationId =
            getKernixConversationId();


        // ============================================================
        // HEADERS
        // ============================================================

        const headers = {

            "Content-Type":
                "application/json",

            "Accept":
                "application/json"
        };


        // ============================================================
        // AGREGAR CONVERSATION ID
        // ============================================================

        if (conversationId) {

            headers[
                "X-Kernix-Conversation-Id"
            ] = conversationId;
        }


        // ============================================================
        // REQUEST
        // ============================================================

        const response =
            await fetch(
                "/api/kernix/actions",
                {
                    method:
                        "POST",

                    headers:
                        headers,

                    body:
                        JSON.stringify({
                            message:
                                message,

                            history:
                                []
                        })
                }
            );


        // ============================================================
        // ERROR
        // ============================================================

        if (!response.ok) {

            console.warn(
                "KerniX IA: no se pudieron obtener acciones."
            );

            return null;
        }


        // ============================================================
        // RESULTADO
        // ============================================================

        return await response.json();

    }
    catch (error) {

        console.warn(
            "KerniX IA: error detectando acciones.",
            error
        );

        return null;
    }
}
/* ============================================================
   MOSTRAR ACCIONES
============================================================ */

function addKernixActions(
    actions,
    assistantElement
) {

    if (!Array.isArray(actions) ||
        actions.length === 0)
        return;

    if (!assistantElement)
        return;

    const wrapper =
        assistantElement.querySelector(
            ".kxc-message-wrapper"
        );

    if (!wrapper)
        return;


    /*
     * Evitamos botones duplicados.
     */

    const previous =
        wrapper.querySelector(
            ".kxc-ai-actions"
        );

    if (previous)
        previous.remove();


    const actionsContainer =
        document.createElement("div");

    actionsContainer.className =
        "kxc-ai-actions";


    for (const action of actions) {

        if (!action?.label ||
            !action?.url)
            continue;


        const link =
            document.createElement("a");

        link.className =
            `kxc-ai-action kxc-ai-action-${action.type || "default"}`;

        link.href =
            action.url;

        link.textContent =
            action.label;


        /*
         * Enlaces externos:
         * WhatsApp, etc.
         */

        if (action.external) {

            link.target =
                "_blank";

            link.rel =
                "noopener noreferrer";
        }


        /*
         * Iconos según acción.
         */

        const icon =
            document.createElement("span");

        icon.className =
            "kxc-ai-action-icon";


        switch (action.type) {

            case "products":

                icon.textContent = "▦";

                break;


            case "quote":

                icon.textContent = "▤";

                break;


            case "seller":

                icon.textContent = "◉";

                break;


            default:

                icon.textContent = "→";

                break;
        }


        link.prepend(icon);

        actionsContainer.appendChild(
            link
        );
    }


    if (!actionsContainer.children.length)
        return;


    wrapper.appendChild(
        actionsContainer
    );

    scrollKernixChat();
}


/* ============================================================
   DETECTAR Y MOSTRAR ACCIONES
============================================================ */

async function processKernixActions(
    userMessage,
    assistantElement
) {

    const result =
        await requestKernixActions(
            userMessage
        );

    if (!result?.success)
        return;

    if (result.intent === "none")
        return;

    addKernixActions(
        result.actions,
        assistantElement
    );

    console.log(
        "KerniX IA: intención detectada:",
        result.intent
    );
}