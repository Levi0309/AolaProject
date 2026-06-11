using System.Collections;
using EnjoyJob.Battle.Skills;
using UnityEngine;
using UnityEngine.UI;

namespace EnjoyJob.Battle
{


    // 单只亚比的战斗状态 UI：只负责显示，数据由 PetCtrl 主动推送。
    public class BattlePetStatusView : MonoBehaviour
    {
        [SerializeField] private Image elementIcon;
        [SerializeField] private Image secondElementIcon;
        [SerializeField] private Text nameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text hpText;
        [SerializeField] private Image hpBackgroundImage;
        [SerializeField] private Image hpFillImage;

        [Header("Stats Properties")]
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private Text attackStageText;
        [SerializeField] private Text specialAttackStageText;
        [SerializeField] private Text defenseStageText;
        [SerializeField] private Text specialDefenseStageText;
        [SerializeField] private Text accuracyStageText;
        [SerializeField] private Text evasionStageText;
        [SerializeField] private Text criticalStageText;
        [SerializeField] private Text speedStageText;
        [SerializeField] private Animator statStageUpAnimator;
        [SerializeField] private Animator statStageDownAnimator;

        [Header("DamageText")]
        #region Damage Popup

        [SerializeField] private Text damagePopupText;
        [SerializeField] private Color criticalDamageColor = new Color(1f, 0.15f, 0.08f);
        [SerializeField] private Color strongDamageColor = new Color(1f, 0.32f, 0.18f);
        [SerializeField] private Color neutralDamageColor = new Color(1f, 0.95f, 0.55f);
        [SerializeField] private Color weakDamageColor = new Color(0.58f, 0.75f, 1f);
        [SerializeField] private Color missDamageColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Vector2 damagePopupStartOffset;
        [SerializeField] private Vector2 damagePopupEndOffset = new Vector2(0f, 48f);
        [SerializeField] private float damagePopupDuration = 0.75f;
        [SerializeField, Range(0f, 1f)] private float damagePopupFadeStart = 0.75f;

        #endregion

        private const float EnemyLevel100NamePositionX = 1.267f;

        private RectTransform nameTextRect;
        private Vector2 originalNameTextPosition;
        private RectTransform hpFillRect;
        private float fullHpWidth;
        private Color hpBackgroundColor;
        private bool statsPanelRequested;
        private bool initialized;
        private RectTransform damagePopupRect;
        private Vector2 damagePopupOriginalPosition;
        private Coroutine damagePopupRoutine;

        public void SetInfo(string petName, int level, string element, BattleSide side)
        {
            SetInfo(petName, level, element, string.Empty, side);
        }

        public void SetInfo(string petName, int level, string element, string secondElement, BattleSide side)
        {
            InitializeIfNeeded();

            if (nameText != null)
            {
                nameText.text = petName;
            }

            if (levelText != null)
            {
                levelText.text = $"{level}";
            }

            ApplyEnemyNamePosition(level, side);
            RefreshElementIcon(elementIcon, element, true);
            RefreshElementIcon(secondElementIcon, secondElement, false);
        }

        public void SetHp(int currentHp, int maxHp)
        {
            InitializeIfNeeded();

            int safeMaxHp = Mathf.Max(1, maxHp);
            float ratio = Mathf.Clamp01((float)currentHp / safeMaxHp);

            if (hpText != null)
            {
                hpText.text = $"{currentHp}/{safeMaxHp}";
            }

            if (hpFillRect != null)
            {
                hpFillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullHpWidth * ratio);
            }

            if (hpBackgroundImage != null)
            {
                hpBackgroundImage.color = currentHp <= 0 ? Color.black : hpBackgroundColor;
            }
        }

        public void ShowStatStages(PetBattleStatStages stages)
        {
            InitializeIfNeeded();
            statsPanelRequested = true;
            SetStatStages(stages);
        }

