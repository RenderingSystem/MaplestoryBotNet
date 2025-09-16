using Interception;
using MaplestoryBotNet.LibraryWrappers;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.LibraryWrappers.Tests
{
    public class MockInterceptionLibrary : AbstractInterceptionLibrary
    {
        public List<string> CallOrder = [];

        public int CreateContextCalls = 0;
        public int CreateContextIndex = 0;
        public List<nint> CreateContextReturn = [];
        public override nint CreateContext()
        {
            var callReference = new TestUtilities().Reference(this) + "CreateContext";
            CallOrder.Add(callReference);
            CreateContextCalls++;
            if (CreateContextIndex < CreateContextReturn.Count)
                return CreateContextReturn[CreateContextIndex++];
            throw new IndexOutOfRangeException();
        }

        public int DestroyContextCalls = 0;
        public List<nint> DestroyContextCallArg_context = [];
        public override void DestroyContext(nint context)
        {
            var callReference = new TestUtilities().Reference(this) + "DestroyContext";
            CallOrder.Add(callReference);
            DestroyContextCalls++;
            DestroyContextCallArg_context.Add(context);
        }

        public int GetFilterCalls = 0;
        public int GetFilterIndex = 0;
        public List<nint> GetFilterCallArg_context = [];
        public List<int> GetFilterCallArg_device = [];
        public List<InterceptionInterop.Filter> GetFilterReturn = [];
        public override InterceptionInterop.Filter GetFilter(nint context, int device)
        {
            var callReference = new TestUtilities().Reference(this) + "GetFilter";
            CallOrder.Add(callReference);
            GetFilterCalls++;
            GetFilterCallArg_context.Add(context);
            GetFilterCallArg_device.Add(device);
            if (GetFilterIndex < GetFilterReturn.Count)
                return GetFilterReturn[GetFilterIndex++];
            throw new IndexOutOfRangeException();
        }

        public int GetHardwareIdCalls = 0;
        public int GetHardwareIdIndex = 0;
        public List<nint> GetHardwareIdCallArg_context = [];
        public List<int> GetHardwareIdCallArg_device = [];
        public List<nint> GetHardwareIdCallArg_hardware_id_buffer = [];
        public List<uint> GetHardwareIdCallArg_buffer_size = [];
        public List<int> GetHardwareIdReturn = [];
        public override unsafe int GetHardwareId(
            nint context,
            int device,
            void* hardware_id_buffer,
            uint buffer_size
        )
        {
            var callReference = new TestUtilities().Reference(this) + "GetHardwareId";
            CallOrder.Add(callReference);
            GetHardwareIdCalls++;
            GetHardwareIdCallArg_context.Add(context);
            GetHardwareIdCallArg_device.Add(device);
            GetHardwareIdCallArg_hardware_id_buffer.Add((nint)hardware_id_buffer);
            GetHardwareIdCallArg_buffer_size.Add(buffer_size);
            if (GetHardwareIdIndex < GetHardwareIdReturn.Count)
                return GetHardwareIdReturn[GetHardwareIdIndex++];
            throw new IndexOutOfRangeException();
        }

        public int GetPrecedenceCalls = 0;
        public int GetPrecedenceIndex = 0;
        public List<nint> GetPrecedenceCallArg_context = [];
        public List<int> GetPrecedenceCallArg_device = [];
        public List<int> GetPrecedenceReturn = [];
        public override int GetPrecedence(nint context, int device)
        {
            var callReference = new TestUtilities().Reference(this) + "GetPrecedence";
            CallOrder.Add(callReference);
            GetPrecedenceCalls++;
            GetPrecedenceCallArg_context.Add(context);
            GetPrecedenceCallArg_device.Add(device);
            if (GetPrecedenceIndex < GetPrecedenceReturn.Count)
                return GetPrecedenceReturn[GetPrecedenceIndex++];
            throw new IndexOutOfRangeException();
        }

        public int ReceiveCalls = 0;
        public int ReceiveIndex = 0;
        public List<nint> ReceiveCallArg_context = [];
        public List<int> ReceiveCallArg_device = [];
        public List<nint> ReceiveCallArg_stroke = [];
        public List<uint> ReceiveCallArg_nstroke = [];
        public List<int> ReceiveReturn = [];
        public override unsafe int Receive(
            nint context,
            int device,
            InterceptionInterop.Stroke* stroke,
            uint nstroke
        )
        {
            var callReference = new TestUtilities().Reference(this) + "Receive";
            CallOrder.Add(callReference);
            ReceiveCalls++;
            ReceiveCallArg_context.Add(context);
            ReceiveCallArg_device.Add(device);
            ReceiveCallArg_stroke.Add((nint)stroke);
            ReceiveCallArg_nstroke.Add(nstroke);
            if (ReceiveIndex < ReceiveCallArg_context.Count)
                return ReceiveReturn[ReceiveIndex++];
            throw new IndexOutOfRangeException();
        }

        public int SendCalls = 0;
        public int SendIndex = 0;
        public List<nint> SendCallArg_context = [];
        public List<int> SendCallArg_device = [];
        public List<InterceptionInterop.Stroke> SendCallArg_stroke = [];
        public List<int> SendCallArg_nstroke = [];
        public List<int> SendReturn = [];
        public override unsafe int Send(
            nint context,
            int device,
            InterceptionInterop.Stroke* stroke,
            int nstroke
        )
        {
            var callReference = new TestUtilities().Reference(this) + "Send";
            CallOrder.Add(callReference);
            SendCalls++;
            SendCallArg_context.Add(context);
            SendCallArg_device.Add(device);
            SendCallArg_stroke.Add(*stroke);
            SendCallArg_nstroke.Add(nstroke);
            if (SendIndex < SendCallArg_context.Count)
                return SendReturn[SendIndex++];
            throw new IndexOutOfRangeException();
        }

        public int SetFilterCalls = 0;
        public List<nint> SetFilterCallArg_context = [];
        public List<InterceptionInterop.Predicate> SetFilterCallArg_interception_predicate = [];
        public List<InterceptionInterop.Filter> SetFilterCallArg_filter = [];
        public override void SetFilter(
            nint context,
            InterceptionInterop.Predicate interception_predicate,
            InterceptionInterop.Filter filter
        )
        {
            var callReference = new TestUtilities().Reference(this) + "SetFilter";
            CallOrder.Add(callReference);
            SetFilterCalls++;
            SetFilterCallArg_context.Add(context);
            SetFilterCallArg_interception_predicate.Add(interception_predicate);
            SetFilterCallArg_filter.Add(filter);
        }

        public int SetPrecedenceCalls = 0;
        public List<nint> SetPrecedenceCallArg_context = [];
        public List<int> SetPrecedenceCallArg_device = [];
        public List<int> SetPrecedenceCallArg_precedence = [];
        public override void SetPrecedence(
            nint context, int device, int precedence
        )
        {
            var callReference = new TestUtilities().Reference(this) + "SetPrecedence";
            CallOrder.Add(callReference);
            SetPrecedenceCalls++;
            SetPrecedenceCallArg_context.Add(context);
            SetPrecedenceCallArg_device.Add(device);
            SetPrecedenceCallArg_precedence.Add(precedence);
        }

        public int WaitWithTimeoutCalls = 0;
        public int WaitWithTimeoutIndex = 0;
        public List<nint> WaitWithTimeoutCallArg_context = [];
        public List<int> WaitWithTimeoutCallArg_milliseconds = [];
        public List<int> WaitWithTimeoutReturn = [];
        public override int WaitWithTimeout(nint context, int milliseconds)
        {
            var callReference = new TestUtilities().Reference(this) + "WaitWithTimeout";
            CallOrder.Add(callReference);
            WaitWithTimeoutCalls++;
            WaitWithTimeoutCallArg_context.Add(context);
            WaitWithTimeoutCallArg_milliseconds.Add(milliseconds);
            if (WaitWithTimeoutIndex < WaitWithTimeoutReturn.Count)
                return WaitWithTimeoutReturn[WaitWithTimeoutIndex++];
            throw new IndexOutOfRangeException();
        }
    }
}
