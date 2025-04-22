// Constants
const BOARD = {
    ROWS: 6,
    COLS: 7
};

// Drawing functions
window.drawBoard = function (canvas, board, cellSize, pieceRadius, previewColumn) {
    const context = canvas.getContext('2d');
    context.clearRect(0, 0, canvas.width, canvas.height);

    // Fill background with gradient
    const gradient = context.createLinearGradient(0, 0, 0, canvas.height);
    gradient.addColorStop(0, '#2196F3');
    gradient.addColorStop(1, '#1976D2');
    context.fillStyle = gradient;
    context.fillRect(0, 0, canvas.width, canvas.height);

    // Draw grid with improved styling
    drawGrid(context, canvas.width, canvas.height, cellSize);

    // Draw pieces with enhanced effects
    drawPieces(context, board, cellSize, pieceRadius);

    // Draw preview piece if hovering
    if (previewColumn !== null && previewColumn !== undefined) {
        drawPreviewPiece(context, previewColumn, cellSize, pieceRadius);
    }
}

function drawGrid(context, width, height, cellSize) {
    // Draw vertical lines with gradient
    for (let x = 0; x <= width; x += cellSize) {
        const gradient = context.createLinearGradient(0, 0, 0, height);
        gradient.addColorStop(0, 'rgba(255, 255, 255, 0.1)');
        gradient.addColorStop(1, 'rgba(255, 255, 255, 0.3)');
        context.strokeStyle = gradient;
        context.lineWidth = 1;
        context.beginPath();
        context.moveTo(x, 0);
        context.lineTo(x, height);
        context.stroke();
    }

    // Draw horizontal lines with gradient
    for (let y = 0; y <= height; y += cellSize) {
        const gradient = context.createLinearGradient(0, 0, width, 0);
        gradient.addColorStop(0, 'rgba(255, 255, 255, 0.1)');
        gradient.addColorStop(1, 'rgba(255, 255, 255, 0.3)');
        context.strokeStyle = gradient;
        context.lineWidth = 1;
        context.beginPath();
        context.moveTo(0, y);
        context.lineTo(width, y);
        context.stroke();
    }

    // Draw border with gradient
    const borderGradient = context.createLinearGradient(0, 0, width, height);
    borderGradient.addColorStop(0, '#1976D2');
    borderGradient.addColorStop(1, '#0D47A1');
    context.strokeStyle = borderGradient;
    context.lineWidth = 2;
    context.strokeRect(0, 0, width, height);
}

function drawPieces(context, board, cellSize, pieceRadius) {
    if (!board || !board.cells || !Array.isArray(board.cells)) {
        console.error('Invalid board data');
        return;
    }

    const cells = board.cells;
    for (let row = 0; row < cells.length; row++) {
        if (!Array.isArray(cells[row])) continue;
        for (let col = 0; col < cells[row].length; col++) {
            const cell = cells[row][col];
            if (cell !== 0) {
                drawPiece(context, col, row, cellSize, pieceRadius, cell === 1 ? '#FF0000' : '#FFFF00');
            }
        }
    }
}

function drawPiece(context, col, row, cellSize, pieceRadius, color) {
    const centerX = col * cellSize + cellSize / 2;
    const centerY = row * cellSize + cellSize / 2;

    // Draw shadow with blur effect
    context.shadowColor = 'rgba(0, 0, 0, 0.3)';
    context.shadowBlur = 5;
    context.shadowOffsetX = 2;
    context.shadowOffsetY = 2;

    // Draw main piece with gradient
    const gradient = context.createRadialGradient(
        centerX - pieceRadius / 3,
        centerY - pieceRadius / 3,
        pieceRadius / 10,
        centerX,
        centerY,
        pieceRadius
    );
    gradient.addColorStop(0, color);
    gradient.addColorStop(1, shadeColor(color, -20));

    context.beginPath();
    context.arc(centerX, centerY, pieceRadius, 0, Math.PI * 2);
    context.fillStyle = gradient;
    context.fill();

    // Reset shadow
    context.shadowColor = 'transparent';
    context.shadowBlur = 0;
    context.shadowOffsetX = 0;
    context.shadowOffsetY = 0;

    // Draw highlight
    const highlightGradient = context.createRadialGradient(
        centerX - pieceRadius / 3,
        centerY - pieceRadius / 3,
        pieceRadius / 10,
        centerX,
        centerY,
        pieceRadius
    );
    highlightGradient.addColorStop(0, 'rgba(255, 255, 255, 0.8)');
    highlightGradient.addColorStop(1, 'rgba(255, 255, 255, 0)');

    context.beginPath();
    context.arc(centerX, centerY, pieceRadius, 0, Math.PI * 2);
    context.fillStyle = highlightGradient;
    context.fill();

    // Draw outline
    context.beginPath();
    context.arc(centerX, centerY, pieceRadius, 0, Math.PI * 2);
    context.strokeStyle = 'rgba(0, 0, 0, 0.2)';
    context.lineWidth = 2;
    context.stroke();
}

