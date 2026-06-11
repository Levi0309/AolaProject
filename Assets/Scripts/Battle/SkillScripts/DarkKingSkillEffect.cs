using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    public sealed class DarkKingSkillEffect : SkillEffectScript
    {
        public override void Execute(SkillEffectContext context)
        {
            if (context.User is PetCtrl user)
            {
                user.ResetStatStages();
            }

            if (context.Target is PetCtrl target)
            {
                target.ResetStatStages();
            }

            Debug.Log($"{context.Skill.Name}: 重置双方能力等级。");
        }
    }
}
