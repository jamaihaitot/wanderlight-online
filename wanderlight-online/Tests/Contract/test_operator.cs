namespace WanderlightOnline.Tests.Contract
{
    using GdUnit4;
    using System;
    using static GdUnit4.Assertions;

    /// <summary>
    /// Contract tests for the Operator telemetry and logging system.
    /// </summary>
    [TestSuite]
    public class OperatorContractTests
    {
        private Operator operatorInstance = null!;

        /// <summary>
        /// Sets up the test environment before each test.
        /// </summary>
        [Before]
        public void Setup()
        {
            this.operatorInstance = new Operator();
            this.operatorInstance.AddOperator("test-op-1", OperatorPermission.Admin);
        }

        /// <summary>
        /// Cleans up resources after each test.
        /// </summary>
        [After]
        public void Cleanup()
        {
            this.operatorInstance?.Dispose();
        }

        /// <summary>
        /// Tests that operator collects telemetry on game events.
        /// </summary>
        [TestCase]
        public void T021_01_OperatorMustCollectTelemetryOnGameEvents()
        {
            // Arrange
            string playerId = "player-123";
            double latency = 45.5;

            // Act
            this.operatorInstance.RecordPlayerJoin(playerId);
            this.operatorInstance.RecordUpdateLatency(latency);

            // Assert
            var dashboardData = this.operatorInstance.GetDashboardData("test-op-1");
            AssertThat(dashboardData).IsNotNull();
            AssertThat((int)dashboardData!["connectedPlayers"]).IsEqual(1);
            AssertThat((double)dashboardData["averageLatency"]).IsLessEqual(latency);
        }

        /// <summary>
        /// Tests that operator logs errors with structured data.
        /// </summary>
        [TestCase]
        public void T021_02_OperatorMustLogErrorsWithStructuredData()
        {
            // Arrange
            string errorMessage = "Critical system failure";
            var exception = new InvalidOperationException("Test exception");

            // Act
            this.operatorInstance.LogError(errorMessage, exception);

            // Assert
            var dashboardData = this.operatorInstance.GetDashboardData("test-op-1");
            AssertThat(dashboardData).IsNotNull();
            AssertThat((int)dashboardData!["totalErrors"]).IsGreater(0);
        }

        /// <summary>
        /// Tests that operator can reset world with clean state.
        /// </summary>
        [TestCase]
        public void T021_03_OperatorMustResetWorldWithCleanState()
        {
            // Arrange
            string playerId = "player-456";
            this.operatorInstance.RecordPlayerJoin(playerId);

            // Act
            var result = this.operatorInstance.ResetWorld("test-op-1");

            // Assert
            AssertThat(result.Success).IsTrue();
            var dashboardData = this.operatorInstance.GetDashboardData("test-op-1");
            AssertThat(dashboardData).IsNotNull();
            AssertThat((int)dashboardData!["connectedPlayers"]).IsEqual(0);
        }

        /// <summary>
        /// Tests that operator provides real-time dashboard.
        /// </summary>
        [TestCase]
        public void T021_04_OperatorMustProvideRealtimeDashboard()
        {
            // Arrange
            this.operatorInstance.RecordPlayerJoin("player-1");
            this.operatorInstance.RecordPlayerJoin("player-2");

            // Act
            var dashboardData = this.operatorInstance.GetDashboardData("test-op-1");

            // Assert
            AssertThat(dashboardData).IsNotNull();
            AssertThat((int)dashboardData!["connectedPlayers"]).IsEqual(2);
            AssertThat((string)dashboardData["lastUpdated"]).IsNotEmpty();
        }

        /// <summary>
        /// Tests that operator includes error context in logs.
        /// </summary>
        [TestCase]
        public void T021_05_OperatorMustIncludeErrorContextInLogs()
        {
            // Arrange
            string infoMessage = "System initialized";
            string warningMessage = "Low memory warning";

            // Act
            this.operatorInstance.LogInfo(infoMessage);
            this.operatorInstance.LogWarning(warningMessage);

            // Assert
            var logs = this.operatorInstance.QueryLogs("test-op-1", limit: 10);
            AssertThat(logs).IsNotNull();
            AssertThat(logs!.Count).IsGreater(0);
        }

        /// <summary>
        /// Tests that operator enforces permission-based access.
        /// </summary>
        [TestCase]
        public void T021_06_OperatorMustEnforcePermissionBasedAccess()
        {
            // Arrange
            var readOnlyOp = new Operator();
            readOnlyOp.AddOperator("read-only-op", OperatorPermission.ReadOnly);

            // Act & Assert
            try
            {
                // ReadOnly operators should not be able to reset world
                var result = readOnlyOp.ResetWorld("read-only-op");
                AssertThat(result.Success).IsFalse();
                AssertThat(result.Message).Contains("permission");
            }
            finally
            {
                readOnlyOp.Dispose();
            }
        }
    }
}