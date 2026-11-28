using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drop
{
    // 敵が倒れたときに働く効果の土台。シーン内の敵を定期的に探して OnDied を購読する
    // 探す間隔より短い間に出現して倒れた敵は数えない
    public abstract class EnemyDeathEffect : PassiveEffect
    {
        private const float ScanInterval = 0.5f;

        private readonly Dictionary<Stats, Action> watched = new Dictionary<Stats, Action>();
        private readonly List<Stats> removed = new List<Stats>();
        private float scanTimer;

        protected abstract void OnEnemyDied(Stats enemy);

        private void OnDisable()
        {
            foreach (var pair in watched)
                if (pair.Key != null) pair.Key.OnDied -= pair.Value;
            watched.Clear();
            scanTimer = 0f;
        }

        private void Update()
        {
            scanTimer -= Time.deltaTime;
            if (scanTimer > 0f) return;
            scanTimer = ScanInterval;

            foreach (var s in FindObjectsByType<Stats>())
            {
                if (s.Team != Team.Enemy || s.IsDead || watched.ContainsKey(s)) continue;
                var enemy = s;
                Action handler = () => OnEnemyDied(enemy);
                enemy.OnDied += handler;
                watched.Add(enemy, handler);
            }

            // 消えた敵を外す
            removed.Clear();
            foreach (var key in watched.Keys)
                if (key == null || key.IsDead) removed.Add(key);
            foreach (var key in removed)
            {
                if (key != null) key.OnDied -= watched[key];
                watched.Remove(key);
            }
        }
    }
}
