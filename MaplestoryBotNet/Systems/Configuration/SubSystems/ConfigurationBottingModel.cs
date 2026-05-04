using System.Text.Json;
using System.Text.Json.Serialization;


namespace MaplestoryBotNet.Systems.Configuration.SubSystems
{
    public class ConfigurationPointMacros
    {
        [JsonPropertyName("macro_name")]
        public string MacroName { set; get; } = "";

        [JsonPropertyName("macro_chance")]
        public int MacroChance { set; get; } = 0;

        [JsonPropertyName("macro_commands")]
        public List<string> MacroCommands { set; get; } = [];

        public ConfigurationPointMacros Copy()
        {
            return new ConfigurationPointMacros
            {
                MacroName = MacroName,
                MacroChance = MacroChance,
                MacroCommands = [.. MacroCommands]
            };
        }
    }


    public class ConfigurationPointData
    {
        [JsonPropertyName("element_name")]
        public string ElementName { set; get; } = "";

        [JsonPropertyName("point_name")]
        public string PointName { set; get; } = "";

        [JsonPropertyName("commands")]
        public List<ConfigurationPointMacros> Commands { set; get; } = [];

        public ConfigurationPointData Copy()
        {
            return new ConfigurationPointData
            {
                ElementName = ElementName,
                PointName = PointName,
                Commands = Commands.Select(cmd => cmd.Copy()).ToList()
            };
        }
    }


    public class ConfigurationMinimapPoint
    {
        [JsonPropertyName("x")]
        public int X { set; get; } = 0;

        [JsonPropertyName("y")]
        public int Y { set; get; } = 0;

        [JsonPropertyName("x_range")]
        public int XRange { set; get; } = 0;

        [JsonPropertyName("y_range")]
        public int YRange { set; get; } = 0;

        [JsonPropertyName("point_data")]
        public ConfigurationPointData PointData { set; get; } = new ConfigurationPointData();

        public ConfigurationMinimapPoint Copy()
        {
            return new ConfigurationMinimapPoint
            {
                X = X,
                Y = Y,
                XRange = XRange,
                YRange = YRange,
                PointData = PointData.Copy()
            };
        }
    }


    public class ConfigurationRuneFrameMacro
    {
        [JsonPropertyName("macro_name")]
        public string MacroName { set; get; } = "";

        [JsonPropertyName("element_name")]
        public string ElementName { set; get; } = "";

        [JsonPropertyName("x")]
        public double X { set; get; } = 0;

        [JsonPropertyName("y")]
        public double Y { set; get; } = 0;

        [JsonPropertyName("scale_x")]
        public double ScaleX { set; get; } = 0;

        [JsonPropertyName("scale_y")]
        public double ScaleY { set; get; } = 0;

        [JsonPropertyName("next_rune_frame")]
        public string NextRuneFrame { set; get; } = "";

        [JsonPropertyName("radius")]
        public double Radius { set; get; } = 0.0;

        [JsonPropertyName("commands")]
        public List<string> PointCommands { set; get; } = [];

        public ConfigurationRuneFrameMacro Copy()
        {
            return new ConfigurationRuneFrameMacro
            {
                MacroName = MacroName,
                ElementName = ElementName,
                X = X,
                Y = Y,
                ScaleX = ScaleX,
                ScaleY = ScaleY,
                NextRuneFrame = NextRuneFrame,
                Radius = Radius,
                PointCommands = [.. PointCommands]
            };
        }
    }


    public class ConfigurationRuneFrameDirection
    {
        [JsonPropertyName("direction_name")]
        public string DirectionName { set; get; } = "";

        [JsonPropertyName("direction")]
        public int Direction { set; get; } = 0;

        [JsonPropertyName("distance")]
        public int Distance { set; get; } = 123;

        [JsonPropertyName("commands")]
        public List<string> DirectionCommands { set; get; } = [];

        public ConfigurationRuneFrameDirection Copy()
        {
            return new ConfigurationRuneFrameDirection
            {
                DirectionName = DirectionName,
                Direction = Direction,
                Distance = Distance,
                DirectionCommands = [.. DirectionCommands]
            };
        }
    }


    public class ConfigurationRuneFrameData
    {
        [JsonPropertyName("element_name")]
        public string ElementName { set; get; } = "";

        [JsonPropertyName("frame_name")]
        public string FrameName { set; get; } = "";

        [JsonPropertyName("frame_macros")]
        public List<ConfigurationRuneFrameMacro> RuneFrameMacros { set; get; } = [];

        [JsonPropertyName("frame_directions")]
        public List<ConfigurationRuneFrameDirection> RuneFrameDirections { set; get; } = [];

