using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests.Mocks
{
    public class MockKeyboardMappingDeserializer : AbstractKeyboardMappingDeserializer
    {
        public List<string> CallOrder = [];

        public override object Deserialize(string data)
        {
            return DeserializeKeyboardMapping(data);
        }

        public int DeserializeCalls = 0;
        public int DeserializeIndex = 0;
        public List<string> DeserializeCallArg_jsonString = [];
        public List<KeyboardMapping> DeserializeReturn = [];
        public override KeyboardMapping DeserializeKeyboardMapping(string jsonString)
        {
            var callReference = new TestUtilities().Reference(this) + "Deserialize";
            CallOrder.Add(callReference);
            DeserializeCalls++;
            if (DeserializeIndex < DeserializeReturn.Count)
                return DeserializeReturn[DeserializeIndex++];
            throw new IndexOutOfRangeException();
        }
    }
}
