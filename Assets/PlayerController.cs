using UnityEngine;
using UnityEngine.UI; // UI를 다루기 위해 필요
using System.Collections; // 코루틴 사용을 위해 필요

public class PlayerController : MonoBehaviour
{
    [Header("1. 기본 설정")]
    public float moveSpeed = 7f;
    public int maxHP = 5;

    [Header("2. 대쉬 설정")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float lastDashTime = -999f;
    private TrailRenderer tr;

    [Header("3. UI/피격 설정")]
    public HeartUI heartUI;
    public Image skillCooldownImage; // R키 쿨타임 UI
    public float flashDuration = 0.1f; // 피격 깜빡임 시간
    public Color hitColor = Color.red; // 피격 시 색상
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private bool isDead = false;

    [Header("4. 공 시스템")]
    public KickableObject currentBall;
    public float kickForce = 25f;
    public int ballDamageBonus = 0;
    public float kickCooldown = 0.5f;
    public float catchRange = 1.5f;

    [Header("5. 스킬/궁극기 설정")]
    public float recallCooldown = 8f; // R키 쿨타임
    private float lastRecallTime = -999f;

    // ★★★ 중요: Project 창의 공 프리팹을 여기에 연결해야 합니다!
    public KickableObject ultBallPrefab;
    public float ultKickForce = 15f;
    public float ultDuration = 3f; // 3초 뒤 사라짐
    public float ultCooldown = 20f; // G키 쿨타임
    public int numBalls = 8;
    private float lastUltTime = -999f;

    // 내부 변수
    private int currentHP;
    private Rigidbody2D rb;
    private Collider2D myCollider;
    private Vector2 movement;
    private Vector2 lastLookDir = Vector2.right;
    private bool canMove = true;
    private Animator anim;
    private bool isHoldingBall = true;
    private float lastKickTime;
    private Vector2 aimDirection;

    void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        tr = GetComponent<TrailRenderer>();
        TryGetComponent(out anim);

        // Sprite Renderer 및 기본 색상 초기화
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (heartUI != null) heartUI.UpdateHearts(currentHP);
        if (tr != null) tr.emitting = false;

        if (currentBall != null)
        {
            Collider2D ballCol = currentBall.GetComponent<Collider2D>();
            if (ballCol != null && myCollider != null)
                Physics2D.IgnoreCollision(myCollider, ballCol, true);
            CatchBall();
        }
    }

    void Update()
    {
        UpdateSkillUI(); // R키 쿨타임 UI 업데이트

        if (isDead) { rb.linearVelocity = Vector2.zero; return; }
        if (isDashing) return; // 대쉬 중에는 입력 무시

        if (!canMove) { rb.linearVelocity = Vector2.zero; return; }

        CalculateAimDirection();
        ManageBall();

        // 1. 이동 입력
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 2. 애니메이션 및 방향 전환
        bool isMoving = movement.magnitude > 0;
        if (anim != null) anim.SetBool("isMoving", isMoving);

        if (movement != Vector2.zero)
        {
            lastLookDir = movement.normalized;
            if (movement.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            else if (movement.x > 0) transform.localScale = new Vector3(1, 1, 1);
        }

        // 3. 스킬 입력
        if (Input.GetKeyDown(KeyCode.F) && isHoldingBall && Time.time >= lastKickTime + kickCooldown)
        {
            TryKickBall();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryRecallBall();
        }
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(DashRoutine());
        }
        if (Input.GetKeyDown(KeyCode.G) && Time.time >= lastUltTime + ultCooldown)
        {
            UseUltimateSkill();
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        if (canMove && !isDead)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // ========== 코루틴/스킬 로직 ==========

    IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        if (tr != null) tr.emitting = true;

        Vector2 dashDir = movement.magnitude > 0 ? movement.normalized : lastLookDir;
        rb.linearVelocity = dashDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        if (tr != null) tr.emitting = false;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }

    void UpdateSkillUI()
    {
        if (skillCooldownImage == null) return;
        float timePassed = Time.time - lastRecallTime;
        if (timePassed < recallCooldown) skillCooldownImage.fillAmount = timePassed / recallCooldown;
        else skillCooldownImage.fillAmount = 1f;
    }

    void UseUltimateSkill()
    {
        if (ultBallPrefab == null)
        {
            Debug.LogError("궁극기 공 프리팹(Ult Ball Prefab)을 인스펙터에 연결해야 합니다!");
            return;
        }

        lastUltTime = Time.time;

        // ★ 궁극기 쿨타임 UI가 따로 있다면 여기서 fillAmount를 0으로 설정해야 합니다.

        // 8방향으로 발사
        for (int i = 0; i < numBalls; i++)
        {
            float angle = i * (360f / numBalls);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            // 공 생성
            KickableObject tempBall = Instantiate(ultBallPrefab, transform.position, Quaternion.identity);

            // 3초 뒤에 사라지도록 설정
            Destroy(tempBall.gameObject, ultDuration);

            // 공 발사
            tempBall.Kick(dir, ultKickForce, ballDamageBonus);
        }
    }

    void TryRecallBall()
    {
        if (isHoldingBall) return;
        if (Time.time < lastRecallTime + recallCooldown) return;

        lastRecallTime = Time.time;
        if (skillCooldownImage != null) skillCooldownImage.fillAmount = 0f;

        if (currentBall != null)
        {
            currentBall.transform.position = transform.position;
            CatchBall();
        }
    }

    // ========== 공 및 조준 로직 ==========

    void CalculateAimDirection()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        aimDirection = (mousePos - transform.position).normalized;
    }

    void ManageBall()
    {
        if (currentBall == null) return;
        if (isHoldingBall)
        {
            Vector2 holdPos = (Vector2)transform.position + (aimDirection * 1.0f);
            currentBall.transform.position = holdPos;
            currentBall.OnCaught();
        }
        else
        {
            if (Time.time >= lastKickTime + 0.2f)
            {
                float distance = Vector2.Distance(transform.position, currentBall.transform.position);
                if (distance <= catchRange) CatchBall();
            }
        }
    }

    void CatchBall()
    {
        isHoldingBall = true;
        if (currentBall != null) currentBall.OnCaught();
    }

    void TryKickBall()
    {
        if (currentBall != null)
        {
            isHoldingBall = false;
            lastKickTime = Time.time;
            currentBall.Kick(aimDirection, kickForce, ballDamageBonus);
        }
    }

    // ========== 체력/데미지/사망 로직 ==========

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        if (heartUI != null) heartUI.UpdateHearts(currentHP);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (isDashing) return; // 대쉬 중 무적

        currentHP -= amount;
        if (heartUI != null) heartUI.UpdateHearts(currentHP);

        // 피격 시 깜빡임 시작
        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashHit());
        }

        if (currentHP <= 0) Die();
    }

    IEnumerator FlashHit()
    {
        // 피격 색상으로 변경
        spriteRenderer.color = hitColor;

        // 잠시 대기
        yield return new WaitForSeconds(flashDuration);

        // 원래 색상으로 복귀
        spriteRenderer.color = originalColor;

        flashCoroutine = null;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        SetCanMove(false);
        if (anim != null) anim.SetTrigger("doDie");
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        if (SurvivalGameManager.Instance != null)
            SurvivalGameManager.Instance.GameOver();
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}