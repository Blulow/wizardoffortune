using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OffsetContentOnClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Vector2 offset = Vector2.zero;
    [SerializeField] AudioClip clip;

    bool isOn = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        OffsetOn();
    }

    void PlayAudio()
    {
        AudioManager.Play(clip, AudioChannel.Sfx);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!GetComponent<Button>().interactable) return;

        OffsetOff();
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

        isOn = true;
    }

    public void OffsetOff()
    {
        if (!isOn) return;

        foreach (Transform c in transform)
        {
            c.position = (Vector2)c.transform.position - offset;
        }
        
        isOn = false;
    }
    private void OnEnable()
    {
        GetComponent<Button>().onClick.AddListener(PlayAudio);
    }
}
