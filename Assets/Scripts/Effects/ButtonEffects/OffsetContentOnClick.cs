using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OffsetContentOnClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Vector2 offset = Vector2.zero;

    bool isOn = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        OffsetOn();
        Debug.Log("on " + offset);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!GetComponent<Button>().interactable) return;

        OffsetOff();
        Debug.Log("off " + offset);
    }

    public void SetOffsetOnOff(bool offset)
    {
        if (offset) OffsetOn();
        else OffsetOff();
    }

    public void OffsetOn()
    {
        if (isOn) return;

        foreach (Transform c in transform)
        {
            c.position = (Vector2)c.transform.position + offset;
        }
        Debug.Log("on2 " + offset);

        isOn = true;
    }

    public void OffsetOff()
    {
        if (!isOn) return;

        foreach (Transform c in transform)
        {
            c.position = (Vector2)c.transform.position - offset;
        }
        Debug.Log("off2 " + offset);
        
        isOn = false;
    }
}
