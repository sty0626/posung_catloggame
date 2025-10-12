using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // ← Text 쓰는 경우 필요 (TMP 쓰면 아래 안내 참고)
using TMPro;
public class SurvivalGameManager : MonoBehaviour
{
    public static SurvivalGameManager Instance { get; private set; }

    [Header("Phase/Timer")]
    public float survivalDuration = 60f;
    public TMP_Text timerText; // TextMeshPro 쓰면 TMP_Text로 바꾸세요

    [Header("Spawner & Player")]
    public EnemySpawner spawner;
    public PlayerController player;

    [Header("Reward UI")]
    public GameObject rewardPanel;
    public RewardUI rewardUI;

    private float timeLeft;
    private bool isPlayingPhase;

    private readonly List<MonsterController> aliveEnemies = new List<MonsterController>();

    public enum Phase { Playing, Reward }
    public Phase CurrentPhase { get; private set; } = Phase.Playing;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartPlayingPhase();
    }

    void Update()
    {
        if (!isPlayingPhase) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0f) timeLeft = 0f;
        UpdateTimerUI();

        if (timeLeft <= 0f)
            OnPhaseComplete();
    }

    public void StartPlayingPhase()
    {
        Time.timeScale = 1f;
        CurrentPhase = Phase.Playing;
        isPlayingPhase = true;
        timeLeft = Mathf.Max(1f, survivalDuration);

        if (rewardPanel) rewardPanel.SetActive(false);
        if (spawner) spawner.BeginSpawning();
    }

    private void OnPhaseComplete()
    {
        isPlayingPhase = false;
        if (spawner) spawner.StopSpawning();
        ClearAllEnemies();

        Time.timeScale = 0f;
        CurrentPhase = Phase.Reward;
        if (rewardPanel) rewardPanel.SetActive(true);
        if (rewardUI) rewardUI.ShowRandomChoices();
    }

    private void UpdateTimerUI()
    {
        if (!timerText) return;
        int sec = Mathf.CeilToInt(timeLeft);
        int m = sec / 60;
        int s = sec % 60;
        timerText.text = $"{m:00}:{s:00}";
    }

    public void RegisterEnemy(MonsterController m)
    {
        if (m != null && !aliveEnemies.Contains(m)) aliveEnemies.Add(m);
    }

    public void UnregisterEnemy(MonsterController m)
    {
        aliveEnemies.Remove(m);
    }

    public void ClearAllEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] != null)
                Destroy(aliveEnemies[i].gameObject);
        }
        aliveEnemies.Clear();
    }

    // 보상 선택 후 RewardUI에서 호출
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
