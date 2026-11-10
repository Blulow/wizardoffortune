using DG.Tweening;
using TMPro;
using UnityEngine;

public class PopMover : TweenMover
{
    [Header("Pop")]
    [SerializeField] Transform[] popTargets;   // empty = pop the root
    [SerializeField] TMP_Text label;
    [SerializeField] float popStagger = 0.06f;
    [SerializeField] float popDuration = 0.25f;
    [SerializeField] float popOvershoot = 1.7f;

    float labelBaseAlpha;

    Vector3 rootScale;
    Vector3[] popBaseScales;
    Sequence popTween;

    protected override void Awake()
    {
        base.Awake();
        rootScale = transform.localScale;   // own copy, so the base's private baseScale stays private
        popBaseScales = new Vector3[popTargets.Length];
        for (int i = 0; i < popTargets.Length; i++)
            popBaseScales[i] = popTargets[i].localScale;
        if (label != null) labelBaseAlpha = label.alpha;
    }

    protected override void AddFadeTweens(Sequence seq, float at, float duration)
    {
        if (label == null) return;
        seq.Insert(at, DOTween.To(() => label.alpha, a => label.alpha = a, 0f, duration)
                              .SetEase(Ease.InQuad));
    }

    protected override void RestoreFade()
    {
        if (label != null) label.alpha = labelBaseAlpha;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        popTween?.Kill();
    }
    public Tween PlayPop(float delay = 0f, string text = null)
    {
        if (label != null && text != null) label.text = text;

        popTween?.Kill();
        popTween = DOTween.Sequence().SetDelay(delay).SetLink(gameObject);

        if (popTargets.Length == 0)
        {
            transform.localScale = Vector3.zero;
            popTween.Append(transform.DOScale(rootScale, popDuration)
                                     .SetEase(Ease.OutBack, popOvershoot));
        }
        else
        {
            for (int i = 0; i < popTargets.Length; i++)
            {
                popTargets[i].localScale = Vector3.zero;
                popTween.Insert(i * popStagger,
                    popTargets[i].DOScale(popBaseScales[i], popDuration)
                                 .SetEase(Ease.OutBack, popOvershoot));
            }
        }

        return popTween;
    }
}