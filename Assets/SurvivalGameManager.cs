using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SurvivalGameManager : MonoBehaviour
{
    public static SurvivalGameManager Instance { get; private set; }

    [Header("게임 설정")]
    public float survivalDuration = 60f;

    [Header("맵 경계 설정 (벽 안쪽 좌표)")]
    public Vector2 minMapLimit = new Vector2(-18f, -10f);
    public Vector2 maxMapLimit = new Vector2(18f, 10f);

    [Header("UI 연결")]
    public UnityEngine.UI.Text timerTextUGUI;
    public TMP_Text timerTextTMP;
    public GameObject gameOverUI;

    [Header("몬스터 소환 설정")]
    public GameObject monsterPrefab;
    public float spawnInterval = 2f;
    public float spawnRadius = 10f;

    // ★ [추가됨] 엘리트 몬스터 등장 확률 (0.2 = 20%)
    [Range(0f, 1f)]
    public float eliteSpawnChance = 0.2f;

    [Header("참조")]
    public PlayerController player;
    public GameObject rewardPanel;
    public RewardUI rewardUI;

    private float timeLeft;
    private bool isPlayingPhase;
    private Coroutine spawnCoroutine;
    private readonly List<MonsterController> aliveEnemies = new List<MonsterController>();

    public enum Phase { Playing, Reward, GameOver }
    public Phase CurrentPhase { get; private set; } = Phase.Playing;

    public int AliveCount
    {
        get { aliveEnemies.RemoveAll(x => x == null); return aliveEnemies.Count; }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        AutoWireTimerText();
    }

    void Start()
    {
        if (survivalDuration <= 0f) survivalDuration = 60f;

        // 플레이어 자동 찾기
        if (player == null) player = FindObjectOfType<PlayerController>();

        StartPlayingPhase();
    }

    void Update()
    {
        if (!isPlayingPhase) return;

        timeLeft -= Time.unscaledDeltaTime;
        if (timeLeft < 0f) timeLeft = 0f;

        UpdateTimerUI();

        if (timeLeft <= 0f) OnPhaseComplete();
    }

    public void StartPlayingPhase()
    {
        Time.timeScale = 1f;
        CurrentPhase = Phase.Playing;
        isPlayingPhase = true;
        timeLeft = Mathf.Max(1f, survivalDuration);

        if (rewardPanel) rewardPanel.SetActive(false);
        if (gameOverUI) gameOverUI.SetActive(false);
        if (timerTextTMP == null && timerTextUGUI == null) AutoWireTimerText();

        UpdateTimerUI();

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void GameOver()
    {
        if (CurrentPhase == Phase.GameOver) return;
        CurrentPhase = Phase.GameOver;
        isPlayingPhase = false;
        Time.timeScale = 0f;

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        if (gameOverUI != null) gameOverUI.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnPhaseComplete()
    {
        isPlayingPhase = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        ClearAllEnemies();
        Time.timeScale = 0f;
        CurrentPhase = Phase.Reward;

        if (rewardUI == null && rewardPanel != null) rewardUI = rewardPanel.GetComponent<RewardUI>();
        if (rewardPanel) rewardPanel.SetActive(true);
        if (rewardUI) rewardUI.ShowRandomChoices();
    }

    IEnumerator SpawnRoutine()
    {
        SpawnOneMonster();

        while (isPlayingPhase)
        {
            yield return new WaitForSecondsRealtime(spawnInterval);

            try
            {
                if (Time.timeScale == 0) continue;
                if (player == null) continue;

                SpawnOneMonster();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"소환 오류(무시 가능): {e.Message}");
            }
        }
    }

    void SpawnOneMonster()
    {
        if (monsterPrefab != null && player != null)
        {
            Vector2 randomPos = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = player.transform.position + new Vector3(randomPos.x, randomPos.y, 0);

            // 맵 밖으로 나가지 않게 가두기
            spawnPos.x = Mathf.Clamp(spawnPos.x, minMapLimit.x, maxMapLimit.x);
            spawnPos.y = Mathf.Clamp(spawnPos.y, minMapLimit.y, maxMapLimit.y);

            // 1. 몬스터를 생성하고 변수(go)에 담습니다.
            GameObject go = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);

            // 2. ★ [핵심] 몬스터 컨트롤러를 가져와서 확률적으로 엘리트로 만듭니다.
            MonsterController mc = go.GetComponent<MonsterController>();
            if (mc != null)
            {
                // 인스펙터의 eliteSpawnChance 확률(0.2)보다 낮으면 당첨!
                if (Random.value < eliteSpawnChance)
                {
                    mc.MakeElite(); // "너는 이제 엘리트야!" 명령
                    Debug.Log("⚡ 엘리트 몬스터 출현! ⚡"); // 확인용 로그
                }

                // 매니저에 등록
                RegisterEnemy(mc);
            }
        }
    }

    private void UpdateTimerUI()
    {
        int sec = Mathf.CeilToInt(timeLeft);
        string text = $"{sec / 60:00}:{sec % 60:00}";
        if (timerTextTMP != null) timerTextTMP.text = text;
        else if (timerTextUGUI != null) timerTextUGUI.text = text;
        else AutoWireTimerText();
    }

    private void AutoWireTimerText()
    {
        if (timerTextTMP == null) timerTextTMP = FindObjectOfType<TMP_Text>(includeInactive: true);
        if (timerTextUGUI == null) timerTextUGUI = FindObjectOfType<UnityEngine.UI.Text>(includeInactive: true);
    }

    public void RegisterEnemy(MonsterController m) { if (m != null && !aliveEnemies.Contains(m)) aliveEnemies.Add(m); }

    public void UnregisterEnemy(MonsterController m)
    {
        if (aliveEnemies != null && m != null) aliveEnemies.Remove(m);
    }

    public void ClearAllEnemies()
    {
        var snapshot = new List<MonsterController>(aliveEnemies);
        foreach (var m in snapshot) if (m != null) Destroy(m.gameObject);
        aliveEnemies.Clear();
    }

    public void ApplyReward(RewardUI.RewardType type)
    {
        if (player != null)
        {
            switch (type)
            {
                case RewardUI.RewardType.Heal:
                    player.Heal(3);
                    Debug.Log("보상: 체력 회복");
                    break;

                case RewardUI.RewardType.MoveSpeedUp:
                    player.moveSpeed += 1.0f;
                    Debug.Log("보상: 이동 속도 증가");
                    break;

                case RewardUI.RewardType.BallDamageUp:
                    player.ballDamageBonus += 1;
                    Debug.Log("보상: 공 데미지 증가");
                    break;
            }
        }

        if (rewardPanel) rewardPanel.SetActive(false);
        StartPlayingPhase();
    }
}