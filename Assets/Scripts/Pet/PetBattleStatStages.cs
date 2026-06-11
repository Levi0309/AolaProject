using System;
using UnityEngine;

namespace EnjoyJob.Battle
{
    // 一场战斗中的临时属性等级。
    // 默认全是0，范围是 -6 到 +6；强化/弱化技能改这里，不改永久种族值。
    [Serializable]
    public class PetBattleStatStages
    {
        public const int MinStage = -6;
        public const int MaxStage = 6;

        public int attack;
        public int specialAttack;
        public int defense;
        public int specialDefense;
        public int speed;
        public int accuracy;
        public int evasion;
        public int critical;

        public void Reset()
        {
            attack = 0;
            specialAttack = 0;
            defense = 0;
            specialDefense = 0;
            speed = 0;
            accuracy = 0;
            evasion = 0;
            critical = 0;
        }

        public int GetStage(PetStatKind statKind)
        {
            switch (statKind)
            {
                case PetStatKind.Attack:
                    return attack;
                case PetStatKind.SpecialAttack:
                    return specialAttack;
                case PetStatKind.Defense:
                    return defense;
                case PetStatKind.SpecialDefense:
                    return specialDefense;
                case PetStatKind.Speed:
                    return speed;
                case PetStatKind.Accuracy:
                    return accuracy;
                case PetStatKind.Evasion:
                    return evasion;
                case PetStatKind.Critical:
                    return critical;
                default:
                    return 0;
            }
        }

        public int AddStage(PetStatKind statKind, int amount)
        {
            int nextStage = Mathf.Clamp(GetStage(statKind) + amount, MinStage, MaxStage);
            SetStage(statKind, nextStage);
            return nextStage;
        }

        public void SetStage(PetStatKind statKind, int value)
        {
            int safeValue = Mathf.Clamp(value, MinStage, MaxStage);
            switch (statKind)
            {
                case PetStatKind.Attack:
                    attack = safeValue;
                    break;
                case PetStatKind.SpecialAttack:
                    specialAttack = safeValue;
                    break;
                case PetStatKind.Defense:
                    defense = safeValue;
                    break;
                case PetStatKind.SpecialDefense:
                    specialDefense = safeValue;
                    break;
                case PetStatKind.Speed:
                    speed = safeValue;
                    break;
                case PetStatKind.Accuracy:
                    accuracy = safeValue;
                    break;
                case PetStatKind.Evasion:
                    evasion = safeValue;
                    break;
                case PetStatKind.Critical:
                    critical = safeValue;
                    break;
            }
        }

        public float GetStatMultiplier(PetStatKind statKind)
        {
            return GetMultiplier(GetStage(statKind));
        }

        public static float GetMultiplier(int stage)
        {
            int safeStage = Mathf.Clamp(stage, MinStage, MaxStage);
            if (safeStage >= 0)
            {
                return (2f + safeStage) / 2f;
            }

            return 2f / (2f - safeStage);
        }
    }
}
