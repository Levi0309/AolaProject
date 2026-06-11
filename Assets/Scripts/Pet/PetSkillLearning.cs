using System.Collections.Generic;
using System.Linq;

namespace EnjoyJob.Battle
{
    // 根据等级从亚比配置里计算应该学会哪些技能。
    public static class PetSkillLearning
    {
        public const int MaxSkillCount = 4;

        public static List<int> GetSkillIdsAtLevel(PetRecord pet, int level)
        {
            List<int> result = new List<int>();
            if (pet == null || pet.learnableSkills == null)
            {
                return result;
            }

            IEnumerable<PetLearnableSkillRecord> learnedRecords = pet.learnableSkills
                .Where(record => record != null && record.learnLevel <= level)
                .OrderBy(record => record.learnLevel);

            foreach (PetLearnableSkillRecord record in learnedRecords)
            {
                LearnSkillWithAutoReplace(result, record.skillId);
            }

            return result;
        }

        public static void LearnSkillWithAutoReplace(List<int> currentSkillIds, int skillId)
        {
            if (currentSkillIds == null || skillId <= 0 || currentSkillIds.Contains(skillId))
            {
                return;
            }

            if (currentSkillIds.Count < MaxSkillCount)
            {
                currentSkillIds.Add(skillId);
                return;
            }

            // 临时规则：技能满4个后，自动替换最后一个。
            // 以后做遗忘技能界面时，把这里换成玩家选择即可。
            currentSkillIds[MaxSkillCount - 1] = skillId;
        }
    }
}
