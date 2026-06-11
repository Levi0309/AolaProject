using System.Collections.Generic;
using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    // 挂在亚比身上的技能栏。
    // 它负责根据技能ID学习技能、检查PP、使用技能时扣PP，以及恢复PP。
    public class YabiSkillSet : MonoBehaviour
    {
        [SerializeField] private List<LearnedSkill> learnedSkills = new List<LearnedSkill>();

        public IReadOnlyList<LearnedSkill> LearnedSkills => learnedSkills;//只读列表

        public bool LearnSkill(int skillId)
        {
            if (HasSkill(skillId))
            {
                return false;
            }

            if (!TryGetDefinition(skillId, out SkillDefinition definition))
            {
                Debug.LogWarning($"Cannot learn missing skill id: {skillId}", this);
                return false;
            }

            learnedSkills.Add(new LearnedSkill(skillId, definition.MaxPp));
            return true;
        }

        public void SetSkills(IEnumerable<int> skillIds)
        {
            learnedSkills.Clear();

            if (skillIds == null)
            {
                return;
            }

            foreach (int skillId in skillIds)
            {
                LearnSkill(skillId);
            }
        }

        public bool TryUseSkill(int skillId, out SkillDefinition definition)
        {
            definition = null;

            LearnedSkill learnedSkill = FindLearnedSkill(skillId);
            if (learnedSkill == null || !learnedSkill.HasPp)
            {
                return false;
            }

            if (!TryGetDefinition(skillId, out definition))
            {
                return false;
            }

            return learnedSkill.TryConsumePp();
        }

        public void RestoreAllPp()
        {
            foreach (LearnedSkill learnedSkill in learnedSkills)
            {
                if (TryGetDefinition(learnedSkill.skillId, out SkillDefinition definition))
                {
                    learnedSkill.RestorePp(definition.MaxPp);
                }
            }
        }

        public SkillDefinition GetDefinitionOrNull(int skillId)
        {
            TryGetDefinition(skillId, out SkillDefinition definition);
            return definition;
        }

        private bool HasSkill(int skillId)
        {
            return FindLearnedSkill(skillId) != null;
        }

        private LearnedSkill FindLearnedSkill(int skillId)
        {
            foreach (LearnedSkill learnedSkill in learnedSkills)
            {
                if (learnedSkill.skillId == skillId)
                {
                    return learnedSkill;
                }
            }

            return null;
        }

        private bool TryGetDefinition(int skillId, out SkillDefinition definition)
        {
            definition = null;

            if (SkillDatabase.Instance == null)
            {
                Debug.LogError("SkillDatabase instance is missing.", this);
                return false;
            }

            return SkillDatabase.Instance.TryGetSkill(skillId, out definition);
        }
    }
}
