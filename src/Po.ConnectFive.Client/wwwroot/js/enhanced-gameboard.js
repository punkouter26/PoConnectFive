// Enhanced Game Board Canvas Helper Functions
window.gameBoard = {
    // Canvas context management
    getCanvasContext: function (canvas) {
        return canvas.getContext('2d');
    },

    // Basic drawing operations
    clearCanvas: function (ctx, width, height) {
        ctx.clearRect(0, 0, width, height);
    },

    setFillStyle: function (ctx, color) {
        ctx.fillStyle = color;
    },

    setStrokeStyle: function (ctx, color) {
        ctx.strokeStyle = color;
    },

    setLineWidth: function (ctx, width) {
        ctx.lineWidth = width;
    },

    setGlobalAlpha: function (ctx, alpha) {
        ctx.globalAlpha = alpha;
    },

    // Shape drawing
    fillRect: function (ctx, x, y, width, height) {
        ctx.fillRect(x, y, width, height);
    },

    strokeRect: function (ctx, x, y, width, height) {
        ctx.strokeRect(x, y, width, height);
    },

    fillCircle: function (ctx, x, y, radius) {
        ctx.beginPath();
        ctx.arc(x, y, radius, 0, 2 * Math.PI);
        ctx.fill();
    },

    strokeCircle: function (ctx, x, y, radius) {
        ctx.beginPath();
        ctx.arc(x, y, radius, 0, 2 * Math.PI);
        ctx.stroke();
    },

    // Animation helpers
    animatePieceDrop: function (ctx, x, startY, endY, color, radius, duration, callback) {
        const startTime = performance.now();

        function animate(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function for bounce effect
            const easeOutBounce = function (t) {
                if (t < (1 / 2.75)) {
                    return 7.5625 * t * t;
                } else if (t < (2 / 2.75)) {
                    return 7.5625 * (t -= (1.5 / 2.75)) * t + 0.75;
                } else if (t < (2.5 / 2.75)) {
                    return 7.5625 * (t -= (2.25 / 2.75)) * t + 0.9375;
                } else {
                    return 7.5625 * (t -= (2.625 / 2.75)) * t + 0.984375;
                }
            };

            const currentY = startY + (endY - startY) * easeOutBounce(progress);

            // Clear the piece area and redraw
            ctx.clearRect(x - radius - 5, Math.min(startY, endY) - radius - 5,
                (radius + 5) * 2, Math.abs(endY - startY) + (radius + 5) * 2);

            ctx.fillStyle = color;
            ctx.beginPath();
            ctx.arc(x, currentY, radius, 0, 2 * Math.PI);
            ctx.fill();

            if (progress < 1) {
                requestAnimationFrame(animate);
            } else if (callback) {
                callback();
            }
        }

        requestAnimationFrame(animate);
    },

    // Accessibility and haptic feedback
    triggerHapticFeedback: function (duration, intensity) {
        if ('vibrate' in navigator) {
            navigator.vibrate(duration);
        }

        // For devices that support haptic feedback (iOS Safari)
        if ('haptic' in navigator) {
            navigator.haptic.impact(intensity);
        }
    },

    // Screen reader announcements
    announceToScreenReader: function (message) {
        const announcement = document.createElement('div');
        announcement.setAttribute('aria-live', 'polite');
        announcement.setAttribute('aria-atomic', 'true');
        announcement.classList.add('sr-only');
        announcement.textContent = message;

        document.body.appendChild(announcement);

        // Remove after announcement
        setTimeout(() => {
            document.body.removeChild(announcement);
        }, 1000);
    },

    // File download helper
    downloadFile: function (filename, contentType, data) {
        const blob = new Blob([data], { type: contentType });
        const url = window.URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        link.style.display = 'none';

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        window.URL.revokeObjectURL(url);
    },

    // Chart helpers for statistics dashboard
    charts: {
        // Initialize chart.js or other charting library
        initializeChart: function (canvasId, config) {
            // This would integrate with Chart.js or similar library
            console.log('Initialize chart:', canvasId, config);
        },

        updateChart: function (chartId, newData) {
            // Update existing chart with new data
            console.log('Update chart:', chartId, newData);
        }
    },

    // Theme management
    applyTheme: function (theme) {
        const root = document.documentElement;

        // Apply CSS custom properties for theming
        root.style.setProperty('--primary-color', theme.player1Color);
        root.style.setProperty('--secondary-color', theme.player2Color);
        root.style.setProperty('--board-color', theme.boardColor);
        root.style.setProperty('--background-color', theme.backgroundColor);
        root.style.setProperty('--text-color', theme.textColor);
        root.style.setProperty('--grid-color', theme.gridLineColor);

        // Update meta theme-color for mobile browsers
        const metaThemeColor = document.querySelector("meta[name=theme-color]");
        if (metaThemeColor) {
            metaThemeColor.setAttribute("content", theme.boardColor);
        }
    },

    // Performance monitoring
    performance: {
        measureFrameRate: function (callback) {
            let lastTime = performance.now();
            let frameCount = 0;

            function measure() {
                const currentTime = performance.now();
                frameCount++;

                if (currentTime - lastTime >= 1000) {
                    const fps = frameCount / ((currentTime - lastTime) / 1000);
                    callback(fps);
                    frameCount = 0;
                    lastTime = currentTime;
                }

                requestAnimationFrame(measure);
            }

            requestAnimationFrame(measure);
        }
    }
};

// Global helper functions for backward compatibility
window.getCanvasContext = window.gameBoard.getCanvasContext;
window.clearCanvas = window.gameBoard.clearCanvas;
window.setFillStyle = window.gameBoard.setFillStyle;
window.setStrokeStyle = window.gameBoard.setStrokeStyle;
window.setLineWidth = window.gameBoard.setLineWidth;
window.setGlobalAlpha = window.gameBoard.setGlobalAlpha;
window.fillRect = window.gameBoard.fillRect;
window.strokeRect = window.gameBoard.strokeRect;
window.fillCircle = window.gameBoard.fillCircle;
window.strokeCircle = window.gameBoard.strokeCircle;
window.triggerHapticFeedback = window.gameBoard.triggerHapticFeedback;
window.downloadFile = window.gameBoard.downloadFile;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('Enhanced game board helpers initialized');
});
