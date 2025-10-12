using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Bounds (Arena)")]
    public Vector2 center = Vector2.zero;
    public Vector2 size = new Vector2(30f, 18f);
    public float edgePadding = 1.5f;

    [Header("Spawn Control")]
    public float baseInterval = 1.5f;
    public float minInterval = 0.4f;
    public AnimationCurve difficultyCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 0.5f),
        new Keyframe(1f, 1f)
    );

    public int baseBatch = 1;
    public int maxBatch = 6;

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
        var gm = SurvivalGameManager.Instance;

        while (spawning)
        {
            float phaseRatio = 0f;
            if (gm != null && gm.survivalDuration > 0f)
                phaseRatio = Mathf.Clamp01(t / gm.survivalDuration);

            float d = difficultyCurve.Evaluate(phaseRatio);
            float interval = Mathf.Lerp(baseInterval, minInterval, d);
            int batch = Mathf.RoundToInt(Mathf.Lerp(baseBatch, maxBatch, d));

            SpawnBatch(batch);

            t += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    void SpawnBatch(int count)
    {
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
        float halfX = size.x * 0.5f - edgePadding;
        float halfY = size.y * 0.5f - edgePadding;
        float x = Random.Range(center.x - halfX, center.x + halfX);
        float y = Random.Range(center.y - halfY, center.y + halfY);
        return new Vector2(x, y);
    }
}
