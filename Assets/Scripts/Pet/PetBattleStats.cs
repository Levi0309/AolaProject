namespace EnjoyJob.Battle
{
    // 进入战斗后实际使用的八项属性。
    // 体力会变成 MaxHp；命中、闪避、暴击先用默认值，后面可以接装备、状态和技能强化。
    public struct PetBattleStats
    {
        public int Attack;
        public int SpecialAttack;
        public int Defense;
        public int SpecialDefense;
        public int MaxHp;
        public int Speed;
        public int Accuracy;
        public int Evasion;
        public int Critical;
    }
}
