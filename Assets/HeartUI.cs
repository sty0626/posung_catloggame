using UnityEngine;
using UnityEngine.UI; // UI를 다루기 위해 필수입니다.

public class HeartUI : MonoBehaviour
{
    [Header("설정")]
    // 하트 이미지 오브젝트 5개를 여기에 넣을 겁니다.
    public Image[] heartImages;

    // 꽉 찬 하트 그림
    public Sprite fullHeartSprite;

    // 빈 하트 그림 (배경이 투명한 회색 하트 등을 추천)
    public Sprite emptyHeartSprite;

    // 체력이 바뀔 때 이 함수를 부르면 됩니다.
    public void UpdateHearts(int currentHealth)
    {
        // 하트 개수(5개)만큼 반복하면서 검사
        for (int i = 0; i < heartImages.Length; i++)
        {
            // 배열은 0부터 시작하므로,
            // 예: 체력이 3이면 index 0, 1, 2는 꽉 찬 하트, 3, 4는 빈 하트가 됨.
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeartSprite;
            }
            else
            {
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }
}