using System.Collections.Generic;
using UnityEngine;

namespace Drop
{
    // プレイヤーの周りを回るやかんが、触れた敵にダメージを与える。レベルでやかんの数が増える
    // ダメージはプレイヤーの攻撃力 × damageRate。同じ敵には hitCooldown 秒に1回だけ当たる
    public class OrbitKettleEffect : PassiveEffect
    {
        [SerializeField] private Sprite kettleSprite;
        [SerializeField] private float kettleScale = 0.8f;
        [SerializeField] private float radius = 1.4f;
        [SerializeField] private float degreesPerSecond = 180f;
        [SerializeField] private float hitRadius = 0.3f;
        [SerializeField] private float damageRate = 0.5f;
        [SerializeField] private float hitCooldown = 0.5f;
        [SerializeField] private float knockback = 5f;

        private readonly List<Transform> kettles = new List<Transform>();
        private readonly Dictionary<Stats, float> nextHitTime = new Dictionary<Stats, float>();
        private readonly List<Collider2D> hits = new List<Collider2D>();
        private ContactFilter2D filter;
        private float angle;

        private void Awake()
        {
            filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        }

        private void OnEnable() => SpawnKettles();

        protected override void OnLevelUp() => SpawnKettles();

        private void SpawnKettles()
        {
            while (kettles.Count < Level)
            {
                var go = new GameObject("Kettle");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = kettleSprite;
                sr.sortingOrder = 5;
                kettles.Add(go.transform);
            }
        }

        private void Update()
        {
            angle = Mathf.Repeat(angle + degreesPerSecond * Time.deltaTime, 360f);
            Vector3 center = Player.transform.position;
            // プレイヤーの向きの反転や拡大に影響されない大きさにする
            var parentScale = transform.lossyScale;
            var scale = new Vector3(kettleScale / Mathf.Abs(parentScale.x), kettleScale / Mathf.Abs(parentScale.y), 1f);

            for (int i = 0; i < kettles.Count; i++)
            {
                float a = (angle + 360f * i / kettles.Count) * Mathf.Deg2Rad;
                var pos = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
                kettles[i].position = pos;
                kettles[i].localScale = scale;
                if (!Stats.IsDead) HitAround(pos);
            }
        }

        private void HitAround(Vector2 pos)
        {
            Physics2D.OverlapCircle(pos, hitRadius, filter, hits);
            foreach (var c in hits)
            {
                var s = c.GetComponentInParent<Stats>();
                if (s == null || s.Team != Team.Enemy || s.IsDead) continue;
                if (nextHitTime.TryGetValue(s, out float t) && Time.time < t) continue;
                float dir = Mathf.Sign(s.transform.position.x - pos.x);
                if (s.TakeDamage(Stats.Attack * damageRate, new Vector2(dir * knockback, knockback), Team.Player))
                    nextHitTime[s] = Time.time + hitCooldown;
            }
        }
    }
}
