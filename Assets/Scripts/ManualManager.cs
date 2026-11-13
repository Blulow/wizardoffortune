using DG.Tweening;
using System.Linq;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ManualManager : MonoBehaviour
{
    [SerializeField] TweenMover[] manuals;
    [SerializeField] float moveDur;
    [SerializeField] Button nextButton, prevButton, closeButton;
    [SerializeField] TextMeshProUGUI pagesText;
    [SerializeField] Transform leftSide, rightSide, middle, manualRootTop, manualRootBottom, manualRootMiddle;
    [SerializeField] TweenMover manualRoot;
    TweenMover showingManual;
    int index = 0;
//<<<<<<< HEAD
//=======
//<<<<<<< Updated upstream
    
//    void TurnPage(bool next)
//=======
//>>>>>>> origin/finalstuff

    bool isShowingManual;
    int turnId;

    int toggleId;
    Tween manualTween;

    [SerializeField] Image backdrop;            // black Image, placed BEHIND manualRoot in the hierarchy
    [SerializeField, Range(0f, 1f)] float backdropAlpha = 0.9f;
    [SerializeField] float backdropFadeTime = 0.25f;
    Tween backdropTween;

    //<<<<<<< HEAD
    //    void Awake()
    //    {
    //        // Start fully clear and not blocking clicks
    //        var c = backdrop.color; c.a = 0f; backdrop.color = c;
    //        backdrop.raycastTarget = false;
    //    }

    //    void FadeBackdrop(bool show)
    //    {
    //        backdropTween?.Kill();
    //        backdrop.raycastTarget = show; // block clicks behind the manual only while it's open

    //        backdropTween = backdrop
    //            .DOFade(show ? backdropAlpha : 0f, backdropFadeTime)
    //            .SetEase(Ease.OutQuad)
    //            .SetLink(backdrop.gameObject);
    //    }

    //    async void ToggleManual()
    //    {
    //        isShowingManual = !isShowingManual;
    //        int myToggle = ++toggleId;
    //        FadeBackdrop(isShowingManual);

    //        // Settled = nothing running, so it's safe to snap to a clean start point
    //        bool inFlight = manualRoot.gameObject.activeSelf && manualTween.IsActive();

    //        Tween tween;
    //        if (isShowingManual)
    //        {
    //            manualRoot.gameObject.SetActive(true);
    //            manualRoot.transform.position = manualRootTop.position; // always enter from the top

    //            tween = manualRoot.MoveTo(manualRootMiddle.position);
    //        }
    //        else
    //        {
    //            if (!inFlight)
    //                manualRoot.transform.position = manualRootMiddle.position;

    //            tween = manualRoot.MoveTo(manualRootBottom.position);
    //        }

    //        manualTween = tween;

    //        try
    //        {
    //            await Waiter.Wait(tween, destroyCancellationToken);
    //        }
    //        catch (OperationCanceledException)
    //        {
    //            return;
    //        }

    //        // A newer toggle took over, so it owns whatever happens next
    //        if (myToggle != toggleId) return;

    //        if (!isShowingManual)
    //            manualRoot.gameObject.SetActive(false);
    //    }

    //    private void Update()
    //    {

    //        if (Keyboard.current.escapeKey.wasPressedThisFrame)
    //        {
    //            ToggleManual();
    //        }
    //    }


    async void TurnPage(bool next)
    {
        SetPage(next);
        int myTurn = ++turnId;

        TweenMover outgoing = showingManual;
        TweenMover incoming = manuals[index];

        // A previously killed tilt may have left a partial rotation
        incoming.transform.rotation = Quaternion.identity;
        incoming.gameObject.SetActive(true);
        incoming.transform.position = next ? leftSide.position : rightSide.position;

        if (outgoing != null && outgoing != incoming)
            outgoing.MoveTo(next ? rightSide.position : leftSide.position);

        Tween tween = incoming.MoveTo(middle.position);

        showingManual = incoming;
        pagesText.text = "(" + (index + 1) + "/" + manuals.Length + ")";

        try
        {
            await Waiter.Wait(tween, destroyCancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        // A newer click took over; it will do the cleanup
        if (myTurn != turnId) return;

        for (int i = 0; i < manuals.Length; i++)
        {
            if (manuals[i] != showingManual)
                manuals[i].gameObject.SetActive(false);
        }
    }
    //=======
    Vector3 idleLocalPos;

    void Start()
    {
        showingManual = manuals[index];
        showingManual?.gameObject.SetActive(true);
        idleLocalPos = showingManual.transform.localPosition; // authored resting spot of the visible page
        pagesText.text = "(" + (index + 1) + "/" + manuals.Length + ")";
    }

    // Deactivating also kills any in-flight page tweens via TweenMover.OnDisable
    void SnapPages()
    {
        turnId++; // cancels cleanup of any in-flight turn

        for (int i = 0; i < manuals.Length; i++)
            manuals[i].gameObject.SetActive(false);

        showingManual.transform.localRotation = Quaternion.identity;
        showingManual.transform.localPosition = idleLocalPos;
        showingManual.gameObject.SetActive(true);
    }

    void Awake()
    {
        // Start fully clear and not blocking clicks
        var c = backdrop.color; c.a = 0f; backdrop.color = c;
        backdrop.raycastTarget = false;
    }

    void FadeBackdrop(bool show)
    {
        backdropTween?.Kill();
        backdrop.raycastTarget = show; // block clicks behind the manual only while it's open

        backdropTween = backdrop
            .DOFade(show ? backdropAlpha : 0f, backdropFadeTime)
            .SetEase(Ease.OutQuad)
            .SetLink(backdrop.gameObject);
    }

    public async void ToggleManual()
    {
        isShowingManual = !isShowingManual;
        int myToggle = ++toggleId;
        FadeBackdrop(isShowingManual);

        // Settled = nothing running, so it's safe to snap to a clean start point
        bool inFlight = manualRoot.gameObject.activeSelf && manualTween.IsActive();

        Tween tween;
        if (isShowingManual)
        {
            manualRoot.gameObject.SetActive(true);
            SnapPages();
            manualRoot.transform.position = manualRootTop.position;
            tween = manualRoot.MoveTo(manualRootMiddle.position);
        }
        else
        {
            if (!inFlight)
                manualRoot.transform.position = manualRootMiddle.position;

            tween = manualRoot.MoveTo(manualRootBottom.position);
        }

        manualTween = tween;

        try
        {
            await Waiter.Wait(tween, destroyCancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        // A newer toggle took over, so it owns whatever happens next
        if (myToggle != toggleId) return;

        if (!isShowingManual)
            manualRoot.gameObject.SetActive(false);
    }

    private void Update()
    {

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleManual();
        }
    }


//    async void TurnPage(bool next)
////>>>>>>> Stashed changes
//    {
//        if (!isShowingManual || manualTween.IsActive()) return; // only turn when settled open
//        SetPage(next);
//        if (showingManual != null) showingManual.SetActive(false);
//        showingManual = manuals[index];
//        showingManual.SetActive(true);
//        pagesText.text = "(" + (index + 1) + "/" + manuals.Count() + ")";
//    }

//<<<<<<< Updated upstream
//>>>>>>> origin/finalstuff
//    private void Start()
//    {
//        showingManual = manuals[index];
//        showingManual?.gameObject.SetActive(true);
//        pagesText.text = "(" + (index + 1) + "/" + manuals.Count() + ")";
//    }
//=======
//>>>>>>> Stashed changes


    private void OnEnable()
    {
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
    }

    private void OnDisable()
    {
        nextButton?.onClick.RemoveListener(NextPage);
        prevButton?.onClick.RemoveListener(PreviousPage);
    }

    void NextPage()
    {
        TurnPage(true);
    }

    void PreviousPage()
    {
        TurnPage(false);
    }

    void SetPage(bool next)
    {
        if (next)
        {
            if (index == manuals.Count() - 1)
            {
                index = 0;
            }
            else
            {
                index++;
            }
        }
        else
        {
            if (index == 0)
            {
                index = manuals.Count() - 1;
            }
            else
            {
                index--;
            }
        }
    }

}
