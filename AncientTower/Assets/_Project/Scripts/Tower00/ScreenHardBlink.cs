using UnityEngine;
using UnityEngine.UI;

public class TimelineScreenBlink : MonoBehaviour
{
    public Image flashImage;

    // 慢速闪烁：0.4秒黑 → 0.4秒亮，循环
    [Header("闪烁间隔，越大越慢")]
    public float blinkInterval = 0.4f;

    private bool _isBlinking;
    private float _timer;

    void Update()
    {
        if (!_isBlinking) return;

        _timer += Time.deltaTime;

        if (_timer >= blinkInterval)
        {
            flashImage.color = flashImage.color.a > 0 
                ? new Color(0, 0, 0, 0) 
                : Color.black;

            _timer = 0;
        }
    }

    // 由 Timeline 信号调用
    public void StartSlowBlink()
    {
        _isBlinking = true;
        _timer = 0;
        flashImage.color = Color.black;
    }

    public void StopBlink()
    {
        _isBlinking = false;
        flashImage.color = new Color(0, 0, 0, 0);
    }
}