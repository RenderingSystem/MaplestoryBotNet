
using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks
{
    public class MockSaveFileDialog : AbstractSaveFileDialog
    {
        public List<string> CallOrder = [];

        public int PromptCalls = 0;
        public List<string> PromptCallArg_initialDirectory = [];
        public List<string> PromptCallArg_saveContent = [];
        public override void Prompt(string initialDirectory, string saveContent)
        {
            CallOrder.Add(new TestUtilities().Reference(this) + "Prompt");
            PromptCalls++;
            PromptCallArg_initialDirectory.Add(initialDirectory);
            PromptCallArg_saveContent.Add(saveContent);
        }
    }


    public class MockLoadFileDialog : AbstractLoadFileDialog
    {
        public List<string> CallOrder = [];

        public int PromptCalls = 0;
        public int PromptIndex = 0;
        public List<string> PromptCallArg_initialDirectory = [];
        public override void Prompt(string initialDirectory)
        {
            CallOrder.Add(new TestUtilities().Reference(this) + "Prompt");
            PromptCalls++;
            PromptCallArg_initialDirectory.Add(initialDirectory);
        }

        public int InvokeFileLoadedCalls = 0;
        public List<string> InvokeFileLoadedCallArg_filePath = [];
        public List<string> InvokeFileLoadedCallArg_loadContent = [];
        public override void InvokeFileLoaded(string filePath, string loadContent)
        {
            CallOrder.Add(new TestUtilities().Reference(this) + "InvokeFileLoaded");
            InvokeFileLoadedCalls++;
            InvokeFileLoadedCallArg_filePath.Add(filePath);
            InvokeFileLoadedCallArg_loadContent.Add(loadContent);
        }
    }
}
