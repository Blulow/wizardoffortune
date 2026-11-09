using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OffsetContentOnClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Vector2 offset = Vector2.zero;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GetComponent<Button>().interactable) return;

        foreach (Transform c in transform)
        {
            c.position = (Vector2)c.transform.position + offset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!GetComponent<Button>().interactable) return;
        
        foreach (Transform c in transform)
        {
            c.position = (Vector2)c.transform.position - offset;
        }
    }
}
