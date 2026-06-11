using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    public sealed class TenWordSkillEffect : SkillEffectScript
    {
        private static readonly PetStatKind[] BoostableStats =
        {
            PetStatKind.Attack,
            PetStatKind.Accuracy,
            PetStatKind.Critical,
            PetStatKind.Speed
        };

        public override void Execute(SkillEffectContext context)
        {
            PetCtrl user = context.User as PetCtrl;
            ISkillDamageReceiver damageReceiver = context.Target as ISkillDamageReceiver;

            for (int i = 0; i < 2; i++)
            {
                damageReceiver?.TakeDamage(context.Damage);
                BoostTwoRandomStats(user);
            }

            user?.PlayStatStageUpAnimation();
            Debug.Log($"{context.Skill.Name}: 攻击2次，每次随机提升2种能力等级。");
        }

        private static void BoostTwoRandomStats(PetCtrl user)
        {
            if (user == null)
            {
                return;
            }

            int firstIndex = Random.Range(0, BoostableStats.Length);
            int secondIndex = Random.Range(0, BoostableStats.Length - 1);
            if (secondIndex >= firstIndex)
            {
                secondIndex++;
            }

            user.ChangeStatStage(BoostableStats[firstIndex], 1);
            user.ChangeStatStage(BoostableStats[secondIndex], 1);
        }
    }
}
