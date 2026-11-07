using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] LayerMask interactMask = ~0;

    IInteractable hovered;

    void Update()
    {
        var mouse = Mouse.current;
        Vector2 world = cam.ScreenToWorldPoint(mouse.position.ReadValue());

        UpdateHover(world);

        if (mouse.leftButton.wasPressedThisFrame && IsAlive(hovered))
            hovered.OnClick();
    }

    void UpdateHover(Vector2 world)
    {
        IInteractable top = GetTop(world);
        if (top == hovered) return;

        if (IsAlive(hovered)) hovered.OnHoverExit();
        hovered = top;
        if (hovered != null) hovered.OnHoverEnter();
    }

    IInteractable GetTop(Vector2 point)
    {
        IInteractable best = null;
        int bestPriority = int.MinValue;

        foreach (var col in Physics2D.OverlapPointAll(point, interactMask))
        {
            if (!col.TryGetComponent(out IInteractable i)) continue;
            if (i.Priority > bestPriority) { best = i; bestPriority = i.Priority; }
        }
        return best;
    }

    static bool IsAlive(IInteractable i) => i != null && (i as Object) != null;
}