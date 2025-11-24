using MaplestoryBotNet.LibraryWrappers;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;


namespace MaplestoryBotNet.Systems.UIHandler.UserInterface
{
    public abstract class AbstractDpi
    {
        public abstract Tuple<double, double> GetDpi(Window window);
    }


    public class HostedMonitorDpi : AbstractDpi
    {
        private AbstractWindowsLibrary _windowsLibary;

        public HostedMonitorDpi(AbstractWindowsLibrary windowsLibrary)
        {
            _windowsLibary = windowsLibrary;
        }

        public override Tuple<double, double> GetDpi(Window window)
        {
            try
            {
                var windowHandle = new WindowInteropHelper(window).Handle;
                if (windowHandle == IntPtr.Zero)
                {
                    return new Tuple<double, double>(1.0, 1.0);
                }
                var monitor = _windowsLibary.MonitorFromWindow(windowHandle, 2);
                if (monitor != IntPtr.Zero)
                {
                    uint dpiX, dpiY;
                    if (_windowsLibary.GetDpiForMonitor(monitor, 0, out dpiX, out dpiY) == 0)
                    {
                        return new Tuple<double, double>(dpiX / 96.0, dpiY / 96.0);
                    }
                }
                return new Tuple<double, double>(1.0, 1.0);
            }
            catch
            {
                return new Tuple<double, double>(1.0, 1.0);
            }
        }
    }


    public class WindowDpi : AbstractDpi
    {
        public override Tuple<double, double> GetDpi(Window window)
        {
            try
            {
                var dpi = VisualTreeHelper.GetDpi(window);
                return new Tuple<double, double>(dpi.DpiScaleX, dpi.DpiScaleY);
            }
            catch
            {
                return new Tuple<double, double>(1.0, 1.0);
            }
        }
    }
}