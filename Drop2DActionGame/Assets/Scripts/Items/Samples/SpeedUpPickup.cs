using UnityEngine;

namespace Drop
{
    // サンプル: 触れると duration 秒間ステータスを補正する(0 なら永続)
    public class SpeedUpPickup : MonoBehaviour
    {
        [SerializeField] private StatType stat = StatType.MoveSpeed;
        [SerializeField] private ModifierKind kind = ModifierKind.Multiply;
        [SerializeField] private float value = 1.5f;
        [SerializeField] private float duration = 5f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!PlayerController.TryGetFromCollider(other, out var player)) return;
            player.Stats.AddModifier(stat, kind, value, duration);
            Destroy(gameObject);
        }
    }
}
