using System;
using System.IO;
using System.Text;
using EnjoyJob;
using EnjoyJob.Battle;
using EnjoyJob.Battle.Skills;
using UnityEditor;
using UnityEngine;

namespace EnjoyJob.EditorTools
{
    public sealed class PetEditorWindow : EditorWindow
    {
        private const string PetJsonPath = "Assets/Resources/Data/pets.json";
        private static readonly string[] ElementLabels = SkillElementCatalog.DisplayNames;
        private static readonly string[] OptionalElementLabels = CreateOptionalElementLabels();

        private PetTable table = new PetTable();
        private Vector2 listScroll;
        private Vector2 detailScroll;
        private string searchText = string.Empty;
        private int selectedIndex = -1;
        private bool dirty;

        [MenuItem("EnjoyJob/亚比编辑器")]
        public static void Open()
        {
            PetEditorWindow window = GetWindow<PetEditorWindow>("亚比编辑器");
            window.minSize = new Vector2(1020f, 620f);
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
                DrawPetList();
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

                if (GUILayout.Button("新增亚比", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                {
                    AddPet();
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label($"亚比: {table.pets.Count}");
            }
        }

        private void DrawPetList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(380f)))
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label("ID", EditorStyles.boldLabel, GUILayout.Width(70f));
                    GUILayout.Label("名称", EditorStyles.boldLabel, GUILayout.Width(120f));
                    GUILayout.Label("属性", EditorStyles.boldLabel, GUILayout.Width(64f));
                }

