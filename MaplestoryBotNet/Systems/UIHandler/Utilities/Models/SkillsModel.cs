namespace MaplestoryBotNet.Systems.UIHandler.Utilities.Models
{
    public class Skill
    {
        public string Name = "";

        public int MinDelay = 0;

        public int MaxDelay = 0;

        public List<string> Macros = [];

        public int Active = 0;

        public Skill Copy()
        {
            return new Skill
            {
                Name = Name,
                MinDelay = MinDelay,
                MaxDelay = MaxDelay,
                Macros = [.. Macros],
                Active = Active
            };
        }
    }


    public abstract class AbstractSkillsModel
    {
        public abstract List<Skill> GetSkills();

        public abstract void SetSkills(List<Skill> skills);

        public abstract void SetSkillsModel(AbstractSkillsModel skillsModel);

        public abstract AbstractSkillsModel Copy();
    }


    public class SkillsModel : AbstractSkillsModel
    {
        private List<Skill> _skills = [];

        public override List<Skill> GetSkills()
        {
            var skills = _skills;
            return [.. skills.Select((s) => s.Copy())];
        }

        public override void SetSkills(List<Skill> skills)
        {
            _skills = [.. skills.Select((s) => s.Copy())];
        }

        public override AbstractSkillsModel Copy()
        {
            return new SkillsModel { _skills = GetSkills() };
        }

        public override void SetSkillsModel(AbstractSkillsModel skillsModel)
        {
            _skills = skillsModel.GetSkills();
        }
    }
}
