namespace EnjoyJob.Battle
{
    // 性格。先放常用骨架：平衡，以及提升/降低某一项的性格。
    // 命名先偏程序可读，编辑器里以后可以再显示中文名。
    public enum PetNature
    {
        Balanced,
        AttackUpSpecialAttackDown,
        AttackUpDefenseDown,
        AttackUpSpecialDefenseDown,
        AttackUpSpeedDown,
        SpecialAttackUpAttackDown,
        SpecialAttackUpDefenseDown,
        SpecialAttackUpSpecialDefenseDown,
        SpecialAttackUpSpeedDown,
        DefenseUpAttackDown,
        DefenseUpSpecialAttackDown,
        DefenseUpSpecialDefenseDown,
        DefenseUpSpeedDown,
        SpecialDefenseUpAttackDown,
        SpecialDefenseUpSpecialAttackDown,
        SpecialDefenseUpDefenseDown,
        SpecialDefenseUpSpeedDown,
        SpeedUpAttackDown,
        SpeedUpSpecialAttackDown,
        SpeedUpDefenseDown,
        SpeedUpSpecialDefenseDown
    }
}
