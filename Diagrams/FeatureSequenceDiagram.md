sequenceDiagram
    title Feature Sequence: Player vs. AI Game

    actor Player
    participant Client as Blazor Client
    participant GameService as GameService (Shared)
    participant AI as AIPlayerService
    participant API as .NET API
    participant DB as Azure Table Storage

    Player->>Client: Starts New Game (vs AI)
    Client->>GameService: CreateNewGame(AIMode: Medium)
    GameService-->>Client: Initial GameState
    Client->>Client: Render GameBoard

    loop Game Loop
        Player->>Client: Clicks Column (e.g., Col 3)
        Client->>GameService: MakeMove(GameId, Player, Col 3)
        GameService->>GameService: UpdateBoard, CheckWin
        alt Player Wins
            GameService-->>Client: GameState (PlayerWon)
            Client->>Client: Display Win Message, Play Sound
            Client->>API: POST /api/player/stats (Win)
            API->>DB: Update PlayerStats
            DB-->>API: Success
            API-->>Client: 200 OK
            Client->>Client: Update UI, Show Stats
        else Draw
            GameService-->>Client: GameState (Draw)
            Client->>Client: Display Draw Message, Play Sound
            Client->>API: POST /api/player/stats (Draw)
            API->>DB: Update PlayerStats
            DB-->>API: Success
            API-->>Client: 200 OK
        else Game Continues
            GameService-->>Client: GameState (AITurn)
            Client->>Client: Update Board, Disable Input
            Client->>AI: RequestMove(GameState, Difficulty: Medium)
            AI->>AI: CalculateBestMove()
            AI-->>Client: AIMove (e.g., Col 4)
            Client->>GameService: MakeMove(GameId, AI, Col 4)
            GameService->>GameService: UpdateBoard, CheckWin
            alt AI Wins
                GameService-->>Client: GameState (AIWon)
                Client->>Client: Display Lose Message, Play Sound
                Client->>API: POST /api/player/stats (Loss)
                API->>DB: Update PlayerStats
                DB-->>API: Success
                API-->>Client: 200 OK
                Client->>Client: Update UI, Show Stats
            else Draw
                GameService-->>Client: GameState (Draw)
                Client->>Client: Display Draw Message, Play Sound
                Client->>API: POST /api/player/stats (Draw)
                API->>DB: Update PlayerStats
                DB-->>API: Success
                API-->>Client: 200 OK
            else Game Continues
                GameService-->>Client: GameState (PlayerTurn)
                Client->>Client: Update Board, Enable Input
            end
        end
    end
