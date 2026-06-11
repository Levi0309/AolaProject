using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    // 25% 概率削弱对方全属性一级。
    public class SunSwallowSkyEffect : SkillEffectScript
    {
        private const float WeakenChance = 0.25f;

        public override void Execute(SkillEffectContext context)
        {
            if (!(context.Target is PetCtrl target))
            {
                Debug.Log($"{context.Skill.Name}: 没有可削弱的目标。");
                return;
            }
            int damage = context.Damage;
            if (context.Target is ISkillDamageReceiver damageReceiver)
            {
                damageReceiver.TakeDamage(damage);
            }

            Debug.Log($"{context.Skill.Name}: 对敌方单体造成 {damage} 点伤害。");
            if (Random.value >= WeakenChance)
            {
                Debug.Log($"{context.Skill.Name}: 削弱效果没有触发。");
                return;
            }
            target.ChangeStatStage(PetStatKind.Attack, -1);
            target.ChangeStatStage(PetStatKind.SpecialAttack, -1);
            target.ChangeStatStage(PetStatKind.Defense, -1);
            target.ChangeStatStage(PetStatKind.SpecialDefense, -1);
            target.ChangeStatStage(PetStatKind.Accuracy, -1);
            target.ChangeStatStage(PetStatKind.Evasion, -1);
            target.ChangeStatStage(PetStatKind.Critical, -1);
            target.ChangeStatStage(PetStatKind.Speed, -1);
            target.PlayStatStageDownAnimation();
            Debug.Log($"{context.Skill.Name}: {target.DisplayName} 全属性等级降低 1。");
        }
    }
}