        public ConfigurationRuneFrameData Copy()
        {
            return new ConfigurationRuneFrameData
            {
                ElementName = ElementName,
                FrameName = FrameName,
                RuneFrameMacros = RuneFrameMacros.Select(macro => macro.Copy()).ToList(),
                RuneFrameDirections = RuneFrameDirections.Select(direction => direction.Copy()).ToList()
            };
        }
    }


    public class ConfigurationRuneFrame
    {
        [JsonPropertyName("x")]
        public double X { set; get; } = 0;

        [JsonPropertyName("y")]
        public double Y { set; get; } = 0;

        [JsonPropertyName("width")]
        public double Width { set; get; } = 0;

        [JsonPropertyName("height")]
        public double Height { set; get; } = 0;

        [JsonPropertyName("rune_frame_data")]
        public ConfigurationRuneFrameData RuneFrameData { set; get; } = (
            new ConfigurationRuneFrameData()
        );

        public ConfigurationRuneFrame Copy()
        {
            return new ConfigurationRuneFrame
            {
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                RuneFrameData = RuneFrameData.Copy()
            };
        }
    }


    public class ConfigurationBottingModel : AbstractConfiguration
    {
        [JsonPropertyName("map_area_left")]
        public int MapAreaLeft { set; get; } = 0;

        [JsonPropertyName("map_area_top")]
        public int MapAreaTop { set; get; } = 0;

        [JsonPropertyName("map_area_right")]
        public int MapAreaRight { set; get; } = 1;

        [JsonPropertyName("map_area_bottom")]
        public int MapAreaBottom { set; get; } = 1;

        [JsonPropertyName("map_points")]
        public List<ConfigurationMinimapPoint> MapPoints { set; get; } = [];

        [JsonPropertyName("rune_frames")]
        public List<ConfigurationRuneFrame> RuneFrames { set; get; } = [];

        [JsonPropertyName("character_threshold")]
        public float CharacterThreshold { set; get; } = 0.6f;

        [JsonPropertyName("rune_threshold")]
        public float RuneThreshold { set; get; } = 0.6f;

        [JsonPropertyName("rune_cooldown")]
        public int RuneCooldown { set; get; } = 0;

        [JsonPropertyName("rune_activation")]
        public int RuneActivation { set; get; } = 0;

        [JsonPropertyName("rune_radius")]
        public int RuneRadius { set; get; } = 0;

        [JsonPropertyName("uniform_movement")]
        public int UniformMovement { set; get; } = 0;

        public override AbstractConfiguration Copy()
        {
            return new ConfigurationBottingModel
            {
                MapAreaLeft = MapAreaLeft,
                MapAreaTop = MapAreaTop,
                MapAreaRight = MapAreaRight,
                MapAreaBottom = MapAreaBottom,
                CharacterThreshold = CharacterThreshold,
                RuneThreshold = RuneThreshold,
                MapPoints = MapPoints.Select(point => point.Copy()).ToList(),
                RuneFrames = RuneFrames.Select(frame => frame.Copy()).ToList(),
                RuneCooldown = RuneCooldown,
                RuneActivation = RuneActivation,
                RuneRadius = RuneRadius,
                UniformMovement = UniformMovement
            };
        }
    }


    public abstract class AbstractBottingModelDeserializer : AbstractDeserializer
    {
        public abstract ConfigurationBottingModel DeserializeBottingModel(string jsonString);
    }


    public class ConfigurationBottingModelDeserializer : AbstractBottingModelDeserializer
    {
        public override object Deserialize(string data)
        {
            return DeserializeBottingModel(data);
        }

        public override ConfigurationBottingModel DeserializeBottingModel(string jsonString)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                };
                var result = JsonSerializer.Deserialize<ConfigurationBottingModel>(jsonString, options);
                return result!;
            }
            catch
            {
                return new ConfigurationBottingModel();
            }
        }
    }


    public abstract class AbstractBottingModelSerializer : AbstractSerializer
    {
        public abstract string SerializeBottingModel(ConfigurationBottingModel macroData);
    }


    public class ConfigurationMapModelSerializer : AbstractBottingModelSerializer
    {
        public override string Serialize(object obj)
        {
            return SerializeBottingModel((ConfigurationBottingModel)obj);
        }

        public override string SerializeBottingModel(ConfigurationBottingModel macroData)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    IndentCharacter = ' ',
                    IndentSize = 4
                };
                var result = JsonSerializer.Serialize(macroData, options);
                return result;
            }
            catch
            {
                return "";
            }
        }
    }
}
