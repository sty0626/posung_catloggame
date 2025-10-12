using UnityEngine;

public class KickableObject : MonoBehaviour
{
    public float kickForce = 10f;

    private Rigidbody2D rb;
    private MonsterController monsterController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("Where is the Rigidbody2D?!");
        }

        //player = playerObj.transform;  // 위치 추적용
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 kickDirection = (transform.position - collision.transform.position).normalized;
            rb.AddForce(kickDirection * kickForce, ForceMode2D.Impulse);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Monster"))
        {
            MonsterController targetMonster = other.GetComponent<MonsterController>();
            if (targetMonster != null)
            {
                AttackMonster(targetMonster);
            }
        }
    }

    void AttackMonster(MonsterController target)
    {
        if (target != null)
        {
            target.TakeDamage(1);
            Debug.Log("공이 몬스터를 공격함!");
            //lastAttackTime = Time.time;
        }
        else
        {
            Debug.LogWarning("monsterController가 null입니다!");
        }
    }
}
