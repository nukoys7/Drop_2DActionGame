using UnityEngine;

namespace Drop
{
    // プレハブには番号で保存されるので、値を足すときは必ず末尾に追加する
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

    // レア度ごとのテーマ色。UI でも同じ色を使う
    public static class ItemRarityColors
    {
        public static Color Get(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon: return new Color(0.30f, 0.80f, 0.35f);
                case ItemRarity.Rare: return new Color(0.25f, 0.55f, 1.00f);
                case ItemRarity.Epic: return new Color(0.84f, 0.24f, 0.64f);
                case ItemRarity.Legendary: return new Color(1.00f, 0.76f, 0.04f);
                default: return new Color(0.78f, 0.78f, 0.80f);
            }
        }
    }
}
