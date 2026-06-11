using System;

namespace EnjoyJob.Battle
{
    // 亚比可学习技能表的一行：到达 learnLevel 时可以学会 skillId。
    [Serializable]
    public class PetLearnableSkillRecord
    {
        public int learnLevel;
        public int skillId;
    }
}
