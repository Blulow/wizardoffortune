using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class IndependentUIMask : MonoBehaviour
{
    [Tooltip("The child RectTransform inside this mask that should stay locked in place.")]
    public RectTransform maskedObject;

    [Tooltip("Toggle the mask effect on or off.")]
    public bool maskActive = true;

    private Vector3 _lockedWorldPos;
    private Quaternion _lockedWorldRot;
    private bool _hasLockedPosition;
    private RectMask2D _rectMask;
    private Mask _standardMask;

    void OnEnable()
    {
        _rectMask = GetComponent<RectMask2D>();
        _standardMask = GetComponent<Mask>();

        if (maskedObject != null)
        {
            LockCurrentTransform();
        }
    }

    // Call this from another script if you ever intentionally reposition the child
    public void LockCurrentTransform()
    {
        if (maskedObject == null) return;
        _lockedWorldPos = maskedObject.position;
        _lockedWorldRot = maskedObject.rotation;
        _hasLockedPosition = true;
    }

    void LateUpdate()
    {
        // Sync the mask component state with our toggle
        if (_rectMask != null && _rectMask.enabled != maskActive)
            _rectMask.enabled = maskActive;

        if (_standardMask != null && _standardMask.enabled != maskActive)
            _standardMask.enabled = maskActive;

        if (maskedObject == null) return;

        // If you just dragged the child itself in the editor, update its locked spot
        if (!Application.isPlaying && maskedObject.hasChanged && !transform.hasChanged)
        {
            LockCurrentTransform();
        }

        // Keep the child locked in its world position while the mask moves freely
        if (_hasLockedPosition)
        {
            maskedObject.position = _lockedWorldPos;
            maskedObject.rotation = _lockedWorldRot;
        }

        transform.hasChanged = false;
        maskedObject.hasChanged = false;
    }
}
