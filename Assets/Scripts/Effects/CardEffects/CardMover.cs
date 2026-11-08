using DG.Tweening;
using UnityEditor.Tilemaps;
using UnityEngine;

[System.Serializable]
public class MoveSettings
{
    public float duration = 0.3f;
    public Ease ease = Ease.OutBack;
    public float overshoot = 1.7f;
}

public class CardMover : MonoBehaviour
{
    [SerializeField] MoveSettings slotMove = new MoveSettings { duration = 0.35f, overshoot = 1.7f };

    [Header("Draw extras (only used when animateExtras = true)")]
    [SerializeField] float startScale = 0.6f;
    [SerializeField] float startRoll = 25f;       // angle while waiting on the deck
    [SerializeField, Range(0.2f, 1f)] float flipFraction = 0.6f;  // flip duration as a share of move duration

    [Header("Motion tilt (applies to all movement)")]
    [SerializeField] float tiltPerVelocity = 1.5f;  // degrees per unit/sec of horizontal speed
    [SerializeField] float maxTilt = 25f;
    [SerializeField] float rotateSharpness = 20f;   // lower = keeps tilt longer

    [Header("Flip")]
    [SerializeField] Transform visual;              // child holding the sprite(s)
    [SerializeField] SpriteRenderer face;
    [SerializeField] Sprite backSprite;
    [SerializeField] Sprite frontSprite;
    
    Vector3 endScale;

    Vector2 slotPos;
    Vector2 lift;
    float rollTarget;           // resting fan angle
    Vector3 lastPos;
    float rollOffset;     // tweened from (startRoll - rollTarget) to 0
    float baseZ;          // smoothed fan angle + motion tilt
    Tween rollTween;

    Tween moveTween;
    Tween scaleTween;
    Sequence flipTween;

    void OnEnable()
    {
        lastPos = transform.position;
        baseZ = Mathf.DeltaAngle(0f, transform.eulerAngles.z);
    }

    void OnDisable()
    {
        moveTween?.Kill();
        scaleTween?.Kill();
        flipTween?.Kill();
        rollTween?.Kill();
    }

    void Awake() => endScale = visual.localScale;

    public void SetSlot(Vector2 pos, float delay = 0f, float roll = 0f, bool animateExtras = false, bool flip = false)
    {
        slotPos = pos;
        rollTarget = roll;                  // always store, so LateUpdate never snaps cards to 0

        PlayMove(slotMove, delay);

        if (animateExtras)
            PlayExtras(slotMove, delay, flip);
    }

    public void SetLift(Vector2 amount, MoveSettings settings)
    {
        lift = amount;
        PlayMove(settings, 0f);
    }

    void PlayMove(MoveSettings s, float delay)
    {
        moveTween?.Kill();
        Vector2 p = slotPos + lift;
        moveTween = transform.DOMove(new Vector3(p.x, p.y, transform.position.z), s.duration)
            .SetDelay(delay)
            .SetEase(s.ease, s.overshoot)
            .SetLink(gameObject);
    }

    void PlayExtras(MoveSettings s, float delay, bool flip )
    {
        transform.localScale = Vector3.one * startScale;
        if (flip){
            face.sprite = backSprite;
        }
        visual.localScale = endScale;          // restore, so a re-deal doesn't start at 0

        // Start from the current rotation, tween to the fan angle
        baseZ = rollTarget;
        rollOffset = Mathf.DeltaAngle(rollTarget, transform.eulerAngles.z);

        rollTween?.Kill();
        rollTween = DOTween.To(() => rollOffset, v => rollOffset = v, 0f, s.duration)
            .SetDelay(delay)
            .SetEase(s.ease, s.overshoot)
            .SetLink(gameObject);

        scaleTween?.Kill();
        scaleTween = transform.DOScale(1f, s.duration)
            .SetDelay(delay)
            .SetEase(s.ease, s.overshoot)
            .SetLink(gameObject);

        float flipDur = s.duration * flipFraction;
        if (flip)
        {
            flipTween?.Kill();
            flipTween = DOTween.Sequence()
                .SetDelay(delay)
                .Append(visual.DOScaleX(0f, flipDur * 0.5f).SetEase(Ease.InQuad))
                .AppendCallback(() => face.sprite = frontSprite)
                .Append(visual.DOScaleX(endScale.x, flipDur * 0.5f).SetEase(Ease.OutQuad))
                .SetLink(gameObject);
        }
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        float vx = (transform.position.x - lastPos.x) / dt;
        lastPos = transform.position;

        float tilt = Mathf.Clamp(-vx * tiltPerVelocity, -maxTilt, maxTilt);
        baseZ = Mathf.Lerp(baseZ, rollTarget + tilt, 1f - Mathf.Exp(-rotateSharpness * dt));

        transform.rotation = Quaternion.Euler(0f, 0f, baseZ + rollOffset);
    }
}