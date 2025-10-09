using MaplestoryBotNet.UserInterface;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.UserInterface.Tests.Mocks
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
        public override void OnEvent(object sender, EventArgs e)
        {
            var callReference = new TestUtilities().Reference(this) + "OnEvent";
            CallOrder.Add(callReference);
            OnEventCalls++;
            OnEventCallArg_sender.Add(sender);
            OnEventCallArg_e.Add(e);
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
}
