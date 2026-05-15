const listeners = new Map();

export function getDeviceMetrics() {
    const viewport = window.visualViewport;
    const dpr = window.devicePixelRatio || 1;
    const screenWidth = window.screen?.width ?? 0;
    const screenHeight = window.screen?.height ?? 0;

    return {
        userAgent: navigator.userAgent,
        platform: navigator.platform,
        screenWidth,
        screenHeight,
        availableWidth: window.screen?.availWidth ?? 0,
        availableHeight: window.screen?.availHeight ?? 0,
        viewportWidth: viewport?.width ?? window.innerWidth,
        viewportHeight: viewport?.height ?? window.innerHeight,
        innerWidth: window.innerWidth,
        innerHeight: window.innerHeight,
        outerWidth: window.outerWidth,
        outerHeight: window.outerHeight,
        devicePixelRatio: dpr,
        physicalWidth: Math.round(screenWidth * dpr),
        physicalHeight: Math.round(screenHeight * dpr),
        orientation: window.screen?.orientation?.type ?? "unknown",
        colorDepth: window.screen?.colorDepth ?? 0
    };
}

export function addDeviceMetricsListener(dotNetReference) {
    const listenerId = globalThis.crypto?.randomUUID?.() ?? Math.random().toString(36).slice(2);
    const notify = () => dotNetReference.invokeMethodAsync("OnDeviceMetricsChanged", getDeviceMetrics());

    window.addEventListener("resize", notify);
    window.screen?.orientation?.addEventListener?.("change", notify);
    window.visualViewport?.addEventListener("resize", notify);
    listeners.set(listenerId, notify);

    return listenerId;
}

export function removeDeviceMetricsListener(listenerId) {
    const notify = listeners.get(listenerId);
    if (!notify) {
        return;
    }

    window.removeEventListener("resize", notify);
    window.screen?.orientation?.removeEventListener?.("change", notify);
    window.visualViewport?.removeEventListener("resize", notify);
    listeners.delete(listenerId);
}
