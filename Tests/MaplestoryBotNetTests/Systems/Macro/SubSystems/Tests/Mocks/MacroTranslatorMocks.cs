using MaplestoryBotNet.Systems.Macro.SubSystems;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Macro.SubSystems.Tests.Mocks
{
    public class MockMacroSleeper : AbstractMacroSleeper
    {
        public List<string> CallOrder = [];

        public int SleepCalls = 0;
        public List<int> SleepCallArg_milliseconds = [];
        public override void Sleep(int milliseconds)
        {
            var callReference = new TestUtilities().Reference(this) + "Sleep";
            CallOrder.Add(callReference);
            SleepCallArg_milliseconds.Add(milliseconds);
            SleepCalls++;
        }
    }


    public class MockMacroRandom : AbstractMacroRandom
    {
        public List<string> CallOrder = [];

        public int NextCalls = 0;
        public int NextIndex = 0;
        public List<int> NextCallArg_minValue = [];
        public List<int> NextCallArg_maxValue = [];
        public List<int> NextReturn = [];
        public override int Next(int minValue, int maxValue)
        {
            var callReference = new TestUtilities().Reference(this) + "Next";
            CallOrder.Add(callReference);
            NextCalls++;
            NextCallArg_minValue.Add(minValue);
            NextCallArg_maxValue.Add(maxValue);
            if (NextIndex < NextReturn.Count)
                return NextReturn[NextIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockMacroAction : AbstractMacroAction
    {
        public List<string> CallOrder = [];

        public int ExecuteCalls = 0;
        public override void Execute()
        {
            var callReference = new TestUtilities().Reference(this) + "Execute";
            CallOrder.Add(callReference);
            ExecuteCalls++;
        }
    }


    public class MockMacroTranslator : AbstractMacroTranslator
    {
        public List<string> CallOrder = [];

        public int TranslateCalls = 0;
        public int TranslateIndex = 0;
        public List<string> TranslateCallArg_macroText = [];
        public List<List<AbstractMacroAction>> TranslateReturn = [];
        public override List<AbstractMacroAction> Translate(string macroText)
        {
            var callReference = new TestUtilities().Reference(this) + "Translate";
            CallOrder.Add(callReference);
            TranslateCalls++;
            TranslateCallArg_macroText.Add(macroText);
            if (TranslateIndex < TranslateReturn.Count)
                return TranslateReturn[TranslateIndex++];
            throw new IndexOutOfRangeException();
        }
    }
}
