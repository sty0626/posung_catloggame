using UnityEngine;
using Unity.Cinemachine; // ⚠️ 에러나면 using Cinemachine;

public class KickableObject : MonoBehaviour
{
    [Header("발차기 설정")]
    public float defaultKickPower = 25f;
    public float lifeTimeAfterKick = 5.0f;

    [Header("타격감(Juice) 설정")]
    public float wallShakeForce = 2f;

    private CinemachineImpulseSource impulseSource;
    private Rigidbody2D rb;
    private TrailRenderer trail;

    public bool IsKicked { get; private set; } = false;
    private int currentDamage = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        // ⭐ 핵심 변경: 태어나자마자 꼬리는 꺼둡니다. (드리블 중엔 안 보이게)
        if (trail != null)
        {
            trail.emitting = false;
        }
    }

    public void Kick(Vector2 direction, float power, int bonusDamage = 0)
    {
        if (IsKicked) return;

        IsKicked = true;
        currentDamage = 1 + bonusDamage;

        if (rb == null) return;

        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;
        if (power <= 0f) power = defaultKickPower;

        rb.linearVelocity = direction.normalized * power;

        // ⭐ 핵심 변경: 찰 때 꼬리를 켭니다!
        if (trail != null)
        {
            trail.Clear(); // 혹시 남아있던 잔상 지우기
            trail.emitting = true; // 이제부터 꼬리 그리기 시작
        }

        if (impulseSource) impulseSource.GenerateImpulse(0.5f);

        Destroy(gameObject, lifeTimeAfterKick);
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
        else if (collision.gameObject.CompareTag("Monster"))
        {
            var monster = collision.gameObject.GetComponent<MonsterController>();
            if (monster != null)
            {
                monster.TakeDamage(currentDamage);
                if (impulseSource) impulseSource.GenerateImpulse(0.5f);
            }
        }
    }
}