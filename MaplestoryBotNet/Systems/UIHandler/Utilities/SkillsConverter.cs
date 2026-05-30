using MaplestoryBotNet.Systems.Configuration;
using MaplestoryBotNet.Systems.UIHandler.Utilities.Models;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public class SkillsConverter : AbstractJsonDataModelConverter
    {
        public override object? ToConfiguration(object dataModel)
        {
            if (dataModel is AbstractSkillsModel skillsModel)
            {
                return new ConfigurationSkills
                {
                    Skills = skillsModel.GetSkills().Select(
                        s => new ConfigurationSkill
                        {
                            Name = s.Name,
                            Active = s.Active,
                            Macros = [.. s.Macros],
                            MinDelay = s.MinDelay,
                            MaxDelay = s.MaxDelay
                        }
                    ).ToList()
                };
            }
            return null;
        }

        public override object? ToDataModel(object configuration)
        {
            if (configuration is ConfigurationSkills skillsConfig)
            {
                var skillsModel = new SkillsModel();
                skillsModel.SetSkills(
                    skillsConfig.Skills.Select(
                        s => new Skill
                        {
                            Name = s.Name,
                            Active = s.Active,
                            Macros = [.. s.Macros],
                            MinDelay= s.MinDelay,
                            MaxDelay= s.MaxDelay
                        }
                    ).ToList()
                );
                return skillsModel;
            }
            return null;
        }
    }
}
