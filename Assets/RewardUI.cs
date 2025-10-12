using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    public enum RewardType { Heal, MoveSpeedUp, BallDamageUp }

    [Header("Buttons (����ֵ� ������ ��)")]
    public Button healBtn;
    public Button speedBtn;
    public Button dmgBtn;

    void Awake()
    {
        // ��ư�� ���� �� �����߾ ���� �� ���� ���
        if (healBtn) healBtn.onClick.AddListener(() => Choose(RewardType.Heal));
        if (speedBtn) speedBtn.onClick.AddListener(() => Choose(RewardType.MoveSpeedUp));
        if (dmgBtn) dmgBtn.onClick.AddListener(() => Choose(RewardType.BallDamageUp));
    }

    public void ShowRandomChoices()
    {
        gameObject.SetActive(true); // ����â ����
    }

    void Choose(RewardType type)
    {
        SurvivalGameManager.Instance?.ApplyReward(type);
    }
}
