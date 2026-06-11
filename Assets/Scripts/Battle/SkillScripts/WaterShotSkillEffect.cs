using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    public class WaterShotSkillEffect : SkillEffectScript
    {
        public override void Execute(SkillEffectContext context)
        {
            int damage = context.Damage;
            if (context.Target is ISkillDamageReceiver damageReceiver)
            {
                damageReceiver.TakeDamage(damage);
            }

            Debug.Log($"{context.Skill.Name}: 对敌方单体造成 {damage} 点伤害。");
        }
    }
}
