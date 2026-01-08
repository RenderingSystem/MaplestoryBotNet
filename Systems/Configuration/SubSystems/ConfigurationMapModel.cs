using System.Diagnostics;
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


    public class ConfigurationMapModel : AbstractConfiguration
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

        public override AbstractConfiguration Copy()
        {
            return new ConfigurationMapModel
            {
                MapAreaLeft = MapAreaLeft,
                MapAreaTop = MapAreaTop,
                MapAreaRight = MapAreaRight,
                MapAreaBottom = MapAreaBottom,
                MapPoints = MapPoints.Select(point => point.Copy()).ToList()
            };
        }
    }


    public abstract class AbstractMapModelDeserializer : AbstractDeserializer
    {
        public abstract ConfigurationMapModel DeserializeMapModel(string jsonString);
    }


    public class ConfigurationMapModelDeserializer : AbstractMapModelDeserializer
    {
        public override object Deserialize(string data)
        {
            return DeserializeMapModel(data);
        }

        public override ConfigurationMapModel DeserializeMapModel(string jsonString)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                };
                var result = JsonSerializer.Deserialize<ConfigurationMapModel>(jsonString, options);
                return result!;
            }
            catch
            {
                return new ConfigurationMapModel();
            }
        }
    }


    public abstract class AbstractMapModelSerializer : AbstractSerializer
    {
        public abstract string SerializeMapModel(ConfigurationMapModel macroData);
    }


    public class ConfigurationMapModelSerializer : AbstractMapModelSerializer
    {
        public override string Serialize(object obj)
        {
            return SerializeMapModel((ConfigurationMapModel)obj);
        }

        public override string SerializeMapModel(ConfigurationMapModel macroData)
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
