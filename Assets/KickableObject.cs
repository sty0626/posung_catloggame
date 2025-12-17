using UnityEngine;
using Unity.Cinemachine;

public class KickableObject : MonoBehaviour
{
    [Header("발차기 설정")]
    public float defaultKickPower = 25f;

    [Header("타격감 설정")]
    public float wallShakeForce = 2f;
    public float knockbackForce = 5f;

    // ★ [추가] 공이 스스로 멈출 때 공격 판정을 끄기 위한 속도 임계값
    public float stopSpeedThreshold = 1f;

    private CinemachineImpulseSource impulseSource;
    private Rigidbody2D rb;
    private TrailRenderer trail;
    private Collider2D myCollider;

    public bool IsKicked { get; private set; } = false;
    private int currentDamage = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        myCollider = GetComponent<Collider2D>();

        if (trail != null) trail.emitting = false;
    }

    // ★ [추가] 공이 굴러가다가 속도가 줄어들면 공격 판정(IsKicked)을 끄는 로직
    void Update()
    {
        // 이미 킥 상태일 때, 속도가 너무 느려지면(멈추면) 공격 판정 해제
        if (IsKicked && rb.linearVelocity.magnitude < stopSpeedThreshold)
        {
            IsKicked = false;
            if (trail != null) trail.emitting = false;
        }
    }

    public void Kick(Vector2 direction, float power, int bonusDamage = 0)
    {
        IsKicked = true; // 공격 모드 ON
        currentDamage = 1 + bonusDamage;

        if (rb == null) return;

        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;
        if (power <= 0f) power = defaultKickPower;

        rb.linearVelocity = direction.normalized * power;

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }

        if (impulseSource) impulseSource.GenerateImpulse(0.5f);
    }

    public void OnCaught()
    {
        IsKicked = false; // ★ 드리블/잡기 상태에서는 공격 모드 OFF
        if (GetComponent<Rigidbody2D>() != null)
        {
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            GetComponent<Rigidbody2D>().angularVelocity = 0f;
        }
        if (GetComponent<TrailRenderer>() != null)
        {
            GetComponent<TrailRenderer>().emitting = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (rb.linearVelocity.magnitude > 5f && impulseSource != null)
            {
                impulseSource.GenerateImpulse(wallShakeForce);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            // =========================================================
            // ⭐ [핵심 수정] 드리블 중이거나 멈춘 공은 데미지를 주지 않음
            // =========================================================
            if (IsKicked == false) return;


            var monster = other.GetComponent<MonsterController>();
            if (monster != null)
            {
                monster.TakeDamage(currentDamage);

                Vector2 pushDir = rb.linearVelocity.normalized;
                if (pushDir == Vector2.zero)
                {
                    pushDir = (monster.transform.position - transform.position).normalized;
                }
                monster.ApplyKnockback(pushDir, knockbackForce);

                if (impulseSource) impulseSource.GenerateImpulse(0.5f);
            }
        }
    }
}