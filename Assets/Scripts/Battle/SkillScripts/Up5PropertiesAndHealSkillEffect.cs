using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    public sealed class Up5PropertiesAndHealSkillEffect : SkillEffectScript
    {
        public override void Execute(SkillEffectContext context)
        {
            if (context.User is PetCtrl user)
            {
                user.ChangeStatStage(PetStatKind.Attack, 1);
                user.ChangeStatStage(PetStatKind.Defense, 1);
                user.ChangeStatStage(PetStatKind.SpecialDefense, 1);
                user.ChangeStatStage(PetStatKind.Accuracy, 1);
                user.ChangeStatStage(PetStatKind.Critical, 1);
                user.ChangeStatStage(PetStatKind.Speed, 1);
                user.PlayStatStageUpAnimation();
                Debug.Log($"{context.Skill.Name}: {user.DisplayName} 上古战魂发动...");
            }
        }
    }
}
