using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OsEngine.TesterAutomation.Runtime;

internal sealed class ProcessTreeSnapshotter
{
    public IReadOnlyList<ProcessTreeEntry> CaptureProcessTree(int rootProcessId)
    {
        Dictionary<int, ProcessTreeEntry> allProcesses = ReadProcesses();

        if (allProcesses.ContainsKey(rootProcessId) == false)
        {
            return Array.Empty<ProcessTreeEntry>();
        }

        List<ProcessTreeEntry> result = new();
        Queue<int> queue = new();
        queue.Enqueue(rootProcessId);

        while (queue.Count > 0)
        {
            int currentProcessId = queue.Dequeue();

            if (allProcesses.TryGetValue(currentProcessId, out ProcessTreeEntry? entry) == false)
            {
                continue;
            }

            result.Add(entry);

            foreach (ProcessTreeEntry child in allProcesses.Values.Where(item => item.ParentProcessId == currentProcessId))
            {
                queue.Enqueue(child.ProcessId);
            }
        }

        return result;
    }

    private static Dictionary<int, ProcessTreeEntry> ReadProcesses()
    {
        IntPtr snapshot = CreateToolhelp32Snapshot(Th32CsSnapProcess, 0);

        if (snapshot == IntPtr.Zero || snapshot == InvalidHandleValue)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not create process snapshot.");
        }

        try
        {
            PROCESSENTRY32 entry = new()
            {
                dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32>()
            };

            Dictionary<int, ProcessTreeEntry> result = new();

            if (Process32First(snapshot, ref entry))
            {
                do
                {
                    result[(int)entry.th32ProcessID] = new ProcessTreeEntry(
                        (int)entry.th32ProcessID,
                        (int)entry.th32ParentProcessID,
                        entry.szExeFile);
                }
                while (Process32Next(snapshot, ref entry));
            }

            return result;
        }
        finally
        {
            CloseHandle(snapshot);
        }
    }

    private const uint Th32CsSnapProcess = 0x00000002;
    private static readonly IntPtr InvalidHandleValue = new(-1);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessId);

    [DllImport("kernel32.dll", EntryPoint = "Process32FirstW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport("kernel32.dll", EntryPoint = "Process32NextW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PROCESSENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ProcessID;
        public IntPtr th32DefaultHeapID;
        public uint th32ModuleID;
        public uint cntThreads;
        public uint th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExeFile;
    }
}

internal sealed record ProcessTreeEntry(int ProcessId, int ParentProcessId, string ProcessName);
