using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EnjoyJob.Battle.Skills
{
    // 技能按钮的显示组件。
    // 用技能定义和当前PP刷新名字、PP文本、属性图标，并在PP为0时禁用按钮。
    public sealed class SkillButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private Image elementIcon;
        [SerializeField] private Text nameText;
        [SerializeField] private Text ppText;
        [SerializeField] private Text powerText;
        [SerializeField] private Color disabledIconColor = new Color(0.35f, 0.35f, 0.35f, 0.7f);
        private SkillTooltipView tooltipView;

        private SkillDefinition currentDefinition;//代表哪个技能的静态资料(技能信息  最大pp power等 id等)

        public void Bind(SkillDefinition definition, LearnedSkill learnedSkill, bool selectionEnabled)
        {
            currentDefinition = definition;

            bool hasSkill = definition != null && learnedSkill != null;
            bool hasPp = hasSkill && learnedSkill.HasPp;
            bool canUse = selectionEnabled && hasPp;

            if (button != null)
            {
                button.interactable = canUse;
            }

            if (elementIcon != null)
            {
                elementIcon.sprite = hasSkill ? definition.LoadIcon() : null;
                elementIcon.color = hasPp ? Color.white : disabledIconColor;
            }

            if (nameText != null)
            {
                nameText.text = hasSkill ? definition.Name : string.Empty;
            }

            if (ppText != null)
            {
                ppText.text = hasSkill ? $"{learnedSkill.currentPp}/{definition.MaxPp}" : string.Empty;
            }

            if (powerText != null)
            {
                powerText.text = hasSkill ? $"威力 {definition.Power}" : string.Empty;
            }

            if (!hasSkill && tooltipView != null)
            {
                tooltipView.Hide();
            }
        }

        public void SetClickAction(UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (action != null)
            {
                button.onClick.AddListener(action);
            }
        }

        public void SetTooltipView(SkillTooltipView nextTooltipView)
        {
            tooltipView = nextTooltipView;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltip(eventData);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (tooltipView != null && tooltipView.IsShowing)
            {
                tooltipView.SetScreenPosition(eventData.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipView != null)
            {
                tooltipView.Hide();
            }
        }

        private void ShowTooltip(PointerEventData eventData)
        {
            if (tooltipView == null || currentDefinition == null)
            {
                return;
            }

            tooltipView.Show(currentDefinition, eventData.position);
        }
    }
}
