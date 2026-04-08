using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeIn : MonoBehaviour
{
    public Image fadeImage;
    public float fadeInDuration = 0.5f;

    void Start()
    {
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();
        // 确保初始为黑色完全不透明
        fadeImage.color = Color.black;
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / fadeInDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);
        // 可选：淡入完成后禁用 Image 的 RaycastTarget 或禁用组件
    }
}