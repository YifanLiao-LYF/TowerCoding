using UnityEngine;
using System.Collections;

public class TabletController : MonoBehaviour
{
    public RectTransform tabletPanel;
    public GameObject overlay;
    public GameObject blockerUI;
    public float slideDuration = 0.2f;
    public float visibleOffset = 20f;

    private bool isVisible = false;
    private float tabletWidth;
    private Coroutine currentCoroutine;

    void Start()
    {
        tabletWidth = tabletPanel.rect.width;
        // 确保 tabletPanel 始终激活
        tabletPanel.gameObject.SetActive(true);
        // 初始位置在屏幕外（右侧）
        tabletPanel.anchoredPosition = new Vector2(tabletWidth, 0);

        if (overlay != null) overlay.SetActive(false);
        if (blockerUI != null) blockerUI.SetActive(false);
    }

    public void ShowTablet()
    {
        if (isVisible) return;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        if (blockerUI != null) blockerUI.SetActive(true);
        if (overlay != null) overlay.SetActive(true);

        // 确保 panel 激活
        tabletPanel.gameObject.SetActive(true);
        currentCoroutine = StartCoroutine(SlideTo(new Vector2(-visibleOffset, 0), true));
    }

    public void HideTablet()
    {
        if (!isVisible) return;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(SlideTo(new Vector2(tabletWidth, 0), false));
    }

    private IEnumerator SlideTo(Vector2 targetPos, bool willShow)
    {
        Vector2 startPos = tabletPanel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            t = Mathf.SmoothStep(0, 1, t);
            tabletPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        tabletPanel.anchoredPosition = targetPos;
        isVisible = willShow;

        if (!willShow)
        {
            if (overlay != null) overlay.SetActive(false);
            if (blockerUI != null) blockerUI.SetActive(false);
            // 注意：不再隐藏 tabletPanel 本身，只是移出屏幕
        }
        currentCoroutine = null;
    }
}