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

        public async Task<List<(int column, double probability)>> CalculateWinProbabilities(GameState gameState, int simulations = 1000)
        {
            var probabilities = new List<(int column, double probability)>();
            var validColumns = Enumerable.Range(0, GameBoard.Columns)
                .Where(col => gameState.Board.IsValidMove(col))
                .ToList();

            foreach (var column in validColumns)
            {
                var wins = 0;
                for (int i = 0; i < simulations; i++)
                {
                    if (await SimulateGame(gameState, column))
                    {
                        wins++;
                    }
                }
                probabilities.Add((column, (double)wins / simulations * 100));
            }

            return probabilities.OrderByDescending(p => p.probability).ToList();
        }

        private async Task<bool> SimulateGame(GameState gameState, int firstMoveColumn)
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

            while (currentState.Status == GameStatus.InProgress)
            {
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