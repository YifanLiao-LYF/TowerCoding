using UnityEngine;

public class ClickRouter2D : MonoBehaviour
{
    public Camera cam;
    public LayerMask interactMask = ~0; // 默认全部层

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (cam == null) return;

        Debug.Log("ClickRouter2D: 鼠标点击");

        // 准备存储所有候选物体
        ClickableDialogue best = null;
        int bestPriority = int.MinValue;

        // 1. 2D 检测（原有逻辑）
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

        // 2. 3D 检测（新增）
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
        else
        {
            Debug.Log("未找到可点击的物体");
        }
    }
}