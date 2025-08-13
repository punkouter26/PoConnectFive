sequenceDiagram
    actor User
    participant Browser as Blazor Client
    participant Server as .NET API
    participant Storage as Azure Table Storage

    User->>Browser: Navigates to App
    Browser->>Server: GET / (Initial Load)
    Server-->>Browser: Returns index.html
    Browser->>Browser: Loads Blazor WASM
    Browser->>Server: GET /_framework/blazor.webassembly.js
    Server-->>Browser: Returns WASM files

    User->>Browser: Clicks "New Game" (PvAI)
    Browser->>Browser: Initializes GameBoardComponent
    Browser->>Browser: Renders game board

    loop Player Turn
        User->>Browser: Clicks column to drop disc
        Browser->>Browser: Updates game state locally
        Browser->>Browser: Plays sound effect
        Browser->>Browser: Checks for win/draw
        alt Player Wins
            Browser->>Browser: Displays "You Win!"
            Browser->>Server: POST /api/player (update stats)
            Server->>Storage: Update PlayerStatsEntity
            Storage-->>Server: Acknowledgement
            Server-->>Browser: 200 OK
        end
    end

    alt AI Turn
        Browser->>Server: POST /api/game/aimove (with game state)
        Server->>Server: Calculates AI move
        Server-->>Browser: Returns AI move (column)
        Browser->>Browser: Updates game board with AI move
        Browser->>Browser: Plays sound effect
        Browser->>Browser: Checks for win/draw
        alt AI Wins
            Browser->>Browser: Displays "AI Wins!"
            Browser->>Server: POST /api/player (update stats)
            Server->>Storage: Update PlayerStatsEntity
            Storage-->>Server: Acknowledgement
            Server-->>Browser: 200 OK
        end
    end

    User->>Browser: Navigates to Leaderboard
    Browser->>Server: GET /api/leaderboard
    Server->>Storage: Query top PlayerStatsEntity
    Storage-->>Server: Returns player stats
    Server-->>Browser: Returns leaderboard data
    Browser->>Browser: Renders leaderboard table

    User->>Browser: Navigates to Diagnostics
    Browser->>Server: GET /healthz
    Server-->>Browser: Returns API Health
    Browser->>Server: GET /healthz/storage
    Server->>Storage: Check connection
    Storage-->>Server: Connection status
    Server-->>Browser: Returns Storage Health
    Browser->>Server: GET /healthz/internet
    Server->>Server: Checks internet connectivity
    Server-->>Browser: Returns Internet Health
    Browser->>Browser: Displays diagnostics results
    Browser->>Server: POST /healthz/log (sends results)
    Server-->>Browser: 200 OK
