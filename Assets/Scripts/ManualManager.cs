using NUnit.Framework;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManualManager : MonoBehaviour
{
    [SerializeField] GameObject[] manuals;
    [SerializeField] Button nextButton, prevButton, closeButton;
    [SerializeField] TextMeshProUGUI pagesText;
    GameObject showingManual;
    int index = 0;
<<<<<<< Updated upstream
    
    void TurnPage(bool next)
=======

    bool isShowingManual;
    int turnId;

    int toggleId;
    Tween manualTween;

    [SerializeField] Image backdrop;            // black Image, placed BEHIND manualRoot in the hierarchy
    [SerializeField, Range(0f, 1f)] float backdropAlpha = 0.9f;
    [SerializeField] float backdropFadeTime = 0.25f;
    Tween backdropTween;

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

    async void ToggleManual()
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

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleManual();
        }
    }


    async void TurnPage(bool next)
>>>>>>> Stashed changes
    {
        if (!isShowingManual || manualTween.IsActive()) return; // only turn when settled open
        SetPage(next);
        if (showingManual != null) showingManual.SetActive(false);
        showingManual = manuals[index];
        showingManual.SetActive(true);
        pagesText.text = "(" + (index + 1) + "/" + manuals.Count() + ")";
    }

<<<<<<< Updated upstream
    private void Start()
    {
        showingManual = manuals[index];
        showingManual?.SetActive(true);
        pagesText.text = "(" + (index + 1) + "/" + manuals.Count() + ")";
    }
=======
>>>>>>> Stashed changes


    private void OnEnable()
    {
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
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
