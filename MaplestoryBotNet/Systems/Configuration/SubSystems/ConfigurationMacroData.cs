using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MaplestoryBotNet.Systems.Configuration.SubSystems
{
    public class ConfigurationMacroData : AbstractConfiguration
    {
        [JsonPropertyName("macro")]
        public string[] Macro { set; get; } = [];

        public override AbstractConfiguration Copy()
        {
            return new ConfigurationMacroData { Macro = (string[])Macro.Clone() };
        }
    }


    public abstract class AbstractMacroDataDeserializer : AbstractDeserializer
    {
        public abstract ConfigurationMacroData DeserializeMacroData(string jsonString);
    }


    public class MacroDataDeserializer : AbstractMacroDataDeserializer
    {
        public override object Deserialize(string data)
        {
            return DeserializeMacroData(data);
        }

        public override ConfigurationMacroData DeserializeMacroData(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
            var result = JsonSerializer.Deserialize<ConfigurationMacroData>(jsonString, options);
            return result!;
        }
    }


    public abstract class AbstractMacroDataSerializer : AbstractSerializer
    {
        public abstract string SerializeMacroData(ConfigurationMacroData macroData);
    }


    public class MacroDataSerializer : AbstractMacroDataSerializer
    {
        public override string Serialize(object obj)
        {
            return SerializeMacroData((ConfigurationMacroData)obj);
        }

        public override string SerializeMacroData(ConfigurationMacroData macroData)
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
    }
}
