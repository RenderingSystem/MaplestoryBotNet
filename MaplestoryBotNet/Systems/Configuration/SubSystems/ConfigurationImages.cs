using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace MaplestoryBotNet.Systems.Configuration.SubSystems
{
    public class ConfigurationImages : AbstractConfiguration
    {
        public Dictionary<string, List<Image<Bgra32>>> AilmentImages = [];

        public Dictionary<string, Image<Bgra32>> MapIconImages = [];

        public override AbstractConfiguration Copy()
        {
            var configurationImages = new ConfigurationImages();
            foreach (var image in AilmentImages)
                configurationImages.AilmentImages.Add(image.Key, [.. image.Value]);
            foreach (var image in MapIconImages)
                configurationImages.MapIconImages.Add(image.Key, image.Value);
            return configurationImages;
        }
    }


    public abstract class AbstractConfigurationImagesDeserializer : AbstractDeserializer
    {
        public abstract ConfigurationImages DeserializeConfigurationImages(string jsonString);
    }


    public class ConfigurationImagesDeserializer : AbstractConfigurationImagesDeserializer
    {
        private AbstractMaplestoryBotImageLoader _imageLoader;

        private AbstractDirectoryFiles _directoryFiles;

        private AbstractMaplestoryBotConfigurationDeserializer _configurationDeserializer;

        public ConfigurationImagesDeserializer(
            AbstractMaplestoryBotImageLoader imageLoader,
            AbstractDirectoryFiles directoryFiles,
            AbstractMaplestoryBotConfigurationDeserializer configurationDeserializer
        )
        {
            _imageLoader = imageLoader;
            _directoryFiles = directoryFiles;
            _configurationDeserializer = configurationDeserializer;
        }

        public override object Deserialize(string data)
        {
            return DeserializeConfigurationImages(data);
        }

        public override ConfigurationImages DeserializeConfigurationImages(string jsonString)
        {
            var configuration = _configurationDeserializer.DeserializeBotConfiguration(jsonString);
            var configurationImages = new ConfigurationImages();
            foreach (var ailment in configuration.Ailments)
            {
                var images = new List<Image<Bgra32>>();
                var imageFileNames =_directoryFiles.Files(ailment.Value.ImageDirectory, "");
                foreach (var imageFileName in imageFileNames)
                {
                    var fullImageFilePath = ailment.Value.ImageDirectory + "/" + imageFileName;
                    images.Add(_imageLoader.LoadImage(fullImageFilePath));
                }
                configurationImages.AilmentImages.Add(ailment.Key, images);
            }
            foreach (var mapIcon in configuration.MapIcons)
            {
                var image = _imageLoader.LoadImage(mapIcon.Value.Image);
                configurationImages.MapIconImages.Add(mapIcon.Key, image);
            }
            return configurationImages;
        }
    }
}
