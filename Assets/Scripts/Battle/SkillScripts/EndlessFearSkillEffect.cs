using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    public sealed class EndlessFearSkillEffect : SkillEffectScript
    {
        private const float WeakenChance = 0.25f;

        public override void Execute(SkillEffectContext context)
        {
            if (context.Target is ISkillDamageReceiver damageReceiver)
            {
                damageReceiver.TakeDamage(context.Damage);
            }

            if (!(context.Target is PetCtrl target) || Random.value >= WeakenChance)
            {
                Debug.Log($"{context.Skill.Name}: 对敌方单体造成 {context.Damage} 点伤害。");
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

            Debug.Log($"{context.Skill.Name}: 对敌方单体造成 {context.Damage} 点伤害，并降低{target.DisplayName}全属性1级。");
        }
    }
}
