using System;
using UnityEngine;

namespace Drop
{
    // 触れると、ステータス補正・回復・無敵をまとめて適用して消える
    // 補正だけ、回復だけのアイテムもこれ1つで作れる
    public class StatBoostPickup : MonoBehaviour
    {
        [Serializable]
        private struct Boost
        {
            public StatType stat;
            public ModifierKind kind;
            public float value;
            [Tooltip("秒数。0 なら永続")]
            public float duration;
        }

        [SerializeField] private Boost[] boosts = Array.Empty<Boost>();
        [SerializeField, Min(0f)] private float heal;
        [SerializeField, Min(0f)] private float invincibleSeconds;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!PlayerController.TryGetFromCollider(other, out var player)) return;
            var stats = player.Stats;
            foreach (var b in boosts) stats.AddModifier(b.stat, b.kind, b.value, b.duration);
            // 最大 HP を上げた分も回復できるよう、補正の後に回復する
            if (heal > 0f) stats.Heal(heal);
            if (invincibleSeconds > 0f) stats.SetInvincible(invincibleSeconds);
            Destroy(gameObject);
        }
    }
}