        public void RefreshStatStages(PetBattleStatStages stages)
        {
            if (!statsPanelRequested)
            {
                return;
            }

            InitializeIfNeeded();
            SetStatStages(stages);
        }

        public void HideStatStages()
        {
            statsPanelRequested = false;
            SetStatsPanelActive(false);
        }

        /// <summary>
        /// 能力等级提升动画
        /// </summary>
        public void PlayStatStageUpAnimation()
        {
            PlayAnimator(statStageUpAnimator);
        }

        public void PlayStatStageDownAnimation()
        {
            PlayAnimator(statStageDownAnimator);
        }



        private void CacheHpFillWidth()
        {
            if (hpFillImage == null)
            {
                hpFillRect = null;
                fullHpWidth = 0f;
                return;
            }

            hpFillRect = hpFillImage.rectTransform;
            fullHpWidth = hpFillRect.rect.width;
            if (fullHpWidth <= 0f)
            {
                fullHpWidth = hpFillRect.sizeDelta.x;
            }
        }

        private void RefreshElementIcon(Image icon, string element, bool showDefault)
        {
            if (icon == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(element) && !showDefault)
            {
                icon.enabled = false;
                return;
            }

            icon.sprite = ResourceImageLoader.LoadSpriteOrDefault(SkillElementCatalog.GetIconResourcePath(element));
            icon.enabled = true;
        }

        private void InitializeIfNeeded()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            if (nameText != null)
            {
                nameTextRect = nameText.rectTransform;
                originalNameTextPosition = nameTextRect.anchoredPosition;
            }

            if (hpBackgroundImage != null)
            {
                hpBackgroundColor = hpBackgroundImage.color;
            }

            if (damagePopupText != null)
            {
                damagePopupRect = damagePopupText.rectTransform;
                damagePopupOriginalPosition = damagePopupRect.anchoredPosition;
                damagePopupText.gameObject.SetActive(false);
            }

            if (statsPanel == null)
            {
                statsPanel = attackStageText != null ? attackStageText.transform.parent.gameObject : null;
            }

