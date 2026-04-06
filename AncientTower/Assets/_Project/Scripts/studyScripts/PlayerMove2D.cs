using UnityEngine;

public class PlayerMove2D : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float minX = -14f;
    public float maxX = 2f;

    [Tooltip("主角图片初始是否面朝右？如果不是（即面朝左），请取消勾选")]
    public bool defaultFacingRight = true;   // 在 Inspector 中根据场景设置

    private Rigidbody2D rb;
    private float moveX;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");

        if (moveX != 0)
        {
            // 计算是否需要翻转
            // 如果默认面朝右，向左走需翻转；如果默认面朝左，向右走需翻转
            bool needFlip = (defaultFacingRight && moveX < 0) || (!defaultFacingRight && moveX > 0);
            spriteRenderer.flipX = needFlip;
        }
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveX * moveSpeed, 0f);
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        transform.position = p;
    }
}