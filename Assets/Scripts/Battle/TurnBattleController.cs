using System;
using System.Collections;
using EnjoyJob.Battle.Skills;
using EnjoyJob;
using UnityEngine;

namespace EnjoyJob.Battle
{
    // 回合制战斗主流程。
    // 玩家选择技能 -> 等敌人选择 -> 玩家释放 -> 等表现结束 -> 敌人释放 -> 回到玩家选择。
    public class TurnBattleController : SingletonMonoBehaviour<TurnBattleController>
    {
        [SerializeField] private PetCtrl playerUnit;
        [SerializeField] private PetCtrl enemyUnit;
        [SerializeField] private EnemySkillAi enemyAi;
        [SerializeField] private float enemyDecisionMinSeconds = 0.6f;
        [SerializeField] private float enemyDecisionMaxSeconds = 1.8f;
        [SerializeField] private float actionFinishDelaySeconds = 0.8f;
        [SerializeField] private bool resetUnitsOnStart = true;

        public event Action<BattleState> StateChanged;
        public event Action<string> BattleLog;
        public event Action<PetCtrl> BattleFinished;
        public event Action<BattleAction> ActionStarted;
        public event Action<BattleAction> ActionFinished;

        public PetCtrl PlayerUnit => playerUnit;
        public PetCtrl EnemyUnit => enemyUnit;
        public BattleState State { get; private set; } = BattleState.NotStarted;

        private readonly SkillEffectExecutor effectExecutor = new SkillEffectExecutor();
        private Coroutine turnRoutine;

        protected override void Awake()
        {
            base.Awake();

            if (enemyAi == null)
            {
                enemyAi = GetComponent<EnemySkillAi>();
            }
        }

        private void Start()
        {
            StartBattle();
        }

        public void StartBattle()
        {
            if (playerUnit == null || enemyUnit == null)
            {
                Log("战斗单位没有配置完整。");
                SetState(BattleState.Finished);
                return;
            }

            if (resetUnitsOnStart)
            {
                playerUnit.ResetForBattle();
                enemyUnit.ResetForBattle();
            }

            EnsureAnimationPlayer(playerUnit).PlayIdle();
            EnsureAnimationPlayer(enemyUnit).PlayIdle();

            Log($"战斗开始：{playerUnit.DisplayName} VS {enemyUnit.DisplayName}");
            SetState(BattleState.WaitingForPlayerSkill);
        }

        public bool TrySelectPlayerSkill(int skillId)
        {
            if (State != BattleState.WaitingForPlayerSkill)
            {
                return false;
            }

            if (!playerUnit.TryPrepareSkill(skillId, out SkillDefinition playerSkill))
            {
                Log("这个技能现在不能使用。");
                return false;
            }

            if (turnRoutine != null)
            {
                StopCoroutine(turnRoutine);
            }

            turnRoutine = StartCoroutine(ResolveTurnRoutine(playerSkill));
            return true;
        }

        private IEnumerator ResolveTurnRoutine(SkillDefinition playerSkill)
        {
            SetState(BattleState.WaitingForEnemySkill);
            Log("等待敌方选择技能...");

            float waitSeconds = UnityEngine.Random.Range(enemyDecisionMinSeconds, enemyDecisionMaxSeconds);
            yield return new WaitForSeconds(waitSeconds);

            if (!TryPrepareEnemySkill(out SkillDefinition enemySkill))
            {
                Log($"{enemyUnit.DisplayName} 没有可用技能。");
                FinishBattle(playerUnit);
                yield break;
            }

            SetState(BattleState.ResolvingTurn);

            yield return ExecuteActionRoutine(new BattleAction(playerUnit, enemyUnit, playerSkill));
            if (TryFinishIfSomeoneFainted())
            {
                yield break;
            }

            yield return ExecuteActionRoutine(new BattleAction(enemyUnit, playerUnit, enemySkill));
            if (TryFinishIfSomeoneFainted())
            {
                yield break;
            }

            SetState(BattleState.WaitingForPlayerSkill);
            turnRoutine = null;
        }

