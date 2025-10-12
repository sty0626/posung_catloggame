using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab; // 생성할 몬스터 프리팹
    public float spawnInterval = 3f; // 몇 초마다 생성할지

    private Camera mainCamera;
    private float timer;

    void Start()
    {
        mainCamera = Camera.main;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnMonsterAtRandomScreenPosition();
            timer = 0f;
        }
    }

    void SpawnMonsterAtRandomScreenPosition()
    {
        // 화면의 랜덤한 Viewport 좌표 (0~1)
        float randomX = Random.Range(0.1f, 0.9f);
        float randomY = Random.Range(0.1f, 0.9f);

        Vector3 viewportPos = new Vector3(randomX, randomY, 0);

        // Viewport -> World
        Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(viewportPos.x, viewportPos.y, 10f));

        Instantiate(monsterPrefab, worldPos, Quaternion.identity);
        Debug.Log("몬스터 생성! 위치: " + worldPos);
    }
}
