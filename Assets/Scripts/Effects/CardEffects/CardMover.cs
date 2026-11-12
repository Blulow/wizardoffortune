using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class MoveSettings
{
    public float duration = 0.3f;
    public Ease ease = Ease.OutBack;
    public float overshoot = 1.7f;
}

public class CardMover : TweenMover
{
    [Header("Card sprites")]
    [SerializeField] SpriteRenderer face;
    [SerializeField] Sprite backSprite;
    [SerializeField] public Sprite frontSprite;

    // roll is unused now; it stays so existing positional calls keep compiling.
    // faceTop = true  -> starts showing the back, flips to the front (a deal)
    // faceTop = false -> starts showing the front, flips to the back (discard)
    public Tween SetSlot(Vector2 pos, float delay = 0f, float roll = 0f,
                         bool animateExtras = false, bool flip = false, bool faceTop = true)
    {
        Tween move = MoveTo(pos, delay);

        if (animateExtras)
        {
            Sprite first  = faceTop ? backSprite  : frontSprite;
            Sprite second = faceTop ? frontSprite : backSprite;

            if (flip) face.sprite = first;
            PlayIntro(delay, flip, flip ? () => face.sprite = second : null);
        }

        return move;
    }
}