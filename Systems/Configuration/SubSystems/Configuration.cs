using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace MaplestoryBotNet.Systems.Configuration.SubSystems
{
    public class Resource : AbstractConfiguration
    {
        [JsonPropertyName("rect")]
        public int[] Rect { get; set; } = [0, 0, 0, 0];

        [JsonPropertyName("pixel")]
        public int[] Pixel { get; set; } = [0, 0];

        [JsonPropertyName("rgb")]
        public int[] Rgb { get; set; } = [0, 0, 0];

        [JsonPropertyName("key")]
        public string Key { get; set; } = "";

        [JsonPropertyName("active")]
        public bool Active { get; set; } = false;

        public override AbstractConfiguration Copy()
        {
            var resource =  new Resource();
            Rect.CopyTo(resource.Rect, 0);
            Pixel.CopyTo(resource.Pixel, 0);
            Rgb.CopyTo(resource.Rgb, 0);
            resource.Key = new string(Key);
            resource.Active = Active;
            return resource;
        }
    }


    public class Ailment : AbstractConfiguration
    {
        [JsonPropertyName("active")]
        public bool Active { get; set; } = false;

        [JsonPropertyName("active_delay")]
        public int ActiveDelay { get; set; } = 0;

        [JsonPropertyName("threshold")]
        public int Threshold { get; set; } = 0;

        [JsonPropertyName("macro_commands")]
        public string[] MacroCommands { get; set; } = [];

        [JsonPropertyName("image")]
        public string Image { get; set; } = "";

        [JsonPropertyName("static_rect")]
        public int[] StaticRect { get; set; } = [];

        public override AbstractConfiguration Copy()
        {
            var ailment = new Ailment();
            ailment.Active = Active;
            ailment.ActiveDelay = ActiveDelay;
            ailment.Threshold = Threshold;
            MacroCommands.CopyTo(ailment.MacroCommands, 0);
            ailment.Image = new string(Image);
            StaticRect.CopyTo(ailment.StaticRect, 0);
            return ailment;
        }
    }


    public class MapIcon : AbstractConfiguration
    {
        [JsonPropertyName("image")]
        public string Image { get; set; } = "";

        public override AbstractConfiguration Copy()
        {
            var mapIcon = new MapIcon();
            mapIcon.Image = new string(Image);
            return mapIcon;
        }
    }


    public class MaplestoryBotConfiguration : AbstractConfiguration
    {
        [JsonPropertyName("process_name")]
        public string ProcessName { get; set; } = "";

        [JsonPropertyName("hp")]
        public Resource Hp { get; set; } = new Resource();

        [JsonPropertyName("mp")]
        public Resource Mp { get; set; } = new Resource();

        [JsonPropertyName("ailments")]
        public Dictionary<string, Ailment> Ailments { get; set; } = new Dictionary<string, Ailment>();

        [JsonPropertyName("ailments_allcure_key")]
        public string AilmentsAllcureKey { get; set; } = "";

        [JsonPropertyName("map_icons")]
        public Dictionary<string, MapIcon> MapIcons { get; set; } = new Dictionary<string, MapIcon>();

        [JsonPropertyName("macro_directory")]
        public string MacroDirectory { get; set; } = "";

        [JsonPropertyName("map_directory")]
        public string MapDirectory { get; set; } = "";

        public override AbstractConfiguration Copy()
        {
            var configuration = new MaplestoryBotConfiguration();
            configuration.ProcessName = new string(ProcessName);
            configuration.Hp = (Resource)Hp.Copy();
            configuration.Mp = (Resource)Mp.Copy();
            foreach (var item in Ailments)
                configuration.Ailments.Add(new string(item.Key), (Ailment)item.Value.Copy());
            configuration.AilmentsAllcureKey = new string(AilmentsAllcureKey);
            foreach (var item in MapIcons)
                configuration.MapIcons.Add(new string(item.Key), (MapIcon)item.Value.Copy());
            configuration.MacroDirectory = new string(MacroDirectory);
            configuration.MapDirectory = new string(MapDirectory);
            return configuration;
        }
    }


    public abstract class AbstractMaplestoryBotConfigurationSerializer : AbstractSerializer
    {
        public abstract string SerializeBotConfiguration(MaplestoryBotConfiguration configuration);
    }


    public abstract class AbstractMaplestoryBotConfigurationDeserializer : AbstractDeserializer
    {
        public abstract MaplestoryBotConfiguration DeserializeBotConfiguration(string jsonString);
    }


    public abstract class AbstractMaplestoryBotImageLoader
    {
        public abstract Image<Bgra32> LoadImage(string imagePath);

    }


    public class MaplestoryBotConfigurationDeserializer : AbstractMaplestoryBotConfigurationDeserializer
    {
        public override MaplestoryBotConfiguration DeserializeBotConfiguration(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
            var result = JsonSerializer.Deserialize<MaplestoryBotConfiguration>(jsonString, options);
            return result!;
        }

        public override object Deserialize(string data)
        {
            return DeserializeBotConfiguration(data);
        }
    }


    public class MaplestoryBotConfigurationSerializer : AbstractMaplestoryBotConfigurationSerializer
    {
        public override string SerializeBotConfiguration(MaplestoryBotConfiguration configuration)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                IndentCharacter = ' ',
                IndentSize = 4
            };
            var result = JsonSerializer.Serialize(configuration, options);
            return result;
        }

        public override string Serialize(object obj)
        {
            return SerializeBotConfiguration((MaplestoryBotConfiguration) obj);
        }
    }


    public class MaplestoryBotImageLoader : AbstractMaplestoryBotImageLoader
    {
        public override Image<Bgra32> LoadImage(string imagePath)
        {
            return Image.Load<Bgra32>(imagePath);
        }
    }
}
