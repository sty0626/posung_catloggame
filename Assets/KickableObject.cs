using UnityEngine;
using Unity.Cinemachine;

public class KickableObject : MonoBehaviour
{
    [Header("발차기 설정")]
    public float defaultKickPower = 25f;

    [Header("타격감 설정")]
    public float wallShakeForce = 2f;
    public float knockbackForce = 5f;

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

    public void Kick(Vector2 direction, float power, int bonusDamage = 0)
    {
        IsKicked = true;
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

    // ⭐ [이 함수가 없어서 에러가 났던 겁니다!]
    // 플레이어가 공을 잡았을 때 호출되는 함수
    public void OnCaught()
    {
        IsKicked = false;
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