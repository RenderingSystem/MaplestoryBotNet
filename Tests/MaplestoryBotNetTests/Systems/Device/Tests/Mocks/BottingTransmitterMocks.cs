using MaplestoryBotNet.Systems.Device.SubSystems;
using MaplestoryBotNet.Systems.Device.SubSystems.Transmitters;
using MaplestoryBotNet.Systems.UIHandler.Utilities.Models;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Device.Tests.Mocks
{
    public class MockMacroCommandsExecutorBuilder : AbstractMacroCommandsExecutorBuilder
    {
        public List<string> CallOrder = [];

        public int BuildCalls = 0;
        public int BuildIndex = 0;
        public List<AbstractMacroCommandsExecutor> BuildReturn = [];
        public override AbstractMacroCommandsExecutor Build()
        {
            var callReference = new TestUtilities().Reference(this) + "Build";
            CallOrder.Add(callReference);
            BuildCalls++;
            if (BuildIndex < BuildReturn.Count)
            {
                return BuildReturn[BuildIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int WithArgCalls = 0;
        public List<object> WithArgCallArg_arg = [];
        public override AbstractMacroCommandsExecutorBuilder WithArg(object arg)
        {
            var callReference = new TestUtilities().Reference(this) + "WithArg";
            CallOrder.Add(callReference);
            WithArgCalls++;
            WithArgCallArg_arg.Add(arg);
            return this;
        }
    }


    public class MockKeystrokeTransmitterThreadHelper : AbstractKeystrokeTransmitterThreadHelper
    {
        public List<string> CallOrder = [];

        public int InjectCalls = 0;
        public List<object> InjectCallArg_dataType = [];
        public List<object?> InjectCallArg_data = [];
        public override void Inject(object dataType, object? data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }

        public int TransmitCalls = 0;
        public int TransmitIndex = 0;
        public List<bool> TransmitReturn = [];
        public override bool Transmit()
        {
            var callReference = new TestUtilities().Reference(this) + "Transmit";
            CallOrder.Add(callReference);
            TransmitCalls++;
            if (TransmitIndex < TransmitReturn.Count)
            {
                return TransmitReturn[TransmitIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int ResetCalls = 0;
        public override void Reset()
        {
            var callReference = new TestUtilities().Reference(this) + "Reset";
            CallOrder.Add(callReference);
            ResetCalls++;
        }
    }


    public class MockKeystrokeTransmitterThreadState : AbstractKeystrokeTransmitterThreadState
    {
        public List<string> CallOrder = [];

        public int GetStateCalls = 0;
        public int GetStateIndex = 0;
        public List<int> GetStateReturn = [];
        public override int GetState()
        {
            var callReference = new TestUtilities().Reference(this) + "GetState";
            CallOrder.Add(callReference);
            GetStateCalls++;
            if (GetStateIndex < GetStateReturn.Count)
            {
                return GetStateReturn[GetStateIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int SetStateCalls = 0;
        public List<int> SetStateCallArg_state = [];
        public override void SetState(int state)
        {
            var callReference = new TestUtilities().Reference(this) + "SetState";
            CallOrder.Add(callReference);
            SetStateCalls++;
            SetStateCallArg_state.Add(state);
        }

        public int TypeCalls = 0;
        public int TypeIndex = 0;
        public List<KeystrokeTransmitterThreadType> TypeReturn = [];
        public override KeystrokeTransmitterThreadType Type()
        {
            var callReference = new TestUtilities().Reference(this) + "Type";
            CallOrder.Add(callReference);
            TypeCalls++;
            if (TypeIndex < TypeReturn.Count)
            {
                return TypeReturn[TypeIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }


    public class MockSkillMacroCommandsSelector : AbstractSkillMacroCommandsSelector
    {
        public List<string> CallOrder = [];

        public int ClearCalls = 0;
        public override void Clear()
        {
            var callReference = new TestUtilities().Reference(this) + "Clear";
            CallOrder.Add(callReference);
            ClearCalls++;
        }

        public int SelectCalls = 0;
        public int SelectIndex = 0;
        public List<AbstractSkillsModel> SelectCallArg_skillsModel = [];
        public List<List<string>> SelectReturn = [];
        public override List<string> Select(AbstractSkillsModel skillsModel)
        {
            var callReference = new TestUtilities().Reference(this) + "Select";
            CallOrder.Add(callReference);
            SelectCalls++;
            SelectCallArg_skillsModel.Add(skillsModel);
            if (SelectIndex < SelectReturn.Count)
            {
                return SelectReturn[SelectIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int UpdateCalls = 0;
        public List<AbstractSkillsModel> UpdateCallArg_skillsModel = [];
        public override void Update(AbstractSkillsModel skillsModel)
        {
            var callReference = new TestUtilities().Reference(this) + "Update";
            CallOrder.Add(callReference);
            UpdateCalls++;
            UpdateCallArg_skillsModel.Add(skillsModel);
        }
    }


    public class MockBottingCommandsExecutor : AbstractBottingCommandsExecutor
    {
        public List<string> CallOrder = [];

        public int ExecuteCalls = 0;
        public int ExecuteIndex = 0;
        public List<bool> ExecuteReturn = [];
        public override bool Execute()
        {
            var callReference = new TestUtilities().Reference(this) + "Execute";
            CallOrder.Add(callReference);
            ExecuteCalls++;
            if (ExecuteIndex < ExecuteReturn.Count)
            {
                return ExecuteReturn[ExecuteIndex++];
            }
            throw new IndexOutOfRangeException();
        }

        public int InjectCalls = 0;
        public List<object> InjectCallArg_dataType = [];
        public List<object?> InjectCallArg_data = [];
        public override void Inject(object dataType, object? data)
        {
            var callReference = new TestUtilities().Reference(this) + "Inject";
            CallOrder.Add(callReference);
            InjectCalls++;
            InjectCallArg_dataType.Add(dataType);
            InjectCallArg_data.Add(data);
        }
    }
}
