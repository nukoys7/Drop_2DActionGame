using UnityEngine;

namespace Drop
{
    // 敵が倒れるたびに HP を回復する。レベルで回復量が増える
    public class VampireFangEffect : EnemyDeathEffect
    {
        [SerializeField] private float healPerLevel = 0.5f;

        protected override void OnEnemyDied(Stats enemy) => Stats.Heal(healPerLevel * Level);
    }
}
