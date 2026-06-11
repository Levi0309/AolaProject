using System.Collections.Generic;
using EnjoyJob;
using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    // 技能数据库：从 Resources/Data/skills.json 加载所有技能，并按技能ID提供查询。
    // 亚比只需要保存技能ID，真正的技能数据统一从这里拿。
    public class SkillDatabase : SingletonMonoBehaviour<SkillDatabase>
    {
        [SerializeField] private string skillTableResourcePath = "Data/skills";

        private readonly Dictionary<int, SkillDefinition> skillsById = new Dictionary<int, SkillDefinition>();
        private bool loaded;

        public IReadOnlyDictionary<int, SkillDefinition> SkillsById => skillsById;

        protected override void Awake()
        {
            base.Awake();
            Load();
        }

        public void Load()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;
            skillsById.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(skillTableResourcePath);
            if (jsonAsset == null)
            {
                Debug.LogError($"Skill table not found in Resources: {skillTableResourcePath}");
                return;
            }

            SkillTable table = JsonUtility.FromJson<SkillTable>(jsonAsset.text);
            if (table == null || table.skills == null)
            {
                Debug.LogError($"Skill table is invalid: {skillTableResourcePath}");
                return;
            }

            foreach (SkillRecord record in table.skills)
            {
                if (record == null)
                {
                    continue;
                }

                if (skillsById.ContainsKey(record.id))
                {
                    Debug.LogWarning($"Duplicate skill id skipped: {record.id}");
                    continue;
                }

                skillsById.Add(record.id, new SkillDefinition(record));
            }
        }

        public bool TryGetSkill(int id, out SkillDefinition skill)
        {
            Load();
            return skillsById.TryGetValue(id, out skill);
        }

        public SkillDefinition GetSkillOrNull(int id)
        {
            TryGetSkill(id, out SkillDefinition skill);
            return skill;
        }
    }
}
