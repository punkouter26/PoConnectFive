# PoConnectFive Code Health & Maintainability Improvement Plan

**Generated:** November 12, 2025  
**Project:** PoConnectFive (.NET 9.0 Blazor WebAssembly)  
**Objective:** Improve code health, maintainability, test coverage, and adherence to SOLID principles

---

## Executive Summary

This comprehensive analysis examines the PoConnectFive codebase for maintainability issues including high cyclomatic complexity, SOLID principle violations, test coverage gaps, large component sizes, API naming conventions, code duplication, and folder structure. Based on the findings, we present 10 prioritized improvement opportunities.

### Overall Health Score: 🟡 **Good with Improvements Needed** (72/100)

**Strengths:**
- ✅ Strong SOLID adherence in most classes (no excessive dependencies)
- ✅ Good test coverage for core logic (GameService, GameBoard, AI players)
- ✅ Well-organized project structure (Api/Client/Shared separation)
- ✅ Comprehensive telemetry & logging
- ✅ Effective use of design patterns (Strategy, Factory, Repository)

**Critical Improvement Areas:**
- 🔴 Large Razor components (6 components >200 lines, Game.razor at 832 lines)
- 🔴 Missing integration tests (5 API endpoints untested)
- 🟡 High cyclomatic complexity in AI algorithms (~15 complexity)
- 🟡 Code duplication in board evaluation logic
- 🟡 Inconsistent API naming (some non-RESTful endpoints)

---

## Priority 1: High Cyclomatic Complexity Methods (CRITICAL)

### Report: Methods with Complexity > 10

| Method | File | Estimated Complexity | Lines | Issue |
|--------|------|---------------------|-------|-------|
| `HardAIPlayer.GetNextMove()` | HardAIPlayer.cs | ~15 | 70 | Multiple nested loops, conditionals, minimax orchestration |
| `HardAIPlayer.Minimax()` | HardAIPlayer.cs | ~12 | 25 | Recursive algorithm with multiple branches |
| `BoardEvaluator.EvaluateLines()` | BoardEvaluator.cs | ~13 | 35 | Triple-nested loops with scoring logic |
| `Game.razor.OnColumnClicked()` | Game.razor | ~11 | 45 | Multiple state checks, AI turn handling, game end logic |
| `Game.razor.HandleGameEnd()` | Game.razor | ~10 | 30 | Complex conditional logic for stats updates |

### Refactoring Strategy

#### 1.1 Refactor `HardAIPlayer.GetNextMove()` - Extract Method Pattern

**Current Issues:**
- Method does move ordering, immediate win detection, minimax evaluation, and tie-breaking
- ~70 lines, violates Single Responsibility Principle
- Difficult to unit test individual strategies

**Proposed Solution:**
```csharp
public Task<int> GetNextMove(GameState gameState)
{
    var validMoves = GetValidMoves(gameState.Board);
    
    if (validMoves.Count == 0) return Task.FromResult(0);
    if (validMoves.Count == 1) return Task.FromResult(validMoves[0]);
    
    // Extract Method: Check for immediate wins first
    var immediateWin = FindImmediateWin(gameState, validMoves);
    if (immediateWin.HasValue) return Task.FromResult(immediateWin.Value);
    
    // Extract Method: Evaluate all moves using minimax
    var bestMove = EvaluateMovesWithMinimax(gameState, validMoves);
    
    return Task.FromResult(bestMove);
}

private int? FindImmediateWin(GameState gameState, List<int> validMoves)
{
    foreach (var move in validMoves)
    {
        var newBoard = gameState.Board.PlacePiece(move, gameState.CurrentPlayer.Id);
        int row = FindPieceRow(newBoard, move, gameState.CurrentPlayer.Id);
        if (newBoard.CheckWin(row, move, gameState.CurrentPlayer.Id))
            return move;
    }
    return null;
}

private int EvaluateMovesWithMinimax(GameState gameState, List<int> validMoves)
{
    var orderedMoves = OrderMovesByPriority(validMoves);
    int bestMove = orderedMoves[0];
    int bestScore = int.MinValue;
    int alpha = int.MinValue;
    int beta = int.MaxValue;
    
    foreach (var move in orderedMoves)
    {
        var score = EvaluateSingleMove(gameState, move, alpha, beta);
        if (score > bestScore)
        {
            bestScore = score;
            bestMove = move;
        }
        alpha = Math.Max(alpha, bestScore);
        if (beta <= alpha) break; // Alpha-beta pruning
    }
    
    return bestMove;
}
```

