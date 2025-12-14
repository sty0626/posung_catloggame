using System.Collections;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
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

    // 넉백 중인지 체크하는 변수
    private bool isKnockedBack = false;

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
        if (spriteRenderer) originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (_dying || player == null) return;

        // 넉백 중이면 이동 로직 건너뜀 (밀려나는 물리 힘을 방해하지 않기 위해)
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

        if (spriteRenderer != null)
        {
            if (_flashCo != null) StopCoroutine(_flashCo);
            _flashCo = StartCoroutine(FlashRed());
        }

        if (currentHP <= 0) Die();
    }

    // 추가된 넉백 함수
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (_dying) return;
        StartCoroutine(KnockbackRoutine(direction, force));
    }

    IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        isKnockedBack = true;
        // 순간적인 힘 가하기 (Impulse)
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        // 0.2초 정도 밀려나는 시간 부여
        yield return new WaitForSeconds(0.2f);

        // 넉백 끝, 속도 초기화 후 다시 추적 시작
        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.white; // 맞으면 하얗게 번쩍이는게 타격감이 더 좋음
        yield return new WaitForSecondsRealtime(0.1f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        if (_dying) return;
        _dying = true;

        // 사망 파티클 등을 여기서 생성하면 좋음

        SurvivalGameManager.Instance?.UnregisterEnemy(this);
        if (_flashCo != null) StopCoroutine(_flashCo);
        Destroy(gameObject);
    }

    // 중복 데미지 방지: KickableObject에서 직접 처리하므로 여기 충돌 처리는 제거하거나 비워둠
    void OnCollisionEnter2D(Collision2D col) { }
}