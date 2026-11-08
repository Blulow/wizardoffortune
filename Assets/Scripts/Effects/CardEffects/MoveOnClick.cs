using UnityEngine;

public class MoveOnClick : MonoBehaviour
{
    [SerializeField] Card card;
    [SerializeField] CardMover mover;

    [SerializeField] Vector2 increaseMovement = new Vector2(0f, 0.5f);
    [SerializeField] MoveSettings select = new MoveSettings { duration = 0.25f, overshoot = 2f };
    [SerializeField] MoveSettings deselect = new MoveSettings { duration = 0.2f, overshoot = 1.5f };

    void OnEnable() => card.Clicked += OnClicked;
    void OnDisable() => card.Clicked -= OnClicked;

    void OnClicked(Card c)
    {
        bool selected = c.IsSelected;
        mover.SetLift(selected ? increaseMovement : Vector2.zero, selected ? select : deselect);
    }
}