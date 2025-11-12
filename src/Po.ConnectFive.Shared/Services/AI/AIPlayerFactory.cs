using System;
using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services.AI;

/// <summary>
/// Factory pattern implementation for creating AI players
/// SOLID Principles:
/// - Single Responsibility: Class is only responsible for creating AI instances
/// - Open/Closed: New AI difficulties can be added without modifying existing code
/// - Dependency Inversion: Works with IAIPlayer interface instead of concrete classes
///
/// Design Patterns:
/// - Factory Pattern: Centralizes object creation logic
/// - Strategy Pattern: Each AI difficulty is a different strategy implementation.
/// </summary>
public static class AIPlayerFactory
{
    public static IAIPlayer CreateAIPlayer(AIDifficulty difficulty)
    {
        return difficulty switch
        {
            AIDifficulty.Easy => new EasyAIPlayer(),
            AIDifficulty.Medium => new MediumAIPlayer(),
            AIDifficulty.Hard => new HardAIPlayer(),
            _ => throw new ArgumentException($"Unknown difficulty: {difficulty}")
        };
    }
}
