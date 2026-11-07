using UnityEngine;

public interface IInteractable
{
    int Priority { get; }

    void OnHoverEnter();
    void OnHoverExit();
    void OnClick();
}