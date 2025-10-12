using System.Collections;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public int maxHP = 3;
    public float attackCooldown = 1f;

    private int currentHP;
    private Transform player;
    private Rigidbody2D rb;
    private float lastAttackTime = -999f;
    private PlayerController playerController;

    private bool hasDetectedPlayer = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // ✅ 추가: 살아있는 적 목록 등록
    void OnEnable()
    {
        SurvivalGameManager.Instance?.RegisterEnemy(this);
    }

    // ✅ 추가: 비활성화 시 해제
    void OnDisable()
    {
        SurvivalGameManager.Instance?.UnregisterEnemy(this);
    }

    void Start()
    {
        currentHP = maxHP;

        // 플레이어 찾기(안전 체크 추가)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }

        rb = GetComponent<Rigidbody2D>();

        // 스프라이트 색상 기억(안전 체크)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 처음 탐지되면 추적 시작
        if (!hasDetectedPlayer && distance <= detectionRange)
        {
            hasDetectedPlayer = true;
            Debug.Log("플레이어 발견!");
        }

        // 추적 상태라면 계속 따라가기
        if (hasDetectedPlayer)
        {
            MoveTowardPlayer();

            if (distance <= attackRange)
            {
                AttackPlayer();
            }
        }
    }

    void MoveTowardPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }

    void AttackPlayer()
    {
        if (playerController == null) return;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            playerController.TakeDamage(1);
            lastAttackTime = Time.time;
            Debug.Log("몬스터가 플레이어를 공격함!");
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;

        // 피격 색상 효과
        if (spriteRenderer != null) StartCoroutine(FlashRed());

        if (currentHP <= 0)
        {
            Die();
        }

        Debug.Log("몬스터가 공격당함! 남은 체력: " + currentHP);
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.7f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        // ✅ 추가: 죽을 때도 해제(안전)
        SurvivalGameManager.Instance?.UnregisterEnemy(this);

        Debug.Log("몬스터 사망");
        Destroy(gameObject);
    }

    // ⚠️ 주의: KickableObject에서도 OnTriggerEnter로 데미지를 주고 있다면
    // 이 충돌 데미지와 "중복"될 수 있습니다. 둘 중 하나만 유지하세요.
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Kickable"))
        {
            TakeDamage(1);
            Debug.Log("몬스터가 공에 맞음!");
        }
    }
}
