using EnjoyJob.Battle.Skills;

namespace EnjoyJob.Battle
{
    // 一次已经选好的行动：谁对谁使用哪个技能。
    public sealed class BattleAction
    {
        public PetCtrl User { get; }
        public PetCtrl Target { get; }
        public SkillDefinition Skill { get; }

        public BattleAction(PetCtrl user, PetCtrl target, SkillDefinition skill)
        {
            User = user;
            Target = target;
            Skill = skill;
        }
    }
}
