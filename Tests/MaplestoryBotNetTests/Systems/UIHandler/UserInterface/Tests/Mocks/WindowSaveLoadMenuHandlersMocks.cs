using MaplestoryBotNet.Systems.UIHandler;
using MaplestoryBotNet.Systems.UIHandler.UserInterface;
using MaplestoryBotNetTests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<string> PromptReturn = [];
        public override string Prompt(string initialDirectory)
        {
            CallOrder.Add(new TestUtilities().Reference(this) + "Prompt");
            PromptCalls++;
            PromptCallArg_initialDirectory.Add(initialDirectory);
            if (PromptIndex < PromptReturn.Count)
            {
                return PromptReturn[PromptIndex++];
            }
            throw new IndexOutOfRangeException();
        }
    }
}
