# Product Requirements Document (PRD) for PoConnectFive

## 1. Application Description

PoConnectFive is a web-based implementation of the classic two-player connection game, Connect Five. The application is built using a modern .NET 9 stack, featuring a Blazor WebAssembly frontend hosted by a .NET Web API backend. This architecture provides a responsive and interactive user experience while maintaining a clear separation of concerns between client-side logic and server-side operations.

The core functionality allows two players to take turns dropping colored discs into a vertically suspended grid. The objective is to be the first to form a horizontal, vertical, or diagonal line of five of one's own discs. The application supports both player-vs-player and player-vs-AI game modes, with varying levels of AI difficulty.

### Key Features:
- **Interactive Game Board**: A dynamic and visually appealing game grid.
- **Multiple Game Modes**: Play against another human or an AI opponent.
- **AI Difficulty Levels**: Easy, Medium, and Hard AI opponents.
- **Game Statistics**: Track wins, losses, and draws.
- **Leaderboard**: Display top players based on their performance.
- **Sound Effects**: Audio feedback for in-game actions.
- **Responsive Design**: Accessible on various device sizes.
- **Diagnostics Page**: Monitor the health of application dependencies.

## 2. UI Pages and Components

### 2.1. Pages

#### 2.1.1. Index.razor (`/`)
- **Purpose**: The landing page and main menu of the application.
- **Components**:
  - Application title and branding.
  - Navigation buttons to other primary pages (Game, Leaderboard, Statistics, Settings, Diagnostics).
  - Brief instructions or an "About" section.
- **User Interaction**: Users navigate to different sections of the application.

#### 2.1.2. Game.razor (`/game`)
- **Purpose**: The primary game interface where users play Connect Five.
- **Components**:
  - **GameBoardComponent**: Displays the interactive grid where the game is played. It handles user input for dropping discs and visually represents the game state.
  - **Game Status Display**: Shows whose turn it is, the current game mode (PvP or PvAI), and AI difficulty if applicable.
  - **Control Buttons**: "New Game", "Main Menu", and potentially "Undo Move".
  - **Player Information**: Displays the names and colors of the current players.
- **User Interaction**:
  - Players click on a column to drop their disc.
  - The game logic validates moves, checks for a win or draw condition, and updates the board accordingly.
  - In PvAI mode, the AI makes its move automatically after the player.

#### 2.1.3. Leaderboard.razor (`/leaderboard`)
- **Purpose**: Displays a ranked list of players based on their performance statistics.
- **Components**:
  - A data table or list showing player rankings.
  - Columns: Rank, Player Name, Wins, Losses, Draws, Win Percentage.
  - Filtering or sorting options (e.g., by total games, win percentage).
- **Data Source**: Fetches player data from the backend API, which retrieves it from Azure Table Storage.

#### 2.1.4. Statistics.razor (`/statistics`)
- **Purpose**: Provides detailed statistics and visualizations of game data.
- **Components**:
  - **WinProbabilityChart**: A chart (e.g., a bar or pie chart) visualizing win/loss/draw ratios.
  - Summary cards displaying total games played, current win/loss streak, etc.
  - Filters to view statistics by game mode or opponent type.
- **Data Source**: Aggregates data from the backend API.

#### 2.1.5. Settings.razor (`/settings`)
- **Purpose**: Allows users to configure application preferences.
- **Components**:
  - **AI Difficulty Selection**: A dropdown or radio button group to choose the default AI difficulty (Easy, Medium, Hard).
  - **Sound Toggle**: An on/off switch for game sound effects.
  - **Theme Selection**: (Optional) Options for light/dark mode or different color themes.
  - **Player Name Input**: Fields for players to set their in-game names.
- **Data Persistence**: Settings are saved locally in the browser's local storage.

#### 2.1.6. Diag.razor (`/diag`)
- **Purpose**: A diagnostics page to monitor the connection status of critical external dependencies.
- **Components**:
  - A status table listing key components (e.g., "API Health", "Storage", "Internet Connectivity").
  - Visual indicators (e.g., badges) for "Healthy" or "Unhealthy" status for each component.
  - A "Run Diagnostics" button to manually trigger health checks.
  - Detailed error messages displayed if a component is unhealthy.
- **Functionality**:
  - On page load, it automatically calls the `/healthz`, `/healthz/storage`, and `/healthz/internet` API endpoints.
  - It displays the status of each dependency based on the API responses.
  - Results are logged back to the server via the `/healthz/log` endpoint for monitoring.

### 2.2. Shared Components

#### 2.2.1. MainLayout.razor
- **Purpose**: The root layout component that wraps all pages.
- **Components**:
  - **NavMenu.razor**: The main navigation sidebar, providing links to all primary pages.
  - A footer or header area for common branding or information.

#### 2.2.2. NavMenu.razor
- **Purpose**: Provides navigation links within the application.
- **Components**:
  - A list of `NavLink` components for routing to Index, Game, Leaderboard, Statistics, Settings, and Diag pages.
  - Icons or visual cues for each navigation item.

#### 2.2.3. DiagnosticsModal.razor
- **Purpose**: (Potentially) A reusable modal component to display diagnostic information, though the current implementation uses a full page. This could be refactored to be a modal triggered from other pages if needed.

### 2.3. Services (Client-Side)

These are not UI components but are crucial for the UI's functionality and are worth noting.

- **`ApiPlayerDataService.cs`**: Handles communication with the backend API for all player-related data (stats, leaderboard).
- **`BrowserStorageService.cs`**: Abstracts browser local/session storage for saving settings and game state.
- **`ErrorHandlingService.cs`**: A centralized service for catching and displaying errors to the user.
- **`LoggingHelper.cs`**: A client-side logging utility.
- **`SoundService.cs`**: Manages the playing of sound effects for game events.

## 3. Non-Functional Requirements (Implied)

- **Performance**: The application should be responsive, with minimal latency for user actions, especially during gameplay.
- **Reliability**: The application should gracefully handle errors, such as network issues when communicating with the backend or storage services.
- **Scalability**: While the current scope is a single-player or two-player game, the architecture should support future scaling, for instance, if real-time multiplayer features were to be added.
- **Maintainability**: The codebase is structured with a separation of concerns (Client, Server, Shared projects) and uses dependency injection to facilitate testing and future modifications.
- **Security**: Sensitive data like connection strings should not be exposed on the client-side and are managed via `appsettings` files and environment variables.
