namespace EnjoyJob.Battle.Skills
{
    // 技能属性枚举。JSON里写中文，运行时会解析成这些枚举值。
    public enum SkillElement
    {
        Wood,
        Water,
        Fire,
        Dark,
        Light,
        Ancient,
        Fighting,
        HolySpirit,
        Ice,
        MySteriouse
    }

    public readonly struct SkillElementInfo
    {
        public SkillElementInfo(SkillElement element, string displayName, string iconResourcePath, params string[] aliases)
        {
            Element = element;
            DisplayName = displayName;
            IconResourcePath = iconResourcePath;
            Aliases = aliases;
        }

        public SkillElement Element { get; }
        public string DisplayName { get; }
        public string IconResourcePath { get; }
        public string[] Aliases { get; }
    }

    // 所有系别配置都放这里。新增系别时：加枚举、加一条配置即可。
    public static class SkillElementCatalog
    {
        private static readonly SkillElementInfo[] Elements =
        {
            new SkillElementInfo(SkillElement.Wood, "木系", "Icons/Elements/wood"),
            new SkillElementInfo(SkillElement.Water, "水系", "Icons/Elements/water"),
            new SkillElementInfo(SkillElement.Fire, "火系", "Icons/Elements/fire"),
            new SkillElementInfo(SkillElement.Dark, "黑暗系", "Icons/Elements/dark"),
            new SkillElementInfo(SkillElement.Light, "光明系", "Icons/Elements/light"),
            new SkillElementInfo(SkillElement.Ancient, "上古系", "Icons/Elements/ancient"),
            new SkillElementInfo(SkillElement.Fighting, "格斗系", "Icons/Elements/fighting"),
            new SkillElementInfo(SkillElement.HolySpirit, "圣灵系", "Icons/Elements/HolySpirit"),
            new SkillElementInfo(SkillElement.Ice, "冰系", "Icons/Elements/Ice"),
            new SkillElementInfo(SkillElement.MySteriouse, "神秘系", "Icons/Elements/Mysterious"),
        };

        public static string[] DisplayNames
        {
            get
            {
                string[] names = new string[Elements.Length];
                for (int i = 0; i < Elements.Length; i++)
                {
                    names[i] = Elements[i].DisplayName;
                }

                return names;
            }
        }

        public static SkillElement Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return SkillElement.Wood;
            }

            string normalized = value.Trim();
            for (int i = 0; i < Elements.Length; i++)
            {
                SkillElementInfo info = Elements[i];
                if (normalized == info.DisplayName || normalized == info.Element.ToString())
                {
                    return info.Element;
                }

                string[] aliases = info.Aliases;
                for (int j = 0; aliases != null && j < aliases.Length; j++)
                {
                    if (normalized == aliases[j])
                    {
                        return info.Element;
                    }
                }
            }

            return System.Enum.TryParse(normalized, true, out SkillElement parsed) ? parsed : SkillElement.Wood;
        }

        public static string GetDisplayName(SkillElement element)
        {
            return GetInfo(element).DisplayName;
        }

        public static string GetDisplayName(string value)
        {
            return GetDisplayName(Parse(value));
        }

        public static string GetIconResourcePath(SkillElement element)
        {
            return GetInfo(element).IconResourcePath;
        }

        public static string GetIconResourcePath(string value)
        {
            return GetIconResourcePath(Parse(value));
        }

        private static SkillElementInfo GetInfo(SkillElement element)
        {
            for (int i = 0; i < Elements.Length; i++)
            {
                if (Elements[i].Element == element)
                {
                    return Elements[i];
                }
            }

            return Elements[0];
        }
    }
}
