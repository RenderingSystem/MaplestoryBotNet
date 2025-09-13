using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests.Mocks
{
    internal class MockConfigurationImagesDeserializer : AbstractConfigurationImagesDeserializer
    {
        List<string> CallOrder = [];

        public override object Deserialize(string data)
        {
            return DeserializeConfigurationImages(data);
        }

        public int DeserializeCalls = 0;
        public int DeserializeIndex = 0;
        public List<string> DeserializeCallArg_jsonString = new List<string>();
        public List<ConfigurationImages> DeserializeReturn = [];
        public override ConfigurationImages DeserializeConfigurationImages(string jsonString)
        {
            var callReference = new TestUtilities().Reference(this) + "Deserialize";
            CallOrder.Add(callReference);
            DeserializeCalls++;
            DeserializeCallArg_jsonString.Add(jsonString);
            if (DeserializeIndex < DeserializeReturn.Count)
                return DeserializeReturn[DeserializeIndex++];
            throw new IndexOutOfRangeException();
        }
    }
}
