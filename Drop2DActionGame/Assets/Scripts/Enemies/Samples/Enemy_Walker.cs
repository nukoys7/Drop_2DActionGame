using UnityEngine;

namespace Drop
{
    // サンプル: 左に歩き続け、HP 0 で消える
    [RequireComponent(typeof(Rigidbody2D), typeof(Stats))]
    public class Enemy_Walker : MonoBehaviour
    {
        private Rigidbody2D rb;
        private Stats stats;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<Stats>();
            stats.OnDied += () => Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            if (stats.IsStunned) return;
            rb.linearVelocity = new Vector2(-stats.MoveSpeed, rb.linearVelocity.y);
        }
    }
}
