using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace EnjoyJob.EditorTools
{
    public sealed class SwfMovieConfigSequenceBuilderWindow : EditorWindow
    {
        private const string DefaultConfigPath = @"D:\aola\sprites\images\binaryData\87_mmo.petfight.bitmap.Pet1461Config_1_MovieConfig.bin";
        private const string DefaultImageFolderPath = @"D:\aola\sprites\images\images";
        private const string DefaultOutputFolder = "Assets/Resources/Animations/Pets/pet1461_rebuilt";

        private string configPath = DefaultConfigPath;
        private string imageFolderPath = DefaultImageFolderPath;
        private string outputFolder = DefaultOutputFolder;
        private string imagePrefix = "pet1461_1";
        private float frameRate = 24f;
        private bool createAnimationClips = true;
        private bool useSharedCanvasForAllActions = true;
        private Vector2 scroll;
        private string lastResult = string.Empty;

        [MenuItem("EnjoyJob/工具/SWF配置还原序列帧")]
        public static void Open()
        {
            SwfMovieConfigSequenceBuilderWindow window = GetWindow<SwfMovieConfigSequenceBuilderWindow>("SWF配置还原");
            window.minSize = new Vector2(640f, 520f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("SWF MovieConfig 序列帧还原", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("读取 Pet*_MovieConfig.bin 里的 图片ID,x,y 配置，把导出的 PNG 按真实偏移贴回统一透明画布，并可生成 Unity .anim。", MessageType.Info);

            configPath = EditorGUILayout.TextField("MovieConfig .bin", configPath);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("选择 .bin", GUILayout.Width(100f)))
                {
                    string selected = EditorUtility.OpenFilePanel("选择 MovieConfig .bin", Path.GetDirectoryName(configPath), "bin");
                    if (!string.IsNullOrEmpty(selected))
                    {
                        configPath = selected;
                    }
                }
            }

            imageFolderPath = EditorGUILayout.TextField("图片文件夹", imageFolderPath);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("选择图片文件夹", GUILayout.Width(120f)))
                {
                    string selected = EditorUtility.OpenFolderPanel("选择导出的 PNG 图片文件夹", imageFolderPath, string.Empty);
                    if (!string.IsNullOrEmpty(selected))
                    {
                        imageFolderPath = selected;
                    }
                }
            }

            outputFolder = EditorGUILayout.TextField("输出到 Assets", outputFolder);
            imagePrefix = EditorGUILayout.TextField("图片名前缀", imagePrefix);
            frameRate = EditorGUILayout.FloatField("动画帧率", frameRate);
            createAnimationClips = EditorGUILayout.Toggle("生成 .anim", createAnimationClips);
            useSharedCanvasForAllActions = EditorGUILayout.Toggle("所有动作共用画布", useSharedCanvasForAllActions);

            GUI.enabled = File.Exists(configPath) && Directory.Exists(imageFolderPath) && outputFolder.StartsWith("Assets", StringComparison.OrdinalIgnoreCase);
            if (GUILayout.Button("还原序列帧", GUILayout.Height(32f)))
            {
                BuildSequences();
            }

            GUI.enabled = true;

            if (Directory.Exists(outputFolder) && GUILayout.Button("仅重新应用清晰导入设置", GUILayout.Height(24f)))
            {
                ApplySpriteImportSettingsToFolder(outputFolder);
                EditorUtility.DisplayDialog("完成", "已重新设置输出文件夹下所有 PNG：Point过滤、无压缩、无MipMap。", "好");
            }

            GUILayout.Space(8f);
            EditorGUILayout.LabelField("结果", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.TextArea(lastResult, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void BuildSequences()
        {
            try
            {
                Dictionary<int, List<MovieFrame>> actions = ParseConfig(File.ReadAllText(configPath, Encoding.UTF8));
                Dictionary<ImageKey, string> imagePaths = IndexImages(imageFolderPath, imagePrefix);

                if (actions.Count == 0)
                {
                    lastResult = "没有从配置里解析到动作。";
                    return;
                }

                Directory.CreateDirectory(outputFolder);

                StringBuilder resultBuilder = new StringBuilder();
                resultBuilder.AppendLine($"动作数量: {actions.Count}");
                resultBuilder.AppendLine($"图片索引: {imagePaths.Count}");

                Dictionary<int, List<LoadedFrame>> loadedActions = LoadActions(actions, imagePaths, resultBuilder);
                CalculateOutputBounds(loadedActions, out int sharedMinX, out int sharedMinY, out int sharedMaxX, out int sharedMaxY);

                if (useSharedCanvasForAllActions)
                {
                    resultBuilder.AppendLine($"共享画布: {Mathf.Max(1, sharedMaxX - sharedMinX)}x{Mathf.Max(1, sharedMaxY - sharedMinY)}");
                }

                foreach (KeyValuePair<int, List<LoadedFrame>> pair in loadedActions)
                {
                    if (useSharedCanvasForAllActions)
                    {
                        BuildAction(pair.Key, pair.Value, sharedMinX, sharedMinY, sharedMaxX, sharedMaxY, resultBuilder);
                    }
                    else
                    {
                        CalculateBounds(pair.Value, out int minX, out int minY, out int maxX, out int maxY);
                        BuildAction(pair.Key, pair.Value, minX, minY, maxX, maxY, resultBuilder);
                    }
                }

                AssetDatabase.Refresh();
                lastResult = resultBuilder.ToString();
                EditorUtility.DisplayDialog("还原完成", lastResult, "好");
            }
            catch (Exception exception)
            {
                lastResult = exception.ToString();
                Debug.LogException(exception);
            }
        }

        private Dictionary<int, List<LoadedFrame>> LoadActions(
            Dictionary<int, List<MovieFrame>> actions,
            Dictionary<ImageKey, string> imagePaths,
            StringBuilder resultBuilder)
        {
            Dictionary<int, List<LoadedFrame>> loadedActions = new Dictionary<int, List<LoadedFrame>>();
            foreach (KeyValuePair<int, List<MovieFrame>> action in actions)
            {
                List<LoadedFrame> loadedFrames = new List<LoadedFrame>();
                foreach (MovieFrame frame in action.Value)
                {
                    ImageKey key = new ImageKey(action.Key, frame.ImageId);
                    if (!imagePaths.TryGetValue(key, out string imagePath))
                    {
                        resultBuilder.AppendLine($"动作 {action.Key}: 找不到图片 imageId={frame.ImageId}");
                        continue;
                    }

                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!texture.LoadImage(File.ReadAllBytes(imagePath)))
                    {
                        DestroyImmediate(texture);
                        resultBuilder.AppendLine($"动作 {action.Key}: 图片读取失败 {imagePath}");
                        continue;
                    }

                    loadedFrames.Add(new LoadedFrame(frame, texture));
                }

                loadedActions[action.Key] = loadedFrames;
            }

            return loadedActions;
        }

        private void BuildAction(int actionId, List<LoadedFrame> loadedFrames, int minX, int minY, int maxX, int maxY, StringBuilder resultBuilder)
        {
            if (loadedFrames.Count == 0)
            {
                resultBuilder.AppendLine($"动作 {actionId}: 没有可输出帧。");
                return;
            }

            int width = Mathf.Max(1, maxX - minX);
            int height = Mathf.Max(1, maxY - minY);

            string actionFolder = $"{outputFolder}/action_{actionId}";
            Directory.CreateDirectory(actionFolder);

            List<string> spriteAssetPaths = new List<string>();
            for (int i = 0; i < loadedFrames.Count; i++)
            {
                LoadedFrame loadedFrame = loadedFrames[i];
                Texture2D rebuiltFrame = CreateFrameTexture(loadedFrame, width, height, minX, maxY);
                string outputPath = $"{actionFolder}/frame_{i:000}.png";
                File.WriteAllBytes(outputPath, rebuiltFrame.EncodeToPNG());
                DestroyImmediate(rebuiltFrame);
                spriteAssetPaths.Add(outputPath);
            }

            foreach (LoadedFrame loadedFrame in loadedFrames)
            {
                DestroyImmediate(loadedFrame.Texture);
            }

            AssetDatabase.Refresh();
            ApplySpriteImportSettings(spriteAssetPaths);

            if (createAnimationClips)
            {
                CreateAnimationClip(actionId, spriteAssetPaths);
            }

            resultBuilder.AppendLine($"动作 {actionId}: 输出 {loadedFrames.Count} 帧, 画布 {width}x{height}, {actionFolder}");
        }

        private void CalculateOutputBounds(
            Dictionary<int, List<LoadedFrame>> loadedActions,
            out int minX,
            out int minY,
            out int maxX,
            out int maxY)
        {
            minX = int.MaxValue;
            minY = int.MaxValue;
            maxX = int.MinValue;
            maxY = int.MinValue;

            foreach (KeyValuePair<int, List<LoadedFrame>> action in loadedActions)
            {
                foreach (LoadedFrame frame in action.Value)
                {
                    minX = Mathf.Min(minX, frame.Frame.X);
                    minY = Mathf.Min(minY, frame.Frame.Y);
                    maxX = Mathf.Max(maxX, frame.Frame.X + frame.Texture.width);
                    maxY = Mathf.Max(maxY, frame.Frame.Y + frame.Texture.height);
                }
            }

            if (minX == int.MaxValue)
            {
                minX = 0;
                minY = 0;
                maxX = 1;
                maxY = 1;
            }
        }

        private Dictionary<int, List<MovieFrame>> ParseConfig(string text)
        {
            Dictionary<int, List<MovieFrame>> actions = new Dictionary<int, List<MovieFrame>>();
            string[] actionParts = text.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string actionPart in actionParts)
            {
                int colonIndex = actionPart.IndexOf(':');
                if (colonIndex <= 0)
                {
                    continue;
                }

                if (!int.TryParse(actionPart.Substring(0, colonIndex), NumberStyles.Integer, CultureInfo.InvariantCulture, out int actionId))
                {
                    continue;
                }

                List<MovieFrame> frames = new List<MovieFrame>();
                string frameText = actionPart.Substring(colonIndex + 1);
                string[] frameParts = frameText.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string framePart in frameParts)
                {
                    string[] values = framePart.Split(',');
                    if (values.Length < 3)
                    {
                        continue;
                    }

                    if (!int.TryParse(values[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int imageId)
                        || !int.TryParse(values[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x)
                        || !int.TryParse(values[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y))
                    {
                        continue;
                    }

                    string marker = values.Length >= 4 ? values[3] : string.Empty;
                    frames.Add(new MovieFrame(imageId, x, y, marker));
                }

                actions[actionId] = frames;
            }

            return actions;
        }

        private Dictionary<ImageKey, string> IndexImages(string folderPath, string prefix)
        {
            Dictionary<ImageKey, string> imagePaths = new Dictionary<ImageKey, string>();
            string escapedPrefix = Regex.Escape(prefix);
            Regex regex = new Regex($@"{escapedPrefix}_(\d+)_(\d+)\.png$", RegexOptions.IgnoreCase);

            foreach (string filePath in Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly))
            {
                string fileName = Path.GetFileName(filePath);
                Match match = regex.Match(fileName);
                if (!match.Success)
                {
                    continue;
                }

                int actionId = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                int imageId = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                imagePaths[new ImageKey(actionId, imageId)] = filePath;
            }

            return imagePaths;
        }

        private void CalculateBounds(List<LoadedFrame> frames, out int minX, out int minY, out int maxX, out int maxY)
        {
            minX = int.MaxValue;
            minY = int.MaxValue;
            maxX = int.MinValue;
            maxY = int.MinValue;

            foreach (LoadedFrame frame in frames)
            {
                minX = Mathf.Min(minX, frame.Frame.X);
                minY = Mathf.Min(minY, frame.Frame.Y);
                maxX = Mathf.Max(maxX, frame.Frame.X + frame.Texture.width);
                maxY = Mathf.Max(maxY, frame.Frame.Y + frame.Texture.height);
            }
        }

        private Texture2D CreateFrameTexture(LoadedFrame frame, int width, int height, int minX, int maxY)
        {
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32[] clearPixels = new Color32[width * height];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = new Color32(0, 0, 0, 0);
            }

            result.SetPixels32(clearPixels);

            int xOffset = frame.Frame.X - minX;
            int yOffset = maxY - (frame.Frame.Y + frame.Texture.height);
            result.SetPixels32(xOffset, yOffset, frame.Texture.width, frame.Texture.height, frame.Texture.GetPixels32());
            result.Apply();
            return result;
        }

        private void ApplySpriteImportSettings(List<string> spriteAssetPaths)
        {
            foreach (string rawPath in spriteAssetPaths)
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

        private void ApplySpriteImportSettingsToFolder(string folder)
        {
            List<string> pngPaths = new List<string>();
            foreach (string path in Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories))
            {
                pngPaths.Add(path.Replace("\\", "/"));
            }

            ApplySpriteImportSettings(pngPaths);
        }

        private void CreateAnimationClip(int actionId, List<string> spriteAssetPaths)
        {
            List<Sprite> sprites = new List<Sprite>();
            foreach (string path in spriteAssetPaths)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path.Replace("\\", "/"));
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            if (sprites.Count == 0)
            {
                return;
            }

            AnimationClip clip = new AnimationClip
            {
                frameRate = Mathf.Max(1f, frameRate)
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / clip.frameRate,
                    value = sprites[i]
                };
            }

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            string clipPath = $"{outputFolder}/action_{actionId}.anim";
            if (File.Exists(clipPath))
            {
                AssetDatabase.DeleteAsset(clipPath);
            }

            AssetDatabase.CreateAsset(clip, clipPath);
        }

        private readonly struct MovieFrame
        {
            public MovieFrame(int imageId, int x, int y, string marker)
            {
                ImageId = imageId;
                X = x;
                Y = y;
                Marker = marker;
            }

            public int ImageId { get; }
            public int X { get; }
            public int Y { get; }
            public string Marker { get; }
        }

        private readonly struct LoadedFrame
        {
            public LoadedFrame(MovieFrame frame, Texture2D texture)
            {
                Frame = frame;
                Texture = texture;
            }

            public MovieFrame Frame { get; }
            public Texture2D Texture { get; }
        }

        private readonly struct ImageKey : IEquatable<ImageKey>
        {
            public ImageKey(int actionId, int imageId)
            {
                ActionId = actionId;
                ImageId = imageId;
            }

            private int ActionId { get; }
            private int ImageId { get; }

            public bool Equals(ImageKey other)
            {
                return ActionId == other.ActionId && ImageId == other.ImageId;
            }

            public override bool Equals(object obj)
            {
                return obj is ImageKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (ActionId * 397) ^ ImageId;
                }
            }
        }
    }
}
