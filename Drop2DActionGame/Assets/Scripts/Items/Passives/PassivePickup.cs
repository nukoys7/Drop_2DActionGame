using UnityEngine;

namespace Drop
{
    // 触れると、子の PassiveEffect をプレイヤーに付け替えて有効にする
    // 同じ効果をすでに持っていれば、そのレベルを上げる
    public class PassivePickup : MonoBehaviour
    {
        [SerializeField] private PassiveEffect effect;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (effect == null || !PlayerController.TryGetFromCollider(other, out var player)) return;

            var owned = FindOwned(player);
            if (owned != null)
            {
                owned.LevelUp();
            }
            else
            {
                effect.transform.SetParent(player.transform, false);
                effect.transform.localPosition = Vector3.zero;
                effect.Attach(player);
                effect.gameObject.SetActive(true);
            }
            // 同じフレームに2回呼ばれても二重に付けない
            effect = null;
            Destroy(gameObject);
        }

        private PassiveEffect FindOwned(PlayerController player)
        {
            foreach (var e in player.GetComponentsInChildren<PassiveEffect>())
                if (e.GetType() == effect.GetType()) return e;
            return null;
        }
    }
}
