using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EnjoyJob.EditorTools
{
    public sealed class SpriteSequencePadderWindow : EditorWindow
    {
        private enum FrameAnchor
        {
            BottomCenter,
            Center,
            TopCenter
        }

        private FrameAnchor anchor = FrameAnchor.BottomCenter;
        private string outputFolderName = "Padded";
        private bool setSpriteImportSettings = true;
        private Vector2 scroll;
        private List<string> selectedTexturePaths = new List<string>();

        [MenuItem("EnjoyJob/工具/序列帧统一画布")]
        public static void Open()
        {
            SpriteSequencePadderWindow window = GetWindow<SpriteSequencePadderWindow>("序列帧统一画布");
            window.minSize = new Vector2(520f, 420f);
            window.RefreshSelection();
            window.Show();
        }

        private void OnSelectionChange()
        {
            RefreshSelection();
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("统一序列帧画布", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("选中一组从 SWF 导出的 PNG 帧，然后生成同样宽高的新 PNG。默认底部居中对齐，适合站在地面上的角色动画。", MessageType.Info);

            anchor = (FrameAnchor)EditorGUILayout.EnumPopup("对齐方式", anchor);
            outputFolderName = EditorGUILayout.TextField("输出文件夹名", outputFolderName);
            setSpriteImportSettings = EditorGUILayout.Toggle("设置为 Sprite", setSpriteImportSettings);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("刷新选择", GUILayout.Width(100f)))
                {
                    RefreshSelection();
                }

                GUI.enabled = selectedTexturePaths.Count > 0;
                if (GUILayout.Button("生成统一画布序列帧", GUILayout.Width(170f)))
                {
                    PadSelectedFrames();
                }

                GUI.enabled = true;
            }

            GUILayout.Space(8f);
            EditorGUILayout.LabelField($"已选择 PNG: {selectedTexturePaths.Count}", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (string path in selectedTexturePaths)
            {
                EditorGUILayout.LabelField(path);
            }

            EditorGUILayout.EndScrollView();
        }

        private void RefreshSelection()
        {
            selectedTexturePaths.Clear();

            foreach (UnityEngine.Object selectedObject in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(selectedObject);
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                selectedTexturePaths.Add(path.Replace("\\", "/"));
            }

            selectedTexturePaths.Sort(StringComparer.OrdinalIgnoreCase);
        }

        private void PadSelectedFrames()
        {
            if (selectedTexturePaths.Count <= 0)
            {
                return;
            }

            List<FrameData> frames = LoadFrames(selectedTexturePaths);
            if (frames.Count <= 0)
            {
                EditorUtility.DisplayDialog("没有可处理图片", "请选择 PNG 序列帧。", "知道了");
                return;
            }

            int maxWidth = 0;
            int maxHeight = 0;
            foreach (FrameData frame in frames)
            {
                maxWidth = Mathf.Max(maxWidth, frame.Texture.width);
                maxHeight = Mathf.Max(maxHeight, frame.Texture.height);
            }

            string firstDirectory = Path.GetDirectoryName(selectedTexturePaths[0])?.Replace("\\", "/") ?? "Assets";
            string outputDirectory = $"{firstDirectory}/{SanitizeFolderName(outputFolderName)}";
            Directory.CreateDirectory(outputDirectory);

            try
            {
                for (int i = 0; i < frames.Count; i++)
                {
                    FrameData frame = frames[i];
                    Texture2D paddedTexture = CreatePaddedTexture(frame.Texture, maxWidth, maxHeight);
                    string fileName = Path.GetFileNameWithoutExtension(frame.AssetPath);
                    string outputPath = $"{outputDirectory}/{fileName}_padded.png";

                    File.WriteAllBytes(outputPath, paddedTexture.EncodeToPNG());
                    DestroyImmediate(paddedTexture);
                }
            }
            finally
            {
                foreach (FrameData frame in frames)
                {
                    DestroyImmediate(frame.Texture);
                }
            }

            AssetDatabase.Refresh();

            if (setSpriteImportSettings)
            {
                ApplySpriteImportSettings(outputDirectory);
            }

            EditorUtility.DisplayDialog("生成完成", $"已输出到：\n{outputDirectory}\n\n统一尺寸：{maxWidth} x {maxHeight}", "好");
        }

        private List<FrameData> LoadFrames(List<string> paths)
        {
            List<FrameData> frames = new List<FrameData>();
            foreach (string path in paths)
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(File.ReadAllBytes(path)))
                {
                    DestroyImmediate(texture);
                    continue;
                }

                texture.name = Path.GetFileNameWithoutExtension(path);
                frames.Add(new FrameData(path, texture));
            }

            return frames;
        }

        private Texture2D CreatePaddedTexture(Texture2D source, int width, int height)
        {
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32[] clearPixels = new Color32[width * height];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = new Color32(0, 0, 0, 0);
            }

            result.SetPixels32(clearPixels);

            int xOffset = Mathf.RoundToInt((width - source.width) * 0.5f);
            int yOffset = GetYOffset(source.height, height);
            result.SetPixels(xOffset, yOffset, source.width, source.height, source.GetPixels());
            result.Apply();
            return result;
        }

        private int GetYOffset(int sourceHeight, int targetHeight)
        {
            switch (anchor)
            {
                case FrameAnchor.Center:
                    return Mathf.RoundToInt((targetHeight - sourceHeight) * 0.5f);
                case FrameAnchor.TopCenter:
                    return targetHeight - sourceHeight;
                default:
                    return 0;
            }
        }

        private void ApplySpriteImportSettings(string outputDirectory)
        {
            string[] pngPaths = Directory.GetFiles(outputDirectory, "*.png");
            foreach (string rawPath in pngPaths)
            {
                string assetPath = rawPath.Replace("\\", "/");
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 4096;
                importer.SaveAndReimport();
            }
        }

        private string SanitizeFolderName(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                return "Padded";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(invalidChar.ToString(), string.Empty);
            }

            return string.IsNullOrWhiteSpace(folderName) ? "Padded" : folderName.Trim();
        }

        private readonly struct FrameData
        {
            public FrameData(string assetPath, Texture2D texture)
            {
                AssetPath = assetPath;
                Texture = texture;
            }

            public string AssetPath { get; }
            public Texture2D Texture { get; }
        }
    }
}
