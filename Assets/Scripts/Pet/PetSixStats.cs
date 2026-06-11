using System;

namespace EnjoyJob.Battle
{
    // 六项可成长属性：攻击、特攻、防御、特防、体力、速度。
    // 种族值、天赋值、学习力都可以复用这个结构。
    [Serializable]
    public class PetSixStats
    {
        public int attack;
        public int specialAttack;
        public int defense;
        public int specialDefense;
        public int stamina;
        public int speed;

        public static PetSixStats CreateDefaultTalent()
        {
            return new PetSixStats
            {
                attack = 31,
                specialAttack = 31,
                defense = 31,
                specialDefense = 31,
                stamina = 31,
                speed = 31
            };
        }
    }
}
