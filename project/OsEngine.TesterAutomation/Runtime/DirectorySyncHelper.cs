using System.IO;

namespace OsEngine.TesterAutomation.Runtime;

internal static class DirectorySyncHelper
{
    public static void CopyRecursive(string sourceDirectory, string destinationDirectory)
    {
        if (Directory.Exists(sourceDirectory) == false)
        {
            throw new DirectoryNotFoundException($"Source directory was not found: {sourceDirectory}");
        }

        Directory.CreateDirectory(destinationDirectory);

        foreach (string directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relativePath));
        }

        foreach (string file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceDirectory, file);
            string targetPath = Path.Combine(destinationDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.Copy(file, targetPath, overwrite: true);
        }
    }
}
