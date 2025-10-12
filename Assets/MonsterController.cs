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

    // ✅ 안전장치
    private bool _dying = false;
    private Coroutine _flashCo;
    private float lastAttackTime = -999f;

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

        // 항상 추적
        MoveTowardPlayer();

        // 사거리면 공격
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

    IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.red;
        // 웨이브 종료로 일시정지되어도 복구되게 Realtime 사용
        yield return new WaitForSecondsRealtime(0.15f);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    void Die()
    {
        if (_dying) return;
        _dying = true;

        SurvivalGameManager.Instance?.UnregisterEnemy(this);

        if (_flashCo != null) StopCoroutine(_flashCo);
        Destroy(gameObject);
    }

    // ⚠️ KickableObject에서 트리거 데미지를 주고 있으면
    // 아래 충돌 데미지는 지우세요(중복 방지).
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Kickable"))
            TakeDamage(1);
    }
}
