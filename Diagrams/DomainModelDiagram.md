erDiagram
    Player ||--o{ PlayerStats : "has"
    Player {
        string PlayerId PK
        string Name
        string Color
    }
    
    PlayerStats {
        string PlayerId PK, FK
        int Wins
        int Losses
        int Draws
        datetime LastPlayed
    }

    Game ||--o{ GameState : "contains"
    Game {
        string GameId PK
        string Player1Id FK
        string Player2Id FK
        AIDifficulty AIDifficulty
        GameStatus Status
        datetime StartTime
        datetime? EndTime
    }

    GameState {
        string GameId PK, FK
        string BoardState
        string CurrentPlayerId FK
        int TurnNumber
    }

    Player ||--o{ Game : "plays in"
    Player ||--o{ GameState : "makes"

    enum AIDifficulty {
        Easy
        Medium
        Hard
    }

    enum GameStatus {
        InProgress
        Player1Won
        Player2Won
        Draw
    }
