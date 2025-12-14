using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance; // 어디서든 부를 수 있게 싱글톤 처리

    void Awake()
    {
        Instance = this;
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCo(duration, magnitude));
    }

    IEnumerator ShakeCo(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.unscaledDeltaTime; // TimeScale이 0일때도 흔들리게 하려면 unscaled 사용
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}