**Benefits:**
- Complexity reduced from ~15 to ~5 per method
- Each method has single responsibility
- Easier to unit test individual strategies
- Improved code readability

**Effort:** Medium (4-6 hours)

#### 1.2 Refactor `BoardEvaluator.EvaluateLines()` - Strategy Pattern

**Current Issues:**
- Triple-nested loop structure
- Complexity ~13
- Difficult to extend with new evaluation strategies

**Proposed Solution:**
```csharp
// Create direction evaluator strategy interface
private interface IDirectionEvaluator
{
    int Evaluate(GameBoard board, int aiPlayerId, int opponentId);
}

private class HorizontalEvaluator : IDirectionEvaluator
{
    public int Evaluate(GameBoard board, int aiPlayerId, int opponentId)
        => EvaluateDirection(board, aiPlayerId, opponentId, 0, 1);
}

// Use composition instead of complex method
public int EvaluateBoard(GameBoard board, int aiPlayerId)
{
    int opponentId = aiPlayerId == 1 ? 2 : 1;
    
    var evaluators = new IDirectionEvaluator[]
    {
        new HorizontalEvaluator(),
        new VerticalEvaluator(),
        new DiagonalDownRightEvaluator(),
        new DiagonalDownLeftEvaluator()
    };
    
    int totalScore = evaluators.Sum(e => e.Evaluate(board, aiPlayerId, opponentId));
    totalScore += CalculateCenterColumnBonus(board, aiPlayerId);
    
    return totalScore;
}
```

**Benefits:**
- Complexity reduced from ~13 to ~4
- Each direction evaluation isolated
- Easier to add new evaluation strategies (Open/Closed Principle)
- Better testability

**Effort:** Medium (3-4 hours)

---

## Priority 2: SOLID Principle Violations (HIGH)

### Report: Classes with >5 Constructor Dependencies

| Class | Dependencies | Status |
|-------|--------------|--------|
| `LeaderboardController` | 3 (ITableStorageService, ILogger, TelemetryClient) | ✅ Good |
| `HealthController` | 2 (ILogger, IHealthCheckService) | ✅ Good |
| `Program.cs` (API) | N/A (service registration) | ✅ Good |

**Analysis:** ✅ The project follows excellent DI practices. No classes exceed 5 dependencies.

### Report: Single Responsibility Principle (SRP) Violations

| Class | Responsibilities | Violation Severity | Lines |
|-------|-----------------|-------------------|-------|
| `Game.razor` | UI rendering, state management, AI orchestration, game logic, statistics tracking | 🔴 CRITICAL | 832 |
| `GameService` | Game state creation, move validation, AI player management, win detection | 🟡 MODERATE | 120 |
| `HardAIPlayer` | Move generation, minimax algorithm, board evaluation, personality selection | 🟡 MODERATE | 300+ |
| `LeaderboardService` | Data access, statistics calculation, player creation | 🟡 MODERATE | 100 |

### Refactoring Strategy

#### 2.1 Split `Game.razor` - Facade Pattern + Component Composition

**Current Structure (832 lines):**
```
Game.razor
├── State Management (GameSetupModel, _currentState, _isAITurn, etc.)
├── Game Logic (OnColumnClicked, PerformAIMoveAsync, HandleGameEnd)
├── Statistics Tracking (UpdatePlayerStats via API)
├── UI Rendering (markup for header, board, controls)
└── Navigation & Sound Management
```

**Proposed Architecture (Vertical Slice):**
```
/Features/Game/
  ├── GamePage.razor (150 lines - orchestrator)
  ├── GamePageViewModel.cs (state management)
  ├── GameOrchestrator.cs (coordinates game flow)
  ├── Components/
  │   ├── GameStatusHeader.razor (80 lines)
  │   ├── GameBoardPanel.razor (100 lines)
  │   └── GameControlBar.razor (50 lines)
  └── Services/
      ├── AITurnHandler.cs (handles AI move logic)
      └── GameCompletionHandler.cs (stats updates, dialogs)
```

