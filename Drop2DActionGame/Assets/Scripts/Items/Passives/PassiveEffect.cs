using UnityEngine;

namespace Drop
{
    // プレイヤーに付いて効き続ける効果。PassivePickup の子に非アクティブで置いておく
    // 同じ効果のアイテムをもう一度拾うと Level が上がる
    public abstract class PassiveEffect : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxLevel = 5;

        public int Level { get; private set; } = 1;
        protected PlayerController Player { get; private set; }
        protected Stats Stats { get; private set; }

        // 有効にする前に PassivePickup が呼ぶ
        public void Attach(PlayerController player)
        {
            Player = player;
            Stats = player.Stats;
        }

        public void LevelUp()
        {
            if (Level >= maxLevel) return;
            Level++;
            OnLevelUp();
        }

        protected virtual void OnLevelUp() { }
    }
}
