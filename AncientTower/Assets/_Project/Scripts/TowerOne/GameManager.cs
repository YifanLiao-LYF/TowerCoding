using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private bool hasTriggeredMemory = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMemoryTriggered(bool value)
    {
        hasTriggeredMemory = value;
    }

    public bool GetMemoryTriggered()
    {
        return hasTriggeredMemory;
    }
}