// Animation utilities for PoConnectFive using Web Animations API

/**
 * Piece drop animation with physics
 * @param {HTMLElement} element - The piece element to animate
 * @param {number} targetRow - The row index where piece will land
 * @param {number} duration - Animation duration in ms (default 600)
 */
export function animatePieceDrop(element, targetRow, duration = 600) {
    if (!element) return Promise.resolve();

    const startY = -500;
    const bounceHeight = 10;

    return element.animate([
        { transform: `translateY(${startY}px)`, offset: 0 },
        { transform: 'translateY(0px)', offset: 0.6 },
        { transform: `translateY(-${bounceHeight}px)`, offset: 0.8 },
        { transform: 'translateY(0px)', offset: 1 }
    ], {
        duration: duration,
        easing: 'cubic-bezier(0.34, 1.56, 0.64, 1)',
        fill: 'forwards'
    }).finished;
}

/**
 * Win sequence animation - highlights winning pieces sequentially
 * @param {HTMLElement[]} pieces - Array of winning piece elements
 * @param {number} delayBetween - Delay between each piece animation in ms
 */
export function animateWinSequence(pieces, delayBetween = 100) {
    if (!pieces || pieces.length === 0) return Promise.resolve();

    const animations = pieces.map((piece, index) => {
        return new Promise(resolve => {
            setTimeout(() => {
                if (piece) {
                    piece.animate([
                        {
                            boxShadow: '0 0 10px gold',
                            transform: 'scale(1)',
                            offset: 0
                        },
                        {
                            boxShadow: '0 0 30px gold, 0 0 60px gold',
                            transform: 'scale(1.15)',
                            offset: 0.5
                        },
                        {
                            boxShadow: '0 0 10px gold',
                            transform: 'scale(1)',
                            offset: 1
                        }
                    ], {
                        duration: 1000,
                        easing: 'ease-in-out',
                        iterations: Infinity
                    });
                }
                resolve();
            }, index * delayBetween);
        });
    });

    return Promise.all(animations);
}

/**
 * Confetti explosion effect
 * @param {HTMLElement} container - Container element for confetti
 * @param {number} count - Number of confetti pieces
 */
export function createConfetti(container, count = 50) {
    if (!container) return;

    const colors = ['#ff0000', '#ffc107', '#4caf50', '#2196f3', '#9c27b0'];
    const confettiElements = [];

    for (let i = 0; i < count; i++) {
        const confetti = document.createElement('div');
        confetti.className = 'confetti-piece';
        confetti.style.cssText = `
            position: absolute;
            width: 10px;
            height: 10px;
            background: ${colors[Math.floor(Math.random() * colors.length)]};
            left: 50%;
            top: 50%;
            pointer-events: none;
            border-radius: ${Math.random() > 0.5 ? '50%' : '0'};
        `;

        container.appendChild(confetti);
        confettiElements.push(confetti);

        const angle = (Math.random() * 360) * (Math.PI / 180);
        const velocity = 200 + Math.random() * 200;
        const x = Math.cos(angle) * velocity;
        const y = Math.sin(angle) * velocity;
        const rotation = Math.random() * 720 - 360;

        confetti.animate([
            {
                transform: 'translate(0, 0) rotate(0deg)',
                opacity: 1,
                offset: 0
            },
            {
                transform: `translate(${x}px, ${y}px) rotate(${rotation}deg)`,
                opacity: 0,
                offset: 1
            }
        ], {
            duration: 2000 + Math.random() * 1000,
            easing: 'cubic-bezier(0, .9, .57, 1)',
            fill: 'forwards'
        }).finished.then(() => {
            confetti.remove();
        });
    }

    // Cleanup after animation
    setTimeout(() => {
        confettiElements.forEach(el => {
            if (el.parentNode) el.remove();
        });
    }, 4000);
}

/**
 * Ghost piece preview (hover effect)
 * @param {HTMLElement} column - The column element
 * @param {string} playerColor - Color of the ghost piece
 */
export function showGhostPiece(column, playerColor = '#rgba(255,0,0,0.3)') {
    if (!column) return null;

    const ghost = document.createElement('div');
    ghost.className = 'ghost-piece';
    ghost.style.cssText = `
        position: absolute;
        width: 40px;
        height: 40px;
        border-radius: 50%;
        background: ${playerColor};
        opacity: 0.3;
        pointer-events: none;
        top: 0;
        left: 50%;
        transform: translateX(-50%);
    `;

    column.appendChild(ghost);

    ghost.animate([
        { opacity: 0, transform: 'translateX(-50%) translateY(-10px)' },
        { opacity: 0.3, transform: 'translateX(-50%) translateY(0)' }
    ], {
        duration: 200,
        easing: 'ease-out',
        fill: 'forwards'
    });

    return ghost;
}

/**
 * Remove ghost piece with fade animation
 * @param {HTMLElement} ghost - The ghost piece element
 */
export function hideGhostPiece(ghost) {
    if (!ghost) return Promise.resolve();

    return ghost.animate([
        { opacity: 0.3 },
        { opacity: 0 }
    ], {
        duration: 200,
        easing: 'ease-out',
        fill: 'forwards'
    }).finished.then(() => {
        ghost.remove();
    });
}