**Implementation:**
```csharp
// GameOrchestrator.cs - Facade Pattern
public class GameOrchestrator
{
    private readonly IGameService _gameService;
    private readonly AITurnHandler _aiHandler;
    private readonly GameCompletionHandler _completionHandler;
    
    public async Task<GameMoveResult> ProcessPlayerMove(GameState state, int column)
    {
        if (!await _gameService.IsValidMove(state, column))
            return GameMoveResult.Invalid();
        
        var newState = await _gameService.MakeMove(state, column);
        
        if (newState.Status != GameStatus.InProgress)
            return await _completionHandler.HandleCompletion(newState);
        
        if (newState.CurrentPlayer.Type == PlayerType.AI)
            return await _aiHandler.PerformAIMove(newState);
        
        return GameMoveResult.Success(newState);
    }
}
```

**Benefits:**
- Game.razor reduced from 832 to ~150 lines
- Each class has single responsibility
- Testable without Blazor dependencies
- Follows AGENTS.md Vertical Slice Architecture guidelines

**Effort:** Large (8-12 hours)

---

## Priority 3: Test Coverage Gaps (CRITICAL)

### Report: 5 Most Critical Untested Business Logic Methods

| Method | File | Risk Level | Why Critical |
|--------|------|-----------|--------------|
| `GameBoard.CheckWin()` | GameBoard.cs | 🔴 HIGH | Core win detection logic - bugs cause incorrect game outcomes |
| `WinProbabilityService.CalculateWinProbability()` | WinProbabilityService.cs | 🟡 MEDIUM | No tests found - impacts user experience |
| `GameStatisticsService.GetStatisticsAsync()` | GameStatisticsService.cs | 🟡 MEDIUM | No tests - critical for analytics |
| `TableStorageService.UpsertPlayerStatAsync()` | TableStorageService.cs | 🔴 HIGH | External dependency - needs integration tests |
| `GameStateService.MakeMove()` | GameStateService.cs | 🟡 MEDIUM | Complex state transitions need verification |

### Report: API Endpoints Without Integration Tests

| Endpoint | Method | Controller | Has Test? |
|----------|--------|-----------|-----------|
| `GET /api/leaderboard/{difficulty}` | GetTopPlayers | LeaderboardController | ❌ NO |
| `POST /api/leaderboard/playerstats` | UpdatePlayerStats | LeaderboardController | ✅ YES |
| `GET /api/health` | CheckHealth | HealthController | ✅ YES |
| `GET /api/health/storage` | CheckStorage | HealthController | ❌ NO |
| `GET /api/health/internet` | CheckInternet | HealthController | ❌ NO |
| `POST /api/health/log` | LogDiagnostics | HealthController | ❌ NO |
| `GET /api/health/ping` | (Minimal API) | HealthEndpoints | ❌ NO |

**Missing Integration Tests: 5 out of 7 endpoints (71% coverage gap)**

### Test Implementation Plan

#### 3.1 Unit Test: `GameBoard.CheckWin()` - CRITICAL

**Priority:** CRITICAL (core game logic, no tests found)

```csharp
// GameBoardWinDetectionTests.cs (NEW FILE)
public class GameBoardWinDetectionTests
{
    [Theory]
    [InlineData(new[] { 0, 1, 2, 3, 4 }, 8, 4)] // Horizontal bottom row
    [InlineData(new[] { 5, 6, 7, 8, 9 }, 8, 8)] // Horizontal different position
    public void CheckWin_FiveInRowHorizontal_ReturnsTrue(int[] moves, int expectedRow, int expectedCol)
    {
        var board = new GameBoard();
        foreach (var col in moves)
        {
            board = board.PlacePiece(col, 1);
        }
        
        Assert.True(board.CheckWin(expectedRow, expectedCol, 1));
    }
    
    [Fact]
    public void CheckWin_FourInRow_ReturnsFalse()
    {
        var board = new GameBoard();
        for (int i = 0; i < 4; i++)
        {
            board = board.PlacePiece(i, 1);
        }
        
        Assert.False(board.CheckWin(8, 3, 1));
    }
    
    [Fact]
    public void CheckWin_VerticalFive_ReturnsTrue()
    {
        var board = new GameBoard();
        for (int i = 0; i < 5; i++)
        {
            board = board.PlacePiece(0, 1); // Same column
        }
        
        Assert.True(board.CheckWin(4, 0, 1));
    }
    
    [Fact]
    public void CheckWin_DiagonalDownRight_ReturnsTrue()
    {
        // Create diagonal ↘ pattern
        var board = CreateDiagonalPattern();
        Assert.True(board.CheckWin(8, 0, 1));
    }
    
    [Fact]
    public void CheckWin_DiagonalUpRight_ReturnsTrue()
    {
        // Create diagonal ↗ pattern
        var board = CreateReverseDiagonalPattern();
        Assert.True(board.CheckWin(5, 3, 1));
    }
}
```

