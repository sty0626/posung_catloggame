using UnityEngine;

public class MonsterCharger : MonoBehaviour
{
    public float patrolSpeed = 1f;
    public float chargeSpeed = 6f;
    public float detectionRange = 6f;
    public float chargeDuration = 1.2f;
    public float fieldOfView = 30f; // 시야각 (도 단위)

    private Rigidbody2D rb;
    private Transform player;

    private Vector2 chargeDirection;
    private bool isCharging = false;
    private float chargeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isCharging)
        {
            chargeTimer -= Time.deltaTime;
            rb.linearVelocity = chargeDirection * chargeSpeed;

            if (chargeTimer <= 0f)
            {
                isCharging = false;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            Patrol();

            if (CanSeePlayer())
            {
                StartCharge();
            }
        }
    }

    void Patrol()
    {
        // 가만히 있게 하거나, 천천히 흔들거나, 랜덤 이동 구현 가능
        rb.linearVelocity = Vector2.zero; // 천천히 이동하고 싶다면 수정 가능
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 toPlayer = player.position - transform.position;
        if (toPlayer.magnitude > detectionRange) return false;

        float angle = Vector2.Angle(transform.up, toPlayer.normalized);
        return angle < fieldOfView / 2f;
    }

    void StartCharge()
    {
        isCharging = true;
        chargeTimer = chargeDuration;
        chargeDirection = (player.position - transform.position).normalized;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging)
        {
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
            {
                isCharging = false;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        Destroy(gameObject); // 테스트용: 바로 죽음
    }
}
