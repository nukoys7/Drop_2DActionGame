using UnityEngine;

namespace Drop
{
    // 被弾したあとの無敵時間をのばす。レベルでさらにのびる
    public class CharmEffect : PassiveEffect
    {
        [SerializeField] private float invincibleSeconds = 2f;
        [SerializeField] private float secondsPerLevel = 0.5f;

        private float lastHP;

        private void OnEnable()
        {
            lastHP = Stats.HP;
            Stats.OnHPChanged += HandleHPChanged;
        }

        private void OnDisable() => Stats.OnHPChanged -= HandleHPChanged;

        private void HandleHPChanged(float hp, float max)
        {
            // 敵の攻撃で減ったときだけ。TakeDamage は HP を減らす前に無敵を付ける
            if (hp < lastHP && hp > 0f && Stats.IsInvincible)
                Stats.SetInvincible(invincibleSeconds + secondsPerLevel * (Level - 1));
            lastHP = hp;
        }
    }
}
