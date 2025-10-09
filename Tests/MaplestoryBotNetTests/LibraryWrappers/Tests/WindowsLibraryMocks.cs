using MaplestoryBotNet.LibraryWrappers;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.WindowsLibrary.Tests
{
    internal class MockWindowsLibrary : AbstractWindowsLibrary
    {
        public List<string> CallOrder = [];

        public int GetClientRectCalls = 0;
        public int GetClientRectIndex = 0;
        public List<IntPtr> GetClientRectCallArg_hWnd = [];
        public List<RECT> GetClientRectOutCallArg_lpRect = [];
        public List<bool> GetClientRectReturn = [];
        public override bool GetClientRect(IntPtr hWnd, out RECT lpRect)
        {
            var callReference = new TestUtilities().Reference(this) + "GetClientRect";
            CallOrder.Add(callReference);
            GetClientRectCalls++;
            GetClientRectCallArg_hWnd.Add(hWnd);
            if (GetClientRectIndex < GetClientRectOutCallArg_lpRect.Count)
                lpRect = GetClientRectOutCallArg_lpRect[GetClientRectIndex];
            else
                throw new IndexOutOfRangeException();
            if (GetClientRectIndex < GetClientRectReturn.Count)
                return GetClientRectReturn[GetClientRectIndex++];
            throw new IndexOutOfRangeException();
        }

        public int ClientToScreenCalls = 0;
        public int ClientToScreenIndex = 0;
        public List<IntPtr> ClientToScreenCallArg_hWnd = [];
        public List<POINTSTRUCT> ClientToScreenRefCallArg_lpPoint = [];
        public List<bool> ClientToScreenReturn = [];
        public override bool ClientToScreen(IntPtr hWnd, ref POINTSTRUCT lpPoint)
        {
            var callReference = new TestUtilities().Reference(this) + "ClientToScreen";
            CallOrder.Add(callReference);
            ClientToScreenCalls++;
            ClientToScreenCallArg_hWnd.Add(hWnd);
            if (ClientToScreenIndex < ClientToScreenRefCallArg_lpPoint.Count)
            {
                lpPoint.x = ClientToScreenRefCallArg_lpPoint[ClientToScreenIndex].x;
                lpPoint.y = ClientToScreenRefCallArg_lpPoint[ClientToScreenIndex].y;
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
            if (ClientToScreenIndex < ClientToScreenReturn.Count)
                return ClientToScreenReturn[ClientToScreenIndex++];
            throw new IndexOutOfRangeException();
        }

        public int MonitorFromWindowCalls = 0;
        public int MonitorFromWindowIndex = 0;
        public List<IntPtr> MonitorFromWindowCallArg_hWnd = [];
        public List<uint> MonitorFromWindowRefCallArg_dwFlags = [];
        public List<IntPtr> MonitorFromWindowReturn = [];
        public override IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags)
        {
            var callReference = new TestUtilities().Reference(this) + "MonitorFromWindow";
            CallOrder.Add(callReference);
            MonitorFromWindowCalls++;
            MonitorFromWindowCallArg_hWnd.Add(hWnd);
            MonitorFromWindowRefCallArg_dwFlags.Add(dwFlags);
            if (MonitorFromWindowCalls < MonitorFromWindowReturn.Count)
                return MonitorFromWindowReturn[MonitorFromWindowCalls++];
            throw new IndexOutOfRangeException();
        }

        public int GetMonitorInfoCalls = 0;
        public int GetMonitorInfoIndex = 0;
        public List<IntPtr> GetMonitorInfoCallArg_hMonitor = [];
        public List<MONITORINFOEX> GetMonitorInfoRefCallArg_lpmi = [];
        public List<bool> GetMonitorInfoReturn = [];
        public override bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi)
        {
            var callReference = new TestUtilities().Reference(this) + "GetMonitorInfo";
            CallOrder.Add(callReference);
            GetMonitorInfoCalls++;
            GetMonitorInfoCallArg_hMonitor.Add(hMonitor);
            if (GetMonitorInfoIndex < GetMonitorInfoRefCallArg_lpmi.Count)
            {
                lpmi.cbSize = GetMonitorInfoRefCallArg_lpmi[GetMonitorInfoIndex].cbSize;
                lpmi.rcMonitor = GetMonitorInfoRefCallArg_lpmi[GetMonitorInfoIndex].rcMonitor;
                lpmi.rcWork = GetMonitorInfoRefCallArg_lpmi[GetMonitorInfoIndex].rcWork;
                lpmi.dwFlags = GetMonitorInfoRefCallArg_lpmi[GetMonitorInfoIndex].dwFlags;
                GetMonitorInfoRefCallArg_lpmi[GetMonitorInfoIndex].szDevice.CopyTo(lpmi.szDevice, 0);
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
            if (GetMonitorInfoIndex < GetMonitorInfoReturn.Count)
                return GetMonitorInfoReturn[GetMonitorInfoIndex++];
            throw new IndexOutOfRangeException();
        }

        public int GetProcessDpiAwarenessCalls = 0;
        public int GetProcessDpiAwarenessIndex = 0;
        public List<IntPtr> GetProcessDpiAwarenessCallArg_hProcess = [];
        public List<int> GetProcessDpiAwarenessOutCallArg_value = [];
        public List<int> GetProcessDpiAwarenessReturn = [];
        public override int GetProcessDpiAwareness(IntPtr hProcess, out int value)
        {
            var callReference = new TestUtilities().Reference(this) + "GetProcessDpiAwareness";
            CallOrder.Add(callReference);
            GetProcessDpiAwarenessCalls++;
            GetProcessDpiAwarenessCallArg_hProcess.Add(hProcess);
            if (GetProcessDpiAwarenessIndex < GetProcessDpiAwarenessOutCallArg_value.Count)
                value = GetProcessDpiAwarenessOutCallArg_value[GetProcessDpiAwarenessIndex];
            else
            {
                throw new IndexOutOfRangeException();
            }
            if (GetProcessDpiAwarenessIndex < GetProcessDpiAwarenessReturn.Count)
                return GetProcessDpiAwarenessReturn[GetProcessDpiAwarenessIndex++];
            throw new IndexOutOfRangeException();
            
        }

        public int SetProcessDpiAwarenessCalls = 0;
        public int SetProcessDpiAwarenessIndex = 0;
        public List<int> SetProcessDpiAwarenessCallArg_value = [];
        public List<int> SetProcessDpiAwarenessReturn = [];
        public override int SetProcessDpiAwareness(int value)
        {
            var callReference = new TestUtilities().Reference(this) + "SetProcessDpiAwareness";
            CallOrder.Add(callReference);
            SetProcessDpiAwarenessCalls++;
            SetProcessDpiAwarenessCallArg_value.Add(value);
            if (SetProcessDpiAwarenessIndex < SetProcessDpiAwarenessReturn.Count)
                return SetProcessDpiAwarenessReturn[SetProcessDpiAwarenessIndex++];
            throw new IndexOutOfRangeException();
        }

        public int SetProcessDPIAwareCalls = 0;
        public int SetProcessDPIAwareIndex = 0;
        public List<bool> SetProcessDPIAwareReturn = [];
        public override bool SetProcessDPIAware()
        {
            var callReference = new TestUtilities().Reference(this) + "SetProcessDPIAware";
            CallOrder.Add(callReference);
            if (SetProcessDPIAwareIndex < SetProcessDPIAwareReturn.Count)
                return SetProcessDPIAwareReturn[SetProcessDPIAwareIndex++];
            throw new IndexOutOfRangeException();
        }

        public int IsWindowCalls = 0;
        public int IsWindowIndex = 0;
        public List<IntPtr> IsWindowCallArg_hWnd = [];
        public List<bool> IsWindowReturn = [];
        public override bool IsWindow(IntPtr hWnd)
        {
            var callReference = new TestUtilities().Reference(this) + "IsWindow";
            CallOrder.Add(callReference);
            IsWindowCalls++;
            IsWindowCallArg_hWnd.Add(hWnd);
            if (IsWindowIndex < IsWindowCallArg_hWnd.Count)
                return IsWindowReturn[IsWindowIndex++];
            throw new IndexOutOfRangeException();
        }

        public int GetWindowLongCalls = 0;
        public int GetWindowLongIndex = 0;
        public List<nint> GetWindowLongCallArg_hWnd = [];
        public List<int> GetWindowLongCallArg_nIndex = [];
        public List<int> GetWindowLongReturn = [];
        public override int GetWindowLong(nint hWnd, int nIndex)
        {
            var callReference = new TestUtilities().Reference(this) + "GetWindowLong";
            CallOrder.Add(callReference);
            GetWindowLongCalls++;
            GetWindowLongCallArg_hWnd.Add(hWnd);
            GetWindowLongCallArg_nIndex.Add(nIndex);
            GetWindowLongReturn.Add(nIndex);
            if (GetWindowLongIndex < GetWindowLongReturn.Count)
                return GetWindowLongReturn[GetWindowLongIndex++];
            throw new IndexOutOfRangeException();
        }

        public int SetWindowLongCalls = 0;
        public int SetWindowLongIndex = 0;
        public List<nint> SetWindowLongCallArg_hWnd = [];
        public List<int> SetWindowLongCallArg_nIndex = [];
        public List<int> SetWindowLongCallArg_dwNewLong = [];
        public List<int> SetWindowLongReturn = [];
        public override int SetWindowLong(nint hWnd, int nIndex, int dwNewLong)
        {
            var callReference = new TestUtilities().Reference(this) + "SetWindowLong";
            CallOrder.Add(callReference);
            SetWindowLongCalls++;
            SetWindowLongCallArg_hWnd.Add(hWnd);
            SetWindowLongCallArg_nIndex.Add(nIndex);
            SetWindowLongCallArg_dwNewLong.Add(dwNewLong);
            SetWindowLongReturn.Add(nIndex);
            if (SetWindowLongIndex < SetWindowLongReturn.Count)
                return SetWindowLongReturn[SetWindowLongIndex++];
            throw new IndexOutOfRangeException();
        }
    }
}
