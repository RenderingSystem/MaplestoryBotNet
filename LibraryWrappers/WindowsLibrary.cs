using System.Runtime.InteropServices;


namespace MaplestoryBotNet.LibraryWrappers
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public class MONITORINFOEX
    {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        public int dwFlags = 0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szDevice = new char[32];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTSTRUCT
    {
        public int x;
        public int y;
        public POINTSTRUCT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public class WindowsLibraryImports
    {
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(Context hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(Context hWnd, ref POINTSTRUCT lpPoint);

        [DllImport("user32.dll")]
        public static extern Context MonitorFromWindow(Context hWnd, uint dwFlags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(Context hmonitor, [In, Out] MONITORINFOEX info);

        [DllImport("shcore.dll")]
        public static extern int GetProcessDpiAwareness(Context hprocess, out int value);

        [DllImport("shcore.dll")]
        public static extern int SetProcessDpiAwareness(int value);

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        [DllImport("user32.dll")]
        public static extern bool IsWindow(Context hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("shcore.dll")]
        public static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);
    }


    public abstract class AbstractWindowsLibrary
    {
        public abstract bool GetClientRect(Context hWnd, out RECT lpRect);

        public abstract bool ClientToScreen(Context hWnd, ref POINTSTRUCT lpPoint);

        public abstract Context MonitorFromWindow(Context hWnd, uint dwFlags);

        public abstract bool GetMonitorInfo(Context hMonitor, ref MONITORINFOEX lpmi);

        public abstract int GetProcessDpiAwareness(Context hProcess, out int value);

        public abstract int SetProcessDpiAwareness(int value);

        public abstract bool SetProcessDPIAware();

        public abstract bool IsWindow(Context hWnd);

        public abstract int GetWindowLong(IntPtr hWnd, int nIndex);

        public abstract int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public abstract int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);
    }


    public class WindowsUserLibrary : AbstractWindowsLibrary
    {
        public override bool GetClientRect(Context hWnd, out RECT lpRect)
        {
            return WindowsLibraryImports.GetClientRect(hWnd, out lpRect);
        }

        public override bool ClientToScreen(Context hWnd, ref POINTSTRUCT lpPoint)
        {
            return WindowsLibraryImports.ClientToScreen(hWnd, ref lpPoint);
        }

        public override bool GetMonitorInfo(Context hMonitor, ref MONITORINFOEX lpmi)
        {
            return WindowsLibraryImports.GetMonitorInfo(hMonitor, lpmi);
        }

        public override nint MonitorFromWindow(Context hWnd, uint dwFlags)
        {
            return WindowsLibraryImports.MonitorFromWindow(hWnd, dwFlags);
        }
        public override int GetProcessDpiAwareness(Context hProcess, out int value)
        {
            return WindowsLibraryImports.GetProcessDpiAwareness(hProcess, out value);
        }
        public override int SetProcessDpiAwareness(int value)
        {
            return WindowsLibraryImports.SetProcessDpiAwareness(value);
        }

        public override bool SetProcessDPIAware()
        {
            return WindowsLibraryImports.SetProcessDPIAware();
        }

        public override bool IsWindow(Context hWnd)
        {
            return WindowsLibraryImports.IsWindow(hWnd);
        }

        public override int GetWindowLong(IntPtr hWnd, int nIndex)
        {
            return WindowsLibraryImports.GetWindowLong(hWnd, nIndex);
        }

        public override int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            return WindowsLibraryImports.SetWindowLong(hWnd, nIndex, dwNewLong);
        }

        public override int GetDpiForMonitor(nint hmonitor, int dpiType, out uint dpiX, out uint dpiY)
        {
            return WindowsLibraryImports.GetDpiForMonitor(hmonitor, dpiType, out dpiX, out dpiY);
        }
    }

}
