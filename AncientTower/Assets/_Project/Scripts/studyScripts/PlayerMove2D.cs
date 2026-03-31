using UnityEngine;

public class PlayerMove2D : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float minX = -14f;   // ×ó±ß½ç
    public float maxX = 2f;   // ÓÒ±ß½ç
    private Rigidbody2D rb;
    private float moveX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal"); // A/D »ò ×óÓÒ
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveX * moveSpeed, 0f);
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        transform.position = p;
    }
}