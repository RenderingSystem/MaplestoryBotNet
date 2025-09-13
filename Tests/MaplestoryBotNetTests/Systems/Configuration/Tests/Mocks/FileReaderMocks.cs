using MaplestoryBotNet.Systems.Configuration.SubSystems;
using MaplestoryBotNetTests.TestHelpers;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests.Mocks
{
    public class MockSerializer : AbstractSerializer
    {
        public List<string> CallOrder = [];

        public int SerializeCalls = 0;
        public int SerializeIndex = 0;
        public List<object> SerializeCallArg_obj = [];
        public List<string> SerializeReturn = [];
        public override string Serialize(object obj)
        {
            var callReference = new TestUtilities().Reference(this) + "Serialize";
            CallOrder.Add(callReference);
            SerializeCalls++;
            SerializeCallArg_obj.Add(obj);
            if (SerializeIndex < SerializeReturn.Count)
                return SerializeReturn[SerializeIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockDeserializer : AbstractDeserializer
    {
        public List<string> CallOrder = [];

        public int DeserializeCalls = 0;
        public int DeserializeIndex = 0;
        public List<string> DeserializeCallArg_data = [];
        public List<object> DeserializeReturn = [];
        public override object Deserialize(string data)
        {
            var callReference = new TestUtilities().Reference(this) + "Deserialize";
            CallOrder.Add(callReference);
            DeserializeCalls++;
            DeserializeCallArg_data.Add(data);
            if (DeserializeIndex < DeserializeReturn.Count)
                return DeserializeReturn[DeserializeIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockFileReader : AbstractFileReader
    {
        public List<string> CallOrder = [];

        public int ReadFileCalls = 0;
        public int ReadFileIndex = 0;
        public List<string> ReadFileCallArg_filePath = [];
        public List<string> ReadFileReturn = [];
        public override string ReadFile(string filePath)
        {
            var callReference = new TestUtilities().Reference(this) + "ReadFile";
            CallOrder.Add(callReference);
            ReadFileCalls++;
            ReadFileCallArg_filePath.Add(filePath);
            if (ReadFileIndex < ReadFileReturn.Count)
                return ReadFileReturn[ReadFileIndex++];
            throw new IndexOutOfRangeException();
        }
    }
}
