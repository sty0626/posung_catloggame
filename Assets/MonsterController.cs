using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    [Header("몬스터 스탯")]
    public float moveSpeed = 3f;
    public float attackRange = 1.2f;
    public int maxHP = 3;
    public float attackCooldown = 1f;

    private int currentHP;
    private Transform player;
    private PlayerController playerController;
    private Rigidbody2D rb;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private bool _dying = false;
    private Coroutine _flashCo;
    private float lastAttackTime = -999f;

    // 넉백 중인지 체크
    private bool isKnockedBack = false;

    // 매니저 등록/해제
    void OnEnable() { SurvivalGameManager.Instance?.RegisterEnemy(this); }
    void OnDisable() { SurvivalGameManager.Instance?.UnregisterEnemy(this); }
    void OnDestroy() { SurvivalGameManager.Instance?.UnregisterEnemy(this); }

    void Start()
    {
        currentHP = maxHP;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 원래 색깔 기억하기
        if (spriteRenderer) originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (_dying || player == null) return;

        // 넉백 중이면 이동 로직 건너뜀 (밀려나는 힘에 맡김)
        if (isKnockedBack) return;

        MoveTowardPlayer();

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange) AttackPlayer();
    }

    void MoveTowardPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
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

        // ⭐ 피격 시 빨갛게 깜빡이기
        if (spriteRenderer != null)
        {
            if (_flashCo != null) StopCoroutine(_flashCo);
            _flashCo = StartCoroutine(FlashRed());
        }

        if (currentHP <= 0) Die();
    }

    // 넉백 함수
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (_dying) return;
        StartCoroutine(KnockbackRoutine(direction, force));
    }

    IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        isKnockedBack = true;

        // 순간적으로 밀어내기
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.2f); // 0.2초 동안 밀림

        // 멈추고 다시 추적 시작
        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }

    // 빨간색 점멸 코루틴
    IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = Color.red; // 빨간색!
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor; // 원상복구
    }

    void Die()
    {
        if (_dying) return;
        _dying = true;

        SurvivalGameManager.Instance?.UnregisterEnemy(this);
        if (_flashCo != null) StopCoroutine(_flashCo);
        Destroy(gameObject);
    }
}