using PoConnectFive.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoConnectFive.Shared.Services
{
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
                GameStatus.InProgress
            );

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
                    return false;

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
                    GameStatus.InProgress
                );
            }

            return false;
        }
    }
}