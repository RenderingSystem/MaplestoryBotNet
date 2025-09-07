using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MaplestoryBotNet.WindowsLibrary
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
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINTSTRUCT lpPoint);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(IntPtr hmonitor, [In, Out] MONITORINFOEX info);

        [DllImport("shcore.dll")]
        public static extern int GetProcessDpiAwareness(IntPtr hprocess, out int value);

        [DllImport("shcore.dll")]
        public static extern int SetProcessDpiAwareness(int value);

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);
    }


    public abstract class AbstractWindowsLibrary
    {
        public abstract bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        public abstract bool ClientToScreen(IntPtr hWnd, ref POINTSTRUCT lpPoint);

        public abstract IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

        public abstract bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        public abstract int GetProcessDpiAwareness(IntPtr hProcess, out int value);

        public abstract int SetProcessDpiAwareness(int value);

        public abstract bool SetProcessDPIAware();

        public abstract bool IsWindow(IntPtr hWnd);
    }


    public class WindowsUserLibrary : AbstractWindowsLibrary
    {
        public override bool GetClientRect(IntPtr hWnd, out RECT lpRect)
        {
            return WindowsLibraryImports.GetClientRect(hWnd, out lpRect);
        }

        public override bool ClientToScreen(IntPtr hWnd, ref POINTSTRUCT lpPoint)
        {
            return WindowsLibraryImports.ClientToScreen(hWnd, ref lpPoint);
        }

        public override bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi)
        {
            return WindowsLibraryImports.GetMonitorInfo(hMonitor, lpmi);
        }

        public override nint MonitorFromWindow(IntPtr hWnd, uint dwFlags)
        {
            return WindowsLibraryImports.MonitorFromWindow(hWnd, dwFlags);
        }
        public override int GetProcessDpiAwareness(IntPtr hProcess, out int value)
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

        public override bool IsWindow(IntPtr hWnd)
        {
            return WindowsLibraryImports.IsWindow(hWnd);
        }
    }

}
