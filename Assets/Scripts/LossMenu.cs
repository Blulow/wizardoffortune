using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LossMenu : MonoBehaviour
{

    [SerializeField] Button restartBtn, quitBtn;
    [SerializeField] Image blkBaground;
    [SerializeField] Transform card, shadow;
    [SerializeField] TweenMover panel;
    [SerializeField] PlayerInteractor playerInteractor;
    [SerializeField] Transform offScreenPanPos, onScreenPanPos;
    [SerializeField] float cardZoomDuration = 1.5f;
    [SerializeField] Vector2 cardEndSize, shadowOffset;
    [SerializeField] MusicManager musicManager;

    Vector2 initOffset;

    void Start()
    {
        initOffset = shadow.transform.localPosition;
        quitBtn.onClick.AddListener(CallQuit);
        restartBtn.onClick.AddListener(Restart);
        gameObject.SetActive(false);
    }

    // Make it public so you can link it to a UI Button


    void CallQuit()
    {
        StartCoroutine(musicManager.SwitchMusic(2, cardZoomDuration, 0));
        QuitGame(card, cardZoomDuration, cardEndSize, shadowOffset,shadow,gameObject, this);
    }

    public async static void QuitGame(Transform card, float cardZoomDuration, Vector2 cardEndSize,Vector2 shadowOffset,Transform shadow ,GameObject gameObject, MonoBehaviour mono)
    {
        shadow.transform.DOLocalMove(shadowOffset, cardZoomDuration);
        await Waiter.Wait(card.DOScale(cardEndSize, cardZoomDuration).SetLink(gameObject),
                      mono.destroyCancellationToken);
        if (!Application.isPlaying) return;

#if UNITY_EDITOR
        // This line stops Play Mode if you are testing inside the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // This line closes the actual built game (Windows, Mac, Android, etc.)
            Application.Quit();
#endif
    }


    async void Restart()
    {
        try
        {
            card.gameObject.SetActive(true);
            shadow.transform.DOLocalMove(shadowOffset, cardZoomDuration);
            await Waiter.Wait(card.DOScale(cardEndSize,cardZoomDuration).SetLink(gameObject),
                                  destroyCancellationToken);
            if (!Application.isPlaying) return;  
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        catch (OperationCanceledException) { }
    }

    public async void InitLossMenu()
    {
        try
        {
            panel.transform.position = offScreenPanPos.position;
            playerInteractor.enabled = false;
            await Waiter.Wait(blkBaground.DOFade(0.95f, 0.2f).SetLink(gameObject),
                                  destroyCancellationToken);
            if (!Application.isPlaying) { return; }
            panel.MoveTo(onScreenPanPos.position);
        }
        catch (OperationCanceledException) { }
    }

}
