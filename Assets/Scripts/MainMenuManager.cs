using DG.Tweening;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] TweenMover card;
    [SerializeField] SpriteRenderer deck, clover;
    [SerializeField] Image shadow;
    [SerializeField] CanvasGroup gameCanvas;
    [SerializeField] Button startGameButton, quitGameButton;
    [SerializeField] Vector2 cardEndSize = new Vector2(5,5), endShadowOffset;
    [SerializeField] float cardZoomDuration = 1.5f, shadowEndAlpha = 0.3f, cameraEndSize;
    [SerializeField] bool scale;
    [SerializeField] Camera camera;
    [SerializeField] Transform mainMenuObjects;


    public static bool hasPlayed = false;
    public static event Action OnStartGame;
    bool isPlaying;
    Vector2 shadowOffset;
    private async void Start()
    {
        shadowOffset = shadow.transform.localPosition;
        quitGameButton.onClick.AddListener(QuitGame);
        startGameButton.onClick.AddListener(StartGame);

        if (hasPlayed)
        {
            card.transform.localScale = cardEndSize;
            mainMenuObjects.gameObject.SetActive(false);
            deck.DOFade(1, 0.2f);
            gameCanvas.DOFade(1,0.2f);
            clover.DOFade(1,0.2f);
            card.transform.localScale = Vector3.one;
            OnStartGame?.Invoke();
            await Waiter.Seconds(0.5f, destroyCancellationToken);
            if (!Application.isPlaying) return;   // never touch the scene in edit mode
            shadow.transform.DOLocalMove(endShadowOffset, cardZoomDuration);
            await Waiter.Wait(card.transform.DOScale(Vector2.zero, cardZoomDuration), destroyCancellationToken);

        }
    }

    void QuitGame()
    {
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
            gameCanvas.DOFade(1, cardZoomDuration + 0.2f);
            clover.DOFade(1, cardZoomDuration + 0.2f);
            mainMenuObjects.gameObject.SetActive(false);
            OnStartGame?.Invoke();
            shadow.transform.DOLocalMove(shadowOffset, cardZoomDuration);
            await Waiter.Wait(card.transform.DOScale(Vector3.zero, cardZoomDuration), destroyCancellationToken);
            if (!Application.isPlaying) return;   // never touch the scene in edit mode
            card.transform.localScale = Vector3.one;
            card.gameObject.SetActive(false);
        }
        catch (OperationCanceledException) { }

    }
}
