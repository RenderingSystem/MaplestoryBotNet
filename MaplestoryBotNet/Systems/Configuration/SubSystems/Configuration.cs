using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace MaplestoryBotNet.Systems.Configuration.SubSystems
{
    public static class MapIconInfo
    {
        public const string Character = "character";

        public const string Rune = "rune";

        public const float DefaultThreshold = 0.6f;
    }


    public class Resource : AbstractConfiguration
    {
        [JsonPropertyName("rect")]
        public int[] Rect { get; set; } = [0, 0, 0, 0];

        [JsonPropertyName("pixel")]
        public int[] Pixel { get; set; } = [0, 0];

        [JsonPropertyName("rgb")]
        public int[] Rgb { get; set; } = [0, 0, 0];

        [JsonPropertyName("tolerance")]
        public int[] Tolerance { get; set; } = [0, 0, 0];

        [JsonPropertyName("key")]
        public string Key { get; set; } = "";

        [JsonPropertyName("active")]
        public int Active { get; set; } = 0;

        public override AbstractConfiguration Copy()
        {
            var resource =  new Resource();
            Rect.CopyTo(resource.Rect, 0);
            Pixel.CopyTo(resource.Pixel, 0);
            Rgb.CopyTo(resource.Rgb, 0);
            Tolerance.CopyTo(resource.Tolerance, 0);
            resource.Key = new string(Key);
            resource.Active = Active;
            return resource;
        }
    }


    public class Consumable : AbstractConfiguration
    {
        [JsonPropertyName("name")]
        public string Name { set; get; } = "";

        [JsonPropertyName("min_delay")]
        public int MinDelay { set; get; } = 0;

        [JsonPropertyName("max_delay")]
        public int MaxDelay { set; get; } = 0;

        [JsonPropertyName("key")]
        public string Key { set; get; } = "";

        [JsonPropertyName("active")]
        public int Active { get; set; } = 0;

        public override AbstractConfiguration Copy()
        {
            return new Consumable
            {
                Name = Name,
                MinDelay = MinDelay,
                MaxDelay = MaxDelay,
                Key = Key,
                Active = Active
            };
        }
    }


    public class Ailment : AbstractConfiguration
    {
        [JsonPropertyName("image_directory")]
        public string ImageDirectory { get; set; } = "";

        [JsonPropertyName("active")]
        public int Active { get; set; } = 0;

        [JsonPropertyName("active_delay")]
        public int ActiveDelay { get; set; } = 0;

        [JsonPropertyName("check_delay")]
        public int CheckDelay { get; set; } = 0;

        [JsonPropertyName("threshold")]
        public int Threshold { get; set; } = 0;

        [JsonPropertyName("overlap")]
        public double Overlap { get; set; } = 0;

        [JsonPropertyName("static_rect")]
        public List<int>? StaticRect { get; set; } = null;

        [JsonPropertyName("all_cure")]
        public int? AllCure { get; set; } = null;

        [JsonPropertyName("arrow_keys")]
        public int? ArrowKeys { get; set; } = null;

        [JsonPropertyName("stop_bot")]
        public int? StopBot { get; set; } = null;

        public override AbstractConfiguration Copy()
        {
            var ailment = new Ailment
            {
                ImageDirectory = new string(ImageDirectory),
                Active = Active,
                ActiveDelay = ActiveDelay,
                CheckDelay = CheckDelay,
                Threshold = Threshold,
                Overlap = Overlap,
                StaticRect = StaticRect == null ? null : [.. StaticRect],
                AllCure = AllCure,
                ArrowKeys = ArrowKeys,
                StopBot = StopBot
            };
            return ailment;
        }
    }


    public class MapIcon : AbstractConfiguration
    {
        [JsonPropertyName("image")]
        public string Image { get; set; } = "";

        [JsonPropertyName("check_frequency")]
        public float Frequency { get; set; } = 0.0f;

        [JsonPropertyName("overlap")]
        public float Overlap { get; set; } = 0.0f;

        public override AbstractConfiguration Copy()
        {
            var mapIcon = new MapIcon();
            mapIcon.Image = new string(Image);
            mapIcon.Frequency = Frequency;
            mapIcon.Overlap = Overlap;
            return mapIcon;
        }
    }


    public class MacroSettings : AbstractConfiguration
    {
        [JsonPropertyName("check_frequency")]
        public double CheckFrequency { set; get; } = 0.5;

        [JsonPropertyName("solve_check_timeout")]
        public double SolveCheckTimeout { set; get; } = 3;

        [JsonPropertyName("login_tolerance")]
        public int LoginTolerance { set; get; } = 3;

        [JsonPropertyName("login_timeout")]
        public int LoginTimeout { set; get; } = 60;

        [JsonPropertyName("potion_frequency")]
        public double PotionFrequency { set; get; } = 2.0;

        public override AbstractConfiguration Copy()
        {
            return new MacroSettings
            {
                CheckFrequency = CheckFrequency,
                SolveCheckTimeout = SolveCheckTimeout,
                LoginTolerance = LoginTolerance,
                LoginTimeout = LoginTimeout,
                PotionFrequency = PotionFrequency,
            };
        }
    }


    public class RuneDetection : AbstractConfiguration
    {
        [JsonPropertyName("ip_address")]
        public string RuneSolverIPAddress { set; get; } = "";

        [JsonPropertyName("port")]
        public string RuneSolverPort { set; get; } = "";

        [JsonPropertyName("route")]
        public string RuneSolverRoute { set; get; } = "";

        [JsonPropertyName("class_tag")]
        public string ClassTag { set; get; } = "";

        [JsonPropertyName("left")]
        public string Left { set; get; } = "";

        [JsonPropertyName("up")]
        public string Up { set; get; } = "";

        [JsonPropertyName("right")]
        public string Right { set; get; } = "";

        [JsonPropertyName("down")]
        public string Down { set; get; } = "";

        public override AbstractConfiguration Copy()
        {
            return new RuneDetection
            {
                RuneSolverIPAddress = RuneSolverIPAddress,
                RuneSolverPort = RuneSolverPort,
                RuneSolverRoute = RuneSolverRoute,
                ClassTag = ClassTag,
                Left = Left,
                Up = Up,
                Right = Right,
                Down = Down
            };
        }
    }


    public class RuneServerSettings : AbstractConfiguration
    {
        [JsonPropertyName("server_executable")]
        public string ServerExecutable { set; get; } = "";

        [JsonPropertyName("client_watchdog_timeout")]
        public int ClientWatchdogTimeout { set; get; } = 0;

        [JsonPropertyName("server_rune_model")]
        public string ServerRuneModel { set; get; } = "";

        [JsonPropertyName("server_rune_model_console")]
        public int ServerRuneModelConsole { get; set; } = 0;

        [JsonPropertyName("server_watchdog_timeout")]
        public int ServerWatchdogTimeout { set; get; } = 0;

        public override AbstractConfiguration Copy()
        {
            return new RuneServerSettings
            {
                ServerExecutable = ServerExecutable,
                ClientWatchdogTimeout = ClientWatchdogTimeout,
                ServerRuneModel = ServerRuneModel,
                ServerRuneModelConsole = ServerRuneModelConsole,
                ServerWatchdogTimeout = ServerWatchdogTimeout
            };
        }
    }


    public class MacroKeySettings : AbstractConfiguration
    {
        [JsonPropertyName("ailments_allcure_key")]
        public string AilmentsAllcureKey { get; set; } = "";

        [JsonPropertyName("rune_interact_key")]
        public string RuneInteractKey { get; set; } = "";

        public override AbstractConfiguration Copy()
        {
            return new MacroKeySettings
            {
                AilmentsAllcureKey = AilmentsAllcureKey,
                RuneInteractKey = RuneInteractKey,
            };
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

        [JsonPropertyName("consumables")]
        public List<Consumable> Consumables { get; set; } = [];

        [JsonPropertyName("ailments")]
        public Dictionary<string, Ailment> Ailments { get; set; } = new Dictionary<string, Ailment>();

        [JsonPropertyName("map_icons")]
        public Dictionary<string, MapIcon> MapIcons { get; set; } = new Dictionary<string, MapIcon>();

        [JsonPropertyName("macro_directory")]
        public string MacroDirectory { get; set; } = "";

        [JsonPropertyName("frame_points_directory")]
        public string FramePointsDirectory { get; set; } = "";

        [JsonPropertyName("frame_movements_directory")]
        public string FrameMovementsDirectory { get; set; } = "";

        [JsonPropertyName("map_directory")]
        public string MapDirectory { get; set; } = "";

        [JsonPropertyName("macro_check_frequency")]
        public MacroSettings MacroSettings { get; set; } = new MacroSettings();

        [JsonPropertyName("rune_detection")]
        public RuneDetection RuneDetection { get; set; } = new RuneDetection();

        [JsonPropertyName("rune_server_settings")]
        public RuneServerSettings RuneServerSettings { get; set; } = new RuneServerSettings();

        [JsonPropertyName("macro_key_settings")]
        public MacroKeySettings MacroKeySettings { get; set; } = new MacroKeySettings();

        public override AbstractConfiguration Copy()
        {
            var configuration = new MaplestoryBotConfiguration()
            {
                ProcessName = new string(ProcessName),
                Hp = (Resource)Hp.Copy(),
                Mp = (Resource)Mp.Copy()
            };
            foreach (var item in Ailments)
                configuration.Ailments.Add(new string(item.Key), (Ailment)item.Value.Copy());
            foreach (var item in MapIcons)
                configuration.MapIcons.Add(new string(item.Key), (MapIcon)item.Value.Copy());
            foreach (var consumable in Consumables)
                configuration.Consumables.Add((Consumable)consumable.Copy());
            configuration.MacroDirectory = new string(MacroDirectory);
            configuration.FramePointsDirectory = new string(FramePointsDirectory);
            configuration.FrameMovementsDirectory = new string(FrameMovementsDirectory);
            configuration.MapDirectory = new string(MapDirectory);
            configuration.MacroSettings = (MacroSettings)MacroSettings.Copy();
            configuration.RuneDetection = (RuneDetection)RuneDetection.Copy();
            configuration.RuneServerSettings = (RuneServerSettings)RuneServerSettings.Copy();
            configuration.MacroKeySettings = (MacroKeySettings)MacroKeySettings.Copy();
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
