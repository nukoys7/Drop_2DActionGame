using UnityEngine;

namespace Drop
{
    // サンプル: 触れるとジャンプ力を上昇させる
    public class JumpUpPickup : MonoBehaviour
    {
        [SerializeField] private StatType stat = StatType.JumpPower;
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
