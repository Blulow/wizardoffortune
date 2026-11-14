using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ToggleWindow : MonoBehaviour
{
    [SerializeField] TweenMover windowRoot;
    [SerializeField] Transform onScreen, top, bottom;
    [SerializeField] Image background;
    [SerializeField, Range(0f, 1f)] float finalBackgroundAlpha = 0.9f;
    [SerializeField] float backgroundFadeTime = 0.25f;
    [SerializeField] Button showButton, hideButton;
    bool isOnScreen;
    int toggleId;
    Tween windowTween, backgroundTween;

    public bool IsOnScreen => isOnScreen;

    void Start()
    {


        if (showButton != null) { showButton.onClick.AddListener(Show); }

        if (hideButton != null) { hideButton.onClick.AddListener(Hide); }

        Color bgColor = Color.black;
        bgColor.a = 0f;
        background.color = bgColor;
        background.raycastTarget = false;

        windowRoot.transform.position = top.position;
        windowRoot.gameObject.SetActive(false);

    }

    public void Toggle() => SetShow(!isOnScreen);



    public void Show() => SetShow(true);

    void Hide() => SetShow(false);

    public async void SetShow(bool value)
    {
        if (value == isOnScreen) return; // already heading to / at that state

        isOnScreen = value;
        int myToggle = ++toggleId;

        FadeBackground(value);

        // A move is running, so closing can continue from the current position without a pop
        bool inFlight = windowRoot.gameObject.activeSelf && windowTween.IsActive();

        Tween tween;
        if (value)
        {
            windowRoot.gameObject.SetActive(true);
            windowRoot.transform.position = top.position; // always enter from the top
            tween = windowRoot.MoveTo(onScreen.position);
        }
        else
        {
            if (!inFlight)
                windowRoot.transform.position = onScreen.position;
            tween = windowRoot.MoveTo(bottom.position);
        }

        windowTween = tween;

        try
        {
            await Waiter.Wait(tween, destroyCancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        // A newer toggle took over, so it owns what happens next
        if (myToggle != toggleId) return;

        if (!isOnScreen)
            windowRoot.gameObject.SetActive(false);
    }

    void FadeBackground(bool show)
    {
        backgroundTween?.Kill();
        background.raycastTarget = show;

        backgroundTween = background
            .DOFade(show ? finalBackgroundAlpha : 0f, backgroundFadeTime)
            .SetEase(Ease.OutQuad)
            .SetLink(background.gameObject);
    }
}