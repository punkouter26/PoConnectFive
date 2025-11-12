# Application Insights KQL Queries for PoConnectFive

This document contains essential Kusto Query Language (KQL) queries for analyzing telemetry data in Application Insights.

## Table of Contents
1. [Game Usage Analytics](#game-usage-analytics)
2. [Error Analysis](#error-analysis)
3. [Performance Monitoring](#performance-monitoring)

---

## Game Usage Analytics

### Query 1: Game Completion Statistics by Difficulty

This query shows game completions grouped by difficulty level and result (Win/Loss/Draw).

```kql
// Game completion statistics by difficulty and result
customEvents
| where name == "GameCompleted"
| where timestamp > ago(30d)
| extend Difficulty = tostring(customDimensions.Difficulty)
| extend Result = tostring(customDimensions.Result)
| extend GameDurationSeconds = todouble(customMeasurements.GameDurationSeconds)
| summarize
    GameCount = count(),
    AvgDurationSec = avg(GameDurationSeconds),
    MinDurationSec = min(GameDurationSeconds),
    MaxDurationSec = max(GameDurationSeconds)
    by Difficulty, Result
| order by Difficulty, Result
| project
    Difficulty,
    Result,
    GameCount,
    AvgDurationMin = round(AvgDurationSec / 60, 2),
    MinDurationMin = round(MinDurationSec / 60, 2),
    MaxDurationMin = round(MaxDurationSec / 60, 2)
```

**Use Case:** Understand which difficulty levels are most popular and how long games typically last.

**Key Metrics:**
- Total games played per difficulty
- Win/loss distribution
- Average game duration
- Helps balance difficulty levels

---

### Query 2: Active Players and Engagement Trends

This query shows daily active players and game sessions over time.

```kql
// Daily active players and game sessions
customEvents
| where name == "GameCompleted"
| where timestamp > ago(30d)
| extend PlayerName = tostring(customDimensions.PlayerName)
| summarize
    UniquePlayersPerDay = dcount(PlayerName),
    TotalGamesPerDay = count()
    by bin(timestamp, 1d)
| order by timestamp desc
| project
    Date = format_datetime(timestamp, 'yyyy-MM-dd'),
    UniquePlayersPerDay,
    TotalGamesPerDay,
    AvgGamesPerPlayer = round(todouble(TotalGamesPerDay) / todouble(UniquePlayersPerDay), 2)
| render timechart
```

**Use Case:** Track user engagement and retention over time.

**Key Metrics:**
- Daily active users (DAU)
- Games per player (engagement metric)
- Trends to identify growth or churn

---

### Query 3: Leaderboard Access Patterns

This query analyzes how users interact with the leaderboard feature.

```kql
// Leaderboard view statistics
customEvents
| where name == "LeaderboardViewed"
| where timestamp > ago(7d)
| extend Difficulty = tostring(customDimensions.Difficulty)
| extend LoadTimeMs = todouble(customDimensions.LoadTimeMs)
| summarize
    ViewCount = count(),
    AvgLoadTimeMs = avg(LoadTimeMs),
    P95LoadTimeMs = percentile(LoadTimeMs, 95),
    P99LoadTimeMs = percentile(LoadTimeMs, 99)
    by Difficulty
| order by ViewCount desc
| project
    Difficulty,
    ViewCount,
    AvgLoadTimeMs = round(AvgLoadTimeMs, 2),
    P95LoadTimeMs = round(P95LoadTimeMs, 2),
    P99LoadTimeMs = round(P99LoadTimeMs, 2)
```

**Use Case:** Understand leaderboard usage and performance.

**Key Metrics:**
- Most viewed difficulty leaderboards
- Load time performance (avg, p95, p99)
- Helps prioritize performance optimizations

---

## Error Analysis

### Query 4: Application Errors by Component

This query identifies the most common errors and their frequency.

```kql
// Top errors by exception type and operation
exceptions
| where timestamp > ago(7d)
| extend Operation = tostring(customDimensions.Operation)
| extend Source = tostring(customDimensions.Source)
| summarize
    ErrorCount = count(),
    AffectedUsers = dcount(user_Id),
    LastOccurrence = max(timestamp)
    by
    ExceptionType = type,
    Operation,
    Source,
    OuterMessage = outerMessage
| order by ErrorCount desc
| take 20
| project
    ExceptionType,
    Operation,
    Source,
    ErrorCount,
    AffectedUsers,
    LastOccurrence,
    OuterMessage
```

**Use Case:** Identify and prioritize bug fixes based on error frequency and user impact.

**Key Metrics:**
- Most common exceptions
- User impact (affected user count)
- Error location (operation/component)

---

### Query 5: Client-Side Errors

This query shows errors originating from the Blazor WebAssembly client.

```kql
// Client-side errors and warnings
traces
| where timestamp > ago(24h)
| where message contains "[Client"
| extend ClientIp = extract(@"Client ([^\]]+)", 1, message)
| extend Level = case(
    severityLevel == 1, "Warning",
    severityLevel == 2, "Error",
    severityLevel == 3, "Critical",
    "Info"
)
| where Level in ("Error", "Critical")
| summarize
    ErrorCount = count(),
    UniqueIPs = dcount(ClientIp),
    LastError = max(timestamp),
    SampleMessages = take_any(message, 3)
    by Level
| order by ErrorCount desc
```

**Use Case:** Monitor client-side issues that may not crash the app but degrade user experience.

**Key Metrics:**
- Client error frequency
- Affected client IPs
- Error severity distribution

---

### Query 6: Failed API Requests

This query tracks failed HTTP requests to identify API issues.

```kql
// Failed API requests with details
requests
| where timestamp > ago(24h)
| where success == false
| extend Operation = operation_Name
| summarize
    FailureCount = count(),
    AvgDurationMs = avg(duration),
    ResultCodes = make_set(resultCode)
    by Operation, resultCode
| order by FailureCount desc
| take 15
| project
    APIEndpoint = Operation,
    StatusCode = resultCode,
    FailureCount,
    AvgDurationMs = round(AvgDurationMs, 2)
```

**Use Case:** Detect API reliability issues and slow endpoints.

**Key Metrics:**
- Failed request count per endpoint
- HTTP status codes
- Response times for failed requests

---

## Performance Monitoring

### Query 7: API Performance Benchmarks

This query analyzes API response times to identify slow operations.

```kql
// API performance metrics (p50, p95, p99)
requests
| where timestamp > ago(24h)
| where success == true
| extend Operation = operation_Name
| summarize
    RequestCount = count(),
    P50 = percentile(duration, 50),
    P95 = percentile(duration, 95),
    P99 = percentile(duration, 99),
    MaxDuration = max(duration)
    by Operation
| where RequestCount > 10  // Filter out low-volume endpoints
| order by P99 desc
| take 20
| project
    APIEndpoint = Operation,
    RequestCount,
    P50_Ms = round(P50, 2),
    P95_Ms = round(P95, 2),
    P99_Ms = round(P99, 2),
    Max_Ms = round(MaxDuration, 2)
```

**Use Case:** Identify slow API endpoints that need optimization.

**Key Metrics:**
- Median (p50), p95, p99 response times
- Request volume per endpoint
- Maximum observed latency

---

### Query 8: Custom Metrics - Game Duration Trends

This query tracks game duration metrics over time.

```kql
// Game duration trends by difficulty
customMetrics
| where name == "GameDuration"
| where timestamp > ago(7d)
| extend Difficulty = tostring(customDimensions.Difficulty)
| extend Result = tostring(customDimensions.Result)
| summarize
    AvgDurationMs = avg(value),
    P50DurationMs = percentile(value, 50),
    P95DurationMs = percentile(value, 95)
    by bin(timestamp, 1d), Difficulty, Result
| order by timestamp desc
| project
    Date = format_datetime(timestamp, 'yyyy-MM-dd'),
    Difficulty,
    Result,
    AvgDurationMin = round(AvgDurationMs / 60000, 2),
    P50DurationMin = round(P50DurationMs / 60000, 2),
    P95DurationMin = round(P95DurationMs / 60000, 2)
| render timechart
```

**Use Case:** Monitor if game duration changes over time (indicates difficulty balance changes).

**Key Metrics:**
- Average game duration trends
- Percentile distributions
- Comparison across difficulties

---

### Query 9: Leaderboard Performance Metrics

This query monitors the performance of leaderboard loading.

```kql
// Leaderboard load performance over time
customMetrics
| where name == "LeaderboardLoadTime"
| where timestamp > ago(7d)
| extend Difficulty = tostring(customDimensions.Difficulty)
| summarize
    AvgLoadTime = avg(value),
    P95LoadTime = percentile(value, 95),
    P99LoadTime = percentile(value, 99),
    LoadCount = count()
    by bin(timestamp, 1h), Difficulty
| order by timestamp desc
| project
    Time = timestamp,
    Difficulty,
    AvgLoadTimeMs = round(AvgLoadTime, 2),
    P95LoadTimeMs = round(P95LoadTime, 2),
    P99LoadTimeMs = round(P99LoadTime, 2),
    LoadCount
| render timechart
```

**Use Case:** Track leaderboard performance and detect degradation.

**Key Metrics:**
- Load time percentiles
- Performance trends over time
- Volume of leaderboard requests

---

### Query 10: Health Check Monitoring

This query monitors the health check endpoint to detect service issues.

```kql
// Health check failures and trends
requests
| where timestamp > ago(24h)
| where name contains "health"
| extend IsHealthy = success
| summarize
    TotalChecks = count(),
    HealthyChecks = countif(IsHealthy == true),
    UnhealthyChecks = countif(IsHealthy == false),
    AvgResponseTime = avg(duration),
    P95ResponseTime = percentile(duration, 95)
    by bin(timestamp, 5m)
| extend HealthPercentage = round((todouble(HealthyChecks) / todouble(TotalChecks)) * 100, 2)
| order by timestamp desc
| project
    Time = timestamp,
    TotalChecks,
    HealthPercentage,
    AvgResponseTimeMs = round(AvgResponseTime, 2),
    P95ResponseTimeMs = round(P95ResponseTime, 2)
| render timechart
```

**Use Case:** Monitor overall service health and uptime.

**Key Metrics:**
- Health check success rate
- Response time trends
- Detect outages or degradation

---

## Advanced Queries

### Query 11: User Journey Analysis

This query maps the typical user flow through the application.

```kql
// User journey - events in sequence
union customEvents, requests, pageViews
| where timestamp > ago(24h)
| extend EventType = case(
    itemType == "customEvent", name,
    itemType == "request", strcat("API: ", operation_Name),
    itemType == "pageView", strcat("Page: ", name),
    "Other"
)
| extend SessionId = tostring(session_Id)
| where isnotempty(SessionId)
| summarize
    EventSequence = make_list(EventType, 100),
    EventTimes = make_list(timestamp, 100),
    EventCount = count()
    by SessionId
| where EventCount > 3  // Filter sessions with meaningful activity
| take 20
| project SessionId, EventCount, EventSequence
```

**Use Case:** Understand how users navigate the app and identify drop-off points.

---

### Query 12: Alerting - Error Rate Spike Detection

This query can be used for alerts to detect sudden error spikes.

```kql
// Detect error rate spikes (for alerting)
exceptions
| where timestamp > ago(1h)
| summarize
    ErrorCount = count()
    by bin(timestamp, 5m)
| extend
    PreviousErrorCount = prev(ErrorCount, 1),
    ErrorRateChange = (ErrorCount - prev(ErrorCount, 1)) / prev(ErrorCount, 1) * 100
| where ErrorRateChange > 50  // 50% increase in errors
| project
    Time = timestamp,
    ErrorCount,
    PreviousErrorCount,
    PercentIncrease = round(ErrorRateChange, 2)
| order by Time desc
```

**Use Case:** Set up alerts for abnormal error rate increases.

---

## How to Use These Queries

### In Azure Portal:
1. Navigate to your Application Insights resource
2. Click "Logs" in the left menu
3. Paste any query from this document
4. Click "Run" to execute
5. Export results or create dashboards

### Creating Dashboards:
1. Run a query
2. Click "Pin to dashboard"
3. Create visualizations for monitoring

### Setting Up Alerts:
1. Use Query 12 or similar
2. Click "New alert rule"
3. Configure thresholds and notifications

---

## Query Optimization Tips

1. **Time Range**: Always specify a time range with `where timestamp > ago(Xd)`
2. **Filtering**: Filter early in the query pipeline for better performance
3. **Aggregation**: Use `summarize` instead of retrieving raw data
4. **Sampling**: For high-volume queries, use `sample` or `take`
5. **Indexes**: Common fields like `timestamp`, `operation_Name`, and `name` are indexed

---

## Custom Dimensions Reference

### Events Tracked:
- **GameCompleted**: PlayerName, Difficulty, Result, GameTimeMs
- **LeaderboardViewed**: Difficulty, LoadTimeMs

### Metrics Tracked:
- **GameDuration**: Game completion time (Difficulty, Result dimensions)
- **LeaderboardLoadTime**: Time to load leaderboard (Difficulty dimension)

### Exception Context:
- **Operation**: The operation that failed
- **Source**: Client or Server
- **PlayerName**: User who experienced the error (when available)

---

## Additional Resources

- [KQL Quick Reference](https://docs.microsoft.com/azure/data-explorer/kql-quick-reference)
- [Application Insights Analytics](https://docs.microsoft.com/azure/azure-monitor/app/analytics)
- [Kusto Query Language Documentation](https://docs.microsoft.com/azure/data-explorer/kusto/query/)
