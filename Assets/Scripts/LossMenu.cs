using DG.Tweening;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class LossMenu : MonoBehaviour
{

    [SerializeField] Button restartBtn, quitBtn;
    [SerializeField] Image blkBaground;
    [SerializeField] TweenMover panel;
    [SerializeField] PlayerInteractor playerInteractor;
    [SerializeField] Transform offScreenPanPos, onScreenPanPos;
    void Start()
    {
        quitBtn.onClick.AddListener(QuitGame);
        gameObject.SetActive(false);
    }

    // Make it public so you can link it to a UI Button
    public void QuitGame()
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



    async public void InitLossMenu()
    {
        panel.transform.position = offScreenPanPos.position;
        playerInteractor.enabled = false;
        await blkBaground.DOFade(0.95f, 0.2f).AsyncWaitForCompletion();
        panel.MoveTo(onScreenPanPos.position);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
