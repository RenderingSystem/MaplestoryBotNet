using MaplestoryBotNet.Systems.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace MaplestoryBotNetTests.Systems.Configuration.Tests
{
    public class MockMaplestoryBotConfigurationDeserializer : AbstractMaplestoryBotConfigurationDeserializer
    {
        public List<string> CallOrder { get; set; } = [];

        public int DeserializeCalls = 0;
        public int DeserializeIndex = 0;
        public List<string> DeserializeCallArg_jsonString = [];
        public List<MaplestoryBotConfiguration> DeserializeReturn = [];
        public override MaplestoryBotConfiguration Deserialize(string jsonString)
        {
            var callReference = new TestUtils().Reference(this) + "Deserialize";
            CallOrder.Add(callReference);
            DeserializeCalls++;
            DeserializeCallArg_jsonString.Add(jsonString);
            if (DeserializeIndex < DeserializeReturn.Count)
                return DeserializeReturn[DeserializeIndex++];
            throw new IndexOutOfRangeException();
        }
    }


    public class MockMaplestoryBotConfigurationSerializer : AbstractMaplestoryBotConfigurationSerializer
    {
        public List<string> CallOrder { get; set; } = [];

        public int SerializeCalls = 0;
        public int SerializeIndex = 0;
        public List<MaplestoryBotConfiguration> SerializeCallArg_configuration = [];
        public List<string> SerializeReturn = [];
        public override string Serialize(MaplestoryBotConfiguration configuration)
        {
            var callReference = new TestUtils().Reference(this) + "Serialize";
            CallOrder.Add(callReference);
            SerializeCalls++;
            if (SerializeIndex < SerializeReturn.Count)
                return SerializeReturn[SerializeIndex++];
            throw new IndexOutOfRangeException();
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
            var callReference = new TestUtils().Reference(this) + "LoadImage";
            CallOrder.Add(callReference);
            LoadImageCalls++;
            if (LoadImageIndex < LoadImageReturn.Count)
                return LoadImageReturn[LoadImageIndex++];
            throw new IndexOutOfRangeException(callReference);
        }
    }
}
