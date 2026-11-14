using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Rendering;

public class ManualVerticalSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("References")]
    [SerializeField] RectTransform handle;
    [SerializeField] RectTransform fill;

    [Header("Handle Y range (anchoredPosition.y)")]
    [SerializeField] float handleYAtZero = 0f;     // value 0 -> this Y
    [SerializeField] float handleYAtOne = 500f;    // value 1 -> this Y

    [Header("Fill top padding")]
    [SerializeField] float topPaddingAtZero = 684f; // value 0 -> 684
    [SerializeField] float topPaddingAtOne = 0f;    // value 1 -> 0

    [SerializeField] TextMeshProUGUI text;
    [SerializeField] bool editChannel;
    [SerializeField] AudioChannel channel;


    [SerializeField, Range(0f, 1f)] float value = 0f;
    public UnityEvent<float> onValueChanged;
    
    RectTransform handleParent;

    void Awake()
    {
        handleParent = (RectTransform)handle.parent;
        SetValue(value, notify: false);
        SetAudio(value);

    }


    void SetAudio(float val)
    {
        if (editChannel)
        {
            AudioManager.SetVolume(channel, val);
        }
        else 
        {
            AudioManager.SetMasterVolume(val);
        }
    }

    public void OnPointerDown(PointerEventData e) => SetFromPointer(e);
    public void OnDrag(PointerEventData e) => SetFromPointer(e);

    void SetFromPointer(PointerEventData e)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                handleParent, e.position, e.pressEventCamera, out Vector2 local)) return;

        // local is relative to the parent's pivot; anchoredPosition is relative to the
        // handle's anchor point, so subtract the anchor's position in parent space.
        float anchorY = Mathf.Lerp(handleParent.rect.yMin, handleParent.rect.yMax, handle.anchorMin.y);
        float anchoredY = local.y - anchorY;

        // Map pointer Y between your two handle Y values to 0..1
        SetValue(Mathf.InverseLerp(handleYAtZero, handleYAtOne, anchoredY));
    }

    public void SetValue(float v, bool notify = true)
    {
        value = Mathf.Clamp01(v);
        text.text = Mathf.RoundToInt(value * 100).ToString();
        // Handle: slide between the two Y values
        float y = Mathf.Lerp(handleYAtZero, handleYAtOne, value);
        handle.anchoredPosition = new Vector2(handle.anchoredPosition.x, y);

        // Fill: top padding goes 684 -> 0 as value goes 0 -> 1
        float top = Mathf.Lerp(topPaddingAtZero, topPaddingAtOne, value);
        fill.offsetMax = new Vector2(fill.offsetMax.x, -top);

        if (notify) SetAudio(value);
    }

    public float Value => value;
}