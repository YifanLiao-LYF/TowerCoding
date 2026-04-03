using UnityEngine;
using System.Collections;

public class TabletController : MonoBehaviour
{
    public RectTransform tabletPanel;   // 拖入平板面板的 RectTransform
    public GameObject overlay;          // 拖入遮罩物体（稍后创建）
    public float slideDuration = 0.2f;  // 滑动时间
    public float visibleOffset = 20f;   // 显示时左侧偏移量（数值越大，平板越靠左）

    private bool isVisible = false;
    private float tabletWidth;          // 平板宽度，用于计算隐藏时的位置

    void Start()
    {
        tabletWidth = tabletPanel.rect.width;
        // 确保初始时平板在屏幕外右侧
        tabletPanel.anchoredPosition = new Vector2(tabletWidth, 0);
        if (overlay != null) overlay.SetActive(false);
    }

    // 显示平板（左滑出现）
    public void ShowTablet()
    {
        if (isVisible) return;
        // 目标位置：向右偏移一个负值，使平板右侧不紧贴屏幕右边缘
        StartCoroutine(SlideTo(new Vector2(-visibleOffset, 0), true));
    }

    // 隐藏平板（右滑消失）
    public void HideTablet()
    {
        if (!isVisible) return;
        StartCoroutine(SlideTo(new Vector2(tabletWidth, 0), false));
    }

    private IEnumerator SlideTo(Vector2 targetPos, bool willShow)
    {
        Vector2 startPos = tabletPanel.anchoredPosition;
        float elapsed = 0f;

        // 如果即将显示，先显示遮罩
        if (willShow && overlay != null) overlay.SetActive(true);

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            // 使用平滑曲线
            t = Mathf.SmoothStep(0, 1, t);
            tabletPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        tabletPanel.anchoredPosition = targetPos;
        isVisible = willShow;

        // 如果刚刚隐藏，关闭遮罩
        if (!willShow && overlay != null) overlay.SetActive(false);
    }
}