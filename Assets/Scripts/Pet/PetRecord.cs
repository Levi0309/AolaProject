using System;
using System.Collections.Generic;

namespace EnjoyJob.Battle
{
    // pets.json 里的一只亚比配置。
    // 这是“亚比品种数据”，不是进入战斗后的 PetCtrl 实例。
    [Serializable]
    public class PetRecord
    {
        public int id;
        public string name;
        public string element;
        public string secondElement;
        public string imageResourcePath;
        public PetAnimationPaths animations = new PetAnimationPaths();
        public PetSpeciesStats speciesStats = new PetSpeciesStats();
        public List<PetLearnableSkillRecord> learnableSkills = new List<PetLearnableSkillRecord>();
    }
}
