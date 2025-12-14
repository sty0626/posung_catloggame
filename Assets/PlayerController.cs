using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public int maxHP = 10;
    public int ballDamageBonus = 0;
    public float kickRange = 1.5f; // 사거리 살짝 증가
    public float kickPower = 20f;  // 파워 증가
    public float kickCooldown = 0.5f; // 쿨타임 추가
    public LayerMask ballLayer;

    private int currentHP;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 lastLookDir = Vector2.up;
    private bool canMove = true;

    private float lastKickTime;

    void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (canMove)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            movement = Vector2.zero;
        }

        // 쿨타임 체크
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= lastKickTime + kickCooldown)
        {
            TryKickBall();
        }

        if (movement != Vector2.zero)
        {
            lastLookDir = movement.normalized;
        }
    }

    void TryKickBall()
    {
        Vector2 dir = lastLookDir;

        // 레이캐스트 디버그 시각화 (Scene 뷰에서 보임)
        Debug.DrawRay(transform.position, dir * kickRange, Color.red, 0.5f);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, kickRange, ballLayer);

        if (hit.collider != null)
        {
            KickableObject ball = hit.collider.GetComponent<KickableObject>();
            if (ball != null)
            {
                lastKickTime = Time.time; // 쿨타임 갱신
                ball.Kick(dir, kickPower);

                // 킥하는 순간 플레이어도 반동으로 살짝 뒤로 밀리거나 멈칫하면 느낌이 좋음 (선택사항)
            }
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;

        // 플레이어 피격 시에도 화면 흔들림
        CameraShake.Instance?.Shake(0.2f, 0.3f);

        Debug.Log("플레이어 남은 체력: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("플레이어 사망");
        SetCanMove(false);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}