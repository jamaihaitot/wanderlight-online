using System;
using System.Linq;

using GdUnit4;

using WanderlightOnline;

namespace WanderlightOnline.Tests.Contract
{
    using static Assertions;

    [TestSuite]
    public class OperatorContractTests
    {
        [TestCase]
        public static void OperatorProvidesTelemetryMetrics()
        {
            var op = new Operator();

            // Simulate player joins and latency measurements
            op.RecordPlayerJoin("p1");
            op.RecordPlayerJoin("p2");
            op.RecordUpdateLatency(12.5);
            op.RecordUpdateLatency(37.5);

            var telemetry = op.CurrentTelemetry;
            AssertThat(telemetry.ConnectedPlayers).IsEqual(2);
            AssertThat(telemetry.PlayerJoinTimes.ContainsKey("p1")).IsTrue();
            AssertThat(telemetry.PlayerJoinTimes.ContainsKey("p2")).IsTrue();
            AssertThat(telemetry.AverageLatency).IsGreaterEqual(0.0);
            AssertThat(telemetry.P95Latency).IsGreaterEqual(0.0);

            op.Dispose();
        }

        [TestCase]
        public static void OperatorProvidesStructuredLogging()
        {
            var op = new Operator();

            op.LogInfo("Player connected", new System.Collections.Generic.Dictionary<string, object> { { "playerId", "p1" } });
            op.LogWarning("High memory usage", new System.Collections.Generic.Dictionary<string, object> { { "percent", 85 } });
            op.LogError("Database connection failed", new InvalidOperationException("db-timeout"));

            var logs = op.RecentLogs;
            AssertThat(logs.Count).IsGreater(0);
            AssertThat(logs.Any(l => l.Level == LogLevel.Info)).IsTrue();
            AssertThat(logs.Any(l => l.Level == LogLevel.Warning)).IsTrue();
            AssertThat(logs.Any(l => l.Level == LogLevel.Error)).IsTrue();
            AssertThat(logs.All(l => !string.IsNullOrEmpty(l.CorrelationId))).IsTrue();
            AssertThat(logs.All(l => l.Timestamp > DateTime.MinValue)).IsTrue();

            var error = logs.LastOrDefault(l => l.Level == LogLevel.Error);
            AssertThat(error).IsNotNull();
            AssertThat(error!.Context.ContainsKey("exception")).IsTrue();
            AssertThat(error.Context.ContainsKey("stackTrace")).IsTrue();

            op.Dispose();
        }

        [TestCase]
        public static void OperatorProvidesWorldManagement()
        {
            var op = new Operator();
            op.AddOperator("admin1", OperatorPermission.Admin);

            op.RecordPlayerJoin("p1");
            op.RecordPlayerJoin("p2");
            op.RecordUpdateLatency(25.5);

            var result = op.ResetWorld("admin1");
            AssertThat(result.Success).IsTrue();
            AssertThat(result.Message.IndexOf("reset", StringComparison.OrdinalIgnoreCase)).IsGreaterEqual(0);

            var telemetry = op.CurrentTelemetry;
            AssertThat(telemetry.ConnectedPlayers).IsEqual(0);
            AssertThat(telemetry.PlayerJoinTimes.Count).IsEqual(0);
            AssertThat(telemetry.UpdateLatencies.Count).IsEqual(0);

            var logs = op.RecentLogs;
            AssertThat(logs.Any(l => l.Message.Contains("World reset completed", StringComparison.OrdinalIgnoreCase))).IsTrue();

            op.Dispose();
        }

        [TestCase]
        public static void OperatorPermissionsEnforceAccessControl()
        {
            var op = new Operator();
            op.AddOperator("admin1", OperatorPermission.Admin);
            // 'readonly' operator is added by default in Operator constructor

            var adminResult = op.ResetWorld("admin1");
            var roResult = op.ResetWorld("readonly");
            var unknownResult = op.ResetWorld("unknown");

            AssertThat(adminResult.Success).IsTrue();
            AssertThat(roResult.Success).IsFalse();
            AssertThat(unknownResult.Success).IsFalse();

            var adminDashboard = op.GetDashboardData("admin1");
            var roDashboard = op.GetDashboardData("readonly");
            var unknownDashboard = op.GetDashboardData("unknown");

            AssertThat(adminDashboard).IsNotNull();
            AssertThat(roDashboard).IsNotNull();
            AssertThat(unknownDashboard).IsNull();

            op.Dispose();
        }

        [TestCase]
        public static void OperatorDashboardProvidesOverview()
        {
            var op = new Operator();
            op.AddOperator("admin1", OperatorPermission.Admin);

            op.RecordPlayerJoin("p1");
            op.RecordUpdateLatency(45.5);
            op.LogError("Test error");

            var dashboard = op.GetDashboardData("admin1");
            AssertThat(dashboard).IsNotNull();
            AssertThat(dashboard!.ContainsKey("connectedPlayers")).IsTrue();
            AssertThat(dashboard.ContainsKey("averageLatency")).IsTrue();
            AssertThat(dashboard.ContainsKey("p95Latency")).IsTrue();
            AssertThat(dashboard.ContainsKey("errorRate")).IsTrue();
            AssertThat(dashboard.ContainsKey("totalErrors")).IsTrue();
            AssertThat(dashboard.ContainsKey("historicalDataPoints")).IsTrue();
            AssertThat(dashboard.ContainsKey("lastUpdated")).IsTrue();

            op.Dispose();
        }

        [TestCase]
        public static void OperatorLogsContainRichErrorContext()
        {
            var op = new Operator();
            var ex = new ArgumentException("arg");
            op.LogError("Critical", ex, new System.Collections.Generic.Dictionary<string, object> { { "userId", "p1" } });

            var err = op.RecentLogs.LastOrDefault(l => l.Level == LogLevel.Error);
            AssertThat(err).IsNotNull();
            AssertThat(err!.Context.ContainsKey("exception")).IsTrue();
            AssertThat(err.Context.ContainsKey("stackTrace")).IsTrue();
            AssertThat(err.Context.ContainsKey("userId")).IsTrue();

            op.Dispose();
        }
    }
}