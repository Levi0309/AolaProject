namespace EnjoyJob.Battle
{
    // 当前战斗处在哪个阶段，用它避免玩家在结算中重复点技能。
    public enum BattleState
    {
        NotStarted,
        WaitingForPlayerSkill,
        WaitingForEnemySkill,
        ResolvingTurn,
        Finished
    }
}
