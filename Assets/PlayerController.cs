using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("기본 설정")]
    public float moveSpeed = 7f;
    // ★ [수정됨] 하트가 5개이므로 최대 체력도 5로 맞췄습니다. (원래 10)
    public int maxHP = 5;

    [Header("UI 설정")]
    // ★ [추가됨] 인스펙터에서 HeartContainer를 여기에 드래그하세요!
    public HeartUI heartUI;

    [Header("공 능력치 (보상으로 증가됨)")]
    public float kickForce = 25f; // 공 차는 힘
    public int ballDamageBonus = 0; // 공 추가 데미지

    [Header("공 시스템")]
    public KickableObject currentBall;
    public float kickCooldown = 0.5f;
    public float catchRange = 1.5f;

    // 내부 변수
    private int currentHP;
    private Rigidbody2D rb;
    private Collider2D myCollider;
    private Vector2 movement;
    private Vector2 lastLookDir = Vector2.up;
    private bool canMove = true;

    // 공 상태 관리
    private bool isHoldingBall = true;
    private float lastKickTime;

    void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();

        // ★ [추가됨] 게임 시작 시 UI 초기화 (꽉 찬 하트 보여주기)
        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHP);
        }

        // 시작할 때 공 설정
        if (currentBall != null)
        {
            Collider2D ballCol = currentBall.GetComponent<Collider2D>();
            if (ballCol != null && myCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, ballCol, true);
            }
            CatchBall();
        }
    }

    void Update()
    {
        if (!canMove) { rb.linearVelocity = Vector2.zero; return; }

        ManageBall();

        // 이동 입력
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero)
        {
            lastLookDir = movement.normalized;
        }

        // 발차기 (F키)
        if (Input.GetKeyDown(KeyCode.F) && isHoldingBall && Time.time >= lastKickTime + kickCooldown)
        {
            TryKickBall();
        }
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void ManageBall()
    {
        if (currentBall == null) return;

        if (isHoldingBall)
        {
            // 드리블 중
            Vector2 holdPos = (Vector2)transform.position + (lastLookDir * 1.2f);
            currentBall.transform.position = holdPos;
            currentBall.OnCaught();
        }
        else
        {
            // 공 줍기 (쿨타임 적용)
            if (Time.time >= lastKickTime + 0.2f)
            {
                float distance = Vector2.Distance(transform.position, currentBall.transform.position);
                if (distance <= catchRange)
                {
                    CatchBall();
                }
            }
        }
    }

    void CatchBall()
    {
        isHoldingBall = true;
        if (currentBall != null)
        {
            currentBall.OnCaught();
        }
    }

    void TryKickBall()
    {
        if (currentBall != null)
        {
            isHoldingBall = false;
            lastKickTime = Time.time;

            // kickForce 변수 사용
            currentBall.Kick(lastLookDir, kickForce, ballDamageBonus);
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        Debug.Log($"체력 회복! 현재 HP: {currentHP}");

        // ★ [추가됨] 체력 회복 시 UI 갱신
        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHP);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log($"으악! HP: {currentHP}");

        // ★ [추가됨] 데미지 입을 때 UI 갱신
        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHP);
        }

        if (currentHP <= 0) Die();
    }

    void Die()
    {
        SetCanMove(false);
        if (SurvivalGameManager.Instance != null) SurvivalGameManager.Instance.GameOver();
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}