/**
 * Column highlight pulse effect
 * @param {HTMLElement} column - The column element to highlight
 */
export function pulseColumn(column) {
    if (!column) return Promise.resolve();

    return column.animate([
        { backgroundColor: 'transparent', offset: 0 },
        { backgroundColor: 'rgba(0, 123, 255, 0.1)', offset: 0.5 },
        { backgroundColor: 'transparent', offset: 1 }
    ], {
        duration: 500,
        easing: 'ease-in-out'
    }).finished;
}

/**
 * Page transition fade
 * @param {HTMLElement} element - Element to fade
 * @param {string} direction - 'in' or 'out'
 */
export function fadeTransition(element, direction = 'in') {
    if (!element) return Promise.resolve();

    const keyframes = direction === 'in'
        ? [{ opacity: 0 }, { opacity: 1 }]
        : [{ opacity: 1 }, { opacity: 0 }];

    return element.animate(keyframes, {
        duration: 300,
        easing: 'ease-in-out',
        fill: 'forwards'
    }).finished;
}

/**
 * Slide transition for toasts/notifications
 * @param {HTMLElement} element - Element to slide
 * @param {string} direction - 'in' or 'out'
 */
export function slideTransition(element, direction = 'in') {
    if (!element) return Promise.resolve();

    const distance = 100;
    const keyframes = direction === 'in'
        ? [
            { transform: `translateX(${distance}px)`, opacity: 0 },
            { transform: 'translateX(0)', opacity: 1 }
        ]
        : [
            { transform: 'translateX(0)', opacity: 1 },
            { transform: `translateX(${distance}px)`, opacity: 0 }
        ];

    return element.animate(keyframes, {
        duration: 300,
        easing: 'cubic-bezier(0.4, 0, 0.2, 1)',
        fill: 'forwards'
    }).finished;
}

/**
 * Counter increment animation
 * @param {HTMLElement} element - Element containing the number
 * @param {number} start - Starting number
 * @param {number} end - Ending number
 * @param {number} duration - Animation duration in ms
 */
export function animateCounter(element, start, end, duration = 1000) {
    if (!element) return Promise.resolve();

    const range = end - start;
    const startTime = performance.now();

    return new Promise(resolve => {
        function update(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function (ease-out)
            const easeOut = 1 - Math.pow(1 - progress, 3);
            const current = Math.floor(start + (range * easeOut));

            element.textContent = current;

            if (progress < 1) {
                requestAnimationFrame(update);
            } else {
                element.textContent = end;
                resolve();
            }
        }

        requestAnimationFrame(update);
    });
}

/**
 * Ripple effect on button click
 * @param {HTMLElement} button - Button element
 * @param {MouseEvent} event - Click event
 */
export function createRipple(button, event) {
    if (!button) return;

    const ripple = document.createElement('span');
    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;

    ripple.style.cssText = `
        position: absolute;
        width: ${size}px;
        height: ${size}px;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.5);
        left: ${x}px;
        top: ${y}px;
        pointer-events: none;
        transform: scale(0);
    `;

    ripple.className = 'ripple-effect';
    button.style.position = 'relative';
    button.style.overflow = 'hidden';
    button.appendChild(ripple);

    ripple.animate([
        { transform: 'scale(0)', opacity: 1 },
        { transform: 'scale(2)', opacity: 0 }
    ], {
        duration: 600,
        easing: 'ease-out',
        fill: 'forwards'
    }).finished.then(() => {
        ripple.remove();
    });
}

/**
 * Shake animation for errors
 * @param {HTMLElement} element - Element to shake
 */
export function shakeElement(element) {
    if (!element) return Promise.resolve();

    return element.animate([
        { transform: 'translateX(0)' },
        { transform: 'translateX(-10px)' },
        { transform: 'translateX(10px)' },
        { transform: 'translateX(-10px)' },
        { transform: 'translateX(10px)' },
        { transform: 'translateX(0)' }
    ], {
        duration: 400,
        easing: 'ease-in-out'
    }).finished;
}

/**
 * Screen shake effect for major events
 * @param {number} intensity - Shake intensity (1-10)
 */
export function shakeScreen(intensity = 5) {
    const body = document.body;
    if (!body) return Promise.resolve();

    return body.animate([
        { transform: 'translate(0, 0)' },
        { transform: `translate(-${intensity}px, ${intensity}px)` },
        { transform: `translate(${intensity}px, -${intensity}px)` },
        { transform: `translate(-${intensity}px, -${intensity}px)` },
        { transform: `translate(${intensity}px, ${intensity}px)` },
        { transform: 'translate(0, 0)' }
    ], {
        duration: 300,
        easing: 'ease-in-out'
    }).finished;
}

// Export all functions as a module
export default {
    animatePieceDrop,
    animateWinSequence,
    createConfetti,
    showGhostPiece,
    hideGhostPiece,
    pulseColumn,
    fadeTransition,
    slideTransition,
    animateCounter,
    createRipple,
    shakeElement,
    shakeScreen
};
