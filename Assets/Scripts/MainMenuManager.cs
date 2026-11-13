using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] TweenMover card;
    [SerializeField] SpriteRenderer deck, deckShadow, deckOverlayShadow, clover;
    [SerializeField] Image shadow;
    [SerializeField] CanvasGroup gameCanvas;
    [SerializeField] Button startGameButton, quitGameButton;
    [SerializeField] Vector2 cardEndSize = new Vector2(5,5), endShadowOffset;
    [SerializeField] float cardZoomDuration = 1.5f, shadowEndAlpha = 0.3f, cameraEndSize;
    [SerializeField] bool scale;
    [SerializeField] Camera camera;
    [SerializeField] Transform mainMenuObjects;
    [SerializeField] MusicManager musicManager;


    public static bool hasPlayed = false;
    public static event Action OnStartGame;
    bool isPlaying;
    Vector2 shadowOffset;
    private async void Start()
    {
        gameCanvas.alpha = 0;
        deck.color = new Color(1, 1, 1, 0);
        deckShadow.color = new Color(0, 0, 0, 0);
        deckOverlayShadow.color = new Color(1, 1, 1, 0);

        shadowOffset = shadow.transform.localPosition;
        quitGameButton.onClick.AddListener(QuitGame);
        startGameButton.onClick.AddListener(StartGame);

        gameCanvas.blocksRaycasts = false;

        if (hasPlayed)
        {
            shadow.transform.localPosition = endShadowOffset;

            card.transform.localScale = cardEndSize;
            mainMenuObjects.gameObject.SetActive(false);
            deck.DOFade(1, 0.2f);
            deckShadow.DOFade(1, 0.2f);
            deckOverlayShadow.DOFade(1, 0.2f);
            gameCanvas.DOFade(1, 0.2f);
            gameCanvas.blocksRaycasts = true;
            StartCoroutine(musicManager.SwitchMusic(1, 0, 1.0f));

            clover.DOFade(1, 0.2f);
            //card.transform.localScale = Vector3.one;
            OnStartGame?.Invoke();
            await Waiter.Seconds(0.5f, destroyCancellationToken);

            if (!Application.isPlaying) return;   // never touch the scene in edit mode
            shadow.transform.DOLocalMove(shadowOffset, cardZoomDuration);

            await Waiter.Wait(card.transform.DOScale(Vector2.zero, cardZoomDuration), destroyCancellationToken);
        }
        else
        {
            StartCoroutine(musicManager.SwitchMusic(0, 0, 0));
        }
    }

    void QuitGame()
    {
        StartCoroutine(musicManager.SwitchMusic(2, cardZoomDuration, 0));
        LossMenu.QuitGame(card.transform, cardZoomDuration, cardEndSize, shadowOffset, shadow.transform, gameObject, this);

    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        hasPlayed = false;
    }

    async private void StartGame()
    {
        if (isPlaying) return;
        isPlaying = true;
        try
        {
            await Waiter.Wait(card.Shake(), destroyCancellationToken);   // null-safe
            await Waiter.Seconds(0.5f, destroyCancellationToken);
            if (!Application.isPlaying) return;   // never touch the scene in edit mode
            if (scale)
            {
                card.transform.DOScale(cardEndSize, cardZoomDuration);
                shadow.DOFade(shadowEndAlpha, cardZoomDuration);
                shadow.transform.DOLocalMove(endShadowOffset, cardZoomDuration);
            }
            else
            {
                shadow.DOFade(shadowEndAlpha, cardZoomDuration);
                //shadow.transform.DOLocalMove(endShadowOffset, cardZoomDuration);
                camera.DOOrthoSize(cameraEndSize, cardZoomDuration);
            }
            hasPlayed = true;
            await Waiter.Seconds(1f, destroyCancellationToken);
            if (!Application.isPlaying) return;   
            deck.DOFade(1, cardZoomDuration + 0.2f);
            deckShadow.DOFade(1, cardZoomDuration + 0.2f);
            deckOverlayShadow.DOFade(1, cardZoomDuration + 0.2f);
            gameCanvas.DOFade(1, cardZoomDuration + 0.2f);
            gameCanvas.blocksRaycasts = true;

            clover.DOFade(1, cardZoomDuration + 0.2f);
            mainMenuObjects.gameObject.SetActive(false);
            OnStartGame?.Invoke();
            shadow.transform.DOLocalMove(shadowOffset, cardZoomDuration);
            StartCoroutine(musicManager.SwitchMusic(1, 0.2f, 1.0f));
            await Waiter.Wait(card.transform.DOScale(Vector3.zero, cardZoomDuration), destroyCancellationToken);
            if (!Application.isPlaying) return;   // never touch the scene in edit mode
            card.transform.localScale = Vector3.one;
            card.gameObject.SetActive(false);
        }
        catch (OperationCanceledException) { }

    }
}
