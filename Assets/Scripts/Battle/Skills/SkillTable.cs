using System;
using System.Collections.Generic;

namespace EnjoyJob.Battle.Skills
{
    // JSON技能表的根对象，对应 skills.json 最外层的 { "skills": [...] }。
    // Unity 的 JsonUtility 不能直接读取顶层数组，所以需要这个包装类。
    [Serializable]
    public class SkillTable
    {
        public List<SkillRecord> skills = new List<SkillRecord>();
    }
}
