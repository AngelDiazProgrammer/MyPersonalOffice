// La escena 3D es un módulo ES que carga de forma asíncrona; Blazor espera a que esté lista antes de iniciar.
window.officeSceneReady = new Promise(function (resolve) { window.__officeSceneResolve = resolve; });

window.officeUi = {
    // Espera a que cargue la escena 3D (máximo 10 segundos).
    sceneReady: function () {
        return Promise.race([
            window.officeSceneReady,
            new Promise(function (_, reject) { setTimeout(function () { reject(new Error('3D scene failed to load')); }, 10000); })
        ]);
    },
    // Baja el chat hasta el último mensaje.
    scrollToBottom: function (element) {
        if (!element) return;
        element.scrollTo({ top: element.scrollHeight, behavior: 'smooth' });
    },
    // Ajusta la altura del cuadro de texto a su contenido (hasta un máximo).
    autoGrow: function (element) {
        if (!element) return;
        element.style.height = 'auto';
        element.style.height = Math.min(element.scrollHeight, 160) + 'px';
    },
    // Inicia la descarga de un archivo por su dirección.
    download: function (url) {
        const a = document.createElement('a');
        a.href = url; a.download = '';
        document.body.appendChild(a); a.click(); a.remove();
    },
    // Restablece la altura del cuadro de texto.
    resetHeight: function (element) {
        if (element) element.style.height = 'auto';
    },
    // Enter envía, Shift+Enter inserta un salto de línea; el cuadro de texto crece con su contenido.
    bindComposer: function (textarea, button) {
        if (!textarea || !button) return;
        textarea.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey && !e.isComposing) {
                e.preventDefault();
                if (!button.disabled) button.click();
            }
        });
        textarea.addEventListener('input', function () { window.officeUi.autoGrow(textarea); });
    },
    // Pone el foco en un elemento.
    focus: function (element) {
        if (element) element.focus();
    }
};
