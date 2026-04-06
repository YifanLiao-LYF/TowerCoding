using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ClickRouter2D : MonoBehaviour
{
    public Camera cam;
    public LayerMask interactMask = ~0;

    [Header("需要屏蔽场景点击的 Canvas（可多选）")]
    public Canvas[] blockCanvases;   // 拖入 OpenTableCanvas 和 MainCanvas

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (cam == null) return;

         //检查鼠标下的 UI 是否属于需要屏蔽的 Canvas
        if (IsPointerOverBlockedCanvas())
        {
             Debug.Log("点击在屏蔽的 UI 上，忽略场景对话");
             return;
         }

        // 正常检测场景物体
        DetectAndTriggerDialogue();
    }

    private bool IsPointerOverBlockedCanvas()
    {
        if (EventSystem.current == null || blockCanvases == null || blockCanvases.Length == 0)
            return false;

        var pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            Transform t = result.gameObject.transform;
            // 向上查找，判断是否属于任一屏蔽 Canvas
            while (t != null)
            {
                Canvas canvas = t.GetComponent<Canvas>();
                if (canvas != null)
                {
                    foreach (var blocked in blockCanvases)
                    {
                        if (canvas == blocked)
                            return true;
                    }
                }
                t = t.parent;
            }
        }
        return false;
    }

    private void DetectAndTriggerDialogue()
    {
        ClickableDialogue best = null;
        int bestPriority = int.MinValue;

        // 2D 检测
        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 point = new Vector2(world.x, world.y);
        Collider2D[] hits2D = Physics2D.OverlapPointAll(point, interactMask);
        foreach (var hit in hits2D)
        {
            var c = hit.GetComponent<ClickableDialogue>();
            if (c != null && c.priority > bestPriority)
            {
                bestPriority = c.priority;
                best = c;
            }
        }

        // 3D 检测
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits3D = Physics.RaycastAll(ray, Mathf.Infinity, interactMask);
        foreach (var hit in hits3D)
        {
            var c = hit.collider.GetComponent<ClickableDialogue>();
            if (c != null && c.priority > bestPriority)
            {
                bestPriority = c.priority;
                best = c;
            }
        }

        if (best != null)
        {
            Debug.Log($"触发对话: {best.name}");
            best.TriggerDialogue();
        }
    }
}