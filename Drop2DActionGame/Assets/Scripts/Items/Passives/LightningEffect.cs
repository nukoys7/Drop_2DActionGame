using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drop
{
    // 一定間隔で、範囲内の近い敵に雷を落とす。レベルで同時に狙う数が増える
    // ダメージはプレイヤーの攻撃力 × damageRate
    public class LightningEffect : PassiveEffect
    {
        [SerializeField] private float interval = 3f;
        [SerializeField] private float range = 6f;
        [SerializeField] private float damageRate = 1f;
        [SerializeField] private Vector2 knockback = new Vector2(0f, 4f);
        [SerializeField] private Material boltMaterial;
        [SerializeField] private Color boltColor = new Color(1f, 0.92f, 0.35f);
        [SerializeField] private float boltTime = 0.12f;

        private readonly List<Collider2D> hits = new List<Collider2D>();
        private readonly List<Stats> targets = new List<Stats>();
        private ContactFilter2D filter;
        private float timer;

        private void Awake()
        {
            filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        }

        private void Update()
        {
            if (Stats.IsDead) return;
            timer += Time.deltaTime;
            if (timer < interval) return;
            // 敵がいなければ、見つかった瞬間に撃てるよう待ったままにする
            if (Strike()) timer = 0f;
        }

        private bool Strike()
        {
            Vector2 center = Player.transform.position;
            Physics2D.OverlapCircle(center, range, filter, hits);
            targets.Clear();
            foreach (var c in hits)
            {
                var s = c.GetComponentInParent<Stats>();
                if (s != null && s.Team == Team.Enemy && !s.IsDead && !targets.Contains(s)) targets.Add(s);
            }
            if (targets.Count == 0) return false;

            targets.Sort((a, b) => ((Vector2)a.transform.position - center).sqrMagnitude
                .CompareTo(((Vector2)b.transform.position - center).sqrMagnitude));
            for (int i = 0; i < targets.Count && i < Level; i++)
            {
                var t = targets[i];
                t.TakeDamage(Stats.Attack * damageRate, knockback, Team.Player);
                StartCoroutine(ShowBolt(t.transform.position));
            }
            return true;
        }

        // 空から敵へ、ジグザグの線を一瞬だけ表示する
        private IEnumerator ShowBolt(Vector3 target)
        {
            var go = new GameObject("Bolt");
            var line = go.AddComponent<LineRenderer>();
            line.material = boltMaterial;
            line.startColor = line.endColor = boltColor;
            line.startWidth = line.endWidth = 0.08f;
            line.sortingOrder = 50;
            const int points = 6;
            line.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float k = i / (points - 1f);
                float jitter = (i == 0 || i == points - 1) ? 0f : Random.Range(-0.25f, 0.25f);
                line.SetPosition(i, target + new Vector3(jitter, (1f - k) * 5f, 0f));
            }
            yield return new WaitForSeconds(boltTime);
            Destroy(go);
        }
    }
}
