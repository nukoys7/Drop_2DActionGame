using System.Collections.Generic;
using UnityEngine;

namespace Drop
{
    // 近くのアイテムをプレイヤーへ引き寄せる。レベルで範囲が広がる
    public class MagnetEffect : PassiveEffect
    {
        [SerializeField] private float radius = 3f;
        [SerializeField] private float radiusPerLevel = 1f;
        [SerializeField] private float pullSpeed = 8f;

        private readonly List<Collider2D> hits = new List<Collider2D>();
        private ContactFilter2D filter;

        private void Awake()
        {
            filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(LayerMask.GetMask("Item"));
        }

        private void FixedUpdate()
        {
            Vector2 center = Player.transform.position;
            float r = radius + radiusPerLevel * (Level - 1);
            Physics2D.OverlapCircle(center, r, filter, hits);
            foreach (var c in hits)
            {
                var body = c.attachedRigidbody;
                if (body != null && body.bodyType == RigidbodyType2D.Dynamic)
                    body.linearVelocity = (center - body.position).normalized * pullSpeed;
                else
                    c.transform.position = Vector2.MoveTowards(c.transform.position, center, pullSpeed * Time.fixedDeltaTime);
            }
        }
    }
}
