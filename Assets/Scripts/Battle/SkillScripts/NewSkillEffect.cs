using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    public class NewSkillEffect : SkillEffectScript
    {
        public override void Execute(SkillEffectContext context)
        {
            Debug.Log($"{context.Skill.Name}: 执行技能效果。");
        }
    }
}
