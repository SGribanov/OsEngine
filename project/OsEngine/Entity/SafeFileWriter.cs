/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OsEngine.Entity
{
    /// <summary>
    /// Atomic file writer with rollback backup in the same directory.
    /// </summary>
    public static class SafeFileWriter
    {
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        public static void WriteAllLines(string path, IEnumerable<string> lines, Encoding encoding = null)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            WriteAtomically(path, stream =>
            {
                using (StreamWriter writer = new StreamWriter(stream, encoding ?? Utf8NoBom, 1024, leaveOpen: true))
                {
                    foreach (string line in lines)
                    {
                        writer.WriteLine(line);
                    }

                    writer.Flush();
                }
            });
        }

        public static void WriteAllText(string path, string content, Encoding encoding = null)
        {
            WriteAtomically(path, stream =>
            {
                using (StreamWriter writer = new StreamWriter(stream, encoding ?? Utf8NoBom, 1024, leaveOpen: true))
                {
                    writer.Write(content ?? string.Empty);
                    writer.Flush();
                }
            });
        }

        private static void WriteAtomically(string path, Action<FileStream> writeAction)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new InvalidOperationException("Unable to resolve file directory.");
            }

            Directory.CreateDirectory(directory);

            string tempPath = fullPath + ".tmp";
            string backupPath = fullPath + ".bak";

            try
            {
                using (FileStream stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    writeAction(stream);
                    stream.Flush(true);
                }

                if (File.Exists(fullPath))
                {
                    File.Replace(tempPath, fullPath, backupPath, true);
                }
                else
                {
                    File.Move(tempPath, fullPath);
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
    }
}
