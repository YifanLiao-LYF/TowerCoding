using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    [Header("背景图片（同场景切换）")]
    public GameObject oldBackground;
    public GameObject newBackground;

    [Header("淡入淡出设置")]
    public float fadeDuration = 1f;

    [Header("相机设置（同场景切换）")]
    public float wideOrthoSize = 6.5f;

    private Image fadeImage;
    private Canvas fadeCanvas;
    private Vector3 originalCameraPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFadeImage();
        SceneManager.sceneLoaded += OnSceneLoaded;  // 监听场景加载
        // 首次绑定当前场景的背景（针对第一个场景）
        BindCurrentSceneBackgrounds();
        RecordCameraPosition();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 每次场景加载完成后，重新绑定背景物体并更新相机位置
        BindCurrentSceneBackgrounds();
        RecordCameraPosition();
    }

    private void BindCurrentSceneBackgrounds()
    {
        // 优先使用 Inspector 中手动拖拽的物体（如果有），否则按名称自动查找
        if (oldBackground == null)
            oldBackground = GameObject.Find("OldBackground");
        if (newBackground == null)
            newBackground = GameObject.Find("NewBackground");

        if (oldBackground == null)
            Debug.LogWarning($"场景 {SceneManager.GetActiveScene().name} 中未找到名为 'OldBackground' 的物体，请在 Inspector 中手动指定");
        if (newBackground == null)
            Debug.LogWarning($"场景 {SceneManager.GetActiveScene().name} 中未找到名为 'NewBackground' 的物体，请在 Inspector 中手动指定");
    }

    private void RecordCameraPosition()
    {
        if (Camera.main != null)
            originalCameraPosition = Camera.main.transform.position;
    }

    private void InitializeFadeImage()
    {
        // 尝试使用场景中已有的 FadeImage（兼容旧场景）
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null)
        {
            fadeImage = existingCanvas.GetComponentInChildren<Image>();
            if (fadeImage != null)
            {
                fadeImage.gameObject.SetActive(false);
                return;
            }
        }
        CreateFadeCanvasAndImage();
    }

    private void CreateFadeCanvasAndImage()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 30000;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rect = imageObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = true;
        fadeImage.gameObject.SetActive(false);
    }

    // ========== 同场景背景切换 ==========
    public void FadeToNewBackground()
    {
        StartCoroutine(FadeOutAndSwitch());
    }

    IEnumerator FadeOutAndSwitch()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        fadeImage.raycastTarget = true;
        fadeImage.color = new Color(0, 0, 0, 0);

        float elapsed = 0f;
        Color color = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // 重置相机（使用当前场景记录的相机位置）
        if (Camera.main != null)
        {
            Camera.main.orthographicSize = wideOrthoSize;
            Camera.main.transform.position = originalCameraPosition;
        }

        // 切换背景
        if (oldBackground != null) oldBackground.SetActive(false);
        if (newBackground != null) newBackground.SetActive(true);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(color.r, color.g, color.b, 0);

        fadeImage.raycastTarget = false;
        fadeImage.gameObject.SetActive(false);
    }

    // ========== 跨场景淡入淡出 ==========
    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutLoadAndFadeIn(sceneName));
    }

    public void FadeAndLoadScene2(string sceneName)
    {
        FadeAndLoadScene(sceneName);
    }

    private IEnumerator FadeOutLoadAndFadeIn(string sceneName)
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        fadeImage.raycastTarget = true;
        fadeImage.color = new Color(0, 0, 0, 0);

        float elapsed = 0f;
        Color color = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(color.r, color.g, color.b, 1);

        yield return new WaitForSeconds(0.1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        // 加载完成后淡入
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(color.r, color.g, color.b, 0);

        fadeImage.raycastTarget = false;
        fadeImage.gameObject.SetActive(false);
    }
}