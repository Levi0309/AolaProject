using UnityEngine;

namespace EnjoyJob.Battle
{
    // 亚比能力值计算器。
    // 当前公式先走“种族值 + 天赋 + 学习力 + 等级 + 性格”的清晰版本，后面可以替换成你想完全复刻的公式。
    public static class PetStatCalculator
    {
        private const int DefaultAccuracy = 100;
        private const int DefaultEvasion = 0;
        private const int DefaultCritical = 5;

        public static PetBattleStats Calculate(
            int level,
            PetSpeciesStats species,
            PetSixStats talent,
            PetSixStats training,
            PetNature nature)
        {
            if (species == null)
            {
                species = new PetSpeciesStats();
            }

            if (talent == null)
            {
                talent = PetSixStats.CreateDefaultTalent();
            }

            if (training == null)
            {
                training = new PetSixStats();
            }

            int safeLevel = Mathf.Max(1, level);

            PetBattleStats stats = new PetBattleStats
            {
                Attack = CalculateNormalStat(species.attack, talent.attack, training.attack, safeLevel, GetNatureRate(nature, PetStatKind.Attack)),
                SpecialAttack = CalculateNormalStat(species.specialAttack, talent.specialAttack, training.specialAttack, safeLevel, GetNatureRate(nature, PetStatKind.SpecialAttack)),
                Defense = CalculateNormalStat(species.defense, talent.defense, training.defense, safeLevel, GetNatureRate(nature, PetStatKind.Defense)),
                SpecialDefense = CalculateNormalStat(species.specialDefense, talent.specialDefense, training.specialDefense, safeLevel, GetNatureRate(nature, PetStatKind.SpecialDefense)),
                MaxHp = CalculateHp(species.stamina, talent.stamina, training.stamina, safeLevel),
                Speed = CalculateNormalStat(species.speed, talent.speed, training.speed, safeLevel, GetNatureRate(nature, PetStatKind.Speed)),
                Accuracy = DefaultAccuracy,
                Evasion = DefaultEvasion,
                Critical = DefaultCritical
            };

            return stats;
        }

        private static int CalculateHp(int speciesValue, int talentValue, int trainingValue, int level)
        {
            return Mathf.Max(1, Mathf.FloorToInt(((speciesValue * 2 + talentValue + trainingValue / 4f) * level) / 100f + level + 10));
        }

        private static int CalculateNormalStat(int speciesValue, int talentValue, int trainingValue, int level, float natureRate)
        {
            float baseValue = ((speciesValue * 2 + talentValue + trainingValue / 4f) * level) / 100f + 5;
            return Mathf.Max(1, Mathf.FloorToInt(baseValue * natureRate));
        }

        private static float GetNatureRate(PetNature nature, PetStatKind statKind)
        {
            GetNatureChange(nature, out PetStatKind up, out PetStatKind down);
            if (statKind == up)
            {
                return 1.1f;
            }

            if (statKind == down)
            {
                return 0.9f;
            }

            return 1f;
        }

        private static void GetNatureChange(PetNature nature, out PetStatKind up, out PetStatKind down)
        {
            up = PetStatKind.None;
            down = PetStatKind.None;

            switch (nature)
            {
                case PetNature.AttackUpSpecialAttackDown:
                    up = PetStatKind.Attack;
                    down = PetStatKind.SpecialAttack;
                    break;
                case PetNature.AttackUpDefenseDown:
                    up = PetStatKind.Attack;
                    down = PetStatKind.Defense;
                    break;
                case PetNature.AttackUpSpecialDefenseDown:
                    up = PetStatKind.Attack;
                    down = PetStatKind.SpecialDefense;
                    break;
                case PetNature.AttackUpSpeedDown:
                    up = PetStatKind.Attack;
                    down = PetStatKind.Speed;
                    break;
                case PetNature.SpecialAttackUpAttackDown:
                    up = PetStatKind.SpecialAttack;
                    down = PetStatKind.Attack;
                    break;
                case PetNature.SpecialAttackUpDefenseDown:
                    up = PetStatKind.SpecialAttack;
                    down = PetStatKind.Defense;
                    break;
                case PetNature.SpecialAttackUpSpecialDefenseDown:
                    up = PetStatKind.SpecialAttack;
                    down = PetStatKind.SpecialDefense;
                    break;
                case PetNature.SpecialAttackUpSpeedDown:
                    up = PetStatKind.SpecialAttack;
                    down = PetStatKind.Speed;
                    break;
                case PetNature.DefenseUpAttackDown:
                    up = PetStatKind.Defense;
                    down = PetStatKind.Attack;
                    break;
                case PetNature.DefenseUpSpecialAttackDown:
                    up = PetStatKind.Defense;
                    down = PetStatKind.SpecialAttack;
                    break;
                case PetNature.DefenseUpSpecialDefenseDown:
                    up = PetStatKind.Defense;
                    down = PetStatKind.SpecialDefense;
                    break;
                case PetNature.DefenseUpSpeedDown:
                    up = PetStatKind.Defense;
                    down = PetStatKind.Speed;
                    break;
                case PetNature.SpecialDefenseUpAttackDown:
                    up = PetStatKind.SpecialDefense;
                    down = PetStatKind.Attack;
                    break;
                case PetNature.SpecialDefenseUpSpecialAttackDown:
                    up = PetStatKind.SpecialDefense;
                    down = PetStatKind.SpecialAttack;
                    break;
                case PetNature.SpecialDefenseUpDefenseDown:
                    up = PetStatKind.SpecialDefense;
                    down = PetStatKind.Defense;
                    break;
                case PetNature.SpecialDefenseUpSpeedDown:
                    up = PetStatKind.SpecialDefense;
                    down = PetStatKind.Speed;
                    break;
                case PetNature.SpeedUpAttackDown:
                    up = PetStatKind.Speed;
                    down = PetStatKind.Attack;
                    break;
                case PetNature.SpeedUpSpecialAttackDown:
                    up = PetStatKind.Speed;
                    down = PetStatKind.SpecialAttack;
                    break;
                case PetNature.SpeedUpDefenseDown:
                    up = PetStatKind.Speed;
                    down = PetStatKind.Defense;
                    break;
                case PetNature.SpeedUpSpecialDefenseDown:
                    up = PetStatKind.Speed;
                    down = PetStatKind.SpecialDefense;
                    break;
            }
        }
    }
}