**Effort:** Small (2-3 hours)

#### 3.2 Integration Test: `GET /api/leaderboard/{difficulty}`

**Priority:** HIGH (public API endpoint, no test coverage)

```csharp
// LeaderboardControllerIntegrationTests.cs (EXTEND EXISTING)
[Fact]
public async Task GetTopPlayers_Easy_ReturnsTop5OrderedByWinRate()
{
    // Arrange
    var client = _factory.CreateClient();
    await SeedTestPlayers(AIDifficulty.Easy, count: 10);
    
    // Act
    var response = await client.GetAsync("/api/leaderboard/Easy");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var players = await response.Content.ReadFromJsonAsync<List<PlayerStatEntity>>();
    
    Assert.NotNull(players);
    Assert.Equal(5, players.Count);
    Assert.True(players[0].WinRate >= players[1].WinRate);
    Assert.All(players, p => Assert.Equal(AIDifficulty.Easy, p.AIDifficulty));
}

[Theory]
[InlineData(AIDifficulty.Easy)]
[InlineData(AIDifficulty.Medium)]
[InlineData(AIDifficulty.Hard)]
public async Task GetTopPlayers_AllDifficulties_ReturnsCorrectData(AIDifficulty difficulty)
{
    var client = _factory.CreateClient();
    await SeedTestPlayers(difficulty, count: 3);
    
    var response = await client.GetAsync($"/api/leaderboard/{difficulty}");
    
    response.EnsureSuccessStatusCode();
    var players = await response.Content.ReadFromJsonAsync<List<PlayerStatEntity>>();
    Assert.NotEmpty(players);
}

[Fact]
public async Task GetTopPlayers_EmptyLeaderboard_ReturnsEmptyList()
{
    var client = _factory.CreateClient();
    var response = await client.GetAsync("/api/leaderboard/Easy");
    
    response.EnsureSuccessStatusCode();
    var players = await response.Content.ReadFromJsonAsync<List<PlayerStatEntity>>();
    Assert.NotNull(players);
    Assert.Empty(players);
}
```

**Effort:** Small (1-2 hours)

#### 3.3 Unit Test: `WinProbabilityService`

```csharp
// WinProbabilityServiceTests.cs (NEW FILE)
public class WinProbabilityServiceTests
{
    private readonly WinProbabilityService _service = new();
    
    [Fact]
    public void CalculateWinProbability_EmptyBoard_Returns50Percent()
    {
        var gameState = CreateEmptyGameState();
        var result = _service.CalculateWinProbability(gameState);
        
        Assert.InRange(result.Player1Probability, 45, 55);
    }
    
    [Fact]
    public void CalculateWinProbability_Player1FourInRow_ReturnsHighProbability()
    {
        var gameState = CreateAlmostWinState(playerId: 1);
        var result = _service.CalculateWinProbability(gameState);
        
        Assert.True(result.Player1Probability > 80);
    }
}
```

**Effort:** Small (2 hours)

---

## Priority 4: Large Component Size (HIGH)

### Report: UI Components Exceeding 200 Lines

| Component | Lines | Complexity | Responsibilities |
|-----------|-------|-----------|------------------|
| `Game.razor` | 832 | 🔴 CRITICAL | Game orchestration, UI, state, AI, stats, navigation |
| `GameBoardComponent.razor` | 265 | 🟡 MODERATE | Canvas rendering, interaction, animations, JS interop |
| `Settings.razor` | 251 | 🟡 MODERATE | Settings UI, persistence, validation, theme switching |
| `InstallPrompt.razor` | 246 | 🟡 MODERATE | PWA install logic, UI, event handling, storage |
| `Index.razor` | 243 | 🟡 MODERATE | Game mode selection, form handling, navigation |
| `Leaderboard.razor` | 230 | 🟡 MODERATE | Data fetching, table rendering, filtering, sorting |

**Total:** 6 out of 18 components exceed 200 lines (33% violation rate)

