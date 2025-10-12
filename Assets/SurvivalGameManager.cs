using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // UGUI Text
using TMPro;           // TextMeshPro

public class SurvivalGameManager : MonoBehaviour
{
    public static SurvivalGameManager Instance { get; private set; }

    // === Phase / Timer ===
    [Header("Phase / Timer")]
    [Tooltip("한 웨이브의 생존 시간(초)")]
    public float survivalDuration = 60f;

    [Header("Timer Text (둘 중 하나만 연결해도 됨)")]
    public Text timerTextUGUI;     // UGUI Text 쓰면 여기
    public TMP_Text timerTextTMP;  // TMP 쓰면 여기

    [Header("Spawner & Player")]
    public EnemySpawner spawner;
    public PlayerController player;

    [Header("Reward UI")]
    public GameObject rewardPanel; // 비활성 시작 권장
    public RewardUI rewardUI;

    // === Internals ===
    private float timeLeft;
    private bool isPlayingPhase;

    // 살아있는 적 목록
    private readonly List<MonsterController> aliveEnemies = new List<MonsterController>();

    public enum Phase { Playing, Reward }
    public Phase CurrentPhase { get; private set; } = Phase.Playing;

    // 스폰러가 참조
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

        // 타이머 텍스트 자동 연결(드래그 안 해도 동작)
        AutoWireTimerText();
    }

    void Start()
    {
        // 잘못된 값 방지
        if (survivalDuration <= 0f) survivalDuration = 60f;

        StartPlayingPhase();
    }

    void Update()
    {
        if (!isPlayingPhase) return;

        timeLeft -= Time.unscaledDeltaTime; // 일시정지 대응을 원하면 Time.deltaTime 대신 이 줄 유지
        if (timeLeft < 0f) timeLeft = 0f;

        UpdateTimerUI();

        if (timeLeft <= 0f)
            OnPhaseComplete();
    }

    // === Phase Control ===
    public void StartPlayingPhase()
    {
        Time.timeScale = 1f;               // 재개
        CurrentPhase = Phase.Playing;
        isPlayingPhase = true;
        timeLeft = Mathf.Max(1f, survivalDuration);

        if (rewardPanel) rewardPanel.SetActive(false);

        // 혹시 타이머 텍스트가 비었으면 다시 한 번 자동 연결 시도
        if (timerTextTMP == null && timerTextUGUI == null)
            AutoWireTimerText();

        UpdateTimerUI();

        if (spawner) spawner.BeginSpawning();
    }

    private void OnPhaseComplete()
    {
        // 제한시간 생존 완료 → 적 정리 + 일시정지 + 보상창
        isPlayingPhase = false;
        if (spawner) spawner.StopSpawning();
        ClearAllEnemies();

        Time.timeScale = 0f;
        CurrentPhase = Phase.Reward;

        // 보상 UI 자동 연결
        if (rewardUI == null && rewardPanel != null)
            rewardUI = rewardPanel.GetComponent<RewardUI>();

        if (rewardPanel) rewardPanel.SetActive(true);
        if (rewardUI) rewardUI.ShowRandomChoices();
    }

    // === UI ===
    private void UpdateTimerUI()
    {
        int sec = Mathf.CeilToInt(timeLeft);
        string text = $"{sec / 60:00}:{sec % 60:00}";

        if (timerTextTMP != null) timerTextTMP.text = text;
        else if (timerTextUGUI != null) timerTextUGUI.text = text;
        // 둘 다 없으면 자동 연결 한 번 더 시도
        else AutoWireTimerText();
    }

    // 타이머 텍스트 자동 검색(씬 어디에 있어도 찾아서 씀)
    private void AutoWireTimerText()
    {
        // 우선 TMP부터 찾아본다
        if (timerTextTMP == null)
            timerTextTMP = FindObjectOfType<TMP_Text>(includeInactive: true);

        // UGUI Text도 시도
        if (timerTextUGUI == null)
            timerTextUGUI = FindObjectOfType<Text>(includeInactive: true);
    }

    // === Enemy Register / Unregister ===
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

    // 안전한 일괄 삭제
    public void ClearAllEnemies()
    {
        var snapshot = new List<MonsterController>(aliveEnemies);
        foreach (var m in snapshot)
        {
            if (m == null) continue;
            var go = m.gameObject;
            if (go != null) Destroy(go);
        }
        aliveEnemies.RemoveAll(x => x == null);
        aliveEnemies.Clear();
    }

    // === Rewards ===
    public void ApplyReward(RewardUI.RewardType type)
    {
        if (player != null)
        {
            switch (type)
            {
                case RewardUI.RewardType.Heal:
                    player.Heal(3);
                    break;
                case RewardUI.RewardType.MoveSpeedUp:
                    player.moveSpeed += 1.0f;
                    break;
                case RewardUI.RewardType.BallDamageUp:
                    player.ballDamageBonus += 1;
                    break;
            }
        }

        if (rewardPanel) rewardPanel.SetActive(false);
        StartPlayingPhase();
    }
}
