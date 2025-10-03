// <copyright file="NetworkManager.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>
// NetworkManager.cs

// NOTE: GdUnit4 C# Limitation (2025-10-03):
// GdUnit4 for C# does not support direct assertions on primitive int values (e.g., assert_int, AssertThat(int)).
// Attempts to use AssertThat((int)value).IsEqual(expected) will fail with "ObjectAssert initial error: current is primitive <System.Int32>".
// Workarounds (boxing, .Equals, casting) do not resolve this. This is a known framework limitation and blocks certain model tests.
// See DatabaseManager model tests for details. Consider tracking upstream or requesting feature support.
namespace WanderlightOnline
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Godot;

    /// <summary>
    /// Represents the connection state of the network.
    /// </summary>
    public enum NetworkConnectionState
    {
        /// <summary>Not connected.</summary>
        Disconnected,

        /// <summary>Currently connecting.</summary>
        Connecting,

        /// <summary>Connected and active.</summary>
        Connected,

        /// <summary>Connection lost, attempting to reconnect.</summary>
        Reconnecting,
    }

    /// <summary>
    /// Represents a network message with metadata.
    /// </summary>
    public class NetworkMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkMessage"/> class.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="data">The message data.</param>
        public NetworkMessage(string type, string data)
        {
            this.Type = type;
            this.Data = data;
            this.Timestamp = DateTime.UtcNow;
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the message type.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the message data.
        /// </summary>
        public string Data { get; }

        /// <summary>
        /// Gets the timestamp when the message was created.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the unique identifier for this message.
        /// </summary>
        public Guid Id { get; }
    }

    /// <summary>
    /// Represents a state delta update.
    /// </summary>
    public class StateDelta
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateDelta"/> class.
        /// </summary>
        /// <param name="playerId">The player ID.</param>
        /// <param name="deltaData">The delta data.</param>
        public StateDelta(string playerId, string deltaData)
        {
            this.PlayerId = playerId;
            this.DeltaData = deltaData;
            this.Timestamp = DateTime.UtcNow;
            this.SequenceNumber = Interlocked.Increment(ref sequenceCounter);
        }

        private static long sequenceCounter = 0;

        /// <summary>
        /// Gets the player ID.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Gets the delta data.
        /// </summary>
        public string DeltaData { get; }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the sequence number for ordering.
        /// </summary>
        public long SequenceNumber { get; }
    }

    /// <summary>
    /// NetworkManager: Handles WebSocket connections, message queuing, state deltas, and synchronization.
    /// Provides real-time networking with 20Hz updates and atomic action handling.
    /// </summary>
    public class NetworkManager
    {
        private const int MessageQueueLimit = 10000; // Memory limit for message queue
        private const int DeltaUpdateIntervalMs = 50; // 20Hz = 50ms intervals
        private const int MiniSnapshotIntervalMs = 2000; // 2 seconds
        private const int ConnectionTimeoutMs = 30000; // 30 seconds
        private const int MaxLatencyMs = 150; // 150ms p95 target

        private NetworkConnectionState connectionState = NetworkConnectionState.Disconnected;
        private readonly Queue<NetworkMessage> messageQueue = new Queue<NetworkMessage>();
        private readonly Dictionary<string, DateTime> messageSentTimes = new Dictionary<string, DateTime>();
        private readonly List<StateDelta> stateDeltaHistory = new List<StateDelta>();
        private readonly object lockObject = new object();

        private DateTime lastHeartbeat = DateTime.MinValue;
        private DateTime lastDeltaUpdate = DateTime.MinValue;
        private DateTime lastMiniSnapshot = DateTime.MinValue;
        private long lastProcessedSequence = 0;
        private bool isReconnecting = false;

        // Statistics for monitoring
        private readonly List<double> updateLatencies = new List<double>();
        private int totalMessagesSent = 0;
        private int totalMessagesReceived = 0;
        private int messagesLost = 0;

        /// <summary>
        /// Gets the current connection state.
        /// </summary>
        public NetworkConnectionState ConnectionState => this.connectionState;

        /// <summary>
        /// Gets the current message queue size.
        /// </summary>
        public int MessageQueueSize
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.messageQueue.Count;
                }
            }
        }

        /// <summary>
        /// Gets the 95th percentile latency in milliseconds.
        /// </summary>
        public double P95LatencyMs
        {
            get
            {
                lock (this.lockObject)
                {
                    if (this.updateLatencies.Count == 0)
                    {
                        return 0;
                    }

                    var sorted = new List<double>(this.updateLatencies);
                    sorted.Sort();
                    int p95Index = (int)(sorted.Count * 0.95);
                    return sorted[Math.Min(p95Index, sorted.Count - 1)];
                }
            }
        }

        /// <summary>
        /// Gets the total messages sent.
        /// </summary>
        public int TotalMessagesSent => this.totalMessagesSent;

        /// <summary>
        /// Gets the total messages received.
        /// </summary>
        public int TotalMessagesReceived => this.totalMessagesReceived;

        /// <summary>
        /// Gets the number of messages lost.
        /// </summary>
        public int MessagesLost => this.messagesLost;

        /// <summary>
        /// Attempts to establish a WebSocket connection to the game server.
        /// </summary>
        /// <param name="serverUrl">The server URL to connect to.</param>
        /// <param name="timeoutMs">Connection timeout in milliseconds.</param>
        /// <returns>True if connection established, false otherwise.</returns>
        public bool TryConnect(string serverUrl, int timeoutMs = ConnectionTimeoutMs)
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                return false;
            }

            lock (this.lockObject)
            {
                if (this.connectionState == NetworkConnectionState.Connected)
                {
                    return true; // Already connected
                }

                this.connectionState = NetworkConnectionState.Connecting;
            }

            try
            {
                // Simulate connection establishment
                // In a real implementation, this would use Godot's WebSocket client
                Thread.Sleep(100); // Simulate connection delay

                lock (this.lockObject)
                {
                    this.connectionState = NetworkConnectionState.Connected;
                    this.lastHeartbeat = DateTime.UtcNow;
                    // Don't reset delta/snapshot timestamps on connect to allow immediate sends
                }

                return true;
            }
            catch
            {
                lock (this.lockObject)
                {
                    this.connectionState = NetworkConnectionState.Disconnected;
                }

                return false;
            }
        }

        /// <summary>
        /// Disconnects from the server and cleans up resources.
        /// </summary>
        public void Disconnect()
        {
            lock (this.lockObject)
            {
                this.connectionState = NetworkConnectionState.Disconnected;
                this.isReconnecting = false;
            }
        }

        /// <summary>
        /// Sends a message to the server with FIFO ordering guarantees.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>True if message was queued successfully, false otherwise.</returns>
        public bool SendMessage(NetworkMessage message)
        {
            if (message == null)
            {
                return false;
            }

            lock (this.lockObject)
            {
                // Check queue size limit
                if (this.messageQueue.Count >= MessageQueueLimit)
                {
                    return false; // Queue full
                }

                // Add to queue for FIFO ordering
                this.messageQueue.Enqueue(message);
                this.messageSentTimes[message.Id.ToString()] = DateTime.UtcNow;

                return true;
            }
        }

        /// <summary>
        /// Processes queued messages and sends them in FIFO order.
        /// </summary>
        /// <returns>Number of messages processed.</returns>
        public int ProcessMessageQueue()
        {
            int processed = 0;
            var messagesToSend = new List<NetworkMessage>();

            lock (this.lockObject)
            {
                // Only process if connected
                if (this.connectionState != NetworkConnectionState.Connected)
                {
                    return 0;
                }

                // Dequeue messages for processing
                while (this.messageQueue.Count > 0)
                {
                    messagesToSend.Add(this.messageQueue.Dequeue());
                }
            }

            // Send messages outside lock to avoid blocking
            foreach (var message in messagesToSend)
            {
                if (this.SendMessageInternal(message))
                {
                    processed++;
                    Interlocked.Increment(ref this.totalMessagesSent);
                }
                else
                {
                    // Re-queue failed message
                    lock (this.lockObject)
                    {
                        var tempQueue = new Queue<NetworkMessage>();
                        tempQueue.Enqueue(message);
                        while (this.messageQueue.Count > 0)
                        {
                            tempQueue.Enqueue(this.messageQueue.Dequeue());
                        }

                        while (tempQueue.Count > 0)
                        {
                            this.messageQueue.Enqueue(tempQueue.Dequeue());
                        }
                    }

                    break; // Stop processing on first failure
                }
            }

            return processed;
        }

        /// <summary>
        /// Sends a state delta update at 20Hz frequency.
        /// </summary>
        /// <param name="playerId">The player ID.</param>
        /// <param name="deltaData">The delta data.</param>
        /// <returns>True if delta was sent, false otherwise.</returns>
        public bool SendStateDelta(string playerId, string deltaData)
        {
            if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(deltaData))
            {
                return false;
            }

            var now = DateTime.UtcNow;

            lock (this.lockObject)
            {
                var timeSinceLastDelta = now - this.lastDeltaUpdate;

                // Enforce 20Hz rate limit (50ms minimum interval) - but allow first call
                if (this.lastDeltaUpdate != DateTime.MinValue && timeSinceLastDelta.TotalMilliseconds < DeltaUpdateIntervalMs)
                {
                    return false; // Too soon for next delta
                }

                var delta = new StateDelta(playerId, deltaData);

                // Check for duplicate or out-of-order deltas
                if (delta.SequenceNumber <= this.lastProcessedSequence)
                {
                    return false; // Out of order
                }

                // Send delta as network message
                var message = new NetworkMessage("STATE_DELTA", $"{playerId}:{deltaData}");
                if (this.SendMessage(message))
                {
                    this.stateDeltaHistory.Add(delta);
                    this.lastProcessedSequence = delta.SequenceNumber;
                    this.lastDeltaUpdate = now;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Sends a mini-snapshot for state correction every 2 seconds.
        /// </summary>
        /// <param name="completeState">The complete state data.</param>
        /// <returns>True if snapshot was sent, false otherwise.</returns>
        public bool SendMiniSnapshot(string completeState)
        {
            if (string.IsNullOrEmpty(completeState))
            {
                return false;
            }

            var now = DateTime.UtcNow;

            lock (this.lockObject)
            {
                var timeSinceLastSnapshot = now - this.lastMiniSnapshot;

                // Enforce 2-second interval - but allow first call
                if (this.lastMiniSnapshot != DateTime.MinValue && timeSinceLastSnapshot.TotalMilliseconds < MiniSnapshotIntervalMs)
                {
                    return false; // Too soon for next snapshot
                }

                var message = new NetworkMessage("MINI_SNAPSHOT", completeState);
                if (this.SendMessage(message))
                {
                    this.lastMiniSnapshot = now;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Processes an action atomically to prevent race conditions.
        /// </summary>
        /// <param name="actionType">The type of action.</param>
        /// <param name="actionData">The action data.</param>
        /// <param name="playerId">The player ID performing the action.</param>
        /// <returns>True if action was processed atomically, false otherwise.</returns>
        public bool ProcessAtomicAction(string actionType, string actionData, string playerId)
        {
            if (string.IsNullOrEmpty(actionType) || string.IsNullOrEmpty(playerId))
            {
                return false;
            }

            lock (this.lockObject)
            {
                // Ensure connection is stable for atomic operations
                if (this.connectionState != NetworkConnectionState.Connected)
                {
                    return false;
                }

                // Simulate atomic processing (all-or-nothing)
                try
                {
                    // In a real implementation, this would:
                    // 1. Validate the action against current state
                    // 2. Apply the action if valid
                    // 3. Update state atomically
                    // 4. Send confirmation to all affected clients

                    var message = new NetworkMessage("ATOMIC_ACTION", $"{actionType}:{playerId}:{actionData}");
                    return this.SendMessage(message);
                }
                catch
                {
                    return false; // Action failed, no partial state changes
                }
            }
        }

        /// <summary>
        /// Attempts to reconnect and restore player state.
        /// </summary>
        /// <param name="serverUrl">The server URL to reconnect to.</param>
        /// <param name="playerState">The player state to restore.</param>
        /// <returns>True if reconnection and state restoration succeeded, false otherwise.</returns>
        public bool TryReconnectAndRestoreState(string serverUrl, string playerState)
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                return false;
            }

            lock (this.lockObject)
            {
                if (this.isReconnecting)
                {
                    return false; // Already attempting reconnection
                }

                this.isReconnecting = true;
                this.connectionState = NetworkConnectionState.Reconnecting;
            }

            try
            {
                // Attempt to reconnect
                if (!this.TryConnect(serverUrl))
                {
                    lock (this.lockObject)
                    {
                        this.isReconnecting = false;
                        this.connectionState = NetworkConnectionState.Disconnected;
                    }

                    return false;
                }

                // Restore state if provided
                if (!string.IsNullOrEmpty(playerState))
                {
                    var restoreMessage = new NetworkMessage("RESTORE_STATE", playerState);
                    if (!this.SendMessage(restoreMessage))
                    {
                        return false;
                    }
                }

                lock (this.lockObject)
                {
                    this.isReconnecting = false;
                }

                return true;
            }
            catch
            {
                lock (this.lockObject)
                {
                    this.isReconnecting = false;
                    this.connectionState = NetworkConnectionState.Disconnected;
                }

                return false;
            }
        }

        /// <summary>
        /// Updates network statistics and performs maintenance tasks.
        /// Should be called regularly (e.g., in game loop).
        /// </summary>
        public void Update()
        {
            var now = DateTime.UtcNow;

            lock (this.lockObject)
            {
                // Check for connection timeout
                if (this.connectionState == NetworkConnectionState.Connected)
                {
                    var timeSinceHeartbeat = now - this.lastHeartbeat;
                    if (timeSinceHeartbeat.TotalMilliseconds > ConnectionTimeoutMs)
                    {
                        this.connectionState = NetworkConnectionState.Disconnected;
                    }
                }

                // Clean up old latency measurements (keep last 1000)
                if (this.updateLatencies.Count > 1000)
                {
                    this.updateLatencies.RemoveRange(0, this.updateLatencies.Count - 1000);
                }

                // Clean up old message send times (keep last 1000)
                if (this.messageSentTimes.Count > 1000)
                {
                    var oldestEntries = new List<string>();
                    int count = 0;
                    foreach (var kvp in this.messageSentTimes)
                    {
                        if (count++ > 1000)
                        {
                            oldestEntries.Add(kvp.Key);
                        }
                    }

                    foreach (var key in oldestEntries)
                    {
                        this.messageSentTimes.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Simulates message reception and updates latency statistics.
        /// </summary>
        /// <param name="message">The received message.</param>
        public void OnMessageReceived(NetworkMessage message)
        {
            if (message == null)
            {
                return;
            }

            lock (this.lockObject)
            {
                Interlocked.Increment(ref this.totalMessagesReceived);

                // Calculate latency if we have send time
                if (this.messageSentTimes.TryGetValue(message.Id.ToString(), out DateTime sentTime))
                {
                    var latency = (DateTime.UtcNow - sentTime).TotalMilliseconds;
                    this.updateLatencies.Add(latency);
                    this.messageSentTimes.Remove(message.Id.ToString());
                }
            }
        }

        /// <summary>
        /// Internal method to actually send a message over the network.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>True if message was sent successfully, false otherwise.</returns>
        private bool SendMessageInternal(NetworkMessage message)
        {
            if (message == null)
            {
                return false;
            }

            try
            {
                // Simulate network send operation
                // In a real implementation, this would use WebSocket to send data
                Thread.Sleep(1); // Simulate network delay
                return true; // Simulate successful send
            }
            catch
            {
                return false; // Simulate network failure
            }
        }
    }
}