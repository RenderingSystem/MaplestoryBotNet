using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;



namespace MaplestoryBotNet.Systems.Configuration
{
    public class Resource
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
    }


    public class Ailment
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
    }


    public class MapIcon
    {
        [JsonPropertyName("image")]
        public string Image { get; set; } = "";
    }


    public class MaplestoryBotConfiguration
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
    }


    public abstract class AbstractMaplestoryBotConfigurationSerializer
    {
        public abstract string Serialize(MaplestoryBotConfiguration configuration);
    }


    public abstract class AbstractMaplestoryBotConfigurationDeserializer
    {
        public abstract MaplestoryBotConfiguration Deserialize(string jsonString);
    }


    public abstract class AbstractMaplestoryBotImageLoader
    {
        public abstract Image<Bgra32> LoadImage(string imagePath);

    }


    public class MaplestoryBotConfigurationDeserializer : AbstractMaplestoryBotConfigurationDeserializer
    {
        public override MaplestoryBotConfiguration Deserialize(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
            var result = JsonSerializer.Deserialize<MaplestoryBotConfiguration>(jsonString, options);
            Debug.Assert(result != null);
            return result;
        }
    }


    public class MaplestoryBotConfigurationSerializer : AbstractMaplestoryBotConfigurationSerializer
    {
        public override string Serialize(MaplestoryBotConfiguration configuration)
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
    }


    public class MaplestoryBotImageLoader : AbstractMaplestoryBotImageLoader
    {
        public override Image<Bgra32> LoadImage(string imagePath)
        {
            return Image.Load<Bgra32>(imagePath);
        }
    }
}
