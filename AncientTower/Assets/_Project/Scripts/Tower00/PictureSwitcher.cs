using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PictureSwitcher : MonoBehaviour
{
    public Camera[] orderedCameras;         
    public Image fadeImage;               
    public float[] transitionDurations;     
    public string nextSceneName = "StudyRoom";
    public float delayAfterLastImage = 5f;   // 最后一张显示多久后跳转

    private int currentIndex = 0;
    private bool isSwitching = false;
    private bool isLoadingScene = false;

    void Start()
    {
        // 确保数组长度一致
        if (transitionDurations.Length != orderedCameras.Length)
        {
            Debug.LogError("transitionDurations 长度必须与相机数量一致");
            transitionDurations = new float[orderedCameras.Length];
            for (int i = 0; i < transitionDurations.Length; i++)
                transitionDurations[i] = 0.5f;
        }

        // 只激活第一个相机
        for (int i = 0; i < orderedCameras.Length; i++)
            orderedCameras[i].gameObject.SetActive(i == 0);
        fadeImage.color = new Color(0, 0, 0, 0);
    }

    public void SwitchToNext()
    {
        if (isSwitching || isLoadingScene) return;
        int nextIndex = (currentIndex + 1) % orderedCameras.Length;
        float duration = transitionDurations[currentIndex];
        StartCoroutine(FadeAndSwitch(nextIndex, duration));
    }

    IEnumerator FadeAndSwitch(int newIndex, float duration)
    {
        isSwitching = true;

        // 淡出到黑色
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = Color.black;

        // 切换相机
        orderedCameras[currentIndex].gameObject.SetActive(false);
        orderedCameras[newIndex].gameObject.SetActive(true);
        currentIndex = newIndex;

        // 淡入回来
        t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);

        isSwitching = false;

        // 检查是否是最后一张（索引 = 数组长度-1）
        if (currentIndex == orderedCameras.Length - 1 && !isLoadingScene)
        {
            StartCoroutine(LoadNextSceneAfterDelay());
        }
    }

    IEnumerator LoadNextSceneAfterDelay()
    {
        isLoadingScene = true;
        yield return new WaitForSeconds(delayAfterLastImage);

        // 淡出到黑色（如果当前遮罩不是完全黑色）
        float t = 0;
        float fadeDuration = 0.5f;  // 场景切换时的淡出时长
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = Color.black;

        // 加载目标场景
        SceneManager.LoadScene(nextSceneName);
    }
}