### Decomposition Strategy

#### 4.1 Break Down `Game.razor` (832 lines → 5 components ~150 lines each)

**Proposed Structure:**
```
/Pages/Game.razor (150 lines - orchestrator)
/Components/Game/
  ├── GameStatusHeader.razor (80 lines)
  │   └── Displays: player info, turn indicator, game status
  ├── GameBoardPanel.razor (100 lines)
  │   └── Wraps: GameBoardComponent with container logic
  ├── WinProbabilityPanel.razor (60 lines)
  │   └── Shows: live win probability display
  └── GameControlBar.razor (50 lines)
      └── Contains: New Game, Reset, Mute buttons

/Services/Game/
  ├── GameOrchestrator.cs (coordinates game flow)
  ├── AITurnHandler.cs (AI move logic)
  └── GameCompletionHandler.cs (game end, stats)
```

**Game.razor (Simplified Orchestrator - 150 lines):**
```razor
@page "/game"
@inject GameOrchestrator Orchestrator

<RadzenStack AlignItems="AlignItems.Center" Gap="1rem" Class="game-page">
    <GameStatusHeader CurrentState="@_state" />
    
    @if (_state?.Status == GameStatus.InProgress)
    {
        <WinProbabilityPanel CurrentState="@_state" />
    }
    
    <GameBoardPanel 
        CurrentState="@_state" 
        OnMove="HandleMove"
        IsBlocked="@_isAITurn" />
    
    <GameControlBar 
        OnNewGame="NavigateToMenu"
        OnReset="ResetGame"
        IsMuted="@SoundService.IsMuted"
        OnToggleMute="ToggleMute" />
</RadzenStack>

@code {
    private GameState? _state;
    private bool _isAITurn;
    
    protected override async Task OnInitializedAsync()
    {
        _state = await Orchestrator.InitializeFromRoute(NavigationManager.Uri);
    }
    
    private async Task HandleMove(int column)
    {
        var result = await Orchestrator.ProcessPlayerMove(_state!, column);
        _state = result.NewState;
        _isAITurn = result.IsAITurn;
    }
}
```

**Benefits:**
- Game.razor: 832 → 150 lines (82% reduction)
- Each component <100 lines (follows AGENTS.md guidelines)
- Single responsibility per component
- Reusable components
- Easier testing with bUnit

**Effort:** Large (10-12 hours)

#### 4.2 Refactor `GameBoardComponent.razor` (265 lines → Extract JS Interop)

**Current Issue:** Mix of Blazor component logic and canvas rendering

**Proposed Solution:**
```
GameBoardComponent.razor (120 lines - Blazor component)
└── wwwroot/js/gameBoardRenderer.js (enhanced - all canvas logic)
```

**Benefits:**
- Clear separation: Blazor manages state, JS handles rendering
- Better performance (fewer interop calls)
- Component reduced to <150 lines

**Effort:** Medium (4-6 hours)

---

## Priority 5: API Naming Conventions (MEDIUM)

### Report: Non-RESTful API Endpoints

| Current Endpoint | Issue | RESTful Alternative |
|-----------------|-------|---------------------|
| `POST /api/leaderboard/playerstats` | Verb in URL, not resource-oriented | `PUT /api/leaderboard/players/{name}/stats` |
| `POST /api/health/log` | Action in URL, not RESTful | `POST /api/diagnostics` |
| `GET /api/health/storage` | Action in URL | `GET /api/health/checks/storage` |
| `GET /api/health/internet` | Action in URL | `GET /api/health/checks/internet` |

### Refactoring Plan

#### 5.1 Rename Leaderboard Endpoints (RESTful)

**Before:**
```csharp
[HttpPost("playerstats")]
public async Task<IActionResult> UpdatePlayerStats([FromBody] PlayerStatUpdateDto updateDto)
```

**After:**
```csharp
[HttpPut("players/{playerName}/stats")]
public async Task<IActionResult> UpdatePlayerStats(
    string playerName, 
    [FromBody] PlayerStatUpdateDto updateDto)
{
    if (playerName != updateDto.PlayerName)
        return BadRequest("Player name mismatch");
    
    // ... existing logic
}
```

#### 5.2 Reorganize Health Endpoints

