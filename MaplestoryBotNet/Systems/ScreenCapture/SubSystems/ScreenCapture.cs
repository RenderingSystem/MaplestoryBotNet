using System.Diagnostics;
using System.Runtime.InteropServices;
using MaplestoryBotNet.LibraryWrappers;
using ScreenCapture.NET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace MaplestoryBotNet.Systems.ScreenCapture
{
    public abstract class AbstractScreenCaptureModule
    {

        public abstract void Initialize();

        public abstract Image<Bgra32>? Capture(nint windowHandle);
    }


    public abstract class AbstractCaptureMonitorHelper
    {
        public abstract MONITORINFOEX? GetMonitorInfo(nint windowHandle);
    }

    public abstract class AbstractWindowHandleHelper
    {
        public abstract nint FindByProcess(string processName);
    }


    public abstract class AbstractScreenCaptureOrchestrator
    {
        public abstract Image<Bgra32>? Capture(string processName);
    }


    public abstract class AbstractScreenCaptureHelper
    {
        public abstract void RegisterScreenCaptures(
            IEnumerable<Display> displays
        );

        public abstract IScreenCapture? GetScreenCapture(
            nint windowHandle, MONITORINFOEX monitorInfo
        );

        public abstract ICaptureZone? GetCaptureZone(
            nint windowHandle, MONITORINFOEX monitorInfo
        );

        public abstract Tuple<int, int, int, int>? GetCaptureArea(
            nint windowHandle, MONITORINFOEX monitorInfo
        );

        public abstract ICaptureZone? UpdateCaptureZone(
            IScreenCapture capture, Tuple<int, int, int, int> captureArea
        );
    }


    public abstract class AbstractMonitorDisplaysHelper
    {

        public abstract void RegisterDisplays();

        public abstract IEnumerable<Display> GetDisplays();
    }


    public abstract class AbstractCaptureImageGenerator
    {
        public abstract Image<Bgra32> GenerateImage(ICaptureZone captureZone);
    }


    public class GameWindowHandleHelper : AbstractWindowHandleHelper
    {
        public override nint FindByProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (Process process in processes)
            {
                if (process.MainWindowHandle != nint.Zero)
                {
                    return process.MainWindowHandle;
                }
            }
            return nint.Zero;
        }
    }


    public class GameCaptureMonitorHelper : AbstractCaptureMonitorHelper
    {
        private AbstractWindowsLibrary _windowsLibrary;

        public GameCaptureMonitorHelper(
            AbstractWindowsLibrary windowsLibrary
        )
        {
            _windowsLibrary = windowsLibrary;
        }

        public override MONITORINFOEX? GetMonitorInfo(nint windowHandle)
        {
            var hMonitor = _windowsLibrary.MonitorFromWindow(windowHandle, 2);
            var hMonitorInfo = new MONITORINFOEX { cbSize = Marshal.SizeOf<MONITORINFOEX>() };
            if (hMonitor == nint.Zero)
            {
                return null;
            }
            if (!_windowsLibrary.GetMonitorInfo(hMonitor, ref hMonitorInfo))
            {
                return null;
            }
            return hMonitorInfo;
        }
    }


    public class GameScreenCaptureHelper : AbstractScreenCaptureHelper
    {
        private IScreenCaptureService _screenCaptureService;

        private AbstractWindowsLibrary _windowsLibrary;

        private List<IScreenCapture> _screenCaptures;

        private List<ICaptureZone> _screenCaptureZones;

        public GameScreenCaptureHelper(
            IScreenCaptureService screenCaptureService,
            AbstractWindowsLibrary windowsLibrary
        ) {
            _screenCaptureService = screenCaptureService;
            _windowsLibrary = windowsLibrary;
            _screenCaptures = new List<IScreenCapture>();
            _screenCaptureZones = new List<ICaptureZone>();
        }

        public override void RegisterScreenCaptures(IEnumerable<Display> displays)
        {
            for (int i = 0; i < _screenCaptures.Count; i++)
            {
                var capture = _screenCaptures[i];
                var captureZone = _screenCaptureZones[i];
                capture.UnregisterCaptureZone(captureZone);
            }
            _screenCaptures.Clear();
            _screenCaptureZones.Clear();
            for (int i = 0; i < displays.Count(); i++)
            {
                var display = displays.ElementAt(i);
                if (display.Width > 0 && display.Height > 0)
                {
                    var capture = _screenCaptureService.GetScreenCapture(display);
                    var captureZone = capture.RegisterCaptureZone(0, 0, display.Width, display.Height);
                    captureZone.AutoUpdate = true;
                    _screenCaptures.Add(capture);
                    _screenCaptureZones.Add(captureZone);
                }
            }
        }

        public override IScreenCapture? GetScreenCapture(
            nint windowHandle, MONITORINFOEX monitorInfo
        ) {
            for (int i = 0; i < _screenCaptures.Count; i++)
            {
                string cSharpDisplayName = new string(monitorInfo.szDevice).TrimEnd('\0');
                if (_screenCaptures[i].Display.DeviceName == cSharpDisplayName)
                    return _screenCaptures[i];
            }
            return null;
        }

        public override ICaptureZone? GetCaptureZone(
            nint windowHandle, MONITORINFOEX monitorInfo
        ) {
            for (int i = 0; i < _screenCaptureZones.Count; i++)
            {
                string cSharpDisplayName = new string(monitorInfo.szDevice).TrimEnd('\0');
                if (_screenCaptureZones[i].Display.DeviceName == cSharpDisplayName)
                    return _screenCaptureZones[i];
            }
            return null;
        }

        public override ICaptureZone? UpdateCaptureZone(
            IScreenCapture capture, Tuple<int, int, int, int> captureArea
        )
        {
            var index = _screenCaptures.IndexOf(capture);
            if (index < 0)
            {
                return null;
            }
            var captureZone = _screenCaptureZones[index];
            int x = Math.Max(0, Math.Min(captureArea.Item1, capture.Display.Width - 1));
            int y = Math.Max(0, Math.Min(captureArea.Item2, capture.Display.Height - 1));
            int width = Math.Max(1, Math.Min(captureArea.Item3, capture.Display.Width - x));
            int height = Math.Max(1, Math.Min(captureArea.Item4, capture.Display.Height - y));
            if (
                captureZone.X != x ||
                captureZone.Y != y ||
                captureZone.Width != width ||
                captureZone.Height != height
            )
            {
                capture.UpdateCaptureZone(captureZone, x, y, width, height);
                capture.CaptureScreen();
            }
            return captureZone;
        }

        public override Tuple<int, int, int, int>? GetCaptureArea(
            nint windowHandle, MONITORINFOEX monitorInfo
        ) {
            var clientRect = new RECT { bottom = 0, left = 0, top = 0, right = 0 };
            var topLeft = new POINTSTRUCT { x = 0, y = 0 };
            if (!_windowsLibrary.GetClientRect(windowHandle, out clientRect))
            {
                return null;
            }
            if (!_windowsLibrary.ClientToScreen(windowHandle, ref topLeft))
            {
                return null;
            }
            int left = topLeft.x - monitorInfo.rcMonitor.left;
            int top = topLeft.y - monitorInfo.rcMonitor.top;
            int width = clientRect.right - clientRect.left;
            int height = clientRect.bottom - clientRect.top;
            return new Tuple<int, int, int, int>(left, top, width, height);
        }
    }


    public class GameCaptureImageGenerator : AbstractCaptureImageGenerator
    {
        public override Image<Bgra32> GenerateImage(ICaptureZone captureZone)
        {
            using (captureZone.Lock())
            {
                var bufferCopy = captureZone.RawBuffer.ToArray();
                return Image.WrapMemory<Bgra32>(
                    bufferCopy, captureZone.Width, captureZone.Height
                );
            }
        }
    }


    public class GameMonitorDisplaysHelper : AbstractMonitorDisplaysHelper
    {

        IScreenCaptureService _screenCaptureService;

        private IEnumerable<GraphicsCard> _graphicsCards;

        private List<Display> _displays;

        public GameMonitorDisplaysHelper(
            IScreenCaptureService screenCaptureService
        )
        {
            _screenCaptureService = screenCaptureService;
            _graphicsCards = new List<GraphicsCard>();
            _displays = new List<Display>();
        }

        public override void RegisterDisplays()
        {
            _displays.Clear();
            _graphicsCards = _screenCaptureService.GetGraphicsCards();
            for (int i = 0; i < _graphicsCards.Count(); i++)
            {
                var graphicsCard = _graphicsCards.ElementAt(i);
                var displays = _screenCaptureService.GetDisplays(graphicsCard);
                for (int j = 0; j < displays.Count(); j++)
                {
                    _displays.Add(displays.ElementAt(j));
                }
            }
        }

        public override IEnumerable<Display> GetDisplays()
        {
            return _displays;
        }
    }


    public class GameScreenCaptureModule : AbstractScreenCaptureModule
    {
        private AbstractWindowsLibrary _windowsLibrary;

        private AbstractScreenCaptureHelper _screenCaptureHelper;

        private AbstractCaptureMonitorHelper _captureMonitorHelper;

        private AbstractMonitorDisplaysHelper _monitorDisplaysHelper;

        private AbstractCaptureImageGenerator _captureImageGenerator;

        public GameScreenCaptureModule(
            AbstractWindowsLibrary windowsLibrary,
            AbstractCaptureMonitorHelper captureMonitorHelper,
            AbstractMonitorDisplaysHelper monitorDisplaysHelper,
            AbstractScreenCaptureHelper screenCaptureHelper,
            AbstractCaptureImageGenerator captureImageGenerator
        )
        {
            _windowsLibrary = windowsLibrary;
            _captureMonitorHelper = captureMonitorHelper;
            _monitorDisplaysHelper = monitorDisplaysHelper;
            _screenCaptureHelper = screenCaptureHelper;
            _captureImageGenerator = captureImageGenerator;
        }

        public override void Initialize()
        {
            _windowsLibrary.SetProcessDpiAwareness(2);
            _monitorDisplaysHelper.RegisterDisplays();
            var displays = _monitorDisplaysHelper.GetDisplays();
            _screenCaptureHelper.RegisterScreenCaptures(displays);
        }

        public override Image<Bgra32>? Capture(nint windowHandle)
        {
            var monitorInfo = _captureMonitorHelper.GetMonitorInfo(windowHandle);
            if (monitorInfo == null)
            {
                return null;
            }
            var captureArea = _screenCaptureHelper.GetCaptureArea(windowHandle, monitorInfo);
            if (captureArea == null)
            {
                return null;
            }
            var screenCapture = _screenCaptureHelper.GetScreenCapture(windowHandle, monitorInfo);
            if (screenCapture == null)
            {
                return null;
            }
            var captureZone = _screenCaptureHelper.UpdateCaptureZone(screenCapture, captureArea);
            if (captureZone == null)
            {
                return null;
            }
            screenCapture.CaptureScreen();
            return _captureImageGenerator.GenerateImage(captureZone);
        }
    }


    public class GameScreenCaptureOrchestrator : AbstractScreenCaptureOrchestrator
    {
        private AbstractWindowsLibrary _windowsLibrary;

        private AbstractScreenCaptureModule _screenCaptureModule;

        private AbstractWindowHandleHelper _windowHandleHelper;

        private nint _windowHandle;

        public GameScreenCaptureOrchestrator(
            AbstractWindowsLibrary windowsLibrary,
            AbstractScreenCaptureModule screenCaptureModule,
            AbstractWindowHandleHelper windowHandleHelper
        )
        {
            _windowsLibrary = windowsLibrary;
            _screenCaptureModule = screenCaptureModule;
            _windowHandleHelper = windowHandleHelper;
            _windowHandle = nint.Zero;
        }

        public override Image<Bgra32>? Capture(string processName)
        {
            if (!_windowsLibrary.IsWindow(_windowHandle))
            {
                _windowHandle = _windowHandleHelper.FindByProcess(processName);
                if (_windowsLibrary.IsWindow(_windowHandle))
                    _screenCaptureModule.Initialize();
            }
            if (_windowsLibrary.IsWindow(_windowHandle))
            {
                var capture = _screenCaptureModule.Capture(_windowHandle);
                if (capture == null)
                    _windowHandle = nint.Zero;
                return capture;
            }
            return null;
        }
    }


    public class ScreenCaptureOrchestratorFacade : AbstractScreenCaptureOrchestrator
    {
        private GameScreenCaptureOrchestrator _screenCaptureModule;

        public ScreenCaptureOrchestratorFacade()
        {
            var screenCaptureService = new DX11ScreenCaptureService();
            var windowsLibrary = new WindowsUserLibrary();
            var captureMonitorHelper = new GameCaptureMonitorHelper(windowsLibrary);
            var monitorDisplaysHelper = new GameMonitorDisplaysHelper(screenCaptureService);
            var screenCaptureHelper = new GameScreenCaptureHelper(screenCaptureService, windowsLibrary);
            var windowHandleHelper = new GameWindowHandleHelper();
            var captureGenerator = new GameCaptureImageGenerator();
            var screenCaptureModule = new GameScreenCaptureModule(
                windowsLibrary,
                captureMonitorHelper,
                monitorDisplaysHelper,
                screenCaptureHelper,
                captureGenerator
            );
            _screenCaptureModule = new GameScreenCaptureOrchestrator(
                windowsLibrary, screenCaptureModule, windowHandleHelper
            );
        }

        public override Image<Bgra32>? Capture(string processName)
        {
            return _screenCaptureModule.Capture(processName);
        }
    }
}