                listScroll = EditorGUILayout.BeginScrollView(listScroll);
                for (int i = 0; i < table.pets.Count; i++)
                {
                    PetRecord pet = table.pets[i];
                    if (!MatchesSearch(pet))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal(GetRowStyle(i), GUILayout.Height(76f));

                    if (GUILayout.Button(GetImageTexture(pet.imageResourcePath), GUIStyle.none, GUILayout.Width(58f), GUILayout.Height(58f)))
                    {
                        SelectPet(i);
                    }

                    string rowText = $"ID: {pet.id}\n名称: {pet.name}\n属性: {GetElementSummary(pet)}";
                    if (GUILayout.Button(rowText, GetListTextButtonStyle(), GUILayout.Height(58f)))
                    {
                        SelectPet(i);
                    }

                    if (GUILayout.Button("删除", GUILayout.Width(48f), GUILayout.Height(24f)))
                    {
                        DeletePet(i);
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
                    EditorGUILayout.HelpBox("选择左侧亚比，或者点击新增亚比。", MessageType.Info);
                    return;
                }

                PetRecord pet = table.pets[selectedIndex];
                EnsureNestedObjects(pet);
                detailScroll = EditorGUILayout.BeginScrollView(detailScroll);

                EditorGUILayout.LabelField("基础信息", EditorStyles.boldLabel);
                DrawIntField("亚比 ID", ref pet.id);
                DrawTextField("亚比名字", ref pet.name);
                DrawElementPopup("第一系别", ref pet.element, false);
                DrawElementPopup("第二系别", ref pet.secondElement, true);
                DrawSpriteResourceField("亚比图片", ref pet.imageResourcePath);
                DrawTextField("亚比图片路径", ref pet.imageResourcePath);

                GUILayout.Space(10f);
                EditorGUILayout.LabelField("亚比动画", EditorStyles.boldLabel);
                DrawAnimationResourceField("待机动画", ref pet.animations.idle);
                DrawAnimationResourceField("普通攻击动画1", ref pet.animations.normalAttack1);
                DrawAnimationResourceField("普通攻击动画2", ref pet.animations.normalAttack2);
                DrawAnimationResourceField("特殊攻击动画1", ref pet.animations.specialAttack1);
                DrawAnimationResourceField("特殊攻击动画2", ref pet.animations.specialAttack2);
                DrawAnimationResourceField("属性攻击动画1", ref pet.animations.attributeAttack1);
                DrawAnimationResourceField("属性攻击动画2", ref pet.animations.attributeAttack2);
                DrawAnimationResourceField("受击动画", ref pet.animations.hurt);

                GUILayout.Space(10f);
                DrawSpeciesStats(pet.speciesStats);

                GUILayout.Space(10f);
                DrawLearnableSkills(pet);

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawSpeciesStats(PetSpeciesStats stats)
        {
            EditorGUILayout.LabelField("种族值", EditorStyles.boldLabel);
            DrawIntField("攻击", ref stats.attack);
            DrawIntField("防御", ref stats.defense);
            DrawIntField("特攻", ref stats.specialAttack);
            DrawIntField("特防", ref stats.specialDefense);
            DrawIntField("体力", ref stats.stamina);
            DrawIntField("速度", ref stats.speed);
        }

        private void DrawLearnableSkills(PetRecord pet)
        {
            EditorGUILayout.LabelField("可学习技能", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("每条配置表示：亚比达到该等级时学习这个技能。当前规则：技能满4个后自动替换最后一个技能。", MessageType.None);

            if (pet.learnableSkills == null)
            {
                pet.learnableSkills = new System.Collections.Generic.List<PetLearnableSkillRecord>();
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("等级", EditorStyles.boldLabel, GUILayout.Width(80f));
                GUILayout.Label("技能ID", EditorStyles.boldLabel, GUILayout.Width(100f));
                GUILayout.Label("技能名", EditorStyles.boldLabel);
                GUILayout.Label("", GUILayout.Width(50f));
            }

            for (int i = 0; i < pet.learnableSkills.Count; i++)
            {
                PetLearnableSkillRecord record = pet.learnableSkills[i];
                if (record == null)
                {
                    record = new PetLearnableSkillRecord();
                    pet.learnableSkills[i] = record;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawIntField("", ref record.learnLevel, GUILayout.Width(80f));
                    DrawIntField("", ref record.skillId, GUILayout.Width(100f));
                    GUILayout.Label(GetSkillName(record.skillId));

                    if (GUILayout.Button("删除", GUILayout.Width(50f)))
                    {
                        pet.learnableSkills.RemoveAt(i);
                        dirty = true;
                        GUIUtility.ExitGUI();
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("添加可学习技能", GUILayout.Width(130f)))
                {
                    pet.learnableSkills.Add(new PetLearnableSkillRecord { learnLevel = 1, skillId = 1001 });
                    dirty = true;
                }

                if (GUILayout.Button("按等级排序", GUILayout.Width(100f)))
                {
                    pet.learnableSkills.Sort((a, b) => a.learnLevel.CompareTo(b.learnLevel));
                    dirty = true;
                }
            }
        }

        private string GetSkillName(int skillId)
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>("Data/skills");
            if (jsonAsset == null)
            {
                return "找不到技能表";
            }

            SkillTableProxy tableProxy = JsonUtility.FromJson<SkillTableProxy>(jsonAsset.text);
            if (tableProxy == null || tableProxy.skills == null)
            {
                return "技能表无效";
            }

            foreach (SkillRecordProxy skill in tableProxy.skills)
            {
                if (skill != null && skill.id == skillId)
                {
                    return skill.name;
                }
            }

            return "未知技能";
        }

        private Texture GetImageTexture(string resourcePath)
        {
            return ResourceImageLoader.LoadSpriteOrDefault(resourcePath).texture;
        }

        private void DrawSpriteResourceField(string label, ref string resourcePath)
        {
            Sprite currentSprite = string.IsNullOrWhiteSpace(resourcePath) ? null : Resources.Load<Sprite>(resourcePath);
            Sprite nextSprite = (Sprite)EditorGUILayout.ObjectField(label, currentSprite, typeof(Sprite), false);
            if (nextSprite == currentSprite)
            {
                return;
            }

            if (nextSprite == null)
            {
                dirty = true;
                resourcePath = string.Empty;
                return;
            }

            if (!TryGetResourcesPath(nextSprite, out string nextResourcePath))
            {
                EditorUtility.DisplayDialog(
                    "图片位置不对",
                    "请选择 Assets/Resources 文件夹里面的 Sprite。这样运行时才能通过 Resources 路径加载。",
                    "知道了");
                return;
            }

            dirty = true;
            resourcePath = nextResourcePath;
            Repaint();
        }

        private void DrawAnimationResourceField(string label, ref string resourcePath)
        {
            AnimationClip currentClip = string.IsNullOrWhiteSpace(resourcePath) ? null : Resources.Load<AnimationClip>(resourcePath);
            AnimationClip nextClip = (AnimationClip)EditorGUILayout.ObjectField(label, currentClip, typeof(AnimationClip), false);
            if (nextClip == currentClip)
            {
                return;
            }

            if (nextClip == null)
            {
                dirty = true;
                resourcePath = string.Empty;
                return;
            }

            if (!TryGetResourcesPath(nextClip, out string nextResourcePath))
            {
                EditorUtility.DisplayDialog(
                    "动画位置不对",
                    "请选择 Assets/Resources 文件夹里面的 .anim 文件。这样运行时才能通过 Resources 路径加载。",
                    "知道了");
                return;
            }

            dirty = true;
            resourcePath = nextResourcePath;
            Repaint();
        }

        private bool TryGetResourcesPath(UnityEngine.Object asset, out string resourcePath)
        {
            resourcePath = string.Empty;

            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            assetPath = assetPath.Replace("\\", "/");
            const string resourcesFolder = "/Resources/";
            int resourcesIndex = assetPath.IndexOf(resourcesFolder, StringComparison.OrdinalIgnoreCase);
            if (resourcesIndex < 0)
            {
                return false;
            }

            int pathStart = resourcesIndex + resourcesFolder.Length;
            string pathWithoutPrefix = assetPath.Substring(pathStart);
            resourcePath = Path.ChangeExtension(pathWithoutPrefix, null);
            return !string.IsNullOrEmpty(resourcePath);
        }

        private GUIStyle GetListTextButtonStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };

            style.normal.textColor = EditorStyles.label.normal.textColor;
            return style;
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

        private bool MatchesSearch(PetRecord pet)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            string text = searchText.Trim();
            return pet.id.ToString().Contains(text)
                || Contains(pet.name, text)
                || Contains(pet.element, text)
                || Contains(pet.secondElement, text)
                || Contains(pet.imageResourcePath, text);
        }