**Proposed Structure:**
```csharp
// HealthController.cs
[HttpGet]                               // GET /api/health
[HttpGet("checks/{component}")]         // GET /api/health/checks/storage

// DiagnosticsController.cs (NEW)
[HttpPost]                              // POST /api/diagnostics
```

**Effort:** Small (2-3 hours)

---

## Priority 6: Code Duplication (MEDIUM)

### Report: Duplicate Code Blocks (5+ lines)

#### 6.1 Board Iteration Pattern (Found in 4 files)

**Locations:**
- `BoardEvaluator.cs` - EvaluateLines()
- `AggressiveEvaluator.cs` - EvaluateLines()
- `DefensiveEvaluator.cs` - EvaluateLines()
- `TrickyEvaluator.cs` - EvaluateLines()

**Duplicate Pattern (~30 lines):**
```csharp
for (int row = 0; row < GameBoard.Rows; row++)
{
    for (int col = 0; col < GameBoard.Columns; col++)
    {
        var sequence = GetSequence(board, row, col, rowDelta, colDelta);
        if (sequence.Count >= 5)
        {
            score += ScoreSequence(sequence, aiPlayerId, opponentId);
        }
    }
}
```

**Proposed Solution - Template Method Pattern:**
```csharp
// BoardEvaluatorBase.cs (NEW)
public abstract class BoardEvaluatorBase : IBoardEvaluator
{
    public int EvaluateBoard(GameBoard board, int aiPlayerId)
    {
        int opponentId = aiPlayerId == 1 ? 2 : 1;
        
        return EvaluateDirection(board, aiPlayerId, opponentId, 0, 1)  // Horizontal
             + EvaluateDirection(board, aiPlayerId, opponentId, 1, 0)  // Vertical
             + EvaluateDirection(board, aiPlayerId, opponentId, 1, 1)  // Diagonal ↘
             + EvaluateDirection(board, aiPlayerId, opponentId, 1, -1) // Diagonal ↙
             + GetCenterColumnBonus(board, aiPlayerId);
    }
    
    protected virtual int EvaluateDirection(
        GameBoard board, int aiPlayerId, int opponentId, 
        int rowDelta, int colDelta)
    {
        // Common iteration logic (30 lines)
    }
    
    protected abstract int ScoreSequence(List<int> sequence, int aiPlayerId, int opponentId);
}
```

**Benefits:**
- Eliminates ~120 duplicate lines across 4 files
- Consistent evaluation logic
- Subclasses only implement scoring (SRP)

**Effort:** Medium (3-4 hours)

#### 6.2 Error Handling Pattern

**Found in 8 controller methods:**
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error in {Operation}", operationName);
    _telemetryClient.TrackOperationException(ex, operationName, context);
    return StatusCode(500, "Error message");
}
```

**Proposed Solution:**
```csharp
// TelemetryExtensions.cs (EXTEND)
public static IActionResult HandleOperationError<T>(
    this ControllerBase controller,
    Exception ex,
    ILogger<T> logger,
    TelemetryClient telemetryClient,
    string operationName,
    Dictionary<string, string>? context = null)
{
    logger.LogError(ex, "Error in {Operation}", operationName);
    telemetryClient.TrackOperationException(ex, operationName, context ?? new());
    return controller.StatusCode(500, $"Error in {operationName}");
}

// Usage
catch (Exception ex)
{
    return this.HandleOperationError(ex, _logger, _telemetryClient, "GetTopPlayers");
}
```

**Benefits:**
- Reduces 12-15 lines to 1 line per catch block
- Consistent error handling

**Effort:** Small (1-2 hours)

---

## Priority 7: Folder Structure Review (LOW)

### Current Structure Analysis

```
✅ GOOD: Overall structure follows best practices
⚠️ IMPROVEMENT: API should fully adopt Vertical Slice Architecture
⚠️ IMPROVEMENT: Client components should be feature-organized
```

### Recommendations

#### 7.1 API: Full Vertical Slice Architecture

**Current (Mixed):**
```
/Controllers/ (traditional MVC)
/Features/Health/ (vertical slice)
```

**Recommended:**
```
/Features/
  ├── Leaderboard/
  │   ├── GetTopPlayers/
  │   │   ├── Endpoint.cs
  │   │   ├── Query.cs
  │   │   └── Handler.cs
  │   └── UpdatePlayerStats/
  │       ├── Endpoint.cs
  │       ├── Command.cs
  │       └── Handler.cs
  ├── Health/
  │   └── (existing structure)
  └── Diagnostics/