        private bool TryPrepareEnemySkill(out SkillDefinition enemySkill)
        {
            enemySkill = null;

            if (enemyAi == null)
            {
                enemyAi = gameObject.AddComponent<EnemySkillAi>();
            }

            return enemyAi.TryChooseSkill(enemyUnit, out int skillId)
                && enemyUnit.TryPrepareSkill(skillId, out enemySkill);
        }

        private IEnumerator ExecuteActionRoutine(BattleAction action)
        {
            if (action.User.IsFainted)
            {
                yield break;
            }

            ActionStarted?.Invoke(action);
            Log($"{action.User.DisplayName} 使用了 {action.Skill.Name}！");

            PetAnimationPlayer userAnimationPlayer = EnsureAnimationPlayer(action.User);
            yield return userAnimationPlayer.PlayAttackRoutine(action.Skill.AttackType);

            int targetHpBeforeEffect = action.Target.CurrentHp;
            effectExecutor.TryExecute(action.Skill, action.User, action.Target);
            ShowDamagePopup(action, targetHpBeforeEffect);

            if (ShouldPlayHurtAnimation(action, targetHpBeforeEffect))
            {
                PetAnimationPlayer targetAnimationPlayer = EnsureAnimationPlayer(action.Target);
                yield return targetAnimationPlayer.PlayHurtRoutine();
            }

            // 留一点表现收尾时间，之后伤害数字/VFX 做完后可以改成等待它们的回调。
            if (actionFinishDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(actionFinishDelaySeconds);
            }

            ActionFinished?.Invoke(action);
        }

        private PetAnimationPlayer EnsureAnimationPlayer(PetCtrl unit)
        {
            PetAnimationPlayer animationPlayer = unit.GetComponent<PetAnimationPlayer>();
            if (animationPlayer == null)
            {
                animationPlayer = unit.gameObject.AddComponent<PetAnimationPlayer>();
            }

            return animationPlayer;
        }

        private static bool ShouldPlayHurtAnimation(BattleAction action, int targetHpBeforeEffect)
        {
            if (action == null || action.Skill == null || action.Target == null)
            {
                return false;
            }

            bool isDamageAttack = action.Skill.AttackType == SkillAttackType.NormalAttack
                || action.Skill.AttackType == SkillAttackType.SpecialAttack;

            return isDamageAttack
                && action.Skill.Power > -1
                && action.Target.CurrentHp < targetHpBeforeEffect;
        }

        private static void ShowDamagePopup(BattleAction action, int targetHpBeforeEffect)
        {
            if (action == null || action.Skill == null || action.Target == null)
            {
                return;
            }

            bool isDamageAttack = action.Skill.AttackType == SkillAttackType.NormalAttack
                || action.Skill.AttackType == SkillAttackType.SpecialAttack;

            if (!isDamageAttack || action.Skill.Power <= 0)
            {
                return;
            }

            int actualDamage = Mathf.Max(0, targetHpBeforeEffect - action.Target.CurrentHp);
            BattleDamagePopupType popupType = actualDamage > 0
                ? BattleDamagePopupType.Neutral
                : BattleDamagePopupType.Miss;

            action.Target.PlayDamagePopup(actualDamage, popupType);
        }

        private bool TryFinishIfSomeoneFainted()
        {
            if (enemyUnit.IsFainted)
            {
                FinishBattle(playerUnit);
                return true;
            }

            if (playerUnit.IsFainted)
            {
                FinishBattle(enemyUnit);
                return true;
            }

            return false;
        }

        private void FinishBattle(PetCtrl winner)
        {
            SetState(BattleState.Finished);
            Log($"{winner.DisplayName} 获胜！");
            BattleFinished?.Invoke(winner);
            turnRoutine = null;
        }

        private void SetState(BattleState nextState)
        {
            if (State == nextState)
            {
                return;
            }

            State = nextState;
            StateChanged?.Invoke(State);
        }

        private void Log(string message)
        {
            Debug.Log(message, this);
            BattleLog?.Invoke(message);
        }
    }
}
