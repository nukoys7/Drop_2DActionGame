using UnityEngine;

namespace Drop
{
    // 敵が倒れるたびに攻撃力がずっと上がる。上限なし。レベルで1回の上がり幅が増える
    public class GoldenChikuwaEffect : EnemyDeathEffect
    {
        [SerializeField] private float attackPerKill = 0.1f;

        protected override void OnEnemyDied(Stats enemy) =>
            Stats.AddModifier(StatType.Attack, ModifierKind.Add, attackPerKill * Level);
    }
}
