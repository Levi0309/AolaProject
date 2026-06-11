namespace EnjoyJob.Battle.Skills
{
    // 能被技能扣血的目标需要实现这个接口。
    // 例如 PetCtrl 实现 TakeDamage 后，叶刃击就能真正让它掉血。
    public interface ISkillDamageReceiver
    {
        void TakeDamage(int damage);
    }
}