        private bool Contains(string value, string text)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void DrawTextField(string label, ref string value)
        {
            string nextValue = EditorGUILayout.TextField(label, value ?? string.Empty);
            if (nextValue != value)
            {
                dirty = true;
                value = nextValue;
            }
        }

        private void DrawIntField(string label, ref int value)
        {
            int nextValue = string.IsNullOrEmpty(label)
                ? EditorGUILayout.IntField(value)
                : EditorGUILayout.IntField(label, value);

            if (nextValue != value)
            {
                dirty = true;
                value = Mathf.Max(0, nextValue);
            }
        }

        private void DrawIntField(string label, ref int value, params GUILayoutOption[] options)
        {
            int nextValue = string.IsNullOrEmpty(label)
                ? EditorGUILayout.IntField(value, options)
                : EditorGUILayout.IntField(label, value, options);

            if (nextValue != value)
            {
                dirty = true;
                value = Mathf.Max(0, nextValue);
            }
        }

        private void DrawElementPopup(string label, ref string element, bool allowNone)
        {
            string[] labels = allowNone ? OptionalElementLabels : ElementLabels;
            string currentElement = allowNone ? GetOptionalElementOrDefault(element) : GetElementOrDefault(element);
            int currentIndex = Mathf.Max(0, Array.IndexOf(labels, currentElement));
            int nextIndex = EditorGUILayout.Popup(label, currentIndex, labels);
            string nextElement = allowNone && nextIndex == 0 ? string.Empty : labels[nextIndex];
            if (nextElement != element)
            {
                dirty = true;
                element = nextElement;
            }
        }

        private static string GetElementOrDefault(string element)
        {
            return string.IsNullOrWhiteSpace(element) ? ElementLabels[0] : SkillElementCatalog.GetDisplayName(element);
        }

        private static string GetOptionalElementOrDefault(string element)
        {
            return string.IsNullOrWhiteSpace(element) ? OptionalElementLabels[0] : SkillElementCatalog.GetDisplayName(element);
        }

