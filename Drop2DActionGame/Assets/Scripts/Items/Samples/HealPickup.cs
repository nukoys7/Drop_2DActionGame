using UnityEngine;

namespace Drop
{
    // サンプル: 触れると HP を回復して消える
    public class HealPickup : MonoBehaviour
    {
        [SerializeField] private float amount = 2f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!PlayerController.TryGetFromCollider(other, out var player)) return;
            player.Stats.Heal(amount);
            Destroy(gameObject);
        }
    }
}
