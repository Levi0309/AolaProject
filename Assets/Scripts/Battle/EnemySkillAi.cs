using System.Collections.Generic;
using EnjoyJob.Battle.Skills;
using UnityEngine;

namespace EnjoyJob.Battle
{
    // 临时敌人AI：从仍有PP的技能里随机选一个。
    // 以后接入联网时，可以把这一块替换成“等待远端玩家选择”。
    public sealed class EnemySkillAi : MonoBehaviour
    {
        public bool TryChooseSkill(PetCtrl enemyUnit, out int skillId)
        {
            skillId = 0;

            if (enemyUnit == null || enemyUnit.SkillSet == null)
            {
                return false;
            }

            List<int> usableSkillIds = new List<int>();
            foreach (LearnedSkill learnedSkill in enemyUnit.SkillSet.LearnedSkills)
            {
                if (learnedSkill != null && learnedSkill.HasPp)
                {
                    usableSkillIds.Add(learnedSkill.skillId);
                }
            }

            if (usableSkillIds.Count <= 0)
            {
                return false;
            }

            skillId = usableSkillIds[Random.Range(0, usableSkillIds.Count)];
            return true;
        }
    }
}