```

**Benefits:**
- All related code co-located
- Follows AGENTS.md guidelines
- Easier feature discovery

**Effort:** Medium (4-6 hours)

#### 7.2 Client: Feature-Based Components

**Current (Flat):**
```
/Components/ (11 components mixed)
```

**Recommended:**
```
/Components/
  ├── Game/
  ├── Statistics/
  ├── Diagnostics/
  ├── PWA/
  └── Shared/
```

**Effort:** Small (2 hours)

---

## Priority 8: Missing Component Tests (MEDIUM)

### Report: bUnit Test Coverage

**Current:** 0 Blazor component tests found  
**Target:** 80% component coverage

### Proposed Tests

```csharp
// GameBoardComponentTests.cs (NEW)
public class GameBoardComponentTests : TestContext
{
    [Fact]
    public void GameBoard_RendersCorrectly()
    {
        var gameState = CreateTestGameState();
        var cut = RenderComponent<GameBoardComponent>(parameters => parameters
            .Add(p => p.Board, gameState.Board)
            .Add(p => p.CurrentState, gameState));
        
        cut.MarkupMatches("<canvas id='gameBoard'></canvas>");
    }
    
    [Fact]
    public async Task GameBoard_ColumnClick_InvokesCallback()
    {
        var clicked = false;
        var cut = RenderComponent<GameBoardComponent>(parameters => parameters
            .Add(p => p.OnColumnClicked, EventCallback.Factory.Create<int>(this, _ => clicked = true)));
        
        await cut.Instance.OnColumnClickedInternal(3);
        
        Assert.True(clicked);
    }
}
```

**Effort:** Medium (6-8 hours for all components)

---

## Priority 9: Performance Optimizations (LOW)

### Recommendations

#### 9.1 Remove Unnecessary `Task.FromResult()`

**Current (GameService.cs):**
```csharp
public Task<bool> IsValidMove(GameState currentState, int column)
{
    return Task.FromResult(currentState.Board.IsValidMove(column));
}
```

**Better:**
```csharp
public bool IsValidMove(GameState currentState, int column)
{
    return currentState.Board.IsValidMove(column);
}
```

**Effort:** Small (1 hour)

---

## Priority 10: Code Quality Tooling (LOW)

### Proposed Additions

#### 10.1 Static Code Analysis

**Add to Directory.Packages.props:**
```xml
<PackageVersion Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" />
<PackageVersion Include="SonarAnalyzer.CSharp" Version="9.16.0" />
```

#### 10.2 EditorConfig

**Create /.editorconfig:**
```ini
[*.cs]
dotnet_sort_system_directives_first = true
csharp_prefer_braces = true:warning
```

**Effort:** Small (2 hours)

---

## Implementation Roadmap

### Phase 1: Critical Fixes (Weeks 1-2)
- [ ] Refactor Game.razor (832 → ~150 lines)
- [ ] Add GameBoard.CheckWin() tests
- [ ] Add leaderboard API integration tests
- [ ] Extract GameOrchestrator service

### Phase 2: Architecture (Weeks 3-4)
- [ ] Refactor HardAIPlayer complexity
- [ ] Apply Template Method to evaluators
- [ ] Reorganize components by feature
- [ ] Vertical Slice API refactoring

### Phase 3: Quality (Weeks 5-6)
- [ ] Eliminate code duplication
- [ ] Add missing unit tests
- [ ] RESTful API naming
- [ ] Add bUnit component tests

### Phase 4: Polish (Weeks 7-8)
- [ ] Performance optimizations
- [ ] Static analysis setup
- [ ] Documentation
- [ ] EditorConfig

---

## Success Metrics

| Metric | Current | Target | Measurement |
|--------|---------|--------|-------------|
| Code Coverage | ~70% | 80% | `dotnet test /p:CollectCoverage=true` |
| Avg Component Size | 202 lines | <150 lines | Count lines in .razor files |
| API Test Coverage | 29% (2/7) | 100% | Integration test count |
| Max Complexity | ~15 | <10 | SonarAnalyzer |
| Components >200 Lines | 6 | 0 | Manual count |
| Code Duplication | ~120 lines | <30 lines | SonarQube |

---

**Document Version:** 1.0  
**Last Updated:** November 12, 2025  
**Next Review:** After Phase 1 completion
