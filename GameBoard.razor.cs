private IDispatcherTimer _gameTimer;

private void StartGame()
{
    // ... other initialization code ...

    _gameTimer = Application.Current.Dispatcher.CreateTimer();
    _gameTimer.Interval = TimeSpan.FromMilliseconds(1000);
    _gameTimer.Tick += async (s, e) =>
    {
        // Ensure we're not processing multiple moves simultaneously
        if (!IsProcessingMove)
        {
            IsProcessingMove = true;
            await MovePieceDown();
            IsProcessingMove = false;
        }
    };
    _gameTimer.Start();
}

// Add this field at the class level
private bool IsProcessingMove { get; set; }

public async Task MovePieceDown()
{
    if (CurrentPiece == null || IsGameOver) return;

    // Store the current position
    var originalY = CurrentPiece.Position.Y;
    
    // Check if we can move down
    if (CanMovePieceDown())
    {
        // Remove any existing animation completion handlers
        CurrentPiece.TranslateTo(CurrentPiece.Position.X, CurrentPiece.Position.Y + BlockSize, 100);
        CurrentPiece.Position = new Point(CurrentPiece.Position.X, CurrentPiece.Position.Y + 1);
        await Task.Delay(100); // Wait for animation to complete
    }
    else
    {
        PlacePiece();
        CheckForCompletedRows();
        SpawnNewPiece();
    }
} 

protected override bool OnKeyDown(KeyEventArgs args)
{
    if (IsProcessingMove) return base.OnKeyDown(args);

    IsProcessingMove = true;
    
    switch (args.Key)
    {
        case Keys.Down:
            await MovePieceDown();
            break;
        // ... other key handlers ...
    }
    
    IsProcessingMove = false;
    return base.OnKeyDown(args);
}

private bool CheckForWin()
{
    // Check diagonals (both directions)
    for (int row = 0; row < BoardHeight - 4; row++)
    {
        for (int col = 0; col < BoardWidth - 4; col++)
        {
            // Check diagonal from bottom-left to top-right
            if (CheckDiagonalWin(row, col, 1, 1)) return true;
            
            // Check diagonal from bottom-right to top-left
            if (col >= 4 && CheckDiagonalWin(row, col, 1, -1)) return true;
        }
    }

    // ... existing horizontal and vertical checks ...
    return false;
}

private bool CheckDiagonalWin(int startRow, int startCol, int rowDelta, int colDelta)
{
    var firstPiece = Board[startRow, startCol];
    if (firstPiece == null) return false;

    for (int i = 1; i < 5; i++)
    {
        var currentRow = startRow + (i * rowDelta);
        var currentCol = startCol + (i * colDelta);
        
        // Bounds check
        if (currentRow < 0 || currentRow >= BoardHeight || 
            currentCol < 0 || currentCol >= BoardWidth)
            return false;

        if (Board[currentRow, currentCol]?.Color != firstPiece.Color)
            return false;
    }

    // If we get here, we found 5 matching pieces
    IsGameOver = true;
    Winner = firstPiece.Color;
    return true;
}