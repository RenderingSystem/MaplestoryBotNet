using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using MaplestoryBotNet.Systems.Configuration.SubSystems;


namespace MaplestoryBotNet.Systems.Configuration
{
    public class KeyboardMapping : AbstractConfiguration
    {
        [JsonPropertyName("characters")]
        public Dictionary<string, string> Characters { get; set; } = new Dictionary<string, string>();

        [JsonPropertyName("specials")]
        public Dictionary<string, string> Specials { get; set; } = new Dictionary<string, string>();

        [JsonPropertyName("numpad")]
        public Dictionary<string, string> Numpad { get; set; } = new Dictionary<string, string>();

        [JsonPropertyName("functions")]
        public Dictionary<string, string> Functions { get; set; } = new Dictionary<string, string>();

        public override AbstractConfiguration Copy()
        {
            var keyboardMapping = new KeyboardMapping();
            foreach (var item in Characters)
                keyboardMapping.Characters.Add(item.Key, item.Value);
            foreach (var item in Specials)
                keyboardMapping.Specials.Add(item.Key, item.Value);
            foreach (var item in Numpad)
                keyboardMapping.Numpad.Add(item.Key, item.Value);
            foreach (var item in Functions)
                keyboardMapping.Functions.Add(item.Key, item.Value);
            return keyboardMapping;
        }

        public string GetMapping(string key)
        {
            if (Functions.ContainsKey(key))
                return Functions[key];
            if (Specials.ContainsKey(key))
                return Specials[key];
            if (Characters.ContainsKey(key))
                return Characters[key];
            if (Numpad.ContainsKey(key))
                return Numpad[key];
            return "";
        }
    }


    public abstract class AbstractKeyboardMappingDeserializer : AbstractDeserializer
    {
        public abstract KeyboardMapping DeserializeKeyboardMapping(string jsonString);
    }


    public class KeyboardMappingDeserializer : AbstractKeyboardMappingDeserializer
    {
        public override KeyboardMapping DeserializeKeyboardMapping(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
            var result = JsonSerializer.Deserialize<KeyboardMapping>(jsonString, options);
            Debug.Assert(result != null);
            return result;
        }

        public override object Deserialize(string jsonString)
        {
            return DeserializeKeyboardMapping(jsonString);
        }
    }
}
