using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    public sealed class xiuluoliuxingSkillEffect : SkillEffectScript
    {
        public override void Execute(SkillEffectContext context)
        {
            if (context.Target is ISkillDamageReceiver damageReceiver)
            {
                damageReceiver.TakeDamage(context.Damage);
            }
            Debug.Log($"{context.Skill.Name}: 执行技能效果。修罗流星");
        }
    }
}
