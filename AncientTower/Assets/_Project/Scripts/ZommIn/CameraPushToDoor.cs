using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CameraPushToDoor : MonoBehaviour
{
    [Header("目标点（大门位置）")]
    public Transform target;

    [Header("镜头参数")]
    public float startOrthoSize = 6.5f;
    public float endOrthoSize = 1.7f;
    public float duration = 3f;

    [Header("推进结束后要启用的物体")]
    public GameObject doorTarget;   // 拖入 DoorTarget 物体

    [Header("可选：推进结束事件")]
    public UnityEvent onPushComplete;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) return;

        mainCamera.orthographicSize = startOrthoSize;
        Vector3 startPos = mainCamera.transform.position;
        Vector3 endPos = new Vector3(target.position.x, target.position.y, startPos.z);

        // 开始前确保 DoorTarget 不可点击（禁用 ClickableDialogue 或碰撞体）
        if (doorTarget != null)
        {
            var clickable = doorTarget.GetComponent<ClickableDialogue>();
            if (clickable != null) clickable.enabled = false;
            // 也可以禁用碰撞体，但禁用脚本更简单
        }

        StartCoroutine(ZoomAndMove(startPos, endPos));
    }

    IEnumerator ZoomAndMove(Vector3 startPos, Vector3 endPos)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            mainCamera.orthographicSize = Mathf.Lerp(startOrthoSize, endOrthoSize, t);
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        mainCamera.orthographicSize = endOrthoSize;
        mainCamera.transform.position = endPos;

        // 推进结束：启用 DoorTarget 的可点击能力
        if (doorTarget != null)
        {
            var clickable = doorTarget.GetComponent<ClickableDialogue>();
            if (clickable != null) clickable.enabled = true;
            else Debug.LogWarning("DoorTarget 没有 ClickableDialogue 组件");
        }

        // 触发自定义事件（可选，不再自动对话）
        onPushComplete?.Invoke();
    }
}