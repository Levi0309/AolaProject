using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    /// <summary>
    /// 提升自身全属性一级
    /// </summary>
    public class UpAllStatusEffect : SkillEffectScript
    {
        public override void Execute(SkillEffectContext context)
        {
            if (context.User is PetCtrl user)
            {
                user.ChangeStatStage(PetStatKind.Attack, 1);
                user.ChangeStatStage(PetStatKind.SpecialAttack, 1);
                user.ChangeStatStage(PetStatKind.Defense, 1);
                user.ChangeStatStage(PetStatKind.SpecialDefense, 1);
                user.ChangeStatStage(PetStatKind.Accuracy, 1);
                user.ChangeStatStage(PetStatKind.Evasion, 1);
                user.ChangeStatStage(PetStatKind.Critical, 1);
                user.ChangeStatStage(PetStatKind.Speed, 1);
                user.PlayStatStageUpAnimation();
                Debug.Log($"{context.Skill.Name}: {user.DisplayName} 全属性等级提升 1。");
            }
        }
    }
}
