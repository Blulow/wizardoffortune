using DG.Tweening;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] TweenMover card;
    [SerializeField] SpriteRenderer shadow, deck, clover;
    [SerializeField] CanvasGroup gameCanvas;
    [SerializeField] Button startGameButton, quitGameButton;
    [SerializeField] Vector2 cardEndSize = new Vector2(5,5), endShadowOffset;
    [SerializeField] float cardZoomDuration = 1.5f, shadowEndAlpha = 0.3f, cameraEndSize;
    [SerializeField] bool scale;
    [SerializeField] Camera camera;
    [SerializeField] Transform mainMenuObjects;
    [SerializeField] Image blackout;


    public static bool hasPlayed;
    public static event Action OnStartGame;
    bool isPlaying;

    private async void Start()
    {
        quitGameButton.onClick.AddListener(LossMenu.QuitGame);
        startGameButton.onClick.AddListener(StartGame);

        if (hasPlayed)
        {
            blackout.gameObject.SetActive(true);
            mainMenuObjects.gameObject.SetActive(false);
            deck.DOFade(1, 0.2f);
            gameCanvas.DOFade(1,0.2f);
            clover.DOFade(1,0.2f);
            Color color = Color.black;
            color.a = 1f;
            blackout.color = color;
            card.transform.localScale = Vector3.one;
            card.gameObject.SetActive(false);
            OnStartGame?.Invoke();
            await blackout.DOFade(0f, 0.8f).AsyncWaitForCompletion();
        }
        else
        {
            blackout.gameObject.SetActive(false);
        }

    }

    async private void StartGame()
    {
        if (isPlaying) return;
        isPlaying = true;
        await card.Shake().AsyncWaitForCompletion();
        await Task.Delay(500);
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
        await Task.Delay(1000);
        deck.DOFade(1, cardZoomDuration + 0.2f);
        gameCanvas.DOFade(1, cardZoomDuration + 0.2f);
        clover.DOFade(1, cardZoomDuration + 0.2f);
        mainMenuObjects.gameObject.SetActive(false);
        OnStartGame?.Invoke();
        await card.transform.DOScale(Vector2.zero, cardZoomDuration).AsyncWaitForCompletion();
        card.transform.localScale = Vector3.one;
        card.gameObject.SetActive(false);
    }
}
