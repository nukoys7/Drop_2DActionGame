using UnityEngine;

namespace Drop
{
    // 一定時間ごとに HP を回復する。レベルで間隔が短くなる(間隔 ÷ レベル)
    public class RegenEffect : PassiveEffect
    {
        [SerializeField] private float amount = 1f;
        [SerializeField] private float interval = 8f;

        private float timer;

        private void Update()
        {
            if (Stats.IsDead) return;
            timer += Time.deltaTime;
            if (timer < interval / Level) return;
            timer = 0f;
            if (Stats.HP < Stats.MaxHP) Stats.Heal(amount);
        }
    }
}
