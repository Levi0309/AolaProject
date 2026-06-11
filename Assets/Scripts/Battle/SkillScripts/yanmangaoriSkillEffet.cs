using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    public sealed class yanmangaoriSkillEffet : SkillEffectScript
    {
        public override void Execute(SkillEffectContext context)
        {
            Debug.Log($"{context.Skill.Name}: 执行技能效果。");
        }
    }
}
