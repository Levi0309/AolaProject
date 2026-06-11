using System;

namespace EnjoyJob.Battle.Skills
{
    // JSON技能表里的一条原始数据。
    // 字段名要和 skills.json 完全对应，JsonUtility 才能正确读写。
    // 这个类只负责存表格数据，不负责计算伤害、扣PP或执行技能效果。
    [Serializable]
    public class SkillRecord
    {
        public int id;
        public string name;
        public string description;
        public string element;
        public int power;
        public int maxPp;
        public string attackType;
        public string iconResourcePath;
        public string SkillEffectScrpit;
    }
}
