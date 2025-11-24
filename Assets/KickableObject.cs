using UnityEngine;

public class KickableObject : MonoBehaviour
{
    [Header("발차기 세기")]
    public float defaultKickPower = 25f;   // 기본 발차기 속도

    [Header("벽 튕김 세기 (1 = 그대로 유지)")]
    [Range(0f, 1.5f)]
    public float bounceFactor = 1f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 플레이어가 F키 눌렀을 때 PlayerController에서 호출하는 함수
    public void Kick(Vector2 direction, float power)
    {
        if (rb == null) return;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.up; // 혹시 0,0이면 위로

        if (power <= 0f)
            power = defaultKickPower;

        // 힘을 더하는 게 아니라, 그냥 속도를 확 세팅
        rb.linearVelocity = direction.normalized * power;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb == null) return;

        // 몬스터 맞으면 데미지
        if (collision.gameObject.CompareTag("Monster"))
        {
            var monster = collision.gameObject.GetComponent<MonsterController>();
            if (monster != null)
                monster.TakeDamage(1);

            // 몬스터 맞고도 계속 튕기게 하고 싶으면 return 없애고 아래까지 내려가도 됨
            return;
        }

        // 벽에 부딪히면 반사
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (rb.linearVelocity.sqrMagnitude < 0.0001f) return;

            Vector2 v = rb.linearVelocity;
            Vector2 n = collision.contacts[0].normal;
            rb.linearVelocity = Vector2.Reflect(v, n) * bounceFactor;
        }
    }
}
