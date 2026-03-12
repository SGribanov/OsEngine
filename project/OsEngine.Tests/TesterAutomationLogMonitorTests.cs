#nullable enable

using System;
using System.IO;
using System.Linq;
using OsEngine.TesterAutomation.Runtime;

namespace OsEngine.Tests;

[Collection("FileSystemIsolation")]
public sealed class TesterAutomationLogMonitorTests
{
    [Fact]
    public void Poll_ShouldReturnOnlyNewLinesAcrossSnapshots()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-log-monitor-" + Guid.NewGuid());
        string logDirectory = Path.Combine(root, "Engine", "Log");
        string logPath = Path.Combine(logDirectory, "prime.txt");

        try
        {
            Directory.CreateDirectory(logDirectory);
            File.WriteAllLines(logPath, ["first", "second"]);

            LogMonitor monitor = new(logDirectory);

            var firstPoll = monitor.Poll();
            Assert.Equal(2, firstPoll.Count);
            Assert.Equal(["first", "second"], firstPoll.Select(static line => line.Text).ToArray());

            File.AppendAllLines(logPath, ["third"]);

            var secondPoll = monitor.Poll();
            Assert.Single(secondPoll);
            Assert.Equal("third", secondPoll[0].Text);
            Assert.Equal(3, secondPoll[0].LineNumber);
            Assert.Equal(["prime.txt"], monitor.GetTrackedFiles());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void Poll_ShouldTreatTruncatedFileAsFreshSnapshot()
    {
        string root = Path.Combine(Path.GetTempPath(), "osengine-log-monitor-" + Guid.NewGuid());
        string logDirectory = Path.Combine(root, "Engine", "Log");
        string logPath = Path.Combine(logDirectory, "prime.txt");

        try
        {
            Directory.CreateDirectory(logDirectory);
            File.WriteAllLines(logPath, ["one", "two"]);

            LogMonitor monitor = new(logDirectory);
            _ = monitor.Poll();

            File.WriteAllLines(logPath, ["reset"]);

            var nextPoll = monitor.Poll();
            Assert.Single(nextPoll);
            Assert.Equal("reset", nextPoll[0].Text);
            Assert.Equal(1, nextPoll[0].LineNumber);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
