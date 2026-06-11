namespace EnjoyJob.Battle
{
    // 可被局内等级修正影响的八项属性。
    // MaxHp 不进属性等级，体力变化由 CurrentHp/MaxHp 处理。
    public enum PetStatKind
    {
        None,
        Attack,
        SpecialAttack,
        Defense,
        SpecialDefense,
        Speed,
        Accuracy,
        Evasion,
        Critical
    }
}
