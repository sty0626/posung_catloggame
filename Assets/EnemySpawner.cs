using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Bounds (Arena)")]
    public Vector2 center = Vector2.zero;           // 아레나 중심(월드)
    public Vector2 size = new Vector2(30f, 18f);    // 아레나 크기(가로x세로)
    public float edgePadding = 1.5f;                // 벽에서 살짝 안쪽으로

    [Header("Spawn Control")]
    public float baseInterval = 2.5f;               // 기본 스폰 간격(↑ 느려짐)
    public float minInterval = 1.0f;                // 최소 간격
    public AnimationCurve difficultyCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 0.45f),
        new Keyframe(1f, 1f)
    );
    public int baseBatch = 1;                       // 한 번에 스폰 수(기본)
    public int maxBatch = 2;                        // 한 번에 스폰 수(최대)

    [Header("Limits")]
    [Tooltip("동시에 존재 가능한 최대 적 수. 0 이하이면 제한 없음.")]
    public int maxAlive = 20;                       // 테스트 중이면 0으로 꺼도 됨
    public float initialDelay = 0f;                 // 웨이브 시작 후 첫 스폰 딜레이

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

            // 웨이브 진행 중일 때만 스폰
            if (gm == null || gm.CurrentPhase != SurvivalGameManager.Phase.Playing)
            {
                yield return null;
                continue;
            }

            // 동시 마리 제한 (0 이하이면 제한 꺼짐)
            if (maxAlive > 0 && gm.AliveCount >= maxAlive)
            {
                yield return new WaitForSeconds(0.5f);
                t += 0.5f;
                continue;
            }

            // 난이도 비율 (0~1)
            float phaseRatio = (gm.survivalDuration > 0f) ? Mathf.Clamp01(t / gm.survivalDuration) : 0f;
            float d = difficultyCurve.Evaluate(phaseRatio);

            float interval = Mathf.Lerp(baseInterval, minInterval, d);
            int batch = Mathf.RoundToInt(Mathf.Lerp(baseBatch, maxBatch, d));

            // 스폰 직전에도 다시 페이즈/슬롯 체크
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
