using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ScrollRect))]
public class HorizontalPageTurn : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [Header("翻页设置")]
    [SerializeField] private float snapSpeed = 10f;           // 吸附动画速度
    [SerializeField] private float dragThreshold = 30f;       // 拖拽翻页阈值（像素）
    [SerializeField] private float minDragVelocity = 100f;    // 最小滑动速度，超过则强制翻页
    [SerializeField] private bool enableDragThreshold = true;  // 是否启用阈值判断
    
    [Header("页码显示（可选）")]
    [SerializeField] private Text pageText;                   // 显示当前页/总页数
    
    private ScrollRect scrollRect;
    private RectTransform contentRect;
    private RectTransform viewportRect;
    private int totalPages = 0;
    private int currentPage = 0;
    private float pageWidth = 0f;
    private bool isDragging = false;
    private float dragStartPosX = 0f;
    private bool isSnapping = false;
    
    // 存储每一页的位置
    private List<float> pagePositions = new List<float>();
    
    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        contentRect = scrollRect.content;
        viewportRect = scrollRect.viewport;
        
        // 设置 ScrollRect 为横向滑动，禁用弹性
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.inertia = true;
        scrollRect.elasticity = 0;
        
        // 禁用水平滚动条（如果有）
        if (scrollRect.horizontalScrollbar != null)
            scrollRect.horizontalScrollbar.gameObject.SetActive(false);
    }
    
    void Start()
    {
        // 延迟一帧计算，确保布局已经完成
        StartCoroutine(DelayedStart());
    }
    
    IEnumerator DelayedStart()
    {
        yield return null; // 等待一帧
        CalculatePages();
        
        if (totalPages > 0)
        {
            // 强制设置到第一页
            currentPage = 0;
            float targetX = GetTargetPositionX(0);
            contentRect.anchoredPosition = new Vector2(targetX, contentRect.anchoredPosition.y);
            scrollRect.velocity = Vector2.zero;
            
            UpdatePageText();
            
            Debug.Log($"初始化完成 - 总页数: {totalPages}, 第一页位置: {targetX}");
            
            // 打印每页位置用于调试
            for (int i = 0; i < pagePositions.Count; i++)
            {
                Debug.Log($"第{i + 1}页位置: {pagePositions[i]}");
            }
        }
    }
    
    /// <summary>
    /// 计算总页数和每页的位置
    /// </summary>
    private void CalculatePages()
    {
        if (contentRect == null) return;
        
        totalPages = contentRect.childCount;
        pagePositions.Clear();
        
        if (totalPages == 0) return;
        
        // 获取视口宽度（用于判断是否显示完整一页）
        float viewportWidth = viewportRect != null ? viewportRect.rect.width : Screen.width;
        
        // 计算每个页面的位置
        float accumulatedX = 0;
        
        for (int i = 0; i < totalPages; i++)
        {
            RectTransform child = contentRect.GetChild(i) as RectTransform;
            if (child != null)
            {
                // 获取子物体的实际宽度（考虑布局组的影响）
                Canvas.ForceUpdateCanvases();
                float childWidth = child.rect.width;
                
                // 如果是第一页，位置是0
                if (i == 0)
                {
                    pagePositions.Add(0);
                }
                else
                {
                    // 累加前一页的宽度
                    accumulatedX += pagePositions.Count > 0 ? GetPageWidth(i - 1) : childWidth;
                    pagePositions.Add(accumulatedX);
                }
                
                Debug.Log($"第{i + 1}页 - 宽度: {childWidth}, 位置: {pagePositions[i]}");
            }
        }
        
        // 设置页面宽度（用于边界判断）
        if (totalPages > 0)
        {
            pageWidth = GetPageWidth(0);
        }
    }
    
    /// <summary>
    /// 获取指定页面的宽度
    /// </summary>
    private float GetPageWidth(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= totalPages) return 0;
        
        RectTransform child = contentRect.GetChild(pageIndex) as RectTransform;
        if (child != null)
        {
            return child.rect.width;
        }
        return 0;
    }
    
    /// <summary>
    /// 获取指定页面的目标位置 X
    /// </summary>
    private float GetTargetPositionX(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= totalPages) 
            return contentRect.anchoredPosition.x;
        
        // 返回负值，因为 content 向左滑动
        return -pagePositions[pageIndex];
    }
    
    /// <summary>
    /// 根据当前位置获取最近页码
    /// </summary>
    private int GetNearestPageIndex()
    {
        if (totalPages == 0) return 0;
        
        float currentX = contentRect.anchoredPosition.x;
        int nearestPage = 0;
        float minDistance = Mathf.Abs(currentX - GetTargetPositionX(0));
        
        for (int i = 1; i < totalPages; i++)
        {
            float targetX = GetTargetPositionX(i);
            float distance = Mathf.Abs(currentX - targetX);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPage = i;
            }
        }
        
        return nearestPage;
    }
    
    /// <summary>
    /// 检查是否可以翻到上一页
    /// </summary>
    private bool CanGoPrev()
    {
        return currentPage > 0;
    }
    
    /// <summary>
    /// 检查是否可以翻到下一页
    /// </summary>
    private bool CanGoNext()
    {
        return currentPage < totalPages - 1;
    }
    
    /// <summary>
    /// 吸附到指定页面
    /// </summary>
    public void SnapToPage(int pageIndex, bool immediate = false)
    {
        if (isSnapping) return;
        if (pageIndex < 0 || pageIndex >= totalPages) 
        {
            Debug.LogWarning($"页码无效: {pageIndex}, 有效范围: 0-{totalPages - 1}");
            return;
        }
        
        currentPage = pageIndex;
        float targetX = GetTargetPositionX(currentPage);
        
        Debug.Log($"吸附到第{currentPage + 1}页, 目标位置: {targetX}");
        
        if (immediate)
        {
            contentRect.anchoredPosition = new Vector2(targetX, contentRect.anchoredPosition.y);
            scrollRect.velocity = Vector2.zero;
            UpdatePageText();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(SnapCoroutine(targetX));
        }
    }
    
    private IEnumerator SnapCoroutine(float targetX)
    {
        isSnapping = true;
        Vector2 startPos = contentRect.anchoredPosition;
        Vector2 targetPos = new Vector2(targetX, contentRect.anchoredPosition.y);
        
        // 根据距离计算动画时长
        float distance = Mathf.Abs(startPos.x - targetX);
        float duration = Mathf.Clamp(distance / 800f, 0.1f, 0.4f);
        float elapsed = 0;
        
        // 停止惯性
        scrollRect.velocity = Vector2.zero;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0, 1, t);
            
            float newX = Mathf.Lerp(startPos.x, targetX, t);
            contentRect.anchoredPosition = new Vector2(newX, contentRect.anchoredPosition.y);
            
            yield return null;
        }
        
        contentRect.anchoredPosition = targetPos;
        scrollRect.velocity = Vector2.zero;
        isSnapping = false;
        
        UpdatePageText();
    }
    
    /// <summary>
    /// 吸附到最近的页面（惯性停止时调用）
    /// </summary>
    private void SnapToNearestPage()
    {
        if (isSnapping || isDragging) return;
        
        int nearestPage = GetNearestPageIndex();
        
        if (nearestPage != currentPage)
        {
            SnapToPage(nearestPage);
        }
        else
        {
            // 确保完美对齐
            float targetX = GetTargetPositionX(currentPage);
            if (Mathf.Abs(contentRect.anchoredPosition.x - targetX) > 1f)
            {
                SnapToPage(currentPage);
            }
        }
    }
    
    /// <summary>
    /// 下一页
    /// </summary>
    public void NextPage()
    {
        if (CanGoNext())
        {
            SnapToPage(currentPage + 1);
        }
        else
        {
            Debug.Log("已经是最后一页");
        }
    }
    
    /// <summary>
    /// 上一页
    /// </summary>
    public void PrevPage()
    {
        if (CanGoPrev())
        {
            SnapToPage(currentPage - 1);
        }
        else
        {
            Debug.Log("已经是第一页");
        }
    }
    
    /// <summary>
    /// 跳转到指定页（从0开始）
    /// </summary>
    public void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < totalPages)
        {
            SnapToPage(pageIndex);
        }
    }
    
    public int GetCurrentPage()
    {
        return currentPage;
    }
    
    public int GetTotalPages()
    {
        return totalPages;
    }
    
    private void UpdatePageText()
    {
        if (pageText != null)
        {
            pageText.text = $"{currentPage + 1} / {totalPages}";
        }
    }
    
    // ==================== 拖拽事件 ====================
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        isSnapping = false;
        dragStartPosX = contentRect.anchoredPosition.x;
        StopAllCoroutines();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // 可选：添加边界阻尼效果
        float currentX = contentRect.anchoredPosition.x;
        float minX = GetTargetPositionX(totalPages - 1); // 最后一页位置
        float maxX = GetTargetPositionX(0);               // 第一页位置
        
        // 超出边界时增加阻尼
        if (currentX > maxX || currentX < minX)
        {
            // 超出边界时增加阻力，让用户感觉到边界
            float delta = eventData.delta.x;
            float dampenedDelta = delta * 0.5f;
            contentRect.anchoredPosition += new Vector2(dampenedDelta, 0);
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        
        float dragDelta = contentRect.anchoredPosition.x - dragStartPosX;
        float dragVelocity = scrollRect.velocity.x;
        
        Debug.Log($"拖拽结束 - 位移: {dragDelta}, 速度: {dragVelocity}");
        
        // 检查是否超出边界
        float minX = GetTargetPositionX(totalPages - 1);
        float maxX = GetTargetPositionX(0);
        float currentX = contentRect.anchoredPosition.x;
        
        // 如果超出边界太多，直接吸附到边界页
        if (currentX > maxX + pageWidth * 0.3f)
        {
            SnapToPage(0);
            return;
        }
        else if (currentX < minX - pageWidth * 0.3f)
        {
            SnapToPage(totalPages - 1);
            return;
        }
        
        // 快速滑动时，根据方向翻页
        if (Mathf.Abs(dragVelocity) > minDragVelocity)
        {
            if (dragVelocity > 0 && CanGoPrev())
            {
                SnapToPage(currentPage - 1);
                return;
            }
            else if (dragVelocity < 0 && CanGoNext())
            {
                SnapToPage(currentPage + 1);
                return;
            }
        }
        
        // 慢速拖拽，根据拖拽距离判断是否翻页
        if (enableDragThreshold && Mathf.Abs(dragDelta) > dragThreshold)
        {
            if (dragDelta > 0 && CanGoPrev())
            {
                SnapToPage(currentPage - 1);
                return;
            }
            else if (dragDelta < 0 && CanGoNext())
            {
                SnapToPage(currentPage + 1);
                return;
            }
        }
        
        // 否则吸附到最近页面
        SnapToNearestPage();
    }
    
    void Update()
    {
        // 拖拽结束后，惯性停止时自动吸附
        if (!isDragging && !isSnapping && scrollRect.velocity.magnitude < 5f)
        {
            SnapToNearestPage();
        }
    }
    
    // ==================== 编辑器辅助 ====================
    
    #if UNITY_EDITOR
    [ContextMenu("重新计算页面")]
    private void RecalculatePages()
    {
        CalculatePages();
        if (currentPage >= 0 && currentPage < totalPages)
        {
            SnapToPage(currentPage, true);
        }
        Debug.Log($"重新计算完成，共 {totalPages} 页");
    }
    
    [ContextMenu("调试信息")]
    private void DebugInfo()
    {
        Debug.Log($"=== 调试信息 ===");
        Debug.Log($"总页数: {totalPages}");
        Debug.Log($"当前页: {currentPage + 1}");
        Debug.Log($"Content位置: {contentRect.anchoredPosition.x}");
        Debug.Log($"第一页目标位置: {GetTargetPositionX(0)}");
        Debug.Log($"最后一页目标位置: {GetTargetPositionX(totalPages - 1)}");
        Debug.Log($"CanGoPrev: {CanGoPrev()}");
        Debug.Log($"CanGoNext: {CanGoNext()}");
    }
    #endif
}