using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests.Mocks
{
    internal class MockConfiguration : AbstractConfiguration
    {
        public List<string> CallOrder = [];

        public int CopyCalls = 0;
        public int CopyIndex = 0;
        public List<AbstractConfiguration> CopyReturn = [];
        public override AbstractConfiguration Copy()
        {
            var callReference = new TestUtilities().Reference(this) + "Copy";
            CallOrder.Add(callReference);
            CopyCalls++;
            if (CopyIndex < CopyReturn.Count)
                return CopyReturn[CopyIndex++];
            throw new IndexOutOfRangeException();
        }
    }
}
