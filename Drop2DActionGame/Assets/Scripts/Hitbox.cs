using System;
using UnityEngine;

namespace Drop
{
    // トリガー内の相手(非トリガー)の Stats にダメージを与える。攻撃力と陣営は親の Stats を使う
    [RequireComponent(typeof(Collider2D))]
    public class Hitbox : MonoBehaviour
    {
        [SerializeField] private float knockbackHorizontal = 7f;
        [SerializeField] private float knockbackUp = 7f;

        private Stats owner;

        // ダメージが通ったとき
        public event Action<Stats> OnHit;

        public bool Active { get; set; } = true;

        private void Awake() => owner = GetComponentInParent<Stats>();

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!Active || owner.IsDead || other.isTrigger) return;
            var target = other.GetComponentInParent<Stats>();
            if (target == null || target == owner) return;

            float dir = Mathf.Sign(other.bounds.center.x - transform.position.x);
            var knockback = new Vector2(dir * knockbackHorizontal, knockbackUp);
            if (target.TakeDamage(owner.Attack, knockback, owner.Team)) OnHit?.Invoke(target);
        }
    }
}