        private static string GetElementSummary(PetRecord pet)
        {
            string firstElement = GetElementOrDefault(pet.element);
            string secondElement = GetOptionalElementOrDefault(pet.secondElement);
            return secondElement == OptionalElementLabels[0] ? firstElement : $"{firstElement} / {secondElement}";
        }

        private static string[] CreateOptionalElementLabels()
        {
            string[] labels = new string[ElementLabels.Length + 1];
            labels[0] = "无";
            Array.Copy(ElementLabels, 0, labels, 1, ElementLabels.Length);
            return labels;
        }

        private void AddPet()
        {
            PetRecord pet = new PetRecord
            {
                id = GetNextId(),
                name = "新亚比",
                element = ElementLabels[0],
                secondElement = string.Empty,
                imageResourcePath = "Images/Pets/new_pet",
                animations = new PetAnimationPaths
                {
                    idle = "Animations/Pets/new_pet/idle",
                    normalAttack1 = "Animations/Pets/new_pet/normal_attack_1",
                    normalAttack2 = "Animations/Pets/new_pet/normal_attack_2",
                    specialAttack1 = "Animations/Pets/new_pet/special_attack_1",
                    specialAttack2 = "Animations/Pets/new_pet/special_attack_2",
                    attributeAttack1 = "Animations/Pets/new_pet/attribute_attack_1",
                    attributeAttack2 = "Animations/Pets/new_pet/attribute_attack_2",
                    hurt = "Animations/Pets/new_pet/hurt"
                },
                speciesStats = new PetSpeciesStats
                {
                    attack = 80,
                    specialAttack = 80,
                    defense = 80,
                    specialDefense = 80,
                    stamina = 80,
                    speed = 80
                }
            };

            pet.learnableSkills.Add(new PetLearnableSkillRecord { learnLevel = 1, skillId = 1001 });
            table.pets.Add(pet);
            selectedIndex = table.pets.Count - 1;
            dirty = true;
        }

        private void DeletePet(int index)
        {
            if (!EditorUtility.DisplayDialog("删除亚比", $"确定删除 {table.pets[index].name} 吗？", "删除", "取消"))
            {
                return;
            }

            table.pets.RemoveAt(index);
            selectedIndex = table.pets.Count > 0 ? Mathf.Clamp(selectedIndex, 0, table.pets.Count - 1) : -1;
            dirty = true;
        }

        private void SelectPet(int index)
        {
            selectedIndex = index;
            GUI.FocusControl(null);
            Repaint();
        }

        private void EnsureNestedObjects(PetRecord pet)
        {
            if (pet.animations == null)
            {
                pet.animations = new PetAnimationPaths();
            }

            if (pet.speciesStats == null)
            {
                pet.speciesStats = new PetSpeciesStats();
            }

            if (pet.learnableSkills == null)
            {
                pet.learnableSkills = new System.Collections.Generic.List<PetLearnableSkillRecord>();
            }
        }

        private int GetNextId()
        {
            int nextId = 1001;
            foreach (PetRecord pet in table.pets)
            {
                nextId = Mathf.Max(nextId, pet.id + 1);
            }

            return nextId;
        }

        private bool HasSelection()
        {
            return selectedIndex >= 0 && selectedIndex < table.pets.Count;
        }

        private void LoadTable()
        {
            if (!File.Exists(PetJsonPath))
            {
                table = new PetTable();
                selectedIndex = -1;
                dirty = false;
                return;
            }

            string json = File.ReadAllText(PetJsonPath, Encoding.UTF8);
            table = JsonUtility.FromJson<PetTable>(json) ?? new PetTable();
            if (table.pets == null)
            {
                table.pets = new System.Collections.Generic.List<PetRecord>();
            }

            selectedIndex = table.pets.Count > 0 ? Mathf.Clamp(selectedIndex, 0, table.pets.Count - 1) : -1;
            dirty = false;
        }

        private void SaveTable()
        {
            string json = JsonUtility.ToJson(table, true);
            File.WriteAllText(PetJsonPath, json, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(PetJsonPath);
            dirty = false;
        }

        [Serializable]
        private class SkillTableProxy
        {
            public SkillRecordProxy[] skills;
        }

        [Serializable]
        private class SkillRecordProxy
        {
            public int id;
            public string name;
        }
    }
}
