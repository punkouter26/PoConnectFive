// PoConnectFive Game Board Interop (updated to rely on Blazor for pointer events)
// Initialize now accepts canvas sizing and cell metrics from Blazor so both sides stay consistent.

window.gameBoardInterop = {
    canvas: null,
    context: null,
    dotNetHelper: null,
    rows: 9,
    columns: 9,
    cellSize: 0,
    pieceRadius: 0,
    inset: 8,
    lastBoardData: null, // store the last board state received from Blazor

    initialize: function (canvas, dotNetHelper, rows, columns, canvasWidth, canvasHeight, cellSize, inset) {
        console.log("Initializing game board interop (JS)...");
        this.canvas = canvas;
        this.context = canvas.getContext('2d');
        this.dotNetHelper = dotNetHelper;
        this.rows = rows;
        this.columns = columns;

        // Accept precomputed geometry from Blazor, but fallback if not provided
        this.cellSize = cellSize || Math.min(canvas.width / this.columns, canvas.height / this.rows);
        this.pieceRadius = this.cellSize * 0.4;
        this.inset = inset || Math.max(8, Math.round(this.cellSize * 0.15));

        // Note: Pointer events (click/mousemove) are handled by Blazor component.
        this.drawBoard(); // Initial draw
        console.log("Game board interop initialized (JS).");
    },

    // Expose canvas bounding rect for Blazor to compute local coordinates accurately
    getCanvasRect: function (canvas) {
        if (!canvas) return { left: 0, top: 0, right: 0, bottom: 0, width: 0, height: 0 };
        var rect = canvas.getBoundingClientRect();
        return { left: rect.left, top: rect.top, right: rect.right, bottom: rect.bottom, width: rect.width, height: rect.height };
    },

    updateBoard: function (boardData, gameStatus) {
        console.log("Updating board (JS)...");
        if (!this.context || !boardData) {
            console.error("Context or board data not available for update.");
            return;
        }
        // Store latest board data so previews can redraw correctly
        this.lastBoardData = boardData;
        // Perform a full redraw: clear, draw grid, then draw pieces.
        this.drawBoard(); // Clears canvas and draws grid
        this.drawPieces(boardData); // Draws pieces on top
        console.log("Board updated (JS).");
    },

    dispose: function () {
        console.log("Disposing game board interop (JS)...");
        this.canvas = null;
        this.context = null;
        this.dotNetHelper = null;
        this.lastBoardData = null;
        console.log("Game board interop disposed (JS).");
    },

    drawBoard: function () {
        if (!this.context || !this.canvas) return;
        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);

        // Fill background with subtle gradient
        const gradient = this.context.createLinearGradient(0, 0, 0, this.canvas.height);
        gradient.addColorStop(0, '#1e88e5');
        gradient.addColorStop(1, '#1976d2');
        this.context.fillStyle = gradient;
        this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);

        // Draw rounded border panel around board
        const inset = this.inset;
        this.context.save();
        this.context.lineWidth = Math.max(8, Math.round(this.cellSize * 0.25));
        const borderGradient = this.context.createLinearGradient(0, 0, this.canvas.width, this.canvas.height);
        borderGradient.addColorStop(0, '#0d47a1');
        borderGradient.addColorStop(1, '#002171');
        this.context.strokeStyle = borderGradient;
        // draw rounded rect as border
        const w = this.canvas.width - inset * 2;
        const h = this.canvas.height - inset * 2;
        const x = inset;
        const y = inset;
        const radius = Math.max(12, Math.round(this.cellSize * 0.2));
        this.roundRect(this.context, x, y, w, h, radius, false, true);
        this.context.restore();

        // Clip board drawing area so grid lines align inside border
        this.context.save();
        this.context.beginPath();
        this.roundRect(this.context, x, y, w, h, radius, true, false);
        this.context.clip();

        // Draw grid inside clipped area
        this.drawGrid(x, y, w, h);
        this.context.restore();
    },

    drawGrid: function (offsetX, offsetY, width, height) {
        if (!this.context) return;
        const cellSize = this.cellSize;
        const cols = this.columns;
        const rows = this.rows;

        // Draw light grid lines
        this.context.strokeStyle = 'rgba(255,255,255,0.08)';
        this.context.lineWidth = 1;
        for (let r = 0; r <= rows; r++) {
            const y = offsetY + r * cellSize;
            this.context.beginPath();
            this.context.moveTo(offsetX, y);
            this.context.lineTo(offsetX + cols * cellSize, y);
            this.context.stroke();
        }
        for (let c = 0; c <= cols; c++) {
            const x = offsetX + c * cellSize;
            this.context.beginPath();
            this.context.moveTo(x, offsetY);
            this.context.lineTo(x, offsetY + rows * cellSize);
            this.context.stroke();
        }
    },

    drawPieces: function (boardData) {
        if (!this.context || !boardData) {
            console.error("Context or board data not available for drawing pieces.");
            return;
        }
        // Draw pieces using last known boardData. Use same offset as grid (inset).
        const inset = this.inset;
        for (let row = 0; row < boardData.length; row++) {
            if (!Array.isArray(boardData[row])) continue;
            for (let col = 0; col < boardData[row].length; col++) {
                const cellValue = boardData[row][col];
                if (cellValue !== 0) {
                    const color = cellValue === 1 ? '#ff5252' : '#ffeb3b';
                    this.drawPieceWithin(col, row, color, inset);
                }
            }
        }
    },

    drawPieceWithin: function (col, row, color, inset) {
        if (!this.context) return;
        const cx = inset + col * this.cellSize + this.cellSize / 2;
        const cy = inset + row * this.cellSize + this.cellSize / 2;
        const radius = this.pieceRadius;

        // draw shadow
        this.context.save();
        this.context.shadowColor = 'rgba(0,0,0,0.35)';
        this.context.shadowBlur = Math.max(6, Math.round(radius * 0.25));
        this.context.shadowOffsetX = 2;
        this.context.shadowOffsetY = 3;

        // main radial gradient
        const grad = this.context.createRadialGradient(
            cx - radius / 3, cy - radius / 3, radius / 10,
            cx, cy, radius
        );
        grad.addColorStop(0, color);
        grad.addColorStop(1, this.shadeColor(color, -25));
        this.context.beginPath();
        this.context.arc(cx, cy, radius, 0, Math.PI * 2);
        this.context.fillStyle = grad;
        this.context.fill();

        // highlight
        this.context.beginPath();
        const highlight = this.context.createRadialGradient(
            cx - radius / 3, cy - radius / 3, radius / 10,
            cx, cy, radius
        );
        highlight.addColorStop(0, 'rgba(255,255,255,0.9)');
        highlight.addColorStop(1, 'rgba(255,255,255,0)');
        this.context.fillStyle = highlight;
        this.context.fill();

        // outline
        this.context.beginPath();
        this.context.arc(cx, cy, radius, 0, Math.PI * 2);
        this.context.strokeStyle = 'rgba(0,0,0,0.25)';
        this.context.lineWidth = Math.max(2, Math.round(radius * 0.12));
        this.context.stroke();

        this.context.restore();
    },

    // Show a preview at the top of column — redraws full board + pieces first to avoid artifacts
    showPreview: function (column) {
        if (!this.context) return;
        if (!this.lastBoardData) {
            // If no data yet, draw base board and preview on it
            this.drawBoard();
        } else {
            this.drawBoard();
            this.drawPieces(this.lastBoardData);
        }
        // draw semi-transparent preview piece at top row (row 0)
        const inset = this.inset;
        const cx = inset + column * this.cellSize + this.cellSize / 2;
        const cy = inset + this.cellSize / 2;
        const radius = this.pieceRadius;

        this.context.save();
        this.context.globalAlpha = 0.7;
        this.context.beginPath();
        this.context.arc(cx, cy, radius, 0, Math.PI * 2);
        this.context.fillStyle = 'rgba(255,255,255,0.6)'; // subtle white preview
        this.context.fill();
        this.context.restore();
    },

    clearPreview: function () {
        // redraw board & pieces from lastBoardData
        if (!this.context) return;
        this.drawBoard();
        if (this.lastBoardData) {
            this.drawPieces(this.lastBoardData);
        }
    },

    drawPiece: function (col, row, color) {
        // Backwards-compat wrapper to call drawPieceWithin using default inset
        const inset = this.inset;
        this.drawPieceWithin(col, row, color, inset);
    },

    // rounded rectangle helper
    roundRect: function (ctx, x, y, w, h, r, fill, stroke) {
        if (typeof r === 'undefined') r = 5;
        if (typeof r === 'number') {
            r = { tl: r, tr: r, br: r, bl: r };
        } else {
            var defaultRadius = { tl: 0, tr: 0, br: 0, bl: 0 };
            for (var side in defaultRadius) {
                r[side] = r[side] || defaultRadius[side];
            }
        }
        ctx.beginPath();
        ctx.moveTo(x + r.tl, y);
        ctx.lineTo(x + w - r.tr, y);
        ctx.quadraticCurveTo(x + w, y, x + w, y + r.tr);
        ctx.lineTo(x + w, y + h - r.br);
        ctx.quadraticCurveTo(x + w, y + h, x + w - r.br, y + h);
        ctx.lineTo(x + r.bl, y + h);
        ctx.quadraticCurveTo(x, y + h, x, y + h - r.bl);
        ctx.lineTo(x, y + r.tl);
        ctx.quadraticCurveTo(x, y, x + r.tl, y);
        ctx.closePath();
        if (fill) ctx.fill();
        if (stroke) ctx.stroke();
    },

    // Helper function to darken or lighten a color
    shadeColor: function (color, percent) {
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
};
