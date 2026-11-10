using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drop
{
    public enum Team { Player, Enemy }

    public enum StatType { MaxHP, Attack, Defense, MoveSpeed, JumpPower, DropKickSpeed }

    // Add: 基礎値に加算 / Multiply: 加算の後に乗算
    public enum ModifierKind { Add, Multiply }

    // 最終値 = (基礎値 + 加算補正の合計) × 乗算補正の積
    // ダメージ = max(1, 攻撃力 - 防御力)。同じ Team からは受けない
    // 被弾すると Rigidbody2D をノックバックさせ、白く点滅し、hurtStunTime の間 IsStunned になる
    public class Stats : MonoBehaviour
    {
        [SerializeField] private Team team = Team.Enemy;
        [SerializeField] private float maxHP = 3f;
        [SerializeField] private float attack = 1f;
        [SerializeField] private float defense;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float jumpPower = 10f;
        [SerializeField] private float dropKickSpeed;
        [SerializeField] private float invincibleAfterHit = 0.2f;
        [SerializeField] private float hurtStunTime = 0.25f;

        private class Modifier
        {
            public StatType Stat;
            public ModifierKind Kind;
            public float Value;
            public float Remaining;
        }

        private readonly List<Modifier> modifiers = new List<Modifier>();
        private Rigidbody2D rb;
        private SpriteRenderer[] renderers;
        private Color[] baseColors;
        private float hp;
        private float invincibleTimer;
        private float stunTimer;

        public event Action OnDied;
        // (現在 HP, 最大 HP)
        public event Action<float, float> OnHPChanged;

        public Team Team => team;
        public float HP => Mathf.Min(hp, MaxHP);
        public bool IsDead { get; private set; }
        // true の間は無敵
        public bool KeepInvincible { get; set; }
        public bool IsInvincible => KeepInvincible || invincibleTimer > 0f;
        public bool IsStunned => stunTimer > 0f;

        public float MaxHP => Get(StatType.MaxHP, maxHP);
        public float Attack => Get(StatType.Attack, attack);
        public float Defense => Get(StatType.Defense, defense);
        public float MoveSpeed => Get(StatType.MoveSpeed, moveSpeed);
        public float JumpPower => Get(StatType.JumpPower, jumpPower);
        public float DropKickSpeed => Get(StatType.DropKickSpeed, dropKickSpeed);

        private void Awake()
        {
            hp = maxHP;
            rb = GetComponent<Rigidbody2D>();
            renderers = GetComponentsInChildren<SpriteRenderer>();
            baseColors = Array.ConvertAll(renderers, r => r.color);
        }

        private void Start() => OnHPChanged?.Invoke(HP, MaxHP);

        private void Update()
        {
            if (invincibleTimer > 0f) invincibleTimer -= Time.deltaTime;
            if (stunTimer > 0f) stunTimer -= Time.deltaTime;

            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                var m = modifiers[i];
                m.Remaining -= Time.deltaTime;
                if (m.Remaining > 0f) continue;
                modifiers.RemoveAt(i);
                if (m.Stat == StatType.MaxHP) OnHPChanged?.Invoke(HP, MaxHP);
            }
        }

        // duration が 0 以下なら永続
        public void AddModifier(StatType stat, ModifierKind kind, float value, float duration = 0f)
        {
            float remaining = duration > 0f ? duration : float.PositiveInfinity;
            modifiers.Add(new Modifier { Stat = stat, Kind = kind, Value = value, Remaining = remaining });
            if (stat == StatType.MaxHP) OnHPChanged?.Invoke(HP, MaxHP);
        }

        // 無効(無敵・同陣営・死亡済み)なら false
        public bool TakeDamage(float attackPower, Vector2 knockback, Team sourceTeam)
        {
            if (IsDead || IsInvincible || sourceTeam == team) return false;
            invincibleTimer = invincibleAfterHit;
            stunTimer = hurtStunTime;
            if (rb != null) rb.linearVelocity = knockback;
            StartCoroutine(Flash());
            ReduceHP(Mathf.Max(1f, attackPower - Defense));
            return true;
        }

        private IEnumerator Flash()
        {
            foreach (var r in renderers) r.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            for (int i = 0; i < renderers.Length; i++) renderers[i].color = baseColors[i];
        }

        // 防御力・無敵を無視する
        public void TakeDirectDamage(float amount) => ReduceHP(amount);

        public void Heal(float amount)
        {
            if (IsDead) return;
            hp = Mathf.Min(MaxHP, HP + amount);
            OnHPChanged?.Invoke(HP, MaxHP);
        }

        // 残り時間の長い方を優先
        public void SetInvincible(float seconds) => invincibleTimer = Mathf.Max(invincibleTimer, seconds);

        private void ReduceHP(float amount)
        {
            if (IsDead) return;
            hp = Mathf.Max(0f, HP - amount);
            OnHPChanged?.Invoke(HP, MaxHP);
            if (hp > 0f) return;
            IsDead = true;
            OnDied?.Invoke();
        }

        private float Get(StatType stat, float baseValue)
        {
            float add = 0f, mul = 1f;
            foreach (var m in modifiers)
            {
                if (m.Stat != stat) continue;
                if (m.Kind == ModifierKind.Add) add += m.Value;
                else mul *= m.Value;
            }
            return Mathf.Max(0f, (baseValue + add) * mul);
        }
    }
}
