using System;
using EnjoyJob.Battle.Skills;
using UnityEngine;

namespace EnjoyJob.Battle
{
    // 战斗中的一只亚比/宠物主控。
    // 它负责属性计算、生命值、死亡判断、扣血，以及从技能栏里消耗PP取出技能定义。
    [RequireComponent(typeof(YabiSkillSet))]
    public sealed class PetCtrl : MonoBehaviour, ISkillDamageReceiver
    {
        [SerializeField] private string displayName = "亚比";
        [SerializeField] private string element = "木系";
        [SerializeField] private string secondElement;
        [SerializeField] private BattleSide side;
        [SerializeField] private int level = 100;
        [SerializeField] private PetSpeciesStats speciesStats = new PetSpeciesStats();
        [SerializeField] private PetSixStats talentStats = PetSixStats.CreateDefaultTalent();
        [SerializeField] private PetSixStats trainingStats = new PetSixStats();
        [SerializeField] private PetNature nature = PetNature.Balanced;
        [SerializeField] private PetBattleStatStages statStages = new PetBattleStatStages();
        [SerializeField] private PetAnimationPaths animationPaths = new PetAnimationPaths();
        [SerializeField] private YabiSkillSet skillSet;
        [SerializeField] private BattlePetStatusView statusView;
        [SerializeField] private int currentHp;
        [SerializeField] private int debugMaxHp;

        public event Action<PetCtrl, int, int> HpChanged;
        public event Action<PetCtrl> Fainted;

        public string DisplayName => displayName;
        public string ElementName => SkillElementCatalog.GetDisplayName(element);
        public string SecondElementName => string.IsNullOrWhiteSpace(secondElement) ? string.Empty : SkillElementCatalog.GetDisplayName(secondElement);
        public SkillElement Element => SkillElementCatalog.Parse(element);
        public bool HasSecondElement => !string.IsNullOrWhiteSpace(secondElement);
        public SkillElement SecondElement => SkillElementCatalog.Parse(secondElement);
        public BattleSide Side => side;
        public int Level => level;
        public PetSpeciesStats SpeciesStats => speciesStats;
        public PetSixStats TalentStats => talentStats;
        public PetSixStats TrainingStats => trainingStats;
        public PetNature Nature => nature;
        public PetBattleStatStages StatStages => statStages;
        public PetAnimationPaths AnimationPaths => animationPaths;
        public PetBattleStats BattleStats { get; private set; }
        public int MaxHp => BattleStats.MaxHp;
        public int CurrentHp => currentHp;
        public bool IsFainted => currentHp <= 0;
        public YabiSkillSet SkillSet => skillSet;

        private void Awake()
        {
            EnsureSkillSet();
            EnsureStatusView();
            RecalculateStats();
            statStages.Reset();
            currentHp = MaxHp;
            RefreshStatusView();
        }

        private void OnValidate()
        {
            level = Mathf.Max(1, level);
            RecalculateStats();
        }

        public void RecalculateStats()
        {
            BattleStats = PetStatCalculator.Calculate(level, speciesStats, talentStats, trainingStats, nature);
            debugMaxHp = MaxHp;
        }

        public void ApplyPetData(string petName, PetSpeciesStats sourceSpeciesStats)
        {
            ApplyPetData(petName, element, secondElement, sourceSpeciesStats, null);
        }

        public void ApplyPetData(string petName, PetSpeciesStats sourceSpeciesStats, PetAnimationPaths sourceAnimationPaths)
        {
            ApplyPetData(petName, element, secondElement, sourceSpeciesStats, sourceAnimationPaths);
        }

        public void ApplyPetData(string petName, string petElement, PetSpeciesStats sourceSpeciesStats, PetAnimationPaths sourceAnimationPaths)
        {
            ApplyPetData(petName, petElement, string.Empty, sourceSpeciesStats, sourceAnimationPaths);
        }

        public void ApplyPetData(string petName, string petElement, string petSecondElement, PetSpeciesStats sourceSpeciesStats, PetAnimationPaths sourceAnimationPaths)
        {
            displayName = petName;
            element = string.IsNullOrWhiteSpace(petElement) ? "木系" : SkillElementCatalog.GetDisplayName(petElement);
            secondElement = string.IsNullOrWhiteSpace(petSecondElement) ? string.Empty : SkillElementCatalog.GetDisplayName(petSecondElement);
            if (sourceSpeciesStats == null)
            {
                speciesStats = new PetSpeciesStats();
            }
            else
            {
                speciesStats = new PetSpeciesStats
                {
                    attack = sourceSpeciesStats.attack,
                    specialAttack = sourceSpeciesStats.specialAttack,
                    defense = sourceSpeciesStats.defense,
                    specialDefense = sourceSpeciesStats.specialDefense,
                    stamina = sourceSpeciesStats.stamina,
                    speed = sourceSpeciesStats.speed
                };
            }

            animationPaths = CopyAnimationPaths(sourceAnimationPaths);
            RecalculateStats();
            RefreshStatusView();
        }

