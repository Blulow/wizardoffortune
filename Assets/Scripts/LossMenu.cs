using DG.Tweening;
using System;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LossMenu : MonoBehaviour
{

    [SerializeField] Button restartBtn, quitBtn;
    [SerializeField] Image blkBaground, blackout;
    [SerializeField] TweenMover panel;
    [SerializeField] PlayerInteractor playerInteractor;
    [SerializeField] Transform offScreenPanPos, onScreenPanPos;
    void Start()
    {
        quitBtn.onClick.AddListener(QuitGame);
        restartBtn.onClick.AddListener(Restart);
        gameObject.SetActive(false);
    }

    // Make it public so you can link it to a UI Button
    public static void QuitGame()
    {
        // Log a message to verify it works in the Unity Inspector
        Debug.Log("Quit game requested.");

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
            Color color = Color.black;
            color.a = 0f;
            blackout.color = color;
            blackout.gameObject.SetActive(true);
            await Waiter.Wait(blackout.DOFade(1f, 0.8f).SetLink(gameObject),
                                  destroyCancellationToken);
            if (!Application.isPlaying) return;   // never touch the scene in edit mode
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
            panel.MoveTo(onScreenPanPos.position);
        }
        catch (OperationCanceledException) { }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
