using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;



public class TweenMover : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] protected MoveSettings slotMove = new MoveSettings { duration = 0.35f, overshoot = 1.7f };

    [Header("Motion tilt")]
    [SerializeField] float tiltPerUnit = 3f;   // degrees per world unit of horizontal travel
    [SerializeField] float maxTilt = 15f;

    [Header("Intro")]
    [SerializeField] Transform pinchTarget;    // child to pinch on X; leave empty to disable pinching
    [SerializeField] float startScale = 0.6f;
    [SerializeField, Range(0.2f, 1f)] float pinchFraction = 0.6f;

    [Header("Shake")]
    [SerializeField] float shakeDuration = 0.3f;

    protected Vector2 slotPos, lift;
    Vector3 baseScale, pinchBaseScale;

    Tween moveTween, tiltTween, scaleTween, shakeTween;
    Sequence pinchTween;

    protected virtual void Awake()
    {
        baseScale = transform.localScale;
        if (pinchTarget != null) pinchBaseScale = pinchTarget.localScale;
        slotPos = transform.position;
    }

    protected virtual void OnDisable()
    {
        moveTween?.Kill();
        tiltTween?.Kill();
        scaleTween?.Kill();
        shakeTween?.Kill();
        pinchTween?.Kill();
    }

    // ---------- Move ----------

    public Tween MoveTo(Vector2 pos, float delay = 0f)
    {
        slotPos = pos;
        return PlayMove(slotMove, delay);
    }

    public void SetLift(Vector2 amount, MoveSettings settings)
    {
        lift = amount;
        PlayMove(settings, 0f);
    }

    Tween PlayMove(MoveSettings s, float delay)
    {
        if (this == null) return null;
        shakeTween?.Kill(true);
        moveTween?.Kill();
        tiltTween?.Kill();

        Vector2 p = slotPos + lift;
        var target = new Vector3(p.x, p.y, transform.position.z);

        moveTween = transform.DOMove(target, s.duration)
            .SetDelay(delay)
            .SetEase(s.ease, s.overshoot)
            .SetLink(gameObject);

        float peak = Mathf.Clamp(-(p.x - transform.position.x) * tiltPerUnit, -maxTilt, maxTilt);

        tiltTween = DOTween.Sequence()
            .SetDelay(delay)
            .Append(transform.DORotate(new Vector3(0f, 0f, peak), s.duration * 0.35f).SetEase(Ease.OutQuad))
            .Append(transform.DORotate(Vector3.zero, s.duration * 0.65f).SetEase(Ease.OutBack))
            .SetLink(gameObject);

        return moveTween;
    }

    // ---------- Intro (scale-in + optional pinch) ----------

    public Tween PlayIntro(float delay = 0f, bool pinch = false, Action onMid = null)
    {
        // Start state, applied immediately so the object waits small until its turn
        if (this == null) return null;
        transform.localScale = baseScale * startScale;

        scaleTween?.Kill();
        scaleTween = transform.DOScale(baseScale, slotMove.duration)
            .SetDelay(delay)
            .SetEase(slotMove.ease, slotMove.overshoot)
            .SetLink(gameObject);

        if (pinch && pinchTarget != null)
        {
            pinchTween?.Kill();
            pinchTarget.localScale = pinchBaseScale;   // restore, so a re-deal doesn't start at 0

            float dur = slotMove.duration * pinchFraction;
            pinchTween = DOTween.Sequence().SetDelay(delay).SetLink(gameObject);
            pinchTween.Append(pinchTarget.DOScaleX(0f, dur * 0.5f).SetEase(Ease.InQuad));
            if (onMid != null) pinchTween.AppendCallback(() => onMid());
            pinchTween.Append(pinchTarget.DOScaleX(pinchBaseScale.x, dur * 0.5f).SetEase(Ease.OutQuad));
        }

        return scaleTween;
    }

    // ---------- Shake ----------

    // Returns null if the object has a pending or running move
    public Tween Shake(float delay = 0f, float strength = 0.15f, float rotStrength = 6f)
    {
        if (moveTween.IsActive()) return null;

        shakeTween?.Kill(true);
        shakeTween = DOTween.Sequence()
            .SetDelay(delay)
            .Join(transform.DOShakePosition(shakeDuration, new Vector3(strength, strength, 0f), 25, 90f, false, true))
            .Join(transform.DOShakeRotation(shakeDuration, new Vector3(0f, 0f, rotStrength), 18, 90f, true))
            .SetLink(gameObject);

        return shakeTween;
    }

    // ---------- Await helper ----------

    // Safe on null or already-finished tweens
    public static async Task Wait(Tween t)
    {
        if (t != null && t.IsActive()) await t.AsyncWaitForCompletion();
    }



}