function drawPreviewPiece(context, column, cellSize, pieceRadius) {
    const centerX = column * cellSize + cellSize / 2;
    const centerY = cellSize / 2;

    // Draw semi-transparent piece
    context.beginPath();
    context.arc(centerX, centerY, pieceRadius, 0, Math.PI * 2);
    context.fillStyle = 'rgba(255, 255, 255, 0.3)';
    context.fill();

    // Draw outline
    context.beginPath();
    context.arc(centerX, centerY, pieceRadius, 0, Math.PI * 2);
    context.strokeStyle = 'rgba(255, 255, 255, 0.5)';
    context.lineWidth = 2;
    context.stroke();
}

// Helper function to darken or lighten a color
function shadeColor(color, percent) {
    let R = parseInt(color.substring(1, 3), 16);
    let G = parseInt(color.substring(3, 5), 16);
    let B = parseInt(color.substring(5, 7), 16);

    R = parseInt(R * (100 + percent) / 100);
    G = parseInt(G * (100 + percent) / 100);
    B = parseInt(B * (100 + percent) / 100);

    R = (R < 255) ? R : 255;
    G = (G < 255) ? G : 255;
    B = (B < 255) ? B : 255;

    R = Math.max(0, R).toString(16).padStart(2, '0');
    G = Math.max(0, G).toString(16).padStart(2, '0');
    B = Math.max(0, B).toString(16).padStart(2, '0');

    return `#${R}${G}${B}`;
}

window.getBoundingClientRect = function (element) {
    const rect = element.getBoundingClientRect();
    return {
        left: rect.left,
        top: rect.top,
        width: rect.width,
        height: rect.height
    };
}

window.animatePieceDrop = function (column, cellSize, pieceRadius, color) {
    const animation = document.createElement('div');
    animation.className = 'piece-animation';
    animation.style.left = `${column * cellSize}px`;
    animation.style.top = `-${cellSize}px`;
    animation.style.width = `${cellSize}px`;
    animation.style.height = `${cellSize}px`;
    animation.style.backgroundColor = color;

    document.body.appendChild(animation);

    setTimeout(() => {
        animation.style.top = `${(BOARD.ROWS - 1) * cellSize}px`;
    }, 0);

    setTimeout(() => {
        document.body.removeChild(animation);
    }, 300);
}

function initGameBoard(canvas, dotNetHelper) {
    canvas.addEventListener('touchstart', handleTouch);
    canvas.addEventListener('touchmove', handleTouch);

    function handleTouch(event) {
        event.preventDefault();
        const touch = event.touches[0];
        const rect = canvas.getBoundingClientRect();
        const x = touch.clientX - rect.left;
        const y = touch.clientY - rect.top;
        dotNetHelper.invokeMethodAsync('HandleTouchCoordinates', x, y);
    }
}

function getCanvas(canvas) {
    const ctx = canvas.getContext('2d');
    return {
        clearRect: (x, y, width, height) => ctx.clearRect(x, y, width, height),
        drawGrid: (cellSize, columns, rows) => {
            ctx.strokeStyle = '#1976D2';
            ctx.lineWidth = 2;

            // Draw vertical lines
            for (let i = 1; i < columns; i++) {
                ctx.beginPath();
                ctx.moveTo(i * cellSize, 0);
                ctx.lineTo(i * cellSize, rows * cellSize);
                ctx.stroke();
            }

            // Draw horizontal lines
            for (let i = 1; i < rows; i++) {
                ctx.beginPath();
                ctx.moveTo(0, i * cellSize);
                ctx.lineTo(columns * cellSize, i * cellSize);
                ctx.stroke();
            }
        },
        drawPiece: (x, y, cellSize, color) => {
            const radius = cellSize * 0.4;
            const centerX = x + cellSize / 2;
            const centerY = y + cellSize / 2;

            ctx.beginPath();
            ctx.arc(centerX, centerY, radius, 0, Math.PI * 2);
            ctx.fillStyle = color;
            ctx.fill();

            // Add a subtle shadow
            ctx.shadowColor = 'rgba(0, 0, 0, 0.3)';
            ctx.shadowBlur = 4;
            ctx.shadowOffsetX = 2;
            ctx.shadowOffsetY = 2;

            // Add a subtle border
            ctx.strokeStyle = 'rgba(0, 0, 0, 0.2)';
            ctx.lineWidth = 2;
            ctx.stroke();

            // Reset shadow
            ctx.shadowColor = 'transparent';
            ctx.shadowBlur = 0;
            ctx.shadowOffsetX = 0;
            ctx.shadowOffsetY = 0;
        }
    };
}
