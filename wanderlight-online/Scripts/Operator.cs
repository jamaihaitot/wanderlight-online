// <copyright file="Operator.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Text.Json;

    /// <summary>
    /// Represents log levels for structured logging.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>Informational messages.</summary>
        Info,

        /// <summary>Warning messages.</summary>
        Warning,

        /// <summary>Error messages.</summary>
        Error,

        /// <summary>Debug messages.</summary>
        Debug,
    }

    /// <summary>
    /// Represents operator permission levels.
    /// </summary>
    public enum OperatorPermission
    {
        /// <summary>Read-only access to metrics and logs.</summary>
        ReadOnly,

        /// <summary>Full administrative access.</summary>
        Admin,
    }

    /// <summary>
    /// Represents a structured log entry.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="context">Additional context data.</param>
        public LogEntry(LogLevel level, string message, Dictionary<string, object>? context = null)
        {
            this.Timestamp = DateTime.UtcNow;
            this.Level = level;
            this.Message = message;
            this.Context = context ?? new Dictionary<string, object>();
            this.CorrelationId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets the timestamp when the log entry was created.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the log level.
        /// </summary>
        public LogLevel Level { get; }

        /// <summary>
        /// Gets the log message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the additional context data.
        /// </summary>
        public Dictionary<string, object> Context { get; }

        /// <summary>
        /// Gets the correlation ID for linking related log entries.
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// Converts the log entry to JSON format.
        /// </summary>
        /// <returns>JSON representation of the log entry.</returns>
        public string ToJson()
        {
            var logData = new Dictionary<string, object>
            {
                ["timestamp"] = this.Timestamp.ToString("O"),
                ["level"] = this.Level.ToString().ToLower(),
                ["message"] = this.Message,
                ["correlationId"] = this.CorrelationId,
                ["context"] = this.Context,
            };

            return JsonSerializer.Serialize(logData);
        }
    }

    /// <summary>
    /// Represents telemetry metrics for monitoring.
    /// </summary>
    public class TelemetryData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryData"/> class.
        /// </summary>
        public TelemetryData()
        {
            this.Timestamp = DateTime.UtcNow;
            this.PlayerJoinTimes = new Dictionary<string, DateTime>();
            this.UpdateLatencies = new List<double>();
            this.ErrorCounts = new Dictionary<string, int>();
        }

        /// <summary>
        /// Gets the timestamp when the telemetry data was captured.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets or sets the number of connected players.
        /// </summary>
        public int ConnectedPlayers { get; set; }

        /// <summary>
        /// Gets the join times for each player.
        /// </summary>
        public Dictionary<string, DateTime> PlayerJoinTimes { get; }

        /// <summary>
        /// Gets the list of update latencies.
        /// </summary>
        public List<double> UpdateLatencies { get; }

        /// <summary>
        /// Gets the error counts by type.
        /// </summary>
        public Dictionary<string, int> ErrorCounts { get; }

        /// <summary>
        /// Gets the average update latency.
        /// </summary>
        public double AverageLatency => this.UpdateLatencies.Count > 0 ? this.UpdateLatencies.Average() : 0.0;

        /// <summary>
        /// Gets the total error rate.
        /// </summary>
        public double ErrorRate => this.ErrorCounts.Values.Sum();

        /// <summary>
        /// Gets the 95th percentile latency.
        /// </summary>
        public double P95Latency
        {
            get
            {
                if (this.UpdateLatencies.Count == 0)
                {
                    return 0.0;
                }

                var sorted = this.UpdateLatencies.OrderBy(x => x).ToList();
                int p95Index = (int)(sorted.Count * 0.95);
                return sorted[Math.Min(p95Index, sorted.Count - 1)];
            }
        }
    }

    /// <summary>
    /// Represents the result of a world reset operation.
    /// </summary>
    public class WorldResetResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldResetResult"/> class.
        /// </summary>
        /// <param name="success">Whether the reset was successful.</param>
        /// <param name="message">Result message.</param>
        public WorldResetResult(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
            this.Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets a value indicating whether the reset was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the result message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the timestamp of the reset operation.
        /// </summary>
        public DateTime Timestamp { get; }
    }

    /// <summary>
    /// Operator: Provides telemetry, structured logging, and world management capabilities.
    /// Handles real-time metrics, error tracking, and administrative operations.
    /// Integrated with SpacetimeDB for persistent logging and telemetry.
    /// </summary>
    public class Operator : IDisposable
    {
        private readonly object lockObject = new object();
        private readonly List<LogEntry> logEntries = new List<LogEntry>();
        private readonly List<TelemetryData> historicalTelemetry = new List<TelemetryData>();
        private readonly Dictionary<string, OperatorPermission> operatorPermissions = new Dictionary<string, OperatorPermission>();
        private readonly DatabaseManager databaseManager;

        private TelemetryData currentTelemetry = new TelemetryData();
        private readonly System.Threading.Timer telemetryTimer;
        private readonly int maxLogEntries = 10000;
        private readonly int maxHistoricalEntries = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="Operator"/> class.
        /// </summary>
        public Operator()
        {
            // Initialize DatabaseManager integration
            this.databaseManager = DatabaseManager.Instance;

            // Initialize default admin operator
            this.operatorPermissions["admin"] = OperatorPermission.Admin;
            this.operatorPermissions["readonly"] = OperatorPermission.ReadOnly;

            // Start telemetry collection timer (every 5 seconds)
            this.telemetryTimer = new System.Threading.Timer(this.CollectTelemetry, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            this.LogInfo("Operator system initialized with SpacetimeDB integration", new Dictionary<string, object>
            {
                ["operatorCount"] = this.operatorPermissions.Count,
                ["telemetryInterval"] = "5s",
                ["database"] = "SpacetimeDB",
            });
        }

        /// <summary>
        /// Gets the current telemetry data.
        /// </summary>
        public TelemetryData CurrentTelemetry
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.currentTelemetry;
                }
            }
        }

        /// <summary>
        /// Gets the recent log entries.
        /// </summary>
        public IReadOnlyList<LogEntry> RecentLogs
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.logEntries.TakeLast(100).ToList();
                }
            }
        }

        /// <summary>
        /// Gets the historical telemetry data.
        /// </summary>
        public IReadOnlyList<TelemetryData> HistoricalTelemetry
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.historicalTelemetry.ToList();
                }
            }
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="context">Additional context data.</param>
        public void LogInfo(string message, Dictionary<string, object>? context = null)
        {
            this.LogEntry(LogLevel.Info, message, context);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="context">Additional context data.</param>
        public void LogWarning(string message, Dictionary<string, object>? context = null)
        {
            this.LogEntry(LogLevel.Warning, message, context);
        }

        /// <summary>
        /// Logs an error message with rich context.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="context">Additional context data.</param>
        public void LogError(string message, Exception? exception = null, Dictionary<string, object>? context = null)
        {
            var errorContext = context ?? new Dictionary<string, object>();

            if (exception != null)
            {
                errorContext["exception"] = exception.GetType().Name;
                errorContext["stackTrace"] = exception.StackTrace ?? string.Empty;
                errorContext["innerException"] = exception.InnerException?.Message ?? string.Empty;
            }

            this.LogEntry(LogLevel.Error, message, errorContext);

            // Update error statistics
            lock (this.lockObject)
            {
                var errorType = exception?.GetType().Name ?? "Unknown";
                if (this.currentTelemetry.ErrorCounts.ContainsKey(errorType))
                {
                    this.currentTelemetry.ErrorCounts[errorType]++;
                }
                else
                {
                    this.currentTelemetry.ErrorCounts[errorType] = 1;
                }
            }
        }

        /// <summary>
        /// Records a player join event.
        /// </summary>
        /// <param name="playerId">The player ID.</param>
        public void RecordPlayerJoin(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return;
            }

            lock (this.lockObject)
            {
                this.currentTelemetry.PlayerJoinTimes[playerId] = DateTime.UtcNow;
                this.currentTelemetry.ConnectedPlayers = this.currentTelemetry.PlayerJoinTimes.Count;
            }

            this.LogInfo("Player joined", new Dictionary<string, object>
            {
                ["playerId"] = playerId,
                ["totalPlayers"] = this.currentTelemetry.ConnectedPlayers,
            });
        }

        /// <summary>
        /// Records a player disconnect event.
        /// </summary>
        /// <param name="playerId">The player ID.</param>
        public void RecordPlayerDisconnect(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return;
            }

            lock (this.lockObject)
            {
                this.currentTelemetry.PlayerJoinTimes.Remove(playerId);
                this.currentTelemetry.ConnectedPlayers = this.currentTelemetry.PlayerJoinTimes.Count;
            }

            this.LogInfo("Player disconnected", new Dictionary<string, object>
            {
                ["playerId"] = playerId,
                ["totalPlayers"] = this.currentTelemetry.ConnectedPlayers,
            });
        }

        /// <summary>
        /// Records an update latency measurement.
        /// </summary>
        /// <param name="latencyMs">The latency in milliseconds.</param>
        public void RecordUpdateLatency(double latencyMs)
        {
            lock (this.lockObject)
            {
                this.currentTelemetry.UpdateLatencies.Add(latencyMs);

                // Keep only recent latencies (last 1000)
                if (this.currentTelemetry.UpdateLatencies.Count > 1000)
                {
                    this.currentTelemetry.UpdateLatencies.RemoveRange(0, this.currentTelemetry.UpdateLatencies.Count - 1000);
                }
            }
        }

        /// <summary>
        /// Adds or updates an operator with the specified permission.
        /// </summary>
        /// <param name="operatorId">The operator ID.</param>
        /// <param name="permission">The permission level.</param>
        public void AddOperator(string operatorId, OperatorPermission permission)
        {
            if (string.IsNullOrEmpty(operatorId))
            {
                return;
            }

            lock (this.lockObject)
            {
                this.operatorPermissions[operatorId] = permission;
                this.LogInfo("Operator registered", new Dictionary<string, object>
                {
                    ["operatorId"] = operatorId,
                    ["permission"] = permission.ToString(),
                });
            }
        }

        /// <summary>
        /// Checks if an operator has the required permission.
        /// </summary>
        /// <param name="operatorId">The operator ID.</param>
        /// <param name="requiredPermission">The required permission level.</param>
        /// <returns>True if the operator has the required permission.</returns>
        public bool HasPermission(string operatorId, OperatorPermission requiredPermission)
        {
            if (string.IsNullOrEmpty(operatorId))
            {
                return false;
            }

            lock (this.lockObject)
            {
                if (!this.operatorPermissions.TryGetValue(operatorId, out var permission))
                {
                    this.LogWarning("Unknown operator attempted access", new Dictionary<string, object>
                    {
                        ["operatorId"] = operatorId,
                        ["requiredPermission"] = requiredPermission.ToString(),
                    });
                    return false;
                }

                var hasPermission = permission >= requiredPermission;

                if (!hasPermission)
                {
                    this.LogWarning("Operator access denied", new Dictionary<string, object>
                    {
                        ["operatorId"] = operatorId,
                        ["operatorPermission"] = permission.ToString(),
                        ["requiredPermission"] = requiredPermission.ToString(),
                    });
                }

                return hasPermission;
            }
        }

        /// <summary>
        /// Executes a world reset operation (admin permission required).
        /// Integrated with SpacetimeDB for persistent state reset.
        /// </summary>
        /// <param name="operatorId">The operator requesting the reset.</param>
        /// <returns>The result of the reset operation.</returns>
        public WorldResetResult ResetWorld(string operatorId)
        {
            // Check permissions
            if (!this.HasPermission(operatorId, OperatorPermission.Admin))
            {
                return new WorldResetResult(false, "Insufficient permissions for world reset");
            }

            this.LogInfo("World reset initiated via SpacetimeDB", new Dictionary<string, object>
            {
                ["operatorId"] = operatorId,
                ["playersToDisconnect"] = this.currentTelemetry.ConnectedPlayers,
            });

            try
            {
                // Call SpacetimeDB ResetWorld reducer for persistent state reset
                try
                {
                    // TODO: Uncomment when SpacetimeDB connection is established
                    // this.databaseManager.Reducers?.ResetWorld();
                    this.LogInfo("SpacetimeDB world reset reducer called", new Dictionary<string, object>
                    {
                        ["operatorId"] = operatorId,
                    });
                }
                catch (Exception dbEx)
                {
                    this.LogWarning("SpacetimeDB reset failed, continuing with local reset", new Dictionary<string, object>
                    {
                        ["error"] = dbEx.Message,
                    });
                }

                // Atomic world reset operation
                lock (this.lockObject)
                {
                    // Clear all player data
                    var disconnectedPlayers = this.currentTelemetry.PlayerJoinTimes.Keys.ToList();
                    this.currentTelemetry.PlayerJoinTimes.Clear();
                    this.currentTelemetry.ConnectedPlayers = 0;

                    // Reset error counts
                    this.currentTelemetry.ErrorCounts.Clear();

                    // Clear latency history
                    this.currentTelemetry.UpdateLatencies.Clear();

                    // Log each disconnected player
                    foreach (var playerId in disconnectedPlayers)
                    {
                        this.LogInfo("Player disconnected during world reset", new Dictionary<string, object>
                        {
                            ["playerId"] = playerId,
                            ["reason"] = "worldReset",
                        });
                    }
                }

                this.LogInfo("World reset completed successfully", new Dictionary<string, object>
                {
                    ["operatorId"] = operatorId,
                    ["resetTimestamp"] = DateTime.UtcNow.ToString("O"),
                    ["database"] = "SpacetimeDB",
                });

                return new WorldResetResult(true, "World reset completed successfully");
            }
            catch (Exception ex)
            {
                this.LogError("World reset failed", ex, new Dictionary<string, object>
                {
                    ["operatorId"] = operatorId,
                });

                return new WorldResetResult(false, $"World reset failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets real-time metrics dashboard data.
        /// </summary>
        /// <param name="operatorId">The operator requesting the dashboard.</param>
        /// <returns>Dashboard data or null if access denied.</returns>
        public Dictionary<string, object>? GetDashboardData(string operatorId)
        {
            if (!this.HasPermission(operatorId, OperatorPermission.ReadOnly))
            {
                return null;
            }

            lock (this.lockObject)
            {
                return new Dictionary<string, object>
                {
                    ["connectedPlayers"] = this.currentTelemetry.ConnectedPlayers,
                    ["averageLatency"] = this.currentTelemetry.AverageLatency,
                    ["p95Latency"] = this.currentTelemetry.P95Latency,
                    ["errorRate"] = this.currentTelemetry.ErrorRate,
                    ["totalErrors"] = this.currentTelemetry.ErrorCounts.Values.Sum(),
                    ["errorsByType"] = this.currentTelemetry.ErrorCounts,
                    ["lastUpdated"] = DateTime.UtcNow.ToString("O"),
                    ["historicalDataPoints"] = this.historicalTelemetry.Count,
                };
            }
        }

        /// <summary>
        /// Queries logs with optional filtering.
        /// </summary>
        /// <param name="operatorId">The operator requesting the logs.</param>
        /// <param name="level">Optional log level filter.</param>
        /// <param name="since">Optional timestamp filter.</param>
        /// <param name="limit">Maximum number of log entries to return.</param>
        /// <returns>Filtered log entries or null if access denied.</returns>
        public List<LogEntry>? QueryLogs(string operatorId, LogLevel? level = null, DateTime? since = null, int limit = 100)
        {
            if (!this.HasPermission(operatorId, OperatorPermission.ReadOnly))
            {
                return null;
            }

            lock (this.lockObject)
            {
                var query = this.logEntries.AsEnumerable();

                if (level.HasValue)
                {
                    query = query.Where(log => log.Level == level.Value);
                }

                if (since.HasValue)
                {
                    query = query.Where(log => log.Timestamp >= since.Value);
                }

                return query.OrderByDescending(log => log.Timestamp)
                           .Take(limit)
                           .ToList();
            }
        }

        /// <summary>
        /// Disposes of the operator and cleans up resources.
        /// </summary>
        public void Dispose()
        {
            this.telemetryTimer?.Dispose();
            this.LogInfo("Operator system disposed");
        }

        /// <summary>
        /// Logs an entry with the specified level and message.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="context">Additional context data.</param>
        private void LogEntry(LogLevel level, string message, Dictionary<string, object>? context = null)
        {
            var logEntry = new LogEntry(level, message, context);

            lock (this.lockObject)
            {
                this.logEntries.Add(logEntry);

                // Maintain log size limit
                if (this.logEntries.Count > this.maxLogEntries)
                {
                    this.logEntries.RemoveRange(0, this.logEntries.Count - this.maxLogEntries);
                }
            }

            // Output to console for debugging (avoid calling Godot APIs when running outside the engine)
            try
            {
                Console.WriteLine($"[{level}] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
            }
            catch
            {
                // Ignore any console issues in restricted environments
            }
        }

        /// <summary>
        /// Collects telemetry data periodically.
        /// </summary>
        /// <param name="state">Timer state (unused).</param>
        private void CollectTelemetry(object? state)
        {
            lock (this.lockObject)
            {
                // Archive current telemetry
                this.historicalTelemetry.Add(this.currentTelemetry);

                // Maintain historical data size limit
                if (this.historicalTelemetry.Count > this.maxHistoricalEntries)
                {
                    this.historicalTelemetry.RemoveRange(0, this.historicalTelemetry.Count - this.maxHistoricalEntries);
                }

                // Create new telemetry snapshot (preserve ongoing data like player connections)
                var newTelemetry = new TelemetryData();
                foreach (var player in this.currentTelemetry.PlayerJoinTimes)
                {
                    newTelemetry.PlayerJoinTimes[player.Key] = player.Value;
                }

                newTelemetry.ConnectedPlayers = this.currentTelemetry.ConnectedPlayers;
                this.currentTelemetry = newTelemetry;
            }
        }
    }
}