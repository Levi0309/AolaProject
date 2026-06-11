using System;
using System.IO;
using System.Text;
using EnjoyJob;
using EnjoyJob.Battle.Skills;
using UnityEditor;
using UnityEngine;

namespace EnjoyJob.EditorTools
{
    public sealed class SkillEditorWindow : EditorWindow
    {
        private const string SkillJsonPath = "Assets/Resources/Data/skills.json";
        private const string EffectScriptFolder = "Assets/Scripts/Battle/SkillScripts";

        private static readonly string[] ElementLabels = SkillElementCatalog.DisplayNames;
        private static readonly string[] AttackTypeLabels = { "属性攻击", "特殊攻击", "普通攻击" };

        private SkillTable table = new SkillTable();
        private Vector2 listScroll;
        private Vector2 detailScroll;
        private string searchText = string.Empty;
        private int selectedIndex = -1;
        private bool dirty;

        [MenuItem("EnjoyJob/技能编辑器")]
        public static void Open()
        {
            SkillEditorWindow window = GetWindow<SkillEditorWindow>("技能编辑器");
            window.minSize = new Vector2(980f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            LoadTable();
        }

        private void OnGUI()
        {
            DrawToolbar();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawSkillList();
                DrawDetailPanel();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUI.enabled = dirty;
                if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(56f)))
                {
                    SaveTable();
                }

