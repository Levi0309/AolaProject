using System;
using UnityEngine;

namespace EnjoyJob.Battle.Skills
{
    // 运行时真正使用的技能定义。
    // 它由 SkillRecord 转换而来，会把中文属性/攻击类型解析成枚举，并按需加载图标。
    // 战斗逻辑应该读取这个类，而不是直接依赖 JSON 原始字符串。
    public sealed class SkillDefinition
    {
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public SkillElement Element { get; }
        public int Power { get; }
        public int MaxPp { get; }
        public SkillAttackType AttackType { get; }
        public string IconResourcePath { get; }
        public string SkillEffectScrpit { get; }

        private Sprite icon;
        private bool iconLoaded;

        public SkillDefinition(SkillRecord record)
        {
            Id = record.id;
            Name = record.name;
            Description = record.description;
            Element = SkillElementCatalog.Parse(record.element);
            Power = Mathf.Max(-1, record.power);
            MaxPp = Mathf.Max(0, record.maxPp);
            AttackType = ParseAttackType(record.attackType);
            IconResourcePath = record.iconResourcePath;
            SkillEffectScrpit = record.SkillEffectScrpit;
        }

        public Sprite LoadIcon()
        {
            if (iconLoaded)
            {
                return icon;
            }

            iconLoaded = true;

            icon = ResourceImageLoader.LoadSpriteOrDefault(IconResourcePath);
            return icon;
        }

        private static SkillAttackType ParseAttackType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return SkillAttackType.NormalAttack;
            }

            switch (value.Trim())
            {
                case "属性攻击":
                    return SkillAttackType.AttributeAttack;
                case "特殊攻击":
                    return SkillAttackType.SpecialAttack;
                case "普通攻击":
                    return SkillAttackType.NormalAttack;
                default:
                    return Enum.TryParse(value, true, out SkillAttackType parsed) ? parsed : SkillAttackType.NormalAttack;
            }
        }
    }
}
