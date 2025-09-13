using HPPH;
using MaplestoryBotNet.Systems.ScreenCapture;
using MaplestoryBotNet.LibraryWrappers;
using ScreenCapture.NET;
using SixLabors.ImageSharp.PixelFormats;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.ScreenCapture.Tests.Mocks
{
    public class MockScreenCaptureService : IScreenCaptureService
    {
        public List<string> CallOrder = [];

        public int DisposeCalls = 0;
        public void Dispose()
        {
            var callReference = new TestUtilities().Reference(this) + "Dispose";
            CallOrder.Add(callReference);
            DisposeCalls++;
        }

        public int GetDisplaysCalls = 0;
        public int GetDisplaysIndex = 0;
        public List<GraphicsCard> GetDisplaysCallArg_graphicsCard = [];
        public List<IEnumerable<Display>> GetDisplaysReturn = [];
        public IEnumerable<Display> GetDisplays(GraphicsCard graphicsCard)
        {
            var callReference = new TestUtilities().Reference(this) + "GetDisplays";
            CallOrder.Add(callReference);
            GetDisplaysCalls++;
            GetDisplaysCallArg_graphicsCard.Add(graphicsCard);
            if (GetDisplaysIndex < GetDisplaysReturn.Count)
                return GetDisplaysReturn[GetDisplaysIndex++];
            throw new IndexOutOfRangeException();
        }

        public int GetGraphicsCardsCalls = 0;
        public int GetGraphicsCardsIndex = 0;
        public List<IEnumerable<GraphicsCard>> GetGraphicsCardsReturn = [];
        public IEnumerable<GraphicsCard> GetGraphicsCards()
        {
            var callReference = new TestUtilities().Reference(this) + "GetGraphicsCards";
            CallOrder.Add(callReference);
            GetGraphicsCardsCalls++;
            if (GetGraphicsCardsIndex < GetGraphicsCardsReturn.Count)
                return GetGraphicsCardsReturn[GetGraphicsCardsIndex++];
            throw new IndexOutOfRangeException();
        }

        public int GetScreenCaptureCalls = 0;
        public int GetScreenCaptureIndex = 0;
        public List<Display> GetScreenCaptureCallArg_display = [];
        public List<IScreenCapture> GetScreenCaptureReturn = [];
        public IScreenCapture GetScreenCapture(Display display)
        {
            var callReference = new TestUtilities().Reference(this) + "GetScreenCapture";
            CallOrder.Add(callReference);
            GetScreenCaptureCalls++;
            if (GetScreenCaptureIndex < GetScreenCaptureReturn.Count)
                return GetScreenCaptureReturn[GetScreenCaptureIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockScreenCapture : IScreenCapture
    {
        public Display Display { set; get; }

        public event EventHandler<ScreenCaptureUpdatedEventArgs>? Updated;

        public List<string> CallOrder = [];

        public int CaptureScreenCalls = 0;
        public int CaptureScreenIndex = 0;
        public List<bool> CaptureScreenReturn = [];

        public MockScreenCapture()
        {
            Updated = null;
            if (Updated == null)
                return;
        }

        public bool CaptureScreen()
        {
            var callReference = new TestUtilities().Reference(this) + "CaptureScreen";
            CallOrder.Add(callReference);
            CaptureScreenCalls++;
            if (CaptureScreenIndex < CaptureScreenReturn.Count)
                return CaptureScreenReturn[CaptureScreenIndex++];
            throw new IndexOutOfRangeException();
        }

        public int DisposeCalls = 0;
        public void Dispose()
        {
            var callReference = new TestUtilities().Reference(this) + "Dispose";
            CallOrder.Add(callReference);
            DisposeCalls++;
        }

        public int RegisterCaptureZoneCalls = 0;
        public int RegisterCaptureZoneIndex = 0;
        public List<int> RegisterCaptureZoneCallArg_x = [];
        public List<int> RegisterCaptureZoneCallArg_y = [];
        public List<int> RegisterCaptureZoneCallArg_width = [];
        public List<int> RegisterCaptureZoneCallArg_height = [];
        public List<int> RegisterCaptureZoneCallArg_downscaleLevel = [];
        public List<ICaptureZone> RegisterCaptureZoneReturn = [];
        public ICaptureZone RegisterCaptureZone(
            int x,
            int y,
            int width,
            int height,
            int downscaleLevel = 0
        ) {
            var callReference = new TestUtilities().Reference(this) + "RegisterCaptureZone";
            CallOrder.Add(callReference);
            RegisterCaptureZoneCalls++;
            RegisterCaptureZoneCallArg_x.Add(x);
            RegisterCaptureZoneCallArg_y.Add(y);
            RegisterCaptureZoneCallArg_width.Add(width);
            RegisterCaptureZoneCallArg_height.Add(height);
            RegisterCaptureZoneCallArg_downscaleLevel.Add(downscaleLevel);
            if (RegisterCaptureZoneIndex < RegisterCaptureZoneReturn.Count)
                return RegisterCaptureZoneReturn[RegisterCaptureZoneIndex++];
            throw new IndexOutOfRangeException();
        }

        public int RestartCalls = 0;
        public void Restart()
        {
            var callReference = new TestUtilities().Reference(this) + "Restart";
            CallOrder.Add(callReference);
            RestartCalls++;
        }

        public int UnregisterCaptureZoneCalls = 0;
        public int UnregisterCaptureZoneIndex = 0;
        public List<ICaptureZone> UnregisterCaptureZoneCallArg_captureZone = [];
        public List<bool> UnregisterCaptureZoneReturn = [];
        public bool UnregisterCaptureZone(ICaptureZone captureZone)
        {
            var callReference = new TestUtilities().Reference(this) + "UnregisterCaptureZone";
            CallOrder.Add(callReference);
            UnregisterCaptureZoneCalls++;
            if (UnregisterCaptureZoneIndex < UnregisterCaptureZoneReturn.Count)
                return UnregisterCaptureZoneReturn[UnregisterCaptureZoneIndex++];
            throw new IndexOutOfRangeException();
        }

        public int UpdateCaptureZoneCalls = 0;
        public List<ICaptureZone> UpdateCaptureZoneCallArg_captureZone = [];
        public List<int?> UpdateCaptureZoneCallArg_x = [];
        public List<int?> UpdateCaptureZoneCallArg_y = [];
        public List<int?> UpdateCaptureZoneCallArg_width = [];
        public List<int?> UpdateCaptureZoneCallArg_height = [];
        public List<int?> UpdateCaptureZoneCallArg_downscaleLevel = [];
        public void UpdateCaptureZone(
            ICaptureZone captureZone,
            int? x = null,
            int? y = null,
            int? width = null,
            int? height = null,
            int? downscaleLevel = null
        ) {
            var callReference = new TestUtilities().Reference(this) + "UpdateCaptureZone";
            CallOrder.Add(callReference);
            UpdateCaptureZoneCalls++;
            UpdateCaptureZoneCallArg_captureZone.Add(captureZone);
            UpdateCaptureZoneCallArg_x.Add(x);
            UpdateCaptureZoneCallArg_y.Add(y);
            UpdateCaptureZoneCallArg_width.Add(width);
            UpdateCaptureZoneCallArg_height.Add(height);
            UpdateCaptureZoneCallArg_downscaleLevel.Add(downscaleLevel);
        }
    }


    public class MockCaptureZone : ICaptureZone
    {
        public List<string> CallOrder = [];

        public Display Display { set; get; }

        public IColorFormat ColorFormat { set; get; }

        public int X { set; get; }

        public int Y { set; get; }

        public int Width { set; get; }

        public int Height { set; get; }

        public int Stride { set; get; }

        public int DownscaleLevel { set; get; }

        public int UnscaledWidth { set; get; }

        public int UnscaledHeight { set; get; }

        #pragma warning disable CS8766
        public IImage? Image { set; get; }
        #pragma warning restore CS8766

        public bool AutoUpdate { set; get; }

        public bool IsUpdateRequested { set; get; }

        public event EventHandler? Updated;

        public byte[] RawBufferArray { set; get; }

        public ReadOnlySpan<byte> RawBuffer => RawBufferArray;

        public MockCaptureZone()
        {
            ColorFormat = ColorFormatBGRA.Instance;
            RawBufferArray = [];
            Image = null;
            Updated = null;
            if (Updated != null)
                return;
        }

        public int GetRefImageCalls = 0;
        private int GetRefImageIndex = 0;
        private readonly List<Delegate> GetRefImageReturn = new List<Delegate>();
        public RefImage<TColor> GetRefImage<TColor>() where TColor : struct, IColor
        {
            var callReference = new TestUtilities().Reference(this) + "GetRefImage";
            CallOrder.Add(callReference);
            GetRefImageCalls++;
            if (GetRefImageIndex < GetRefImageReturn.Count)
                return ((Func<RefImage<TColor>>)GetRefImageReturn[GetRefImageIndex++])();
            throw new IndexOutOfRangeException();
        }

        public int LockCalls = 0;
        public int LockIndex = 0;
        public List<IDisposable> LockReturn = [];
        public IDisposable Lock()
        {
            var callReference = new TestUtilities().Reference(this) + "Lock";
            CallOrder.Add(callReference);
            LockCalls++;
            if (LockIndex < LockReturn.Count)
                return LockReturn[LockIndex++];
            throw new IndexOutOfRangeException();
        }

        public int RequestUpdateCalls = 0;
        public void RequestUpdate()
        {
            var callReference = new TestUtilities().Reference(this) + "RequestUpdate";
            CallOrder.Add(callReference);
            RequestUpdateCalls++;
        }
    }


    public class MockImageLock : IDisposable
    {
        public int DisposeCalls = 0;
        public List<string> CallOrder = [];
        public void Dispose()
        {
            var callReference = new TestUtilities().Reference(this) + "Dispose";
            CallOrder.Add(callReference);
            DisposeCalls++;
        }
    }


    public class MockCaptureMonitorHelper : AbstractCaptureMonitorHelper
    {
        public List<string> CallOrder = [];

        public int GetMonitorInfoCalls = 0;
        public int GetMonitorInfoIndex = 0;
        public List<nint> GetMonitorInfoCallArg_windowHandle = [];
        public List<MONITORINFOEX?> GetMonitorInfoReturn = [];
        public override MONITORINFOEX? GetMonitorInfo(nint windowHandle) {
            var callReference = new TestUtilities().Reference(this) + "GetMonitorInfo";
            CallOrder.Add(callReference);
            GetMonitorInfoCalls++;
            GetMonitorInfoCallArg_windowHandle.Add(windowHandle);
            if (GetMonitorInfoIndex < GetMonitorInfoReturn.Count)
                return GetMonitorInfoReturn[GetMonitorInfoIndex++];
            throw new IndexOutOfRangeException();
        }
    }

    public class MockMonitorDisplaysHelper : AbstractMonitorDisplaysHelper
    {
        public List<string> CallOrder = [];

        public int GetDisplaysCalls = 0;
        public int GetDisplaysIndex = 0;
        public List<List<Display>> GetDisplaysReturn = [];
        public override IEnumerable<Display> GetDisplays()
        {
            var callReference = new TestUtilities().Reference(this) + "GetDisplays";
            CallOrder.Add(callReference);
            GetDisplaysCalls++;
            if (GetDisplaysIndex < GetDisplaysReturn.Count)
                return GetDisplaysReturn[GetDisplaysIndex++];
            throw new IndexOutOfRangeException();
        }

        public int RegisterDisplaysCalls = 0;
        public override void RegisterDisplays()
        {
            var callReference = new TestUtilities().Reference(this) + "RegisterDisplays";
            CallOrder.Add(callReference);
            RegisterDisplaysCalls++;
        }
    }


    public class MockScreenCaptureHelper : AbstractScreenCaptureHelper
    {
        public List<string> CallOrder = [];

        public int GetCaptureZoneCalls = 0;
        public int GetCaptureZoneIndex = 0;
        public List<nint> GetCaptureZoneCallArg_windowHandle = [];
        public List<MONITORINFOEX> GetCaptureZoneCallArg_monitorInfo = [];
        public List<ICaptureZone?> GetCaptureZoneReturn = [];
        public override ICaptureZone? GetCaptureZone(
            nint windowHandle, MONITORINFOEX monitorInfo
        ) {
            var callReference = new TestUtilities().Reference(this) + "GetCaptureZone";
            CallOrder.Add(callReference);
            GetCaptureZoneCalls++;
            GetCaptureZoneCallArg_windowHandle.Add(windowHandle);
            GetCaptureZoneCallArg_monitorInfo.Add(monitorInfo);
            if (GetCaptureZoneIndex < GetCaptureZoneReturn.Count)
                return GetCaptureZoneReturn[GetCaptureZoneIndex++];
            throw new IndexOutOfRangeException();
        }

        public int GetScreenCaptureCalls = 0;
        public int GetScreenCaptureIndex = 0;
        public List<nint> GetScreenCaptureCallArg_windowHandle = [];
        public List<MONITORINFOEX> GetScreenCaptureCallArg_monitorInfo = [];
        public List<IScreenCapture?> GetScreenCaptureReturn = [];
        public override IScreenCapture? GetScreenCapture(
            nint windowHandle, MONITORINFOEX monitorInfo
        ) {
            var callReference = new TestUtilities().Reference(this) + "GetScreenCapture";
            CallOrder.Add(callReference);
            GetScreenCaptureCalls++;
            GetScreenCaptureCallArg_windowHandle.Add(windowHandle);
            GetScreenCaptureCallArg_monitorInfo.Add(monitorInfo);
            if (GetScreenCaptureIndex < GetScreenCaptureReturn.Count)
                return GetScreenCaptureReturn[GetScreenCaptureIndex++];
            throw new IndexOutOfRangeException();
        }

        public int RegisterScreenCapturesCalls = 0;
        public List<IEnumerable<Display>> RegisterScreenCapturesCallArg_displays = [];
        public override void RegisterScreenCaptures(IEnumerable<Display> displays)
        {
            var callReference = new TestUtilities().Reference(this) + "RegisterScreenCaptures";
            CallOrder.Add(callReference);
            RegisterScreenCapturesCalls++;
            RegisterScreenCapturesCallArg_displays.Add(displays);
        }

        public int UpdateCaptureZoneCalls = 0;
        public int UpdateCaptureZoneIndex = 0;
        public List<IScreenCapture> UpdateCaptureZoneCallArg_capture = [];
        public List<Tuple<int, int, int, int>> UpdateCaptureZoneCallArg_captureArea = [];
        public List<ICaptureZone?> UpdateCaptureZoneReturn = [];
        public override ICaptureZone? UpdateCaptureZone(
            IScreenCapture capture, Tuple<int, int, int, int> captureArea
        ) {
            var callReference = new TestUtilities().Reference(this) + "UpdateCaptureZone";
            CallOrder.Add(callReference);
            UpdateCaptureZoneCalls++;
            UpdateCaptureZoneCallArg_capture.Add(capture);
            UpdateCaptureZoneCallArg_captureArea.Add(captureArea);
            if (UpdateCaptureZoneIndex < UpdateCaptureZoneReturn.Count)
                return UpdateCaptureZoneReturn[UpdateCaptureZoneIndex++];
            throw new IndexOutOfRangeException();
        }

        public int GetCaptureAreaCalls = 0;
        public int GetCaptureAreaIndex = 0;
        public List<nint> GetCaptureAreaCallArg_windowHandle = [];
        public List<MONITORINFOEX> GetCaptureAreaCallArg_monitorInfo = [];
        public List<Tuple<int, int, int, int>?> GetCaptureAreaReturn = [];
        public override Tuple<int, int, int, int>? GetCaptureArea(
            nint windowHandle, MONITORINFOEX monitorInfo
        )
        {
            var callReference = new TestUtilities().Reference(this) + "GetCaptureArea";
            CallOrder.Add(callReference);
            GetCaptureAreaCalls++;
            GetCaptureAreaCallArg_windowHandle.Add(windowHandle);
            GetCaptureAreaCallArg_monitorInfo.Add(monitorInfo);
            if (GetCaptureAreaIndex < GetCaptureAreaReturn.Count)
                return GetCaptureAreaReturn[GetCaptureAreaIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockCaptureImageGenerator : AbstractCaptureImageGenerator
    {
        public List<string> CallOrder = [];

        public int GenerateImageCalls = 0;
        public int GenerateImageIndex = 0;
        public List<ICaptureZone> GenerateImageCallArg_captureZone = [];
        public List<SixLabors.ImageSharp.Image<Bgra32>> GenerateImageReturn = [];
        public override SixLabors.ImageSharp.Image<Bgra32> GenerateImage(ICaptureZone captureZone)
        {
            var callReference = new TestUtilities().Reference(this) + "GenerateImage";
            CallOrder.Add(callReference);
            GenerateImageCalls++;
            GenerateImageCallArg_captureZone.Add(captureZone);
            if (GenerateImageIndex < GenerateImageReturn.Count)
                return GenerateImageReturn[GenerateImageIndex++];
            throw new IndexOutOfRangeException();
        }
    }

    public class MockWindowHandleHelper : AbstractWindowHandleHelper
    {
        public List<string> CallOrder = [];

        public int FindByProcessCalls = 0;
        public int FindByProcessIndex = 0;
        public List<string> FindByProcessCallArg_processName = [];
        public List<nint> FindByProcessReturn = [];
        public override nint FindByProcess(string processName)
        {
            var callReference = new TestUtilities().Reference(this) + "FindByProcess";
            CallOrder.Add(callReference);
            FindByProcessCalls++;
            FindByProcessCallArg_processName.Add(processName);
            if (FindByProcessIndex < FindByProcessReturn.Count)
                return FindByProcessReturn[FindByProcessIndex++];
            throw new IndexOutOfRangeException();
        }
    }

    public class MockScreenCaptureModule : AbstractScreenCaptureModule
    {
        public List<string> CallOrder = [];

        public int InitializeCalls = 0;
        public override void Initialize()
        {
            var callReference = new TestUtilities().Reference(this) + "Initialize";
            CallOrder.Add(callReference);
            InitializeCalls++;
        }

        public int CaptureCalls = 0;
        public int CaptureIndex = 0;
        public List<nint> CaptureCallArg_windowHandle = [];
        public List<SixLabors.ImageSharp.Image<Bgra32>?> CaptureReturn = [];
        public override SixLabors.ImageSharp.Image<Bgra32>? Capture(nint windowHandle)
        {
            var callReference = new TestUtilities().Reference(this) + "Capture";
            CallOrder.Add(callReference);
            CaptureCalls++;
            CaptureCallArg_windowHandle.Add(windowHandle);
            if (CaptureIndex < CaptureReturn.Count)
                return CaptureReturn[CaptureIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockScreenCaptureOrchestrator : AbstractScreenCaptureOrchestrator
    {
        public List<string> CallOrder = [];

        public int CaptureCalls = 0;
        public int CaptureIndex = 0;
        public List<string> CaptureCallArg_processName = [];
        public List<SixLabors.ImageSharp.Image<Bgra32>?> CaptureReturn = [];
        public override SixLabors.ImageSharp.Image<Bgra32>? Capture(string processName)
        {
            var callReference = new TestUtilities().Reference(this) + "Capture";
            CallOrder.Add(callReference);
            CaptureCalls++;
            CaptureCallArg_processName.Add(processName);
            if (CaptureIndex < CaptureReturn.Count)
                return CaptureReturn[CaptureIndex++];
            throw new IndexOutOfRangeException();
        }
    }
}
