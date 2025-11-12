// Accessibility functions for screen reader support

// Create ARIA live region for announcements if it doesn't exist
(function initializeAccessibility() {
    if (!document.getElementById('aria-live-region')) {
        const liveRegion = document.createElement('div');
        liveRegion.id = 'aria-live-region';
        liveRegion.setAttribute('aria-live', 'polite');
        liveRegion.setAttribute('aria-atomic', 'true');
        liveRegion.className = 'sr-only';
        document.body.appendChild(liveRegion);
    }

    if (!document.getElementById('aria-live-region-assertive')) {
        const assertiveRegion = document.createElement('div');
        assertiveRegion.id = 'aria-live-region-assertive';
        assertiveRegion.setAttribute('aria-live', 'assertive');
        assertiveRegion.setAttribute('aria-atomic', 'true');
        assertiveRegion.className = 'sr-only';
        document.body.appendChild(assertiveRegion);
    }
})();

// Announce message to screen reader
window.announceToScreenReader = function (message, assertive = false) {
    const regionId = assertive ? 'aria-live-region-assertive' : 'aria-live-region';
    const region = document.getElementById(regionId);

    if (region) {
        // Clear and set message to ensure it's announced
        region.textContent = '';
        setTimeout(() => {
            region.textContent = message;
        }, 100);
    }
};

// Focus management
window.setFocusToElement = function (selector) {
    const element = document.querySelector(selector);
    if (element) {
        element.focus();
    }
};

// Trap focus within a container (for modals)
window.trapFocus = function (containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const focusableElements = container.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );

    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];

    container.addEventListener('keydown', function (e) {
        if (e.key === 'Tab') {
            if (e.shiftKey && document.activeElement === firstElement) {
                e.preventDefault();
                lastElement.focus();
            } else if (!e.shiftKey && document.activeElement === lastElement) {
                e.preventDefault();
                firstElement.focus();
            }
        }
    });

    // Focus first element
    if (firstElement) {
        firstElement.focus();
    }
};
