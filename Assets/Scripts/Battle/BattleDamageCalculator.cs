using EnjoyJob.Battle.Skills;
using UnityEngine;

namespace EnjoyJob.Battle
{
    // 战斗伤害计算器。
    // 现在先用基础公式接上攻击/特攻/防御/特防，后面再加属性克制、暴击、随机浮动和状态修正。
    public static class BattleDamageCalculator
    {
        public static int CalculateDamage(PetCtrl attacker, PetCtrl defender, SkillDefinition skill)
        {
            if (attacker == null || defender == null || skill == null || skill.Power <= 0)
            {
                return 0;
            }

            int attackStat = skill.AttackType == SkillAttackType.SpecialAttack
                ? ApplyStage(attacker.BattleStats.SpecialAttack, attacker.StatStages, PetStatKind.SpecialAttack)
                : ApplyStage(attacker.BattleStats.Attack, attacker.StatStages, PetStatKind.Attack);

            int defenseStat = skill.AttackType == SkillAttackType.SpecialAttack
                ? ApplyStage(defender.BattleStats.SpecialDefense, defender.StatStages, PetStatKind.SpecialDefense)
                : ApplyStage(defender.BattleStats.Defense, defender.StatStages, PetStatKind.Defense);

            float levelFactor = (2f * attacker.Level + 10f) / 250f;
            float rawDamage = levelFactor * skill.Power * attackStat / Mathf.Max(1, defenseStat) + 2f;
            return Mathf.Max(1, Mathf.FloorToInt(rawDamage));
        }

        private static int ApplyStage(int statValue, PetBattleStatStages stages, PetStatKind statKind)
        {
            if (stages == null)
            {
                return statValue;
            }

            return Mathf.Max(1, Mathf.FloorToInt(statValue * stages.GetStatMultiplier(statKind)));
        }
    }
}
