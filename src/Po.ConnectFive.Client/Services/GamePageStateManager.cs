using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Client.Services;

/// <summary>
/// Manages game state and lifecycle for the Game page component
/// Extracted from Game.razor to reduce complexity and improve testability.
/// </summary>
public class GamePageStateManager
{
    private readonly IGameService _gameService;

    public GameState? CurrentState { get; private set; }
    public DateTime GameStartTime { get; private set; }
    public string? Player1Name { get; private set; }
    public string? Player2Name { get; private set; }
    public AIDifficulty? AIDifficulty { get; private set; }
    public AIPersonality? AIPersonality { get; private set; }
    public bool IsAITurn { get; private set; }
    public bool IsInvalidMove { get; private set; }

    public GamePageStateManager(IGameService gameService)
    {
        _gameService = gameService;
    }

    public async Task InitializeGame(string mode, string? difficultyStr = null, string? personalityStr = null)
    {
        if (mode == "single" && !string.IsNullOrEmpty(difficultyStr))
        {
            if (Enum.TryParse<AIDifficulty>(difficultyStr, true, out var difficulty))
            {
                AIPersonality? personality = null;
                if (!string.IsNullOrEmpty(personalityStr) &&
                    Enum.TryParse<AIPersonality>(personalityStr, true, out var parsedPersonality))
                {
                    personality = parsedPersonality;
                }
                await StartNewGameWithAI("Player 1", difficulty, personality);
            }
        }
        else
        {
            await StartNewGameTwoPlayer("Player 1", "Player 2");
        }
    }

    public async Task StartNewGameWithAI(string playerName, AIDifficulty difficulty, AIPersonality? personality = null)
    {
        Player1Name = playerName;
        AIDifficulty = difficulty;
        AIPersonality = personality;

        Player2Name = personality.HasValue
            ? $"AI ({difficulty} - {personality.Value})"
            : $"AI ({difficulty})";

        GameStartTime = DateTime.UtcNow;
        CurrentState = await _gameService.StartNewGame(
            Player1Name,
            Player2Name,
            true,
            difficulty,
            personality);
    }

    public async Task StartNewGameTwoPlayer(string player1Name, string player2Name)
    {
        Player1Name = player1Name;
        Player2Name = player2Name;
        AIDifficulty = null;
        AIPersonality = null;

        GameStartTime = DateTime.UtcNow;
        CurrentState = await _gameService.StartNewGame(player1Name, player2Name);
    }

    public async Task ResetGame()
    {
        if (CurrentState == null)
        {
            return;
        }

        GameStartTime = DateTime.UtcNow;
        CurrentState = await _gameService.StartNewGame(
            CurrentState.Player1.Name,
            CurrentState.Player2.Name,
            CurrentState.Player2.Type == PlayerType.AI,
            CurrentState.Player2.AIDifficulty);
    }

    public async Task<bool> MakeMove(int column)
    {
        if (CurrentState == null)
        {
            return false;
        }

        if (!await _gameService.IsValidMove(CurrentState, column))
        {
            IsInvalidMove = true;
            return false;
        }

        CurrentState = await _gameService.MakeMove(CurrentState, column);
        IsInvalidMove = false;
        return true;
    }

    public async Task<int> GetAIMove()
    {
        if (CurrentState == null)
        {
            throw new InvalidOperationException("Cannot get AI move - no active game state");
        }

        return await _gameService.GetAIMove(CurrentState);
    }

    public void SetAITurn(bool isAITurn)
    {
        IsAITurn = isAITurn;
    }

    public void ClearInvalidMoveFlag()
    {
        IsInvalidMove = false;
    }

    public TimeSpan GetGameDuration()
    {
        return DateTime.UtcNow - GameStartTime;
    }

    public bool IsGameInProgress()
    {
        return CurrentState?.Status == GameStatus.InProgress;
    }

    public bool IsCurrentPlayerAI()
    {
        return CurrentState?.CurrentPlayer.Type == PlayerType.AI;
    }

    public Player? GetWinner()
    {
        if (CurrentState == null)
        {
            return null;
        }

        return CurrentState.Status switch
        {
            GameStatus.Player1Won => CurrentState.Player1,
            GameStatus.Player2Won => CurrentState.Player2,
            _ => null
        };
    }

    public PlayerGameResult? GetGameResult()
    {
        if (CurrentState == null)
        {
            return null;
        }

        return CurrentState.Status switch
        {
            GameStatus.Player1Won => PlayerGameResult.Win,
            GameStatus.Player2Won => PlayerGameResult.Loss,
            GameStatus.Draw => PlayerGameResult.Draw,
            _ => null
        };
    }
}
