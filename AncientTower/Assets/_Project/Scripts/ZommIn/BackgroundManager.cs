using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // 新增：用于加载场景

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    [Header("背景图片")]
    public GameObject oldBackground;
    public GameObject newBackground;

    [Header("淡入淡出")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("相机设置")]
    public float wideOrthoSize = 6.5f;   // 远景相机大小

    private Vector3 originalCameraPosition;

    void Awake()
    {
        Instance = this;
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.raycastTarget = false;   // 初始不阻挡
            fadeImage.gameObject.SetActive(false); // 初始未激活
        }
        if (Camera.main != null)
            originalCameraPosition = Camera.main.transform.position;
    }

    // 原有方法：切换背景并重置相机（黑屏期间完成重置）
    public void FadeToNewBackground()
    {
        StartCoroutine(FadeOutAndSwitch());
    }

    IEnumerator FadeOutAndSwitch()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = true;   // 开始阻挡点击
            fadeImage.color = new Color(0, 0, 0, 0);
        }
        // 1. 淡出（变黑）
        float elapsed = 0f;
        Color color = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // 2. 画面全黑 → 重置相机（用户看不见跳变）
        if (Camera.main != null)
        {
            Camera.main.orthographicSize = 5f;
            Camera.main.transform.position = originalCameraPosition;
        }

        // 3. 切换背景
        if (oldBackground != null) oldBackground.SetActive(false);
        if (newBackground != null) newBackground.SetActive(true);

        // 4. 淡入（恢复画面）
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(color.r, color.g, color.b, 0);

        // 可选：淡入结束后禁用 FadeImage（避免阻挡点击，但保留透明也可）
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false;  // 恢复点击
            fadeImage.gameObject.SetActive(false);
        }
    }

    // 新增方法：渐黑后加载指定场景
    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        // 确保 FadeImage 激活
        if (fadeImage != null && !fadeImage.gameObject.activeSelf)
            fadeImage.gameObject.SetActive(true);

        // 淡出到全黑
        float elapsed = 0f;
        Color color = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        // 确保完全不透明（保险）
        fadeImage.color = new Color(color.r, color.g, color.b, 1);

        // 可选：短暂停顿，让黑屏持续一瞬间
        yield return new WaitForSeconds(0.1f);

        // 异步加载场景（不卡顿）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        // 场景切换后，新场景的 BackgroundManager 会接管，这里无需额外操作
    }
}