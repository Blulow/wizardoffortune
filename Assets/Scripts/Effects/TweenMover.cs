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

    [Header("Knockback")]
    [SerializeField] float knockDistance = 0.6f;
    [SerializeField] float knockTime = 0.12f;
    [SerializeField] float fallDistance = 3f;
    [SerializeField] float fallTime = 0.5f;
    [SerializeField] float knockSpin = 90f;
    [SerializeField] float stopShort = 0.3f;
    Sequence fadeTween;


    Vector2 knockDir = Vector2.up;
    Sequence knockTween;
    SpriteRenderer[] fadeSprites;
    float[] spriteBaseAlpha;

    protected Vector2 slotPos, lift;
    Vector3 baseScale, pinchBaseScale;

    Tween moveTween, tiltTween, scaleTween, shakeTween;
    Sequence pinchTween;

    protected virtual void Awake()
    {
        baseScale = transform.localScale;
        if (pinchTarget != null) pinchBaseScale = pinchTarget.localScale;
        slotPos = transform.position;
        fadeSprites = GetComponentsInChildren<SpriteRenderer>(true);
        spriteBaseAlpha = new float[fadeSprites.Length];
        for (int i = 0; i < fadeSprites.Length; i++) spriteBaseAlpha[i] = fadeSprites[i].color.a;
    }


    protected virtual void AddFadeTweens(Sequence seq, float at, float duration) { }
    protected virtual void RestoreFade() { }


    public Tween ReachKnockback(Vector2 target, float delay = 0f)
    {
        Vector2 toTarget = target - (Vector2)transform.position;
        knockDir = toTarget.sqrMagnitude > 0.0001f ? -toTarget.normalized : Vector2.up;

        slotPos = target + knockDir * stopShort;
        return PlayMove(slotMove, delay);
    }


    // ---------- Fade ----------

    // Fades all sprites (and anything the subclass adds via AddFadeTweens) to zero alpha.
    public Tween PlayFade(float delay = 0f, float duration = 0.5f)
    {
        if (this == null) return null;

        fadeTween?.Kill();
        fadeTween = DOTween.Sequence().SetDelay(delay).SetLink(gameObject);

        foreach (var s in fadeSprites)
            fadeTween.Join(s.DOFade(0f, duration).SetEase(Ease.InQuad));

        AddFadeTweens(fadeTween, 0f, duration);

        return fadeTween;
    }

    public void ResetFade()
    {
        fadeTween?.Kill();
        for (int i = 0; i < fadeSprites.Length; i++)
        {
            var c = fadeSprites[i].color; c.a = spriteBaseAlpha[i]; fadeSprites[i].color = c;
        }
        RestoreFade();
    }

    public Tween ApplyKnockback(float delay = 0f)
    {
        if (this == null) return null;

        moveTween?.Kill();
        tiltTween?.Kill();
        shakeTween?.Kill();
        knockTween?.Kill();

        Vector3 hit = transform.position + (Vector3)(knockDir * knockDistance);
        Vector3 end = hit + Vector3.down * fallDistance;
        float spin = (knockDir.x >= 0f ? -1f : 1f) * knockSpin;

        knockTween = DOTween.Sequence().SetDelay(delay).SetLink(gameObject);

        knockTween.Append(transform.DOMove(hit, knockTime).SetEase(Ease.OutQuad));
        knockTween.Join(transform.DORotate(new Vector3(0f, 0f, spin * 0.3f), knockTime));

        knockTween.Append(transform.DOMove(end, fallTime).SetEase(Ease.InQuad));
        knockTween.Join(transform.DORotate(new Vector3(0f, 0f, spin), fallTime));

        // Fade starts when the fall starts
        knockTween.Insert(knockTime, PlayFade(0f, fallTime));

        knockTween.OnComplete(() =>
        {
            transform.rotation = Quaternion.identity;
            ResetFade();
            gameObject.SetActive(false);
        });

        return knockTween;
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