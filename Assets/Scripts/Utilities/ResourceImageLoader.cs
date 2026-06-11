using UnityEngine;

namespace EnjoyJob
{
    // 统一加载 Resources 里的图片。
    // 找不到目标图片时返回程序生成的默认图片，避免 UI 出现空白。
    public static class ResourceImageLoader
    {
        private static Sprite defaultSprite;
        private static Texture2D defaultTexture;

        public static Sprite LoadSpriteOrDefault(string resourcePath)
        {
            if (!string.IsNullOrWhiteSpace(resourcePath))
            {
                Sprite sprite = Resources.Load<Sprite>(resourcePath);
                if (sprite != null)
                {
                    return sprite;
                }

                Debug.LogWarning($"Sprite not found in Resources: {resourcePath}. Use default image instead.");
            }

            return GetDefaultSprite();
        }

        public static Texture2D GetDefaultTexture()
        {
            if (defaultTexture != null)
            {
                return defaultTexture;
            }

            const int size = 64;
            defaultTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "DefaultMissingImage",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color background = new Color(0.22f, 0.22f, 0.22f, 1f);
            Color line = new Color(0.85f, 0.85f, 0.85f, 1f);
            Color border = new Color(0.05f, 0.05f, 0.05f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = x < 2 || y < 2 || x >= size - 2 || y >= size - 2;
                    bool isDiagonal = Mathf.Abs(x - y) <= 1 || Mathf.Abs(x + y - (size - 1)) <= 1;
                    defaultTexture.SetPixel(x, y, isBorder ? border : isDiagonal ? line : background);
                }
            }

            defaultTexture.Apply();
            return defaultTexture;
        }

        private static Sprite GetDefaultSprite()
        {
            if (defaultSprite != null)
            {
                return defaultSprite;
            }

            Texture2D texture = GetDefaultTexture();
            defaultSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            defaultSprite.name = "DefaultMissingImageSprite";
            defaultSprite.hideFlags = HideFlags.HideAndDontSave;
            return defaultSprite;
        }
    }
}
