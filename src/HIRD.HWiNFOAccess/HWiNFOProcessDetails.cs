using HIRD.HWiNFOAccess.Exceptions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace HIRD.HWiNFOAccess
{
    public class HWiNFOProcessDetails
    {
        private const string ProcessName = "HWiNFO64";
        private const string KernelDll = "kernel32.dll";

        private static Process? _hwinfoProcess;
        private static string? _hwinfoProcessPath;
        private static string? _hwinfoSettingsPath;

        public static bool IsRunning()
        {
            _hwinfoProcess = Process.GetProcessesByName(ProcessName).FirstOrDefault() ?? null;
            return _hwinfoProcess != null;
        }

        public static Process GetProcess()
        {
            _hwinfoProcess = Process.GetProcessesByName(ProcessName).FirstOrDefault() ?? throw new HWiNFONotRunningException();
            return _hwinfoProcess;
        }

        public static string GetProcessPath()
        {
            if (_hwinfoProcess == null)
                GetProcess();

            if (_hwinfoProcessPath != null)
                return _hwinfoProcessPath;

            int capacity = 2000;
            StringBuilder builder = new(capacity);
            IntPtr ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, _hwinfoProcess!.Id);

            try
            {
                if (!QueryFullProcessImageName(ptr, 0, builder, ref capacity))
                    return String.Empty;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    CloseHandle(ptr);
            }

            _hwinfoProcessPath = builder.ToString();

            return _hwinfoProcessPath;
        }

        public static string GetProcessSettingsPath()
        {
            if (_hwinfoProcessPath == null)
                GetProcessPath();

            if (_hwinfoSettingsPath != null)
                return _hwinfoSettingsPath;

            _hwinfoSettingsPath = _hwinfoProcessPath!.Replace(".exe", ".INI");

            return _hwinfoSettingsPath;
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            QueryLimitedInformation = 0x00001000
        }

        [DllImport(KernelDll, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName(
              [In] IntPtr hProcess,
              [In] int dwFlags,
              [Out] StringBuilder lpExeName,
              ref int lpdwSize);

        [DllImport(KernelDll, SetLastError = true)]
        private static extern IntPtr OpenProcess(
         ProcessAccessFlags processAccess,
         bool bInheritHandle,
         int processId);

        [DllImport(KernelDll, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
