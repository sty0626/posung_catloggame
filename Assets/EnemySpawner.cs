using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Bounds (Arena)")]
    public Vector2 center = Vector2.zero;           // �Ʒ��� �߽�(����)
    public Vector2 size = new Vector2(30f, 18f);    // �Ʒ��� ũ��(����x����)
    public float edgePadding = 1.5f;                // ������ ��¦ ��������

    [Header("Spawn Control")]
    public float baseInterval = 2.5f;               // �⺻ ���� ����(�� ������)
    public float minInterval = 1.0f;                // �ּ� ����
    public AnimationCurve difficultyCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 0.45f),
        new Keyframe(1f, 1f)
    );
    public int baseBatch = 1;                       // �� ���� ���� ��(�⺻)
    public int maxBatch = 2;                        // �� ���� ���� ��(�ִ�)

    [Header("Limits")]
    [Tooltip("���ÿ� ���� ������ �ִ� �� ��. 0 �����̸� ���� ����.")]
    public int maxAlive = 20;                       // �׽�Ʈ ���̸� 0���� ���� ��
    public float initialDelay = 0f;                 // ���̺� ���� �� ù ���� ������

    private bool spawning;
    private Coroutine spawnRoutine;

    public void BeginSpawning()
    {
        if (spawning) return;
        spawning = true;
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (!spawning) return;
        spawning = false;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    IEnumerator SpawnLoop()
    {
        float t = 0f;

        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (spawning)
        {
            var gm = SurvivalGameManager.Instance;

            // ���̺� ���� ���� ���� ����
            if (gm == null || gm.CurrentPhase != SurvivalGameManager.Phase.Playing)
            {
                yield return null;
                continue;
            }

            // ���� ���� ���� (0 �����̸� ���� ����)
            if (maxAlive > 0 && gm.AliveCount >= maxAlive)
            {
                yield return new WaitForSeconds(0.5f);
                t += 0.5f;
                continue;
            }

            // ���̵� ���� (0~1)
            float phaseRatio = (gm.survivalDuration > 0f) ? Mathf.Clamp01(t / gm.survivalDuration) : 0f;
            float d = difficultyCurve.Evaluate(phaseRatio);

            float interval = Mathf.Lerp(baseInterval, minInterval, d);
            int batch = Mathf.RoundToInt(Mathf.Lerp(baseBatch, maxBatch, d));

            // ���� �������� �ٽ� ������/���� üũ
            if (gm.CurrentPhase == SurvivalGameManager.Phase.Playing)
            {
                int canSpawn = (maxAlive > 0) ? Mathf.Max(0, maxAlive - gm.AliveCount) : batch;
                if (canSpawn > 0)
                    SpawnBatch(Mathf.Min(batch, canSpawn));
            }

            t += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    void SpawnBatch(int count)
    {
        if (count <= 0) return;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Vector2 pos = GetRandomPointInArena();
            var go = Instantiate(prefab, pos, Quaternion.identity);

            var mc = go.GetComponent<MonsterController>();
            if (mc != null)
                SurvivalGameManager.Instance?.RegisterEnemy(mc);
        }
    }

    Vector2 GetRandomPointInArena()
    {
        float halfX = Mathf.Max(0f, size.x * 0.5f - edgePadding);
        float halfY = Mathf.Max(0f, size.y * 0.5f - edgePadding);
        float x = Random.Range(center.x - halfX, center.x + halfX);
        float y = Random.Range(center.y - halfY, center.y + halfY);
        return new Vector2(x, y);
    }
}
