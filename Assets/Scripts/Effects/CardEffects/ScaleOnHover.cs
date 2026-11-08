using UnityEngine;
using DG.Tweening;

public class ScaleOnHover : MonoBehaviour
{
    [SerializeField] Card card;
    [SerializeField] Transform target; // leave empty to use the card's transform

    [Header("Scale")]
    [SerializeField] Vector3 scaleMult = new Vector3(1.05f, 1.05f, 1f);

    [Header("Hover In")]
    [SerializeField] float hoverDuration = 0.25f;
    [SerializeField] Ease hoverEase = Ease.OutBack;
    [SerializeField] float hoverOvershoot = 2.5f;

    [Header("Hover Out")]
    [SerializeField] float exitDuration = 0.2f;
    [SerializeField] Ease exitEase = Ease.OutBack;
    [SerializeField] float exitOvershoot = 1.5f;

    Vector3 baseScale;
    Tween scaleTween;

    void Awake()
    {
        if (target == null) target = card.transform;
        baseScale = target.localScale;
    }

    void OnEnable()
    {
        card.OnHover += OnHoverLogic;
    }

    void OnDisable()
    {
        card.OnHover -= OnHoverLogic;
        scaleTween?.Kill();
        if (target != null) target.localScale = baseScale;
    }

    void OnHoverLogic(bool isHover)
    {
        scaleTween?.Kill();

        Vector3 endScale = isHover
            ? Vector3.Scale(baseScale, scaleMult)
            : baseScale;

        scaleTween = target
            .DOScale(endScale, isHover ? hoverDuration : exitDuration)
            .SetEase(isHover ? hoverEase : exitEase, isHover ? hoverOvershoot : exitOvershoot)
            .SetLink(gameObject); // auto-kills if this object is destroyed
    }
}