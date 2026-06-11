namespace EnjoyJob.Battle.Skills
{
    // 执行技能效果时传给技能脚本的上下文。
    // 里面放当前技能、使用者和目标；后面可以继续扩展战斗场景、随机数、日志等。
    public sealed class SkillEffectContext
    {
        public SkillDefinition Skill { get; }
        public object User { get; }
        public object Target { get; }
        public int Damage { get; }

        public SkillEffectContext(SkillDefinition skill, object user, object target, int damage)
        {
            Skill = skill;
            User = user;
            Target = target;
            Damage = damage;
        }
    }
}
