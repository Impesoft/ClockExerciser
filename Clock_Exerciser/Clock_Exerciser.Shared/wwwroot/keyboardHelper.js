// Keyboard utility functions

/**
 * Dismisses the mobile keyboard by blurring the currently focused input
 * Uses multiple strategies for better Android WebView compatibility
 */
export function dismissKeyboard() {
    const activeElement = document.activeElement;

    if (!activeElement || !(activeElement instanceof HTMLInputElement || activeElement instanceof HTMLTextAreaElement)) {
        return;
    }

    // Strategy 1: Standard blur
    activeElement.blur();

    // Strategy 2: Set readonly temporarily (forces Android to hide keyboard)
    activeElement.setAttribute('readonly', 'readonly');
    activeElement.setAttribute('disabled', 'true');

    // Strategy 3: Remove focus via timeout (gives Android WebView time to process)
    setTimeout(() => {
        if (activeElement) {
            activeElement.removeAttribute('readonly');
            activeElement.removeAttribute('disabled');
        }
    }, 100);

    // Strategy 4: Focus a dummy element (forces focus away)
    // This is a common workaround for Android WebView
    const dummyInput = document.createElement('input');
    dummyInput.setAttribute('type', 'text');
    dummyInput.style.position = 'absolute';
    dummyInput.style.opacity = '0';
    dummyInput.style.height = '0';
    dummyInput.style.fontSize = '16px'; // Prevent zoom on iOS
    document.body.prepend(dummyInput);
    dummyInput.focus();

    setTimeout(() => {
        dummyInput.blur();
        dummyInput.remove();
    }, 100);
}

/**
 * Focuses an input element by ID or selector
 */
export function focusInput(selector) {
    const element = document.querySelector(selector);
    if (element && element instanceof HTMLInputElement) {
        element.focus();
    }
}
