using UnityEngine;

public class DragHandler : MonoBehaviour
{
    [Header("配置")]
    public WoodenBoxController controller;
    public string partType;
    public Vector3 dragDirection;      // 局部移动方向（如 (0,1,0) 向上，(0,0,1) 向前）
    public float dragDistance = 0.5f;   // 最大移动距离
    public GameObject visual;

    [Header("移动限制")]
    public bool allowNegativeMove = true; // 是否允许向 dragDirection 反方向移动
    public bool invertMove = false;       // 是否翻转移动方向

    private bool isDragging = false;
    private bool dragStartedThisFrame = false;
    private Vector3 startPos;
    private Camera dragCamera;
    private Vector3 lastMousePos;

    void Start()
    {
        if (visual == null) visual = gameObject;
    }

    public void StartDrag(Camera uiCamera)
    {
        if (controller == null || !controller.CanPull(partType)) return;
        dragCamera = uiCamera;
        isDragging = true;
        dragStartedThisFrame = true;
        startPos = visual.transform.localPosition;
        lastMousePos = Input.mousePosition;
    }

    void Update()
    {
        // 鼠标抬起结束拖拽
        if (Input.GetMouseButtonUp(0) && isDragging && !dragStartedThisFrame)
        {
            EndDrag();
        }
        if (dragStartedThisFrame) dragStartedThisFrame = false;

        if (isDragging)
        {
            if (dragCamera == null) return;

            // 1. 计算鼠标屏幕移动增量
            Vector3 delta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            // 2. 根据移动方向的主轴选择使用 delta.x 还是 delta.y
            float inputDelta;
            if (Mathf.Abs(dragDirection.x) > 0.5f)          // 水平移动（左右）
                inputDelta = delta.x;
            else                                             // 垂直移动（上下/前后）
                inputDelta = delta.y;

            // 3. 翻转
            if (invertMove) inputDelta = -inputDelta;

            // 4. 速度
            float move = inputDelta * Time.deltaTime * 200f;

            // 5. 如果不允许反向移动且移动量为负，则禁止
            if (!allowNegativeMove && move < 0)
                move = 0;

            // 6. 计算新位置
            Vector3 newPos = visual.transform.localPosition + dragDirection * move;

            // 7. 计算沿移动方向的投影距离，并限制在 [0, dragDistance]
            float distance = Vector3.Dot(newPos - startPos, dragDirection);
            distance = Mathf.Clamp(distance, 0, dragDistance);
            newPos = startPos + dragDirection * distance;

            // 8. 应用位置
            visual.transform.localPosition = newPos;
        }
    }

    void EndDrag()
    {
        isDragging = false;

        Vector3 currentLocalPos = visual.transform.localPosition;
        float moved = Vector3.Dot(currentLocalPos - startPos, dragDirection);

        if (Mathf.Abs(moved) >= dragDistance * 0.01f)
        {
            visual.transform.localPosition = startPos + dragDirection * dragDistance;
            controller.OnPartPulled(partType);
            enabled = false; // 防止二次拖拽
            Debug.Log($"拖拽成功，移动距离：{moved}");
        }
        else
        {
            // 失败：回弹到起始位置
            visual.transform.localPosition = startPos;
            Debug.Log($"拖拽失败，移动距离不足：{moved}");
        }
    }
}