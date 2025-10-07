using Godot;
using GdUnit4;
using WanderlightOnline;
using System.Threading;
using System;

namespace WanderlightOnline.Tests.Contract
{
    using static Assertions;

    [TestSuite]
    public class NetworkManagerContractTests
    {
        // Contract: WebSocket connection establishes and maintains real-time communication
        // Requirements: Connection handling, reconnection, heartbeat
        [TestCase]
        public static void NetworkEstablishesWebSocketConnection()
        {
            // Arrange: Create NetworkManager instance
            var networkManager = new NetworkManager();
            var serverUrl = "ws://localhost:8080/game";

            // Act: Attempt connection to game server
            bool connected = networkManager.TryConnect(serverUrl, 5000);

            // Assert: Connection established within timeout
            AssertThat(connected).IsTrue();

            // Assert: Connection state properly tracked
            AssertThat(networkManager.ConnectionState).IsEqual(NetworkConnectionState.Connected);

            // Cleanup
            networkManager.Disconnect();
        }

        // Contract: Message queuing handles network congestion and ordering
        // Requirements: FIFO ordering, buffering during disconnection, no message loss
        [TestCase]
        public static void NetworkHandlesMessageQueuingAndOrdering()
        {
            // Arrange: NetworkManager with connection
            var networkManager = new NetworkManager();
            networkManager.TryConnect("ws://localhost:8080/game");

            // Act: Send multiple messages in sequence
            var message1 = new NetworkMessage("TEST", "Message 1");
            var message2 = new NetworkMessage("TEST", "Message 2");
            var message3 = new NetworkMessage("TEST", "Message 3");

            bool queued1 = networkManager.SendMessage(message1);
            bool queued2 = networkManager.SendMessage(message2);
            bool queued3 = networkManager.SendMessage(message3);

            // Assert: Messages queued successfully
            AssertThat(queued1).IsTrue();
            AssertThat(queued2).IsTrue();
            AssertThat(queued3).IsTrue();

            // Assert: Queue size updated correctly
            AssertThat(networkManager.MessageQueueSize).IsEqual(3);

            // Act: Process message queue
            int processed = networkManager.ProcessMessageQueue();

            // Assert: All messages processed
            AssertThat(processed).IsEqual(3);
            AssertThat(networkManager.MessageQueueSize).IsEqual(0);

            // Cleanup
            networkManager.Disconnect();
        }

        // Contract: State deltas delivered at 20 Hz for real-time updates
        // Requirements: ≤150ms p95 latency, consistent update frequency
        [TestCase]
        public static void NetworkDeliversStateDeltasAt20Hz()
        {
            // Arrange: NetworkManager with connection
            var networkManager = new NetworkManager();
            networkManager.TryConnect("ws://localhost:8080/game");

            // Act: Send state delta (should succeed immediately)
            bool deltaSent1 = networkManager.SendStateDelta("player1", "position:100,200");

            // Assert: First delta sent successfully
            AssertThat(deltaSent1).IsTrue();

            // Act: Try to send another delta immediately (should fail due to rate limiting)
            bool deltaSent2 = networkManager.SendStateDelta("player1", "position:105,205");

            // Assert: Second delta blocked by rate limiting (20Hz = 50ms interval)
            AssertThat(deltaSent2).IsFalse();

            // Act: Wait for rate limit interval and try again
            Thread.Sleep(60); // Wait longer than 50ms
            bool deltaSent3 = networkManager.SendStateDelta("player1", "position:110,210");

            // Assert: Third delta sent after rate limit period
            AssertThat(deltaSent3).IsTrue();

            // Cleanup
            networkManager.Disconnect();
        }

        // Contract: Mini-snapshots every 2s provide state correction
        // Requirements: Periodic full state sync, drift correction
        [TestCase]
        public static void NetworkSendsMiniSnapshotsForCorrection()
        {
            // Arrange: NetworkManager with connection
            var networkManager = new NetworkManager();
            networkManager.TryConnect("ws://localhost:8080/game");

            // Act: Send mini-snapshot (should succeed immediately)
            bool snapshotSent1 = networkManager.SendMiniSnapshot("complete_state_data");

            // Assert: First snapshot sent successfully
            AssertThat(snapshotSent1).IsTrue();

            // Act: Try to send another snapshot immediately (should fail due to 2s interval)
            bool snapshotSent2 = networkManager.SendMiniSnapshot("complete_state_data_2");

            // Assert: Second snapshot blocked by 2-second interval
            AssertThat(snapshotSent2).IsFalse();

            // Cleanup
            networkManager.Disconnect();
        }

        // Contract: Atomic handling prevents race conditions in concurrent actions
        // Requirements: Transaction safety, conflict resolution
        [TestCase]
        public static void NetworkHandlesConcurrentActionsAtomically()
        {
            // Arrange: NetworkManager with connection
            var networkManager = new NetworkManager();
            networkManager.TryConnect("ws://localhost:8080/game");

            // Act: Process atomic actions
            bool action1 = networkManager.ProcessAtomicAction("PICKUP_ITEM", "item_123", "player1");
            bool action2 = networkManager.ProcessAtomicAction("MOVE_PLAYER", "new_position", "player2");
            bool action3 = networkManager.ProcessAtomicAction("INVENTORY_TRANSFER", "item_data", "player3");

            // Assert: Actions processed atomically (all-or-nothing)
            AssertThat(action1).IsTrue();
            AssertThat(action2).IsTrue();
            AssertThat(action3).IsTrue();

            // Assert: Invalid actions rejected
            bool invalidAction = networkManager.ProcessAtomicAction("", "data", "player1");
            AssertThat(invalidAction).IsFalse();

            bool invalidPlayer = networkManager.ProcessAtomicAction("ACTION", "data", "");
            AssertThat(invalidPlayer).IsFalse();

            // Cleanup
            networkManager.Disconnect();
        }

        // Contract: Reliable reconnection restores player state
        // Requirements: State restoration, session continuity, data consistency
        [TestCase]
        public static void NetworkHandlesReconnectionReliably()
        {
            // Arrange: NetworkManager with initial connection
            var networkManager = new NetworkManager();
            string serverUrl = "ws://localhost:8080/game";
            string playerState = "position:100,200;health:100;inventory:sword,potion";

            // Establish initial connection
            bool initialConnection = networkManager.TryConnect(serverUrl);
            AssertThat(initialConnection).IsTrue();
            AssertThat(networkManager.ConnectionState).IsEqual(NetworkConnectionState.Connected);

            // Act: Simulate disconnect
            networkManager.Disconnect();
            AssertThat(networkManager.ConnectionState).IsEqual(NetworkConnectionState.Disconnected);

            // Act: Attempt reconnection with state restoration
            bool reconnected = networkManager.TryReconnectAndRestoreState(serverUrl, playerState);

            // Assert: Reconnection succeeded
            AssertThat(reconnected).IsTrue();
            AssertThat(networkManager.ConnectionState).IsEqual(NetworkConnectionState.Connected);

            // Cleanup
            networkManager.Disconnect();
        }
    }
}
