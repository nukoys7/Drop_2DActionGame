using UnityEngine;

namespace Drop
{
    // 落下判定。プレイヤーは GameManager に通知、それ以外は削除
    public class KillZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.isTrigger) return;
            var root = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            if (root.TryGetComponent<PlayerController>(out var player)) GameManager.Instance.HandlePlayerFell(player);
            else Destroy(root);
        }
    }
}
