using EnjoyJob.Battle.Skills;
using UnityEngine;

namespace EnjoyJob.Battle
{
    // 玩家技能按钮面板。
    // 它把 YabiSkillSet 里的技能绑定到按钮上，并在玩家选择阶段把点击转给 TurnBattleController。
    public sealed class BattleSkillPanel : MonoBehaviour
    {
        [SerializeField] private SkillButtonView[] skillButtons;
        [SerializeField] private SkillTooltipView tooltipView;

        private TurnBattleController battleController;
        private TurnBattleController subscribedController;

        private void OnEnable()
        {
            SubscribeBattleController();
            Refresh();
        }

        private void OnDisable()
        {
            if (subscribedController != null)
            {
                subscribedController.StateChanged -= OnBattleStateChanged;
                subscribedController = null;
            }
        }

        public void Refresh()
        {
            ResolveBattleController();

            if (skillButtons == null)
            {
                return;
            }

            YabiSkillSet skillSet = battleController != null && battleController.PlayerUnit != null
                ? battleController.PlayerUnit.SkillSet
                : null;
            bool canSelect = CanSelectSkill();

            for (int i = 0; i < skillButtons.Length; i++)
            {
                SkillButtonView buttonView = skillButtons[i];
                if (buttonView == null)
                {
                    continue;
                }

                buttonView.SetTooltipView(tooltipView);

                if (skillSet == null || i >= skillSet.LearnedSkills.Count)
                {
                    ClearButton(buttonView);
                    continue;
                }

                BindSkillButton(buttonView, skillSet, skillSet.LearnedSkills[i], canSelect);
            }
        }

        private void SubscribeBattleController()
        {
            ResolveBattleController();

            if (battleController == null || subscribedController == battleController)
            {
                return;
            }

            if (subscribedController != null)
            {
                subscribedController.StateChanged -= OnBattleStateChanged;
            }

            subscribedController = battleController;
            subscribedController.StateChanged += OnBattleStateChanged;
        }

        private void ResolveBattleController()
        {
            if (battleController == null)
            {
                battleController = TurnBattleController.Instance;
            }
        }

        private bool CanSelectSkill()
        {
            return battleController != null
                && battleController.State == BattleState.WaitingForPlayerSkill;
        }

        private void BindSkillButton(SkillButtonView buttonView, YabiSkillSet skillSet, LearnedSkill learnedSkill, bool canSelect)
        {
            if (learnedSkill == null)
            {
                ClearButton(buttonView);
                return;
            }

            SkillDefinition definition = skillSet.GetDefinitionOrNull(learnedSkill.skillId);
            int skillId = learnedSkill.skillId;

            buttonView.Bind(definition, learnedSkill, canSelect);
            buttonView.SetClickAction(() => SubmitSkill(skillId));
        }

        private void SubmitSkill(int skillId)
        {
            if (battleController != null)
            {
                battleController.TrySelectPlayerSkill(skillId);
            }
        }

        private static void ClearButton(SkillButtonView buttonView)
        {
            buttonView.Bind(null, null, false);
            buttonView.SetClickAction(null);
        }

        private void OnBattleStateChanged(BattleState state)
        {
            Refresh();
        }
    }
}