                GUI.enabled = true;
                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(56f)))
                {
                    LoadTable();
                }

                GUILayout.Space(8f);
                GUILayout.Label("搜索", GUILayout.Width(36f));
                searchText = GUILayout.TextField(searchText, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField, GUILayout.Width(220f));

                if (GUILayout.Button("新增技能", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                {
                    AddSkill();
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label($"技能: {table.skills.Count}");
            }
        }

        private void DrawSkillList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(340f)))
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label("ID", EditorStyles.boldLabel, GUILayout.Width(56f));
                    GUILayout.Label("名称", EditorStyles.boldLabel, GUILayout.Width(120f));
                    GUILayout.Label("属性", EditorStyles.boldLabel, GUILayout.Width(64f));
                    GUILayout.Label("威力", EditorStyles.boldLabel, GUILayout.Width(48f));
                    GUILayout.Label("PP", EditorStyles.boldLabel, GUILayout.Width(48f));
                }

                listScroll = EditorGUILayout.BeginScrollView(listScroll);
                for (int i = 0; i < table.skills.Count; i++)
                {
                    SkillRecord skill = table.skills[i];
                    if (!MatchesSearch(skill))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal(GetRowStyle(i), GUILayout.Height(96f));

                    if (GUILayout.Button(GetIconTexture(skill.iconResourcePath), GUIStyle.none, GUILayout.Width(54f), GUILayout.Height(54f)))
                    {
                        SelectSkill(i);
                    }

                    string rowText = $"ID: {skill.id}\n名称: {skill.name}\n属性: {skill.element}  威力: {skill.power}  PP: {skill.maxPp}\n描述: {GetListDescription(skill.description)}";
                    if (GUILayout.Button(rowText, GetListTextButtonStyle(), GUILayout.Height(82f)))
                    {
                        SelectSkill(i);
                    }

                    if (GUILayout.Button("删除", GUILayout.Width(48f), GUILayout.Height(24f)))
                    {
                        DeleteSkill(i);
                        EditorGUILayout.EndHorizontal();
                        GUIUtility.ExitGUI();
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawDetailPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (!HasSelection())
                {
                    EditorGUILayout.HelpBox("选择左侧技能，或者点击新增技能。", MessageType.Info);
                    return;
                }

                SkillRecord skill = table.skills[selectedIndex];
                detailScroll = EditorGUILayout.BeginScrollView(detailScroll);

                EditorGUILayout.LabelField("基础属性", EditorStyles.boldLabel);
                DrawIntField("技能 ID", ref skill.id);
                DrawTextField("技能名", ref skill.name);
                DrawElementPopup(skill);
                DrawIntField("技能威力", ref skill.power, -1);
                DrawIntField("最大 PP", ref skill.maxPp);
                DrawAttackTypePopup(skill);
                DrawTextField("属性图标路径", ref skill.iconResourcePath);

                GUILayout.Space(10f);
                EditorGUILayout.LabelField("描述", EditorStyles.boldLabel);
                string description = EditorGUILayout.TextArea(skill.description, GetDescriptionTextAreaStyle(), GUILayout.MinHeight(44f), GUILayout.MaxHeight(50f), GUILayout.ExpandWidth(true));
                if (description != skill.description)
                {
                    UndoableChange();
                    skill.description = description;
                }

                GUILayout.Space(10f);
                EditorGUILayout.LabelField("高级选项", EditorStyles.boldLabel);
                DrawTextField("技能脚本类名", ref skill.SkillEffectScrpit);
                DrawScriptObjectField(skill);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("复制当前技能", GUILayout.Width(120f)))
                    {
                        DuplicateSkill(skill);
                    }

                    if (GUILayout.Button("创建技能脚本", GUILayout.Width(120f)))
                    {
                        CreateEffectScript(skill);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private Texture GetIconTexture(string resourcePath)
        {
            return ResourceImageLoader.LoadSpriteOrDefault(resourcePath).texture;
        }

        private GUIStyle GetListTextButtonStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };

            style.normal.textColor = EditorStyles.label.normal.textColor;
            return style;
        }

        private GUIStyle GetDescriptionTextAreaStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };

            return style;
        }

        private static string GetListDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return "无";
            }

            return description.Replace("\r", " ").Replace("\n", " ");
        }

        private void SelectSkill(int index)
        {
            selectedIndex = index;
            GUI.FocusControl(null);
            Repaint();
        }

        private void DrawElementPopup(SkillRecord skill)
        {
            string currentElement = SkillElementCatalog.GetDisplayName(skill.element);
            int currentIndex = Mathf.Max(0, Array.IndexOf(ElementLabels, currentElement));
            int nextIndex = EditorGUILayout.Popup("技能属性", currentIndex, ElementLabels);
            if (nextIndex != currentIndex || skill.element != currentElement)
            {
                UndoableChange();
                bool usesElementIcon = IsElementIconPath(skill.iconResourcePath);
                skill.element = ElementLabels[nextIndex];
                if (usesElementIcon)
                {
                    skill.iconResourcePath = SkillElementCatalog.GetIconResourcePath(skill.element);
                }
            }
        }

        private void DrawAttackTypePopup(SkillRecord skill)
        {
            int currentIndex = Mathf.Max(0, Array.IndexOf(AttackTypeLabels, skill.attackType));
            int nextIndex = EditorGUILayout.Popup("攻击类型", currentIndex, AttackTypeLabels);
            if (nextIndex != currentIndex)
            {
                UndoableChange();
                skill.attackType = AttackTypeLabels[nextIndex];
            }
        }

        private void DrawScriptObjectField(SkillRecord skill)
        {
            MonoScript currentScript = FindMonoScript(skill.SkillEffectScrpit);
            MonoScript nextScript = (MonoScript)EditorGUILayout.ObjectField("技能脚本文件", currentScript, typeof(MonoScript), false);
            if (nextScript != currentScript && nextScript != null)
            {
                UndoableChange();
                skill.SkillEffectScrpit = nextScript.GetClass() != null ? nextScript.GetClass().Name : nextScript.name;
            }
        }

        private void DrawTextField(string label, ref string value)
        {
            string nextValue = EditorGUILayout.TextField(label, value ?? string.Empty);
            if (nextValue != value)
            {
                UndoableChange();
                value = nextValue;
            }
        }

        private void DrawIntField(string label, ref int value, int minValue = 0)
        {
            int nextValue = EditorGUILayout.IntField(label, value);
            if (nextValue != value)
            {
                UndoableChange();
                value = Mathf.Max(minValue, nextValue);
            }
        }

        private GUIStyle GetRowStyle(int index)
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            if (index == selectedIndex)
            {
                style.normal.background = Texture2D.grayTexture;
            }

            return style;
        }

        private bool MatchesSearch(SkillRecord skill)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            string text = searchText.Trim();
            return skill.id.ToString().Contains(text)
                || Contains(skill.name, text)
                || Contains(skill.description, text)
                || Contains(skill.element, text)
                || Contains(skill.attackType, text)
                || Contains(skill.SkillEffectScrpit, text);
        }

        private bool Contains(string value, string text)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsElementIconPath(string resourcePath)
        {
            return string.IsNullOrWhiteSpace(resourcePath)
                || resourcePath.StartsWith("Icons/Elements/", StringComparison.OrdinalIgnoreCase);
        }

        private void AddSkill()
        {
            SkillRecord skill = new SkillRecord
            {
                id = GetNextId(),
                name = "新技能",
                description = "对敌方单体造成伤害。",
                element = SkillElementCatalog.GetDisplayName(SkillElement.Wood),
                power = 40,
                maxPp = 35,
                attackType = "普通攻击",
                iconResourcePath = SkillElementCatalog.GetIconResourcePath(SkillElement.Wood),
                SkillEffectScrpit = "NewSkillEffect"
            };

            table.skills.Add(skill);
            selectedIndex = table.skills.Count - 1;
            dirty = true;
            SaveTable();
        }

        private void DuplicateSkill(SkillRecord source)
        {
            SkillRecord copy = new SkillRecord
            {
                id = GetNextId(),
                name = source.name + " 副本",
                description = source.description,
                element = source.element,
                power = source.power,
                maxPp = source.maxPp,
                attackType = source.attackType,
                iconResourcePath = source.iconResourcePath,
                SkillEffectScrpit = source.SkillEffectScrpit
            };

            table.skills.Add(copy);
            selectedIndex = table.skills.Count - 1;
            dirty = true;
        }

        private void DeleteSkill(int index)
        {
            if (!EditorUtility.DisplayDialog("删除技能", $"确定删除 {table.skills[index].name} 吗？", "删除", "取消"))
            {
                return;
            }

            table.skills.RemoveAt(index);
            selectedIndex = table.skills.Count > 0 ? Mathf.Clamp(selectedIndex, 0, table.skills.Count - 1) : -1;
            dirty = true;
        }

        private void CreateEffectScript(SkillRecord skill)
        {
            string className = SanitizeClassName(skill.SkillEffectScrpit);
            if (string.IsNullOrWhiteSpace(className))
            {
                className = $"Skill{skill.id}Effect";
                skill.SkillEffectScrpit = className;
                dirty = true;
            }

            string path = $"{EffectScriptFolder}/{className}.cs";
            if (File.Exists(path))
            {
                EditorUtility.DisplayDialog("脚本已存在", $"{path} 已经存在。", "知道了");
                return;
            }

            string code =
$@"using UnityEngine;

namespace EnjoyJob.Battle.Skills
{{
    public sealed class {className} : SkillEffectScript
    {{
        public override void Execute(SkillEffectContext context)
        {{
            Debug.Log($""{{context.Skill.Name}}: 执行技能效果。"");
        }}
    }}
}}
";

            File.WriteAllText(path, code, new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private string SanitizeClassName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            foreach (char c in value.Trim())
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    builder.Append(c);
                }
            }

            if (builder.Length == 0)
            {
                return string.Empty;
            }

            if (char.IsDigit(builder[0]))
            {
                builder.Insert(0, '_');
            }

            return builder.ToString();
        }

        private MonoScript FindMonoScript(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                return null;
            }

            string[] scriptGuids = AssetDatabase.FindAssets($"{className} t:MonoScript");
            foreach (string guid in scriptGuids)
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
                Type scriptClass = script != null ? script.GetClass() : null;
                if (scriptClass != null && scriptClass.Name == className)
                {
                    return script;
                }
            }

            return null;
        }

        private int GetNextId()
        {
            int nextId = 1001;
            foreach (SkillRecord skill in table.skills)
            {
                nextId = Mathf.Max(nextId, skill.id + 1);
            }

            return nextId;
        }

        private bool HasSelection()
        {
            return selectedIndex >= 0 && selectedIndex < table.skills.Count;
        }

        private void UndoableChange()
        {
            dirty = true;
        }

        private void LoadTable()
        {
            if (!File.Exists(SkillJsonPath))
            {
                table = new SkillTable();
                selectedIndex = -1;
                dirty = false;
                return;
            }

            string json = File.ReadAllText(SkillJsonPath, Encoding.UTF8);
            table = JsonUtility.FromJson<SkillTable>(json) ?? new SkillTable();
            if (table.skills == null)
            {
                table.skills = new System.Collections.Generic.List<SkillRecord>();
            }

            selectedIndex = table.skills.Count > 0 ? Mathf.Clamp(selectedIndex, 0, table.skills.Count - 1) : -1;
            dirty = false;
        }

        private void SaveTable()
        {
            string json = JsonUtility.ToJson(table, true);
            File.WriteAllText(SkillJsonPath, json, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(SkillJsonPath);
            dirty = false;
        }
    }
}
