// Presentation helpers only. No business logic lives in JavaScript.
window.officeUi = {
    scrollToBottom: function (element) {
        if (!element) return;
        element.scrollTo({ top: element.scrollHeight, behavior: 'smooth' });
    },
    autoGrow: function (element) {
        if (!element) return;
        element.style.height = 'auto';
        element.style.height = Math.min(element.scrollHeight, 160) + 'px';
    },
    resetHeight: function (element) {
        if (element) element.style.height = 'auto';
    },
    // Enter sends, Shift+Enter inserts a newline; the textarea grows with its content.
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
    focus: function (element) {
        if (element) element.focus();
    }
};
