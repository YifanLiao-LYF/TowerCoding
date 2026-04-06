using UnityEngine;

public class TowerSceneInit : MonoBehaviour
{
    void Start()
    {
        // 进入塔内场景时，重置记忆标志为 false
        DialogueTreeManager.HasTriggeredMemory = false;
        Debug.Log("塔内场景：记忆标志已重置为 false");
    }
}