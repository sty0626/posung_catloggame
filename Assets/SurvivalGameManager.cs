using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SurvivalGameManager : MonoBehaviour
{
    public static SurvivalGameManager Instance { get; private set; }

    [Header("게임 설정")]
    public float survivalDuration = 60f; // 한 판 시간

    [Header("UI 연결")]
    public Text timerTextUGUI;
    public TMP_Text timerTextTMP;

    [Header("몬스터 소환 설정 (스포너 통합됨)")]
    public GameObject monsterPrefab; // ⭐ 몬스터 프리팹 연결 필수!
    public float spawnInterval = 2f;
    public float spawnRadius = 10f;

    [Header("참조")]
    public PlayerController player;
    public GameObject rewardPanel;
    public RewardUI rewardUI;

    // 내부 변수
    private float timeLeft;
    private bool isPlayingPhase;
    private Coroutine spawnCoroutine;
    private readonly List<MonsterController> aliveEnemies = new List<MonsterController>();

    public enum Phase { Playing, Reward }
    public Phase CurrentPhase { get; private set; } = Phase.Playing;

    public int AliveCount
    {
        get
        {
            aliveEnemies.RemoveAll(x => x == null);
            return aliveEnemies.Count;
        }
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

        if (timerTextTMP == null && timerTextUGUI == null) AutoWireTimerText();

        UpdateTimerUI();

        // 소환 시작
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void OnPhaseComplete()
    {
        isPlayingPhase = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

        ClearAllEnemies(); // 몬스터 싹 정리

        Time.timeScale = 0f;
        CurrentPhase = Phase.Reward;

        if (rewardUI == null && rewardPanel != null)
            rewardUI = rewardPanel.GetComponent<RewardUI>();

        if (rewardPanel) rewardPanel.SetActive(true);
        if (rewardUI) rewardUI.ShowRandomChoices();
    }

    // ⭐ 무한 소환 루틴
    IEnumerator SpawnRoutine()
    {
        while (isPlayingPhase)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (monsterPrefab != null && player != null)
            {
                Vector2 randomPos = Random.insideUnitCircle.normalized * spawnRadius;
                Vector3 spawnPos = player.transform.position + new Vector3(randomPos.x, randomPos.y, 0);
                Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
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
        if (timerTextUGUI == null) timerTextUGUI = FindObjectOfType<Text>(includeInactive: true);
    }

    public void RegisterEnemy(MonsterController m)
    {
        aliveEnemies.RemoveAll(x => x == null);
        if (m != null && !aliveEnemies.Contains(m)) aliveEnemies.Add(m);
    }

    public void UnregisterEnemy(MonsterController m)
    {
        aliveEnemies.RemoveAll(x => x == null);
        aliveEnemies.Remove(m);
    }

    public void ClearAllEnemies()
    {
        var snapshot = new List<MonsterController>(aliveEnemies);
        foreach (var m in snapshot)
        {
            if (m == null) continue;
            var go = m.gameObject;
            if (go != null) Destroy(go);
        }
        aliveEnemies.Clear();
    }

    public void ApplyReward(RewardUI.RewardType type)
    {
        if (player != null)
        {
            switch (type)
            {
                case RewardUI.RewardType.Heal: player.Heal(3); break;
                case RewardUI.RewardType.MoveSpeedUp: player.moveSpeed += 1.0f; break;
                case RewardUI.RewardType.BallDamageUp: player.ballDamageBonus += 1; break;
            }
        }
        if (rewardPanel) rewardPanel.SetActive(false);
        StartPlayingPhase();
    }
}