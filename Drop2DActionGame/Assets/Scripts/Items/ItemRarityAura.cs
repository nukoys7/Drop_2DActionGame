using System.Collections.Generic;
using UnityEngine;

namespace Drop
{
    // アイテムの輪郭からレア度の色のオーラを出す
    // 輪郭に沿って光る層と、そこから外へ広がって消える波の層を本体の後ろに重ねる
    [DisallowMultipleComponent]
    public class ItemRarityAura : MonoBehaviour
    {
        private const int MaxGlowPixels = 32;

        // 元スプライトと光の幅(px)ごとに、輪郭の光のスプライトを共有する
        private static readonly Dictionary<(Sprite, int), Sprite> glowCache = new Dictionary<(Sprite, int), Sprite>();

        [SerializeField] private ItemRarity rarity = ItemRarity.Common;
        // 未設定なら子の最初の SpriteRenderer
        [SerializeField] private SpriteRenderer target;
        // 未設定なら target と同じマテリアル。暗い場所でも光らせるなら Sprite-Unlit-Default を使う
        [SerializeField] private Material auraMaterial;
        [SerializeField] private float glowWidth = 0.15f;
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float waveInterval = 1.2f;
        [SerializeField] private float waveScale = 0.35f;

        private SpriteRenderer glow;
        private SpriteRenderer wave;
        private float waveTimer;

        public ItemRarity Rarity => rarity;

        private void Awake() => Build();

        private void Update()
        {
            if (glow == null) return;
            waveTimer = (waveTimer + Time.deltaTime) % waveInterval;
            Animate(Time.time, waveTimer / waveInterval);
        }

        // オーラの層を作る。作成済みなら何もしない
        public void Build()
        {
            if (glow != null) return;
            if (target == null) target = GetComponentInChildren<SpriteRenderer>();
            if (target == null || target.sprite == null) return;

            int glowPixels = Mathf.Clamp(Mathf.CeilToInt(glowWidth * target.sprite.pixelsPerUnit), 1, MaxGlowPixels);
            var sprite = GetGlowSprite(target.sprite, glowPixels);
            wave = CreateLayer("AuraWave", sprite, target.sortingOrder - 2);
            glow = CreateLayer("Aura", sprite, target.sortingOrder - 1);
            Animate(0f, 0.3f);
        }

        public void SetRarity(ItemRarity value)
        {
            rarity = value;
            Animate(Time.time, waveTimer / waveInterval);
        }

        // waveProgress は波が広がり始めてからの割合(0〜1)
        private void Animate(float time, float waveProgress)
        {
            if (glow == null) return;
            var color = ItemRarityColors.Get(rarity);

            float pulse = 0.5f + 0.5f * Mathf.Sin(time * pulseSpeed);
            color.a = Mathf.Lerp(0.75f, 1f, pulse);
            glow.color = color;

            wave.transform.localScale = Vector3.one * (1f + waveScale * waveProgress);
            color.a = 0.6f * (1f - waveProgress);
            wave.color = color;
        }

        private SpriteRenderer CreateLayer(string layerName, Sprite sprite, int sortingOrder)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(target.transform, false);
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = sprite;
            r.sharedMaterial = auraMaterial != null ? auraMaterial : target.sharedMaterial;
            r.sortingLayerID = target.sortingLayerID;
            r.sortingOrder = sortingOrder;
            return r;
        }

        private static Sprite GetGlowSprite(Sprite source, int glowPixels)
        {
            var key = (source, glowPixels);
            if (!glowCache.TryGetValue(key, out var sprite) || sprite == null)
            {
                sprite = CreateGlowSprite(source, glowPixels);
                glowCache[key] = sprite;
            }
            return sprite;
        }

        // 元スプライトの不透明な部分から glowPixels 以内を、離れるほど薄くなる白で塗ったスプライトを作る
        private static Sprite CreateGlowSprite(Sprite source, int glowPixels)
        {
            var rect = source.textureRect;
            int w = Mathf.RoundToInt(rect.width), h = Mathf.RoundToInt(rect.height);
            var src = ReadPixels(source.texture, rect);

            int r = glowPixels;
            int gw = w + r * 2, gh = h + r * 2;
            var pixels = new Color32[gw * gh];
            for (int gy = 0; gy < gh; gy++)
            {
                for (int gx = 0; gx < gw; gx++)
                {
                    float best = 0f;
                    for (int dy = -r; dy <= r; dy++)
                    {
                        int sy = gy - r + dy;
                        if (sy < 0 || sy >= h) continue;
                        for (int dx = -r; dx <= r; dx++)
                        {
                            int sx = gx - r + dx;
                            if (sx < 0 || sx >= w) continue;
                            byte a = src[sy * w + sx].a;
                            if (a < 26) continue;
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            if (d > r) continue;
                            float v = 1f - d / (r + 1f);
                            best = Mathf.Max(best, v * v * a / 255f);
                        }
                    }
                    pixels[gy * gw + gx] = new Color32(255, 255, 255, (byte)(best * 255f));
                }
            }

            var tex = new Texture2D(gw, gh, TextureFormat.RGBA32, false)
            {
                name = source.name + "_Aura",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = source.texture.filterMode
            };
            tex.SetPixels32(pixels);
            tex.Apply();
            var pivot = new Vector2((source.pivot.x + r) / gw, (source.pivot.y + r) / gh);
            return Sprite.Create(tex, new Rect(0, 0, gw, gh), pivot, source.pixelsPerUnit);
        }

        // Read/Write が無効なテクスチャでも読めるように、GPU 経由でコピーする
        private static Color32[] ReadPixels(Texture2D texture, Rect rect)
        {
            int x = Mathf.RoundToInt(rect.x), y = Mathf.RoundToInt(rect.y);
            int w = Mathf.RoundToInt(rect.width), h = Mathf.RoundToInt(rect.height);
            if (texture.isReadable) return ToColor32(texture.GetPixels(x, y, w, h));

            var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, rt);
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var copy = new Texture2D(w, h, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(x, y, w, h), 0, 0);
            copy.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            var result = copy.GetPixels32();
            if (Application.isPlaying) Destroy(copy);
            else DestroyImmediate(copy);
            return result;
        }

        private static Color32[] ToColor32(Color[] colors)
        {
            var result = new Color32[colors.Length];
            for (int i = 0; i < colors.Length; i++) result[i] = colors[i];
            return result;
        }

        // エディタ上でもレア度がわかるように、選択中は色付きの枠を表示する
        private void OnDrawGizmosSelected()
        {
            var r = target != null ? target : GetComponentInChildren<SpriteRenderer>();
            if (r == null) return;
            Gizmos.color = ItemRarityColors.Get(rarity);
            Gizmos.DrawWireCube(r.bounds.center, r.bounds.size + Vector3.one * glowWidth * 2f);
        }
    }
}
