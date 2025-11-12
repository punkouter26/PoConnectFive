namespace PoConnectFive.Shared.Models;

/// <summary>
/// View model for the game board that handles UI-specific calculations
/// like cell sizing and coordinate mapping.
/// </summary>
public class GameBoardViewModel
{
    public int Rows { get; }
    public int Columns { get; }
    public int BoardWidth { get; }
    public int BoardHeight { get; }
    public double CellSize { get; }
    public double Inset { get; }

    public GameBoardViewModel(int rows, int columns, int boardWidth, int boardHeight)
    {
        Rows = rows;
        Columns = columns;
        BoardWidth = boardWidth;
        BoardHeight = boardHeight;

        // Calculate cell size based on board dimensions
        // Add some padding/inset around the board
        Inset = 20.0;
        var usableWidth = boardWidth - (2 * Inset);
        var usableHeight = boardHeight - (2 * Inset);

        // Use the smaller dimension to maintain square cells
        CellSize = Math.Min(usableWidth / columns, usableHeight / rows);
    }

    /// <summary>
    /// Maps a client X coordinate to a board column.
    /// </summary>
    /// <param name="clientX">The X coordinate relative to the canvas.</param>
    /// <returns>The column index, or -1 if outside the board.</returns>
    public int MapClientXToColumn(double clientX)
    {
        // Account for the inset
        var boardRelativeX = clientX - Inset;

        if (boardRelativeX < 0)
        {
            return -1;
        }

        var column = (int)(boardRelativeX / CellSize);

        if (column >= Columns)
        {
            return -1;
        }

        return column;
    }
}
