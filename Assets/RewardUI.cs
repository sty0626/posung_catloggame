using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    public enum RewardType { Heal, MoveSpeedUp, BallDamageUp }

    [Header("Buttons (비워둬도 실행은 됨)")]
    public Button healBtn;
    public Button speedBtn;
    public Button dmgBtn;

    void Awake()
    {
        // 버튼을 아직 안 연결했어도 에러 안 나게 방어
        if (healBtn) healBtn.onClick.AddListener(() => Choose(RewardType.Heal));
        if (speedBtn) speedBtn.onClick.AddListener(() => Choose(RewardType.MoveSpeedUp));
        if (dmgBtn) dmgBtn.onClick.AddListener(() => Choose(RewardType.BallDamageUp));
    }

    public void ShowRandomChoices()
    {
        gameObject.SetActive(true); // 보상창 열기
    }

    void Choose(RewardType type)
    {
        SurvivalGameManager.Instance?.ApplyReward(type);
    }
}
