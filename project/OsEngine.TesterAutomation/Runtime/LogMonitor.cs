using System.IO;
using OsEngine.TesterAutomation.Models;

namespace OsEngine.TesterAutomation.Runtime;

internal sealed class LogMonitor
{
    private readonly string _directoryPath;
    private readonly Dictionary<string, string[]> _snapshots = new(StringComparer.OrdinalIgnoreCase);

    public LogMonitor(string directoryPath)
    {
        _directoryPath = directoryPath;
    }

    public IReadOnlyList<ObservedLogLine> Poll()
    {
        List<ObservedLogLine> newLines = new();

        if (Directory.Exists(_directoryPath) == false)
        {
            return newLines;
        }

        string[] files = Directory.GetFiles(_directoryPath, "*.txt", SearchOption.TopDirectoryOnly);
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        foreach (string file in files)
        {
            string[] currentLines;

            try
            {
                currentLines = File.ReadAllLines(file);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            _snapshots.TryGetValue(file, out string[]? previousLines);
            previousLines ??= Array.Empty<string>();

            int commonPrefixLength = GetCommonPrefixLength(previousLines, currentLines);

            for (int i = commonPrefixLength; i < currentLines.Length; i++)
            {
                newLines.Add(new ObservedLogLine
                {
                    FileName = Path.GetFileName(file),
                    LineNumber = i + 1,
                    Text = currentLines[i]
                });
            }

            _snapshots[file] = currentLines;
        }

        return newLines;
    }

    public IReadOnlyList<string> GetTrackedFiles()
    {
        return _snapshots.Keys
            .Select(Path.GetFileName)
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;
    }

    private static int GetCommonPrefixLength(IReadOnlyList<string> previousLines, IReadOnlyList<string> currentLines)
    {
        int limit = Math.Min(previousLines.Count, currentLines.Count);

        for (int i = 0; i < limit; i++)
        {
            if (!string.Equals(previousLines[i], currentLines[i], StringComparison.Ordinal))
            {
                return i;
            }
        }

        return limit;
    }
}
