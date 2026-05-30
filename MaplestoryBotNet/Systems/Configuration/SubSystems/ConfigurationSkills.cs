using MaplestoryBotNet.Systems.Configuration.SubSystems;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MaplestoryBotNet.Systems.Configuration
{
    public class ConfigurationSkill : AbstractConfiguration
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("min_delay")]
        public int MinDelay { get; set; } = 0;

        [JsonPropertyName("max_delay")]
        public int MaxDelay { get; set; } = 0;

        [JsonPropertyName("macros")]
        public List<string> Macros { get; set; } = [];

        [JsonPropertyName("active")]
        public int Active { get; set; } = 0;

        public override AbstractConfiguration Copy()
        {
            return new ConfigurationSkill
            {
                MinDelay = MinDelay,
                MaxDelay = MaxDelay,
                Macros = [.. Macros],
                Active = Active
            };
        }
    }


    public class ConfigurationSkills : AbstractConfiguration
    {
        [JsonPropertyName("skills")]
        public List<ConfigurationSkill> Skills { get; set; } = [];

        public override AbstractConfiguration Copy()
        {
            return new ConfigurationSkills
            {
                Skills = Skills.Select(s => (ConfigurationSkill)s.Copy()).ToList()
            };
        }
    }


    public abstract class AbstractSkillsDeserializer : AbstractDeserializer
    {
        public abstract ConfigurationSkills DeserializeSkills(string jsonString);
    }


    public class SkillsDeserializer : AbstractSkillsDeserializer
    {
        public override object Deserialize(string data)
        {
            return DeserializeSkills(data);
        }

        public override ConfigurationSkills DeserializeSkills(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
            var result = JsonSerializer.Deserialize<ConfigurationSkills>(jsonString, options);
            return result!;
        }
    }


    public abstract class AbstractSkillsSerializer : AbstractSerializer
    {
        public abstract string SerializeSkills(ConfigurationSkills skills);
    }


    public class SkillsSerializer : AbstractSkillsSerializer
    {
        public override string Serialize(object obj)
        {
            return SerializeSkills((ConfigurationSkills)obj);
        }

        public override string SerializeSkills(ConfigurationSkills skills)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                IndentCharacter = ' ',
                IndentSize = 4
            };
            var result = JsonSerializer.Serialize(skills, options);
            return result;

        }
    }
}
