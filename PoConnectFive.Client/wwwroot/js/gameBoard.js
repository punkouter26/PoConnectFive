export function drawBoard(canvas, board, cellSize, pieceRadius, previewColumn) {
    const context = canvas.getContext('2d');
    context.clearRect(0, 0, canvas.width, canvas.height);

    // Fill background
    context.fillStyle = '#2196F3';
    context.fillRect(0, 0, canvas.width, canvas.height);

    // Draw grid
    drawGrid(context, canvas.width, canvas.height, cellSize);

    // Draw pieces
    drawPieces(context, board, cellSize, pieceRadius);

    // Draw preview piece if hovering
    if (previewColumn !== null && previewColumn !== undefined) {
        drawPreviewPiece(context, previewColumn, cellSize, pieceRadius);
    }
}

function drawGrid(context, width, height, cellSize) {
    context.strokeStyle = 'rgba(255, 255, 255, 0.3)';
    context.lineWidth = 1;

    // Draw vertical lines
    for (let x = 0; x <= width; x += cellSize) {
        context.beginPath();
        context.moveTo(x, 0);
        context.lineTo(x, height);
        context.stroke();
    }

    // Draw horizontal lines
    for (let y = 0; y <= height; y += cellSize) {
        context.beginPath();
        context.moveTo(0, y);
        context.lineTo(width, y);
        context.stroke();
    }

    // Draw border
    context.strokeStyle = '#1976D2';
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

    // Draw shadow
    context.beginPath();
    context.arc(centerX + 2, centerY + 2, pieceRadius, 0, Math.PI * 2);
    context.fillStyle = 'rgba(0, 0, 0, 0.2)';
    context.fill();

    // Draw piece
    context.beginPath();
    context.arc(centerX, centerY, pieceRadius, 0, Math.PI * 2);
    context.fillStyle = color;
    context.fill();

    // Draw highlight
    const gradient = context.createRadialGradient(
        centerX - pieceRadius/3,
        centerY - pieceRadius/3,
        pieceRadius/10,
        centerX,
        centerY,
        pieceRadius
    );
    gradient.addColorStop(0, 'rgba(255, 255, 255, 0.8)');
    gradient.addColorStop(1, 'rgba(255, 255, 255, 0)');
    
    context.beginPath();
    context.arc(centerX, centerY, pieceRadius, 0, Math.PI * 2);
    context.fillStyle = gradient;
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

    context.beginPath();
    context.arc(centerX, centerY, pieceRadius, 0, Math.PI * 2);
    context.fillStyle = 'rgba(255, 255, 255, 0.3)';
    context.fill();
    context.strokeStyle = 'rgba(255, 255, 255, 0.5)';
    context.lineWidth = 2;
    context.stroke();
}

export function getBoundingClientRect(element) {
    return element.getBoundingClientRect();
}
