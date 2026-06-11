namespace EnjoyJob.Battle.Skills
{
    // 所有“每个技能一个脚本”的基类。
    // 具体技能脚本继承它，在 Execute 里写这个技能真正发生的效果。
    public abstract class SkillEffectScript
    {
        public abstract void Execute(SkillEffectContext context);
    }
}
