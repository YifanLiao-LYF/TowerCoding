using UnityEngine;
using UnityEngine.UI;
using System;

public class LongPressQHandler : MonoBehaviour
{
    [Header("≥§∞¥…Ë÷√")]
    public float requiredHoldTime = 1.5f;
    public Image circleImage;

    private float currentHoldTime = 0f;
    private bool isPressing = false;
    private Action onComplete;

    void Awake()
    {
        if (circleImage == null)
            circleImage = GetComponent<Image>();
        circleImage.fillAmount = 0f;
        gameObject.SetActive(false);
    }

    public void StartLongPress(Action callback)
    {
        onComplete = callback;
        currentHoldTime = 0f;
        isPressing = true;
        gameObject.SetActive(true);
        circleImage.fillAmount = 0f;
    }

    void Update()
    {
        if (!isPressing) return;

        if (Input.GetKey(KeyCode.Q))
        {
            currentHoldTime += Time.deltaTime;
            circleImage.fillAmount = currentHoldTime / requiredHoldTime;

            if (currentHoldTime >= requiredHoldTime)
            {
                isPressing = false;
                gameObject.SetActive(false);
                onComplete?.Invoke();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            isPressing = false;
            gameObject.SetActive(false);
        }
    }
}