using UnityEngine;
using UnityEngine.UI;

namespace EnjoyJob.Battle.Skills
{
    // 技能悬停说明框。
    // 显示格式：
    // 技能名(普通攻击)
    // 技能描述
    public sealed class SkillTooltipView : MonoBehaviour
    {
        private const int DescriptionCharsPerLine = 9;

        [SerializeField] private RectTransform panel;
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private Color titleColor = Color.white;
        [SerializeField] private Color descriptionColor = new Color(0.86f, 0.86f, 0.86f, 1f);
        [SerializeField] private Vector2 screenOffset = new Vector2(18f, -18f);
        [SerializeField] private bool ignorePunctuationInDescriptionWrap;

        public bool IsShowing => panel != null && panel.gameObject.activeSelf;

        private void Awake()
        {
            if (panel == null)
            {
                panel = transform as RectTransform;
            }

            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInParent<Canvas>();
            }

            Hide();
        }

        public void Show(SkillDefinition definition, Vector2 screenPosition)
        {
            if (definition == null || panel == null)
            {
                return;
            }

            if (nameText != null)
            {
                nameText.text = $"{definition.Name}({GetAttackTypeName(definition.AttackType)})";
                nameText.color = titleColor;
            }

            if (descriptionText != null)
            {
                descriptionText.text = WrapDescription(definition.Description, ignorePunctuationInDescriptionWrap);
                descriptionText.color = descriptionColor;
            }

            panel.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
            SetScreenPosition(screenPosition);
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.gameObject.SetActive(false);
            }
        }

        public void SetScreenPosition(Vector2 screenPosition)
        {
            if (panel == null)
            {
                return;
            }

            Canvas canvas = rootCanvas != null ? rootCanvas : GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Vector2 targetScreenPosition = screenPosition + screenOffset;

            RectTransform parentRect = panel.parent as RectTransform;
            if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, targetScreenPosition, camera, out Vector2 localPoint))
            {
                panel.anchoredPosition = localPoint;
                ClampInsideParent(parentRect);
            }
        }

        private void ClampInsideParent(RectTransform parentRect)
        {
            Vector2 position = panel.anchoredPosition;
            Vector2 panelSize = panel.rect.size;
            Vector2 parentSize = parentRect.rect.size;
            Vector2 pivot = panel.pivot;

            float minX = -parentSize.x * parentRect.pivot.x + panelSize.x * pivot.x;
            float maxX = parentSize.x * (1f - parentRect.pivot.x) - panelSize.x * (1f - pivot.x);
            float minY = -parentSize.y * parentRect.pivot.y + panelSize.y * pivot.y;
            float maxY = parentSize.y * (1f - parentRect.pivot.y) - panelSize.y * (1f - pivot.y);

            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);
            panel.anchoredPosition = position;
        }

        private static string WrapDescription(string text, bool ignorePunctuation)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder(text.Length + text.Length / DescriptionCharsPerLine);
            int lineCharCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char current = text[i];
                builder.Append(current);

                if (current == '\n')
                {
                    lineCharCount = 0;
                    continue;
                }

                if (!ignorePunctuation || !IsPunctuation(current))
                {
                    lineCharCount++;
                }

                if (lineCharCount >= DescriptionCharsPerLine && i < text.Length - 1)
                {
                    builder.Append('\n');
                    lineCharCount = 0;
                }
            }

            return builder.ToString();
        }

        private static bool IsPunctuation(char value)
        {
            switch (value)
            {
                case '，':
                case '。':
                case '、':
                case '；':
                case '：':
                case '！':
                case '？':
                case '（':
                case '）':
                case '《':
                case '》':
                case ',':
                case '.':
                case ';':
                case ':':
                case '!':
                case '?':
                case '(':
                case ')':
                case '<':
                case '>':
                case '%':
                    return true;
                default:
                    return false;
            }
        }

        private static string GetAttackTypeName(SkillAttackType attackType)
        {
            switch (attackType)
            {
                case SkillAttackType.AttributeAttack:
                    return "属性攻击";
                case SkillAttackType.SpecialAttack:
                    return "特殊攻击";
                case SkillAttackType.NormalAttack:
                default:
                    return "普通攻击";
            }
        }
    }
}
