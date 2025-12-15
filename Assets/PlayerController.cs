using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("기본 설정")]
    public float moveSpeed = 7f;
    public int maxHP = 10;

    public int ballDamageBonus = 0;

    [Header("공 시스템")]
    public GameObject ballPrefab;   // 공 프리팹
    public float kickCooldown = 0.5f;

    // 내부 변수
    private int currentHP;
    private Rigidbody2D rb;
    private Collider2D myCollider; // ⭐ 플레이어 충돌체 저장용 변수 추가
    private Vector2 movement;
    private Vector2 lastLookDir = Vector2.up;
    private bool canMove = true;

    private KickableObject currentBall;
    private float lastKickTime;

    void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();

        // ⭐ 내 몸의 충돌체(Collider)를 미리 찾아놓습니다.
        myCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        ManageBall();

        if (canMove)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            movement = Vector2.zero;
        }

        if (movement != Vector2.zero)
        {
            lastLookDir = movement.normalized;
        }

        if (Input.GetKeyDown(KeyCode.F) && Time.time >= lastKickTime + kickCooldown)
        {
            TryKickBall();
        }
    }

    void ManageBall()
    {
        if (currentBall == null)
        {
            SpawnBall();
        }
        else
        {
            if (!currentBall.IsKicked)
            {
                Vector2 holdPos = (Vector2)transform.position + (lastLookDir * 1.2f);
                currentBall.transform.position = holdPos;

                if (currentBall.GetComponent<Rigidbody2D>())
                {
                    currentBall.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                }
            }
        }
    }

    void SpawnBall()
    {
        if (ballPrefab == null) return;

        Vector2 spawnPos = (Vector2)transform.position + (lastLookDir * 1.2f);
        GameObject newBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        currentBall = newBall.GetComponent<KickableObject>();

        // ⭐⭐⭐ [핵심 수정] 플레이어와 공 사이의 충돌을 무시하게 설정 ⭐⭐⭐
        Collider2D ballCollider = newBall.GetComponent<Collider2D>();
        if (myCollider != null && ballCollider != null)
        {
            Physics2D.IgnoreCollision(myCollider, ballCollider, true);
        }
    }

    void TryKickBall()
    {
        if (currentBall != null && !currentBall.IsKicked)
        {
            lastKickTime = Time.time;
            currentBall.Kick(lastLookDir, 25f, ballDamageBonus);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log("Player Hit! HP: " + currentHP);
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Player Died");
        SetCanMove(false);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}