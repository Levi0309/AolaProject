using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    public sealed class HevenContractSkillEffect : SkillEffectScript
    {
        public override void Execute(SkillEffectContext context)
        {
            if (!(context.Target is PetCtrl target))
            {
                return;
            }

            int amount = Random.value < 0.3f ? -2 : -1;
            target.ChangeStatStage(PetStatKind.Defense, amount);
            target.ChangeStatStage(PetStatKind.Evasion, amount);
            target.ChangeStatStage(PetStatKind.Speed, amount);
            target.PlayStatStageDownAnimation();

            Debug.Log($"{context.Skill.Name}: 降低{target.DisplayName}防御、闪避、速度等级{Mathf.Abs(amount)}级。");
        }
    }
}
