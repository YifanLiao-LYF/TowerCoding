using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public GameObject target;
    public float rotateSpeed = 5f;
    private bool isRotating = false;
    private Vector2 lastMousePos;
    private Vector3 targetCenter;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (target != null)
        {
            Bounds bounds = new Bounds(target.transform.position, Vector3.zero);
            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }
            targetCenter = bounds.center;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

            bool hasDraggable = false;
            foreach (var hit in hits)
            {
            }

            if (hasDraggable)
            {
                // 鼠标下有点击到可拖拽物体，让 Unity 自动处理，不启动旋转
                return;
            }

            // 没有点击到任何可拖拽物体，开始旋转
            isRotating = true;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }

        if (isRotating)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            float xRot = delta.y * rotateSpeed * Time.deltaTime;
            float yRot = delta.x * rotateSpeed * Time.deltaTime;

            target.transform.RotateAround(targetCenter, Vector3.up, -yRot);
            target.transform.RotateAround(targetCenter, Vector3.right, xRot);
        }
    }
}