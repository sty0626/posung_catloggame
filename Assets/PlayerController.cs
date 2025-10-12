using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public int maxHP = 10;
    public int ballDamageBonus = 0;

    private int currentHP;
    private Rigidbody2D rb;
    private Vector2 movement;

    private bool canMove = true; // 이동 가능 여부를 저장하는 변수

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
            movement = Vector2.zero; // 이동 불가 시 움직임 0
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        // 필요하면 HP UI 갱신 코드 추가
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log("플레이어가 공격당함! 남은 체력: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("플레이어 사망");
        SetCanMove(false); // 사망하면 이동 불가
    }

    // 이동 가능 상태 설정 함수
    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}
