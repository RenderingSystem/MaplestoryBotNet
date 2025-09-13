using MaplestoryBotNetTests.TestHelpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MaplestoryBotNet.Systems.Configuration.SubSystems;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests.Mocks
{
    public class MockMaplestoryBotConfigurationDeserializer : AbstractMaplestoryBotConfigurationDeserializer
    {
        public List<string> CallOrder { get; set; } = [];

        public int DeserializeCalls = 0;
        public int DeserializeIndex = 0;
        public List<string> DeserializeCallArg_jsonString = [];
        public List<MaplestoryBotConfiguration> DeserializeReturn = [];
        public override MaplestoryBotConfiguration DeserializeBotConfiguration(string jsonString)
        {
            var callReference = new TestUtilities().Reference(this) + "Deserialize";
            CallOrder.Add(callReference);
            DeserializeCalls++;
            DeserializeCallArg_jsonString.Add(jsonString);
            if (DeserializeIndex < DeserializeReturn.Count)
                return DeserializeReturn[DeserializeIndex++];
            throw new IndexOutOfRangeException();
        }

        public override object Deserialize(string data)
        {
            return DeserializeBotConfiguration(data);
        }
    }


    public class MockMaplestoryBotConfigurationSerializer : AbstractMaplestoryBotConfigurationSerializer
    {
        public List<string> CallOrder { get; set; } = [];

        public int SerializeCalls = 0;
        public int SerializeIndex = 0;
        public List<MaplestoryBotConfiguration> SerializeCallArg_configuration = [];
        public List<string> SerializeReturn = [];
        public override string SerializeBotConfiguration(MaplestoryBotConfiguration configuration)
        {
            var callReference = new TestUtilities().Reference(this) + "Serialize";
            CallOrder.Add(callReference);
            SerializeCalls++;
            if (SerializeIndex < SerializeReturn.Count)
                return SerializeReturn[SerializeIndex++];
            throw new IndexOutOfRangeException();
        }

        public override string Serialize(object obj)
        {
            return SerializeBotConfiguration((MaplestoryBotConfiguration)obj);
        }
    }


    public class MockMaplestoryBotImageLoader : AbstractMaplestoryBotImageLoader
    {
        public List<string> CallOrder { get; set; } = [];

        public int LoadImageCalls = 0;
        public int LoadImageIndex = 0;
        public List<MaplestoryBotConfiguration> LoadImageCallArg_imagePath = [];
        public List<Image<Bgra32>> LoadImageReturn = [];
        public override Image<Bgra32> LoadImage(string imagePath)
        {
            var callReference = new TestUtilities().Reference(this) + "LoadImage";
            CallOrder.Add(callReference);
            LoadImageCalls++;
            if (LoadImageIndex < LoadImageReturn.Count)
                return LoadImageReturn[LoadImageIndex++];
            throw new IndexOutOfRangeException(callReference);
        }
    }
}
