using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MaplestoryBotNet.Systems.Configuration.SubSystems
{
    public class MacroData : AbstractConfiguration
    {
        [JsonPropertyName("macro")]
        public string[] Macro { set; get; } = [];

        public override AbstractConfiguration Copy()
        {
            return new MacroData { Macro = (string[])Macro.Clone() };
        }
    }


    public abstract class AbstractMacroDataDeserializer : AbstractDeserializer
    {
        public abstract MacroData DeserializeMacroData(string jsonString);
    }


    public class MacroDataDeserializer : AbstractMacroDataDeserializer
    {
        public override object Deserialize(string data)
        {
            return DeserializeMacroData(data);
        }

        public override MacroData DeserializeMacroData(string jsonString)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                };
                var result = JsonSerializer.Deserialize<MacroData>(jsonString, options);
                Debug.Assert(result != null);
                return result;
            }
            catch
            {
                return new MacroData();
            }
        }
    }


    public abstract class AbstractMacroDataSerializer : AbstractSerializer
    {
        public abstract string SerializeMacroData(MacroData macroData);
    }


    public class MacroDataSerializer : AbstractMacroDataSerializer
    {
        public override string Serialize(object obj)
        {
            return SerializeMacroData((MacroData)obj);
        }

        public override string SerializeMacroData(MacroData macroData)
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
