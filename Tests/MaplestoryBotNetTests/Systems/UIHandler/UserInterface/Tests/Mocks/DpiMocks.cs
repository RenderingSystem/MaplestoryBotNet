using MaplestoryBotNet.Systems.UIHandler.Utilities;
using MaplestoryBotNetTests.TestHelpers;
using System.Windows;


namespace MaplestoryBotNetTests.Systems.UIHandler.UserInterface.Tests.Mocks
{
    public class MockDpi : AbstractDpi
    {
        public List<string> CallOrder = [];

        public int GetDpiCalls = 0;
        public int GetDpiIndex = 0;
        public List<Window> GetDpiCallArg_window = [];
        public List<Tuple<double, double>> GetDpiReturn = [];
        public override Tuple<double, double> GetDpi(Window window)
        {
            CallOrder.Add(new TestUtilities().Reference(this) + "GetDpi");
            GetDpiCalls++;
            GetDpiCallArg_window.Add(window);
            if (GetDpiIndex < GetDpiReturn.Count)
            {
                return GetDpiReturn[GetDpiIndex++];
            }
            throw new NotImplementedException();
        }
    }
}