        public void SetLevel(int nextLevel)
        {
            level = Mathf.Max(1, nextLevel);
            RecalculateStats();
            RefreshStatusView();
        }

        public int ChangeStatStage(PetStatKind statKind, int amount)
        {
            int nextStage = statStages.AddStage(statKind, amount);
            RefreshStatusViewStatStages();
            return nextStage;
        }

        public void ResetStatStages()
        {
            statStages.Reset();
            RefreshStatusViewStatStages();
        }

        public void PlayStatStageUpAnimation()
        {
            EnsureStatusView();
            if (statusView != null)
            {
                statusView.PlayStatStageUpAnimation();
            }
        }

        public void PlayStatStageDownAnimation()
        {
            EnsureStatusView();
            if (statusView != null)
            {
                statusView.PlayStatStageDownAnimation();
            }
        }

        public void PlayDamagePopup(int damage, BattleDamagePopupType popupType)
        {
            EnsureStatusView();
            if (statusView != null)
            {
                statusView.PlayDamagePopup(damage, popupType);
            }
        }

        public bool TryPrepareSkill(int skillId, out SkillDefinition skill)
        {
            skill = null;
            EnsureSkillSet();

            if (IsFainted || skillSet == null)
            {
                return false;
            }

            return skillSet.TryUseSkill(skillId, out skill);
        }

        public void TakeDamage(int damage)
        {
            debugMaxHp = MaxHp;
            if (IsFainted)
            {
                return;
            }

            int safeDamage = Mathf.Max(0, damage);
            int previousHp = currentHp;
            currentHp = Mathf.Max(0, currentHp - safeDamage);

            if (currentHp != previousHp)
            {
                HpChanged?.Invoke(this, currentHp, MaxHp);
                RefreshStatusViewHp();
            }

            if (currentHp <= 0)
            {
                Fainted?.Invoke(this);
            }
        }

        public void Heal(int amount)
        {
            debugMaxHp = MaxHp;
            if (IsFainted)
            {
                return;
            }

            int safeAmount = Mathf.Max(0, amount);
            int previousHp = currentHp;
            currentHp = Mathf.Min(MaxHp, currentHp + safeAmount);

            if (currentHp != previousHp)
            {
                HpChanged?.Invoke(this, currentHp, MaxHp);
                RefreshStatusViewHp();
            }
        }

        public void ResetForBattle()
        {
            EnsureSkillSet();
            RecalculateStats();
            statStages.Reset();
            currentHp = MaxHp;
            debugMaxHp = MaxHp;
            if (skillSet != null)
            {
                skillSet.RestoreAllPp();
            }

            HpChanged?.Invoke(this, currentHp, MaxHp);
            RefreshStatusView();
        }

        private void OnMouseEnter()
        {
            EnsureStatusView();
            if (statusView != null)
            {
                statusView.ShowStatStages(StatStages);
            }
        }

        private void OnMouseOver()
        {
            RefreshStatusViewStatStages();
        }

        private void OnMouseExit()
        {
            EnsureStatusView();
            if (statusView != null)
            {
                statusView.HideStatStages();
            }
        }

        private void EnsureSkillSet()
        {
            if (skillSet == null)
            {
                skillSet = GetComponent<YabiSkillSet>();
            }
        }

        private void EnsureStatusView()
        {
            if (statusView == null)
            {
                statusView = GetComponentInChildren<BattlePetStatusView>(true);
            }
        }

        private void RefreshStatusView()
        {
            EnsureStatusView();
            if (statusView == null)
            {
                return;
            }

            statusView.SetInfo(DisplayName, Level, ElementName, SecondElementName, Side);
            statusView.SetHp(CurrentHp, MaxHp);
        }

        private void RefreshStatusViewHp()
        {
            EnsureStatusView();
            if (statusView != null)
            {
                statusView.SetHp(CurrentHp, MaxHp);
            }
        }

        private void RefreshStatusViewStatStages()
        {
            EnsureStatusView();
            if (statusView != null)
            {
                statusView.RefreshStatStages(StatStages);
            }
        }

        private static PetAnimationPaths CopyAnimationPaths(PetAnimationPaths source)
        {
            if (source == null)
            {
                return new PetAnimationPaths();
            }

            return new PetAnimationPaths
            {
                idle = source.idle,
                normalAttack1 = source.normalAttack1,
                normalAttack2 = source.normalAttack2,
                specialAttack1 = source.specialAttack1,
                specialAttack2 = source.specialAttack2,
                attributeAttack1 = source.attributeAttack1,
                attributeAttack2 = source.attributeAttack2,
                hurt = source.hurt
            };
        }

    }
}
