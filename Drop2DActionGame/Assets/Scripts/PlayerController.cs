using UnityEngine;
using UnityEngine.InputSystem;

namespace Drop
{
    // 移動: A/D・←/→ / ジャンプ: 地上で Space(離すと低く跳ぶ)
    // ドロップキック: 空中で Space。向いている方向の斜め下へ突進、無敵。空中 1 回、着地か命中で回復
    [RequireComponent(typeof(Rigidbody2D), typeof(Stats))]
    public class PlayerController : MonoBehaviour
    {
        private enum State { Normal, DropKick, Dead }

        // シーン内のプレイヤー
        public static PlayerController Current { get; private set; }

        [SerializeField] private float groundAcceleration = 90f;
        [SerializeField] private float airAcceleration = 50f;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float coyoteTime = 0.08f;

        [SerializeField] private Hitbox dropKickHitbox;
        [SerializeField] private float dropKickAngle = 35f;
        [SerializeField] private float dropKickMaxTime = 0.8f;
        [SerializeField] private Vector2 dropKickBounce = new Vector2(4f, 11f);
        [SerializeField] private float invincibleAfterKickHit = 0.3f;

        private Rigidbody2D rb;
        private Collider2D body;
        private Stats stats;
        private SpriteRenderer[] renderers;
        private InputAction move;
        private InputAction jump;
        private int groundMask;

        private State state = State.Normal;
        private float moveX;
        private bool jumpRequested;
        private bool kickRequested;
        private bool grounded;
        private bool wasGrounded;
        private float coyoteTimer;
        private float groundIgnoreTimer;
        private bool risingFromJump;
        private bool airKickAvailable = true;
        private float stateTimer;
        private float defaultGravity;
        private float baseScaleX;
        private int facing = 1;

        public Stats Stats => stats;

        // プレイヤーの体(非トリガー)かつ生存中なら true
        public static bool TryGetFromCollider(Collider2D other, out PlayerController player)
        {
            player = null;
            if (other.isTrigger || other.attachedRigidbody == null) return false;
            return other.attachedRigidbody.TryGetComponent(out player) && !player.stats.IsDead;
        }

        private void Awake()
        {
            Current = this;
            rb = GetComponent<Rigidbody2D>();
            body = GetComponent<Collider2D>();
            stats = GetComponent<Stats>();
            renderers = GetComponentsInChildren<SpriteRenderer>();
            move = InputSystem.actions.FindAction("Player/Move");
            jump = InputSystem.actions.FindAction("Player/Jump");
            groundMask = LayerMask.GetMask("Ground");
            defaultGravity = rb.gravityScale;
            baseScaleX = Mathf.Abs(transform.localScale.x);
            dropKickHitbox.Active = false;
        }

        private void OnEnable()
        {
            stats.OnDied += HandleDied;
            dropKickHitbox.OnHit += HandleKickHit;
        }

        private void OnDisable()
        {
            stats.OnDied -= HandleDied;
            dropKickHitbox.OnHit -= HandleKickHit;
        }

        private void Update()
        {
            moveX = move.ReadValue<Vector2>().x;

            if (jump.WasPressedThisFrame() && state == State.Normal && !stats.IsStunned)
            {
                if (grounded || coyoteTimer > 0f) jumpRequested = true;
                else if (airKickAvailable) kickRequested = true;
            }

            bool blink = stats.IsInvincible && state != State.DropKick && !stats.IsDead;
            bool visible = !blink || Mathf.Repeat(Time.time * 12f, 1f) > 0.5f;
            foreach (var r in renderers) r.enabled = visible;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            if (groundIgnoreTimer > 0f) groundIgnoreTimer -= dt;

            grounded = groundIgnoreTimer <= 0f && CheckGround();
            if (grounded)
            {
                coyoteTimer = coyoteTime;
                airKickAvailable = true;
                if (!wasGrounded)
                {
                    risingFromJump = false;
                    EndDropKick();
                }
            }
            else if (coyoteTimer > 0f)
            {
                coyoteTimer -= dt;
            }
            wasGrounded = grounded;

            switch (state)
            {
                case State.Normal:
                    if (stats.IsStunned) risingFromJump = false;
                    else TickNormal(dt);
                    break;
                case State.DropKick:
                    stateTimer += dt;
                    ApplyKickVelocity();
                    if (stateTimer >= dropKickMaxTime) EndDropKick();
                    break;
                case State.Dead:
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    break;
            }

            jumpRequested = false;
            kickRequested = false;
        }

        private void TickNormal(float dt)
        {
            var v = rb.linearVelocity;
            float accel = grounded ? groundAcceleration : airAcceleration;
            v.x = Mathf.MoveTowards(v.x, moveX * stats.MoveSpeed, accel * dt);
            if (Mathf.Abs(moveX) > 0.1f) SetFacing(moveX > 0f ? 1 : -1);

            if (jumpRequested)
            {
                v.y = stats.JumpPower;
                coyoteTimer = 0f;
                groundIgnoreTimer = 0.1f;
                grounded = false;
                wasGrounded = false;
                risingFromJump = true;
            }
            else if (risingFromJump && (v.y <= 0f || !jump.IsPressed()))
            {
                if (v.y > 0f) v.y *= jumpCutMultiplier;
                risingFromJump = false;
            }

            rb.linearVelocity = v;

            if (kickRequested) StartDropKick();
        }

        private void StartDropKick()
        {
            state = State.DropKick;
            stateTimer = 0f;
            airKickAvailable = false;
            risingFromJump = false;
            rb.gravityScale = 0f;
            stats.KeepInvincible = true;
            dropKickHitbox.Active = true;
            ApplyKickVelocity();
        }

        private void ApplyKickVelocity()
        {
            float rad = dropKickAngle * Mathf.Deg2Rad;
            rb.linearVelocity = new Vector2(Mathf.Cos(rad) * facing, -Mathf.Sin(rad)) * stats.DropKickSpeed;
        }

        private void EndDropKick()
        {
            if (state != State.DropKick) return;
            state = State.Normal;
            rb.gravityScale = defaultGravity;
            stats.KeepInvincible = false;
            dropKickHitbox.Active = false;
        }

        private void HandleKickHit(Stats target)
        {
            if (state != State.DropKick) return;
            EndDropKick();
            rb.linearVelocity = new Vector2(-facing * dropKickBounce.x, dropKickBounce.y);
            groundIgnoreTimer = 0.1f;
            stats.SetInvincible(invincibleAfterKickHit);
            airKickAvailable = true;
        }

        private void HandleDied()
        {
            EndDropKick();
            state = State.Dead;
        }

        public void Respawn(Vector3 position)
        {
            EndDropKick();
            transform.position = position;
            rb.linearVelocity = Vector2.zero;
            if (!stats.IsDead) state = State.Normal;
            airKickAvailable = true;
            stats.SetInvincible(1f);
        }

        private void SetFacing(int dir)
        {
            if (dir == facing) return;
            facing = dir;
            var s = transform.localScale;
            s.x = baseScaleX * dir;
            transform.localScale = s;
        }

        private bool CheckGround()
        {
            if (rb.linearVelocity.y > 0.1f) return false;
            var b = body.bounds;
            var center = new Vector2(b.center.x, b.min.y - 0.025f);
            return Physics2D.OverlapBox(center, new Vector2(b.size.x * 0.9f, 0.15f), 0f, groundMask) != null;
        }
    }
}
