using MaplestoryBotNet.Systems;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks
{
    public class MockWindowStateModifier : AbstractWindowStateModifier
    {
        public List<string> CallOrder = [];

        public int InitializeCalls = 0;
        public override void Initialize()
        {
            var callReference = new TestUtilities().Reference(this) + "Initialize";
            CallOrder.Add(callReference);
            InitializeCalls++;
        }

        public int ModifyCalls = 0;
        public int ModifyIndex = 0;
        public List<object?> ModifyCallArg_value = [];
        public override void Modify(object? value)
        {
            var callReference = new TestUtilities().Reference(this) + "Modify";
            CallOrder.Add(callReference);
            ModifyCalls++;
            ModifyCallArg_value.Add(value);
        }

        public int StateCalls = 0;
        public int StateIndex = 0;
        public List<int> StateCallArg_stateType = [];
        public List<object?> StateReturn = [];
        public override object? State(int stateType)
        {
            var callReference = new TestUtilities().Reference(this) + "State";
            CallOrder.Add(callReference);
            StateCalls++;
            StateCallArg_stateType.Add(stateType);
            if (StateIndex < StateCallArg_stateType.Count)
                return StateReturn[StateIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockWindowActionHandler : AbstractWindowActionHandler
    {
        public List<string> CallOrder = [];

        public int ModifierCalls = 0;
        public int ModifierIndex = 0;
        public List<AbstractWindowStateModifier> ModifierReturn = [];
        public override AbstractWindowStateModifier Modifier()
        {
            var callReference = new TestUtilities().Reference(this) + "Modifier";
            CallOrder.Add(callReference);
            ModifierCalls++;
            if (ModifierIndex < ModifierReturn.Count)
                return ModifierReturn[ModifierIndex++];
            throw new IndexOutOfRangeException();
        }

        public int OnEventCalls = 0;
        public List<object> OnEventCallArg_sender = [];
        public List<EventArgs> OnEventCallArg_e = [];
        public override void OnEvent(object? sender, EventArgs e)
        {
            var callReference = new TestUtilities().Reference(this) + "OnEvent";
            CallOrder.Add(callReference);
            OnEventCalls++;
            OnEventCallArg_sender.Add(sender);
            OnEventCallArg_e.Add(e);
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
    }


    public class MockWindowActionHandlerBuilder : AbstractWindowActionHandlerBuilder
    {
        public List<string> CallOrder = [];

        public int BuildCalls = 0;
        public int BuildIndex = 0;
        public List<AbstractWindowActionHandler> BuildReturn = [];
        public override AbstractWindowActionHandler Build()
        {
            var callReference = new TestUtilities().Reference(this) + "Build";
            CallOrder.Add(callReference);
            BuildCalls++;
            if (BuildIndex < BuildReturn.Count)
                return BuildReturn[BuildIndex++];
            throw new IndexOutOfRangeException();
        }

        public int WIthArgsCalls = 0;
        public List<object?> WIthArgsCallArg_args = [];
        public override AbstractWindowActionHandlerBuilder WithArgs(object? args)
        {
            var callReference = new TestUtilities().Reference(this) + "WithArgs";
            CallOrder.Add(callReference);
            WIthArgsCallArg_args.Add(args);
            return this;
        }
    }


    public class MockWindowActionHandlerRegistry : AbstractWindowActionHandlerRegistry
    {
        public List<string> CallOrder = [];

        public int RegisterHandlerCalls = 0;
        public List<object?> RegisterHandlerCallArg_args = [];
        public override void RegisterHandler(object? args)
        {
            var callReference = new TestUtilities().Reference(this) + "RegisterHandler";
            CallOrder.Add(callReference);
            RegisterHandlerCalls++;
            RegisterHandlerCallArg_args.Add(args);
        }

        public int ClearHandlersCalls = 0;
        public override void ClearHandlers()
        {
            var callReference = new TestUtilities().Reference(this) + "ClearHandlers";
            CallOrder.Add(callReference);
            ClearHandlersCalls++;
        }

        public int GetHandlersCalls = 0;
        public int GetHandlersIndex = 0;
        public List<List<AbstractWindowActionHandler>> GetHandlersReturn = [];
        public override List<AbstractWindowActionHandler> GetHandlers()
        {
            var callReference = new TestUtilities().Reference(this) + "GetHandlers";
            CallOrder.Add(callReference);
            GetHandlersCalls++;
            if (GetHandlersIndex < GetHandlersReturn.Count)
                return GetHandlersReturn[GetHandlersIndex++];
            throw new IndexOutOfRangeException();
        }
    }
}
