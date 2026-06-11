using System;

namespace EnjoyJob.Battle.Skills
{
    // 某只亚比已经学会的一个技能。
    // skillId 指向技能表里的配置，currentPp 记录这只亚比当前剩余PP。
    [Serializable]
    public class LearnedSkill
    {
        public int skillId;
        public int currentPp;

        public bool HasPp => currentPp > 0;

        public LearnedSkill(int skillId, int currentPp)
        {
            this.skillId = skillId;
            this.currentPp = currentPp;
        }

        public bool TryConsumePp()
        {
            if (!HasPp)
            {
                return false;
            }

            currentPp--;
            return true;
        }

        public void RestorePp(int maxPp)
        {
            currentPp = maxPp;
        }
    }
}
