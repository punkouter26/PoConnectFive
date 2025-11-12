using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services;

public class WinProbabilityService
{
    private readonly Random _random = new Random();

    public Task<List<(int column, double probability)>> CalculateWinProbabilities(GameState gameState, int simulations = 1000)
    {
        // Cap simulations at 100 to prevent excessive calculations
        simulations = Math.Min(simulations, 100);

        var probabilities = new List<(int column, double probability)>();
        var validColumns = Enumerable.Range(0, GameBoard.Columns)
            .Where(col => gameState.Board.IsValidMove(col))
            .ToList();

        // Calculate probabilities for each valid column
        foreach (var column in validColumns)
        {
            var wins = 0;
            // Run fewer simulations (20) per column for better performance
            int actualSimulations = Math.Min(simulations, 20);
            for (int i = 0; i < actualSimulations; i++)
            {
                if (SimulateGame(gameState, column))
                {
                    wins++;
                }
            }
            probabilities.Add((column, (double)wins / actualSimulations * 100));
        }

        // Return ordered probabilities
        return Task.FromResult(probabilities.OrderByDescending(p => p.probability).ToList());
    }

    // Simplified method without async/await for better performance
    private bool SimulateGame(GameState gameState, int firstMoveColumn)
    {
        var currentState = gameState;
        var currentBoard = currentState.Board.PlacePiece(firstMoveColumn, currentState.CurrentPlayer.Id);
        currentState = new GameState(
            currentBoard,
            currentState.Player1,
            currentState.Player2,
            currentState.CurrentPlayer.Id == currentState.Player1.Id ? currentState.Player2 : currentState.Player1,
            GameStatus.InProgress);

        // Limit simulation to prevent excessive iterations (max 50 moves)
        int maxIterations = 50;
        int iterations = 0;

        while (currentState.Status == GameStatus.InProgress && iterations < maxIterations)
        {
            iterations++;
            var validMoves = Enumerable.Range(0, GameBoard.Columns)
                .Where(col => currentState.Board.IsValidMove(col))
                .ToList();

            if (!validMoves.Any())
            {
                return false;
            }

            var randomMove = validMoves[_random.Next(validMoves.Count)];
            currentBoard = currentState.Board.PlacePiece(randomMove, currentState.CurrentPlayer.Id);

            if (currentBoard.CheckWin(currentBoard.GetTargetRow(randomMove), randomMove, currentState.CurrentPlayer.Id))
            {
                return currentState.CurrentPlayer.Id == gameState.CurrentPlayer.Id;
            }

            currentState = new GameState(
                currentBoard,
                currentState.Player1,
                currentState.Player2,
                currentState.CurrentPlayer.Id == currentState.Player1.Id ? currentState.Player2 : currentState.Player1,
                GameStatus.InProgress);
        }

        return false;
    }

    /// <summary>
    /// Calculate real-time win probability for current board position (simplified, fast evaluation)
    /// Returns percentage chance for current player to win.
    /// </summary>
    public double CalculateCurrentWinProbability(GameState gameState)
    {
        if (gameState.Status != GameStatus.InProgress)
        {
            return gameState.Status == GameStatus.Player1Won && gameState.CurrentPlayer.Id == 1 ? 100 :
                   gameState.Status == GameStatus.Player2Won && gameState.CurrentPlayer.Id == 2 ? 100 : 0;
        }

        // Use simplified heuristic evaluation
        int currentPlayerScore = EvaluateBoardPosition(gameState.Board, gameState.CurrentPlayer.Id);
        int opponentId = gameState.CurrentPlayer.Id == 1 ? 2 : 1;
        int opponentScore = EvaluateBoardPosition(gameState.Board, opponentId);

        // Convert scores to probability (sigmoid-like function)
        double scoreDiff = currentPlayerScore - opponentScore;
        double probability = 50 + (scoreDiff / 100.0) * 10; // Scale difference

        // Clamp between 5% and 95%
        return Math.Max(5, Math.Min(95, probability));
    }

    private int EvaluateBoardPosition(GameBoard board, int playerId)
    {
        int score = 0;
        int opponentId = playerId == 1 ? 2 : 1;

        // Quick evaluation in all directions
        score += EvaluateDirection(board, playerId, opponentId, 0, 1);  // Horizontal
        score += EvaluateDirection(board, playerId, opponentId, 1, 0);  // Vertical
        score += EvaluateDirection(board, playerId, opponentId, 1, 1);  // Diagonal \
        score += EvaluateDirection(board, playerId, opponentId, 1, -1); // Diagonal /

        return score;
    }

    private int EvaluateDirection(GameBoard board, int playerId, int opponentId, int rowDelta, int colDelta)
    {
        int score = 0;

        for (int row = 0; row < GameBoard.Rows; row++)
        {
            for (int col = 0; col < GameBoard.Columns; col++)
            {
                var sequence = GetSequence(board, row, col, rowDelta, colDelta, 5);
                if (sequence.Count >= 5)
                {
                    score += QuickScoreSequence(sequence, playerId, opponentId);
                }
            }
        }

        return score;
    }

    private List<int> GetSequence(GameBoard board, int startRow, int startCol, int rowDelta, int colDelta, int length)
    {
        var sequence = new List<int>();
        int row = startRow;
        int col = startCol;

        for (int i = 0; i < length && row >= 0 && row < GameBoard.Rows && col >= 0 && col < GameBoard.Columns; i++)
        {
            sequence.Add(board.GetCell(row, col));
            row += rowDelta;
            col += colDelta;
        }

        return sequence;
    }

    private int QuickScoreSequence(List<int> sequence, int playerId, int opponentId)
    {
        int playerCount = sequence.Count(x => x == playerId);
        int opponentCount = sequence.Count(x => x == opponentId);
        int emptyCount = 5 - playerCount - opponentCount;

        if (playerCount > 0 && opponentCount > 0)
        {
            return 0;
        }

        // Quick scoring
        if (playerCount == 4 && emptyCount == 1)
        {
            return 100;
        }

        if (playerCount == 3 && emptyCount == 2)
        {
            return 20;
        }

        if (playerCount == 2 && emptyCount == 3)
        {
            return 5;
        }

        if (opponentCount == 4 && emptyCount == 1)
        {
            return -100;
        }

        if (opponentCount == 3 && emptyCount == 2)
        {
            return -20;
        }

        if (opponentCount == 2 && emptyCount == 3)
        {
            return -5;
        }

        return 0;
    }
}