            SetStatsPanelActive(false);
            CacheHpFillWidth();
        }

        private void SetStatStages(PetBattleStatStages stages)
        {
            bool hasAnyStage;
            if (stages == null)
            {
                SetStageText(attackStageText, "攻击", 0);
                SetStageText(specialAttackStageText, "特攻", 0);
                SetStageText(defenseStageText, "防御", 0);
                SetStageText(specialDefenseStageText, "特防", 0);
                SetStageText(accuracyStageText, "命中", 0);
                SetStageText(evasionStageText, "闪避", 0);
                SetStageText(criticalStageText, "暴击", 0);
                SetStageText(speedStageText, "速度", 0);
                SetStatsPanelActive(false);
                return;
            }

            hasAnyStage = SetStageText(attackStageText, "攻击", stages.attack);
            hasAnyStage |= SetStageText(specialAttackStageText, "特攻", stages.specialAttack);
            hasAnyStage |= SetStageText(defenseStageText, "防御", stages.defense);
            hasAnyStage |= SetStageText(specialDefenseStageText, "特防", stages.specialDefense);
            hasAnyStage |= SetStageText(accuracyStageText, "命中", stages.accuracy);
            hasAnyStage |= SetStageText(evasionStageText, "闪避", stages.evasion);
            hasAnyStage |= SetStageText(criticalStageText, "暴击", stages.critical);
            hasAnyStage |= SetStageText(speedStageText, "速度", stages.speed);
            SetStatsPanelActive(statsPanelRequested && hasAnyStage);
        }

        private bool SetStageText(Text text, string label, int stage)
        {
            if (text == null)
            {
                return false;
            }

            bool shouldShow = stage != 0;
            text.gameObject.SetActive(shouldShow);
            if (!shouldShow)
            {
                return false;
            }

            text.text = stage > 0 ? $"{label}+{stage}" : stage < 0 ? $"{label}{stage}" : label;
            text.color = stage > 0 ? Color.red : Color.green;
            return true;
        }

        private void SetStatsPanelActive(bool active)
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(active);
            }
        }

        private static void PlayAnimator(Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            if (!animator.gameObject.activeSelf)
            {
                animator.gameObject.SetActive(true);
            }

            animator.enabled = true;
            animator.Play(0, 0, 0f);
            animator.Update(0f);
        }

        private void ApplyEnemyNamePosition(int level, BattleSide side)
        {
            if (nameTextRect == null)
            {
                return;
            }

            Vector2 position = originalNameTextPosition;
            if (side == BattleSide.Enemy && level >= 100)
            {
                position.x = EnemyLevel100NamePositionX;
            }

            nameTextRect.anchoredPosition = position;
        }
        #region Damage Popup

        public void PlayDamagePopup(int damage, BattleDamagePopupType popupType)
        {
            InitializeIfNeeded();
            if (damagePopupText == null)
            {
                return;
            }

            if (damagePopupRoutine != null)
            {
                StopCoroutine(damagePopupRoutine);
            }

            Color popupColor = GetDamagePopupColor(popupType);
            popupColor.a = 1f;

            damagePopupText.text = BuildDamagePopupText(damage, popupType);
            damagePopupText.color = popupColor;
            damagePopupText.gameObject.SetActive(true);

            if (damagePopupRect != null)
            {
                damagePopupRect.anchoredPosition = damagePopupOriginalPosition + damagePopupStartOffset;
            }

            damagePopupRoutine = StartCoroutine(PlayDamagePopupRoutine(popupColor));
        }

        private IEnumerator PlayDamagePopupRoutine(Color baseColor)
        {
            float safeDuration = Mathf.Max(0.01f, damagePopupDuration);
            Vector2 startPosition = damagePopupOriginalPosition + damagePopupStartOffset;
            Vector2 endPosition = damagePopupOriginalPosition + damagePopupEndOffset;
            float elapsed = 0f;

            while (elapsed < safeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / safeDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                if (damagePopupRect != null)
                {
                    damagePopupRect.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, eased);
                }

                Color nextColor = baseColor;
                float fadeT = Mathf.InverseLerp(damagePopupFadeStart, 1f, t);
                nextColor.a = 1f - fadeT;
                damagePopupText.color = nextColor;
                yield return null;
            }

            damagePopupText.gameObject.SetActive(false);
            damagePopupRoutine = null;
        }

        private string BuildDamagePopupText(int damage, BattleDamagePopupType popupType)
        {
            int safeDamage = Mathf.Max(0, damage);
            switch (popupType)
            {
                case BattleDamagePopupType.Critical:
                    return safeDamage > 0 ? $"暴击 -{safeDamage}" : "暴击";
                case BattleDamagePopupType.Strong:
                    return safeDamage > 0 ? $"克制 -{safeDamage}" : "克制";
                case BattleDamagePopupType.Weak:
                    return safeDamage > 0 ? $"微弱 -{safeDamage}" : "微弱";
                case BattleDamagePopupType.Miss:
                    return "未命中";
                default:
                    return safeDamage > 0 ? $"-{safeDamage}" : "0";
            }
        }

        private Color GetDamagePopupColor(BattleDamagePopupType popupType)
        {
            switch (popupType)
            {
                case BattleDamagePopupType.Critical:
                    return criticalDamageColor;
                case BattleDamagePopupType.Strong:
                    return strongDamageColor;
                case BattleDamagePopupType.Weak:
                    return weakDamageColor;
                case BattleDamagePopupType.Miss:
                    return missDamageColor;
                default:
                    return neutralDamageColor;
            }
        }
        #endregion

    }
    public enum BattleDamagePopupType
    {
        Critical,
        Strong,
        Neutral,
        Weak,
        Miss
    }

}
