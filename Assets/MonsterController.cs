using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    [Header("몬스터 스탯")]
    public float moveSpeed = 3f;
    public float attackRange = 1.2f;
    public int maxHP = 3;
    public float attackCooldown = 1f;

    [Header("애니메이션 & 이펙트")]
    public GameObject deathEffectPrefab;

    // 내부 변수
    private int currentHP;
    private Transform player;
    private PlayerController playerController;
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D myCollider;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private bool _dying = false;
    private Coroutine _flashCo;
    private float lastAttackTime = -999f;
    private bool isKnockedBack = false;

    // ★ [추가] 엘리트 몬스터 여부
    public bool IsElite { get; private set; } = false;

    void OnEnable() { SurvivalGameManager.Instance?.RegisterEnemy(this); }
    void OnDisable() { SurvivalGameManager.Instance?.UnregisterEnemy(this); }
    void OnDestroy() { SurvivalGameManager.Instance?.UnregisterEnemy(this); }

    void Start()
    {
        // 체력 초기화 (MakeElite에서 변경될 수 있으므로 여기서 최종 확정)
        // 만약 스포너에서 먼저 Init을 했다면 currentHP가 덮어씌워질 수 있으니 주의.
        // 안전하게 currentHP가 0이면 maxHP로 설정.
        if (currentHP <= 0) currentHP = maxHP;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();

        if (spriteRenderer) originalColor = spriteRenderer.color;
    }

    // ★ [추가] 엘리트 몬스터로 만드는 함수 (스포너가 호출함)
    // ★ [수정] 엘리트 몬스터로 만드는 함수
    public void MakeElite()
    {
        IsElite = true;

        // 1. 능력치 강화
        // 기존: maxHP *= 2; 
        // 수정: 1.3배로 변경 (소수점 계산 후 정수로 변환)
        maxHP = (int)(maxHP * 1.3f);

        currentHP = maxHP;     // 현재 체력도 꽉 채우기
        moveSpeed *= 2.3f;     // 속도 1.5배 (더 빠르게 하고 싶으면 2.0f 등으로 변경)

        // 2. 외형 변경 (노란색 + 덩치 키우기)
        if (GetComponent<SpriteRenderer>())
        {
            GetComponent<SpriteRenderer>().color = new Color(1f, 0.8f, 0.2f); // 금색
            originalColor = new Color(1f, 0.8f, 0.2f);
        }
        transform.localScale *= 1.2f; // 덩치 20% 증가
    }

    void Update()
    {
        if (_dying || player == null) return;
        if (isKnockedBack) return;

        MoveTowardPlayer();

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange) AttackPlayer();
    }

    void MoveTowardPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);

        if (dir.x < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    void AttackPlayer()
    {
        if (playerController == null) return;
        if (Time.time - lastAttackTime < attackCooldown) return;

        playerController.TakeDamage(1);
        lastAttackTime = Time.time;
    }

    public void TakeDamage(int amount)
    {
        if (_dying) return;

        currentHP -= amount;

        if (anim != null) anim.SetTrigger("doHit");

        if (spriteRenderer != null)
        {
            if (_flashCo != null) StopCoroutine(_flashCo);
            _flashCo = StartCoroutine(FlashRed());
        }

        if (currentHP <= 0) Die();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (_dying) return;
        // 엘리트 몬스터는 넉백 저항을 조금 줄 수도 있음 (선택사항)
        if (IsElite) force *= 0.5f;
        StartCoroutine(KnockbackRoutine(direction, force));
    }

    IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        isKnockedBack = true;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);
        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor; // 원래 색(엘리트면 금색)으로 복귀
    }

    void Die()
    {
        if (_dying) return;
        _dying = true;

        SurvivalGameManager.Instance?.UnregisterEnemy(this);
        if (_flashCo != null) StopCoroutine(_flashCo);

        if (anim != null) anim.SetTrigger("doDie");
        if (myCollider != null) myCollider.enabled = false;

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject, 1f);
    }
}