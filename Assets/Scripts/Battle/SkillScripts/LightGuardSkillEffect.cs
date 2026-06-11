using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    public class LightGuardSkillEffect : SkillEffectScript
    {
        public override void Execute(SkillEffectContext context)
        {
            if (context.User is PetCtrl user)
            {
                user.Heal(50);
                Debug.Log($"{context.Skill.Name}: {user.DisplayName} 恢复了 50 点生命值。");
                return;
            }

            Debug.Log($"{context.Skill.Name}: 执行技能效果。");
        }
    }
}
