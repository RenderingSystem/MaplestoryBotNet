using MaplestoryBotNet.Systems;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Tests
{
    public class MockSystem : AbstractSystem
    {
        public List<string> CallOrder = [];

        public int InitializeSystemCalls = 0;
        public override void Initialize()
        {
            var callReference = new TestUtilities().Reference(this) + "InitializeSystem";
            CallOrder.Add(callReference);
            InitializeSystemCalls++;
        }

        public int StartSystemCalls = 0;
        public override void Start()
        {
            var callReference = new TestUtilities().Reference(this) + "StartSystem";
            CallOrder.Add(callReference);
            StartSystemCalls++;
        }

        public int UpdateSystemCalls = 0;
        public override void Update()
        {
            var callReference = new TestUtilities().Reference(this) + "UpdateSystem";
            CallOrder.Add(callReference);
            UpdateSystemCalls++;
        }

        public int InjectCalls = 0;
        public List<SystemInjectType> InjectCallArg_dataType = [];
        public List<object?> InjectCallArg_data = [];
        public override void Inject(SystemInjectType dataType, object? data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }

        public int StateCalls = 0;
        public int StateIndex = 0;
        public List<object?> StateReturn = [];
        public override object? State()
        {
            var callReference = new TestUtilities().Reference(this) + "State";
            CallOrder.Add(callReference);
            StateCalls++;
            if (StateIndex < StateReturn.Count)
                return StateReturn[StateIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockSystemBuilder : AbstractSystemBuilder
    {
        public List<string> CallOrder = [];

        public int WithArgCalls = 0;
        public List<object> WithArgCallArg_arg = [];
        public override AbstractSystemBuilder WithArg(object arg)
        {
            var callReference = new TestUtilities().Reference(this) + "WithArg";
            CallOrder.Add(callReference);
            WithArgCalls++;
            WithArgCallArg_arg.Add(arg);
            return this;
        }

        public int BuildCalls = 0;
        public int BuildIndex = 0;
        public List<AbstractSystem> BuildReturn = [];
        public override AbstractSystem Build()
        {
            var callReference = new TestUtilities().Reference(this) + "Build";
            CallOrder.Add(callReference);
            BuildCalls++;
            if (BuildIndex < BuildReturn.Count)
                return BuildReturn[BuildIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockInjector : AbstractInjector
    {
        public List<string> CallOrder = [];

        public int InjectCalls = 0;
        public List<SystemInjectType> InjectCallArg_dataType = [];
        public List<object?> InjectCallArg_data = [];
        public override void Inject(SystemInjectType dataType, object? data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }
    }


    public class MockSystemInfoList : AbstractSubSystemInfoList
    {
        public List<string> CallOrder = [];

        public int GetSubSystemInfoCalls = 0;
        public int GetSubSystemInfoIndex = 0;
        public List<List<SystemInformation>> GetSubSystemInfoReturn = [];
        public override List<SystemInformation> GetSubSystemInfo()
        {
            var callReference = new TestUtilities().Reference(this) + "GetSubSystemInfo";
            CallOrder.Add(callReference);
            GetSubSystemInfoCalls++;
            if (GetSubSystemInfoIndex < GetSubSystemInfoReturn.Count)
                return GetSubSystemInfoReturn[GetSubSystemInfoIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockApplication : AbstractApplication
    {
        public List<string> CallOrder = [];

        public int LaunchCalls = 0;
        public List<List<string>> LaunchCallArg_args = [];
        public override void Launch(List<string> args)
        {
            var callReference = new TestUtilities().Reference(this) + "Launch";
            CallOrder.Add(callReference);
            LaunchCallArg_args.Add(args);
            LaunchCalls++;
        }

        public int ShutDownCalls = 0;
        public override void ShutDown()
        {
            var callReference = new TestUtilities().Reference(this) + "ShutDown";
            CallOrder.Add(callReference);
            ShutDownCalls++;
        }

        public int SystemCalls = 0;
        public int SystemIndex = 0;
        public List<AbstractSystem> SystemReturn = [];
        public override AbstractSystem System()
        {
            var callReference = new TestUtilities().Reference(this) + "System";
            CallOrder.Add(callReference);
            SystemCalls++;
            if (SystemIndex < SystemReturn.Count)
                return SystemReturn[SystemIndex++];
            throw new IndexOutOfRangeException();
        }
    }

}
