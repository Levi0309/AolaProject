using System.Collections.Generic;
using EnjoyJob.Battle.Skills;
using UnityEngine;

namespace EnjoyJob.Battle
{
    // 测试用战斗初始化器。
    // 游戏开始时从 pets.json 读取玩家和敌人的亚比配置，并按等级自动配置技能。
    public sealed class BattleTestInitializer : MonoBehaviour
    {
        [SerializeField] private PetCtrl playerPet;
        [SerializeField] private int playerPetId = 1001;
        [SerializeField] private int playerLevel = 20;

        [SerializeField] private PetCtrl enemyPet;
        [SerializeField] private int enemyPetId = 1001;
        [SerializeField] private int enemyLevel = 20;

        private void Awake()
        {
            InitializePet(playerPet, playerPetId, playerLevel);
            InitializePet(enemyPet, enemyPetId, enemyLevel);
        }

        private void InitializePet(PetCtrl petCtrl, int petId, int level)
        {
            if (petCtrl == null)
            {
                return;
            }

            PetDatabase petDatabase = PetDatabase.GetOrCreate();
            if (!petDatabase.TryGetPet(petId, out PetRecord record))
            {
                Debug.LogError($"Pet id not found: {petId}", this);
                return;
            }

            petCtrl.ApplyPetData(record.name, record.element, record.secondElement, record.speciesStats, record.animations);
            petCtrl.SetLevel(level);
            petCtrl.ResetForBattle();

            YabiSkillSet skillSet = petCtrl.SkillSet;
            if (skillSet == null)
            {
                Debug.LogError($"{petCtrl.DisplayName} has no YabiSkillSet.", petCtrl);
                return;
            }

            List<int> skillIds = PetSkillLearning.GetSkillIdsAtLevel(record, level);
            skillSet.SetSkills(skillIds);
        }
    }
}
