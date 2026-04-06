using UnityEngine;
using UnityEngine.EventSystems;

public class TabletOpenButton : MonoBehaviour, IPointerClickHandler
{
    public TabletController tabletController;

    public void OnPointerClick(PointerEventData eventData)
    {
        //ClickRouter2D.IgnoreNextSceneClick();   // ÈÃ ClickRouter2D ºöÂÔ±¾´Îµã»÷
        if (tabletController != null)
            tabletController.ShowTablet();
    }
}