using UnityEngine;

public class KickableObject : MonoBehaviour
{
    [Header("발차기 설정")]
    public float defaultKickPower = 25f;
    public float minVelocityToDamage = 5f; // 이 속도 이상일 때만 데미지 줌

    [Header("타격감(Juice) 설정")]
    public GameObject hitParticlePrefab; // 벽/적 충돌 시 터지는 이펙트 프리팹
    public float shakeIntensity = 0.2f;  // 화면 흔들림 강도
    public float shakeDuration = 0.1f;   // 화면 흔들림 시간

    private Rigidbody2D rb;
    private TrailRenderer trail; // 꼬리 효과 (Add Component -> Effects -> Trail Renderer)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();

        // 도탄을 위해 물리 재질 코드로 강제 설정 (에디터에서 설정 추천)
        // 만약 에디터에서 Physics Material 2D를 넣었다면 이 부분은 없어도 됩니다.
        /*
        PhysicsMaterial2D mat = new PhysicsMaterial2D();
        mat.bounciness = 1f;
        mat.friction = 0f;
        rb.sharedMaterial = mat;
        */
    }

    public void Kick(Vector2 direction, float power)
    {
        if (rb == null) return;

        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;
        if (power <= 0f) power = defaultKickPower;

        // 순간적인 힘으로 발사!
        rb.linearVelocity = direction.normalized * power;

        // 꼬리 효과 켜기
        if (trail) trail.emitting = true;

        // 발차기 순간 약한 흔들림
        CameraShake.Instance?.Shake(0.05f, 0.1f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 속도가 너무 느리면 그냥 굴러가는 중이니 효과 무시
        float speed = rb.linearVelocity.magnitude;
        if (speed < 1f) return;

        // 1. 충돌 지점과 방향 계산
        ContactPoint2D contact = collision.contacts[0];
        Vector2 hitPoint = contact.point;

        // 2. 화면 흔들림 (속도가 빠를수록 더 세게)
        if (speed > minVelocityToDamage)
        {
            float powerScale = Mathf.Clamp(speed / defaultKickPower, 0.5f, 1.5f);
            CameraShake.Instance?.Shake(shakeDuration, shakeIntensity * powerScale);

            // 3. 파티클 생성
            if (hitParticlePrefab != null)
            {
                Instantiate(hitParticlePrefab, hitPoint, Quaternion.identity);
            }
        }

        // 4. 몬스터 충돌 처리
        if (collision.gameObject.CompareTag("Monster"))
        {
            var monster = collision.gameObject.GetComponent<MonsterController>();
            // 일정 속도 이상일 때만 데미지
            if (monster != null && speed >= minVelocityToDamage)
            {
                monster.TakeDamage(1);

                // 넉백 효과 (공이 몬스터를 밀어냄)
                monster.ApplyKnockback(rb.linearVelocity.normalized, 5f);
            }
        }

        // 5. 벽 충돌 처리 (소리 등 추가 가능)
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 벽에 부딪히면 속도가 줄지 않게 보정하고 싶다면 아래 주석 해제
            // rb.linearVelocity = rb.linearVelocity.normalized * speed; 
        }
    }
}