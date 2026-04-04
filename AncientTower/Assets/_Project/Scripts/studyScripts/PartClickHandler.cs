using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PartClickHandler : MonoBehaviour
{
    [Tooltip("零件类型，如001，必须与stepOrder一致")]
    public string partType;

    [Tooltip("移动方向（相对于零件的局部坐标系）")]
    public Vector3 pullDirection = new Vector3(0, 1, 0);

    [Tooltip("移动距离")]
    public float pullDistance = 1.5f;

    private WoodenBoxController boxController;

    void Start()
    {
        boxController = FindObjectOfType<WoodenBoxController>();
        if (boxController == null)
            Debug.LogError("场景中没有WoodenBoxController，请确保盒子物体上有该脚本");
    }

    void OnMouseDown()
    {
        Debug.Log($"PartClickHandler: 点击了零件 {partType}");
        if (boxController != null)
            boxController.OnPartClicked(partType);
        else
            Debug.LogError("未找到 WoodenBoxController");
    }
}