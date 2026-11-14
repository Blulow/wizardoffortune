using DG.Tweening;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] TweenMover card;
    [SerializeField] SpriteRenderer deck, deckShadow, deckOverlayShadow, clover;
    [SerializeField] Image shadow, cardUnderlay, cardBackground, cardCharacter, cardOverlay, cardOverlayShadow, dialogueBox, dialogueShadow, cardBlocker;
    [SerializeField] GameObject luckbar, goldenChance, failPenalty, luckNumber, sum, playHand;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] RectMask2D mask;
    [SerializeField] Button nextButton;
    [SerializeField] CanvasGroup gameCanvas;
    [SerializeField] Button startGameButton, quitGameButton;
    [SerializeField] Vector2 cardEndSize = new Vector2(5,5), endShadowOffset;
    [SerializeField] float cardZoomDuration = 1.5f, shadowEndAlpha = 0.3f, cameraEndSize;
    [SerializeField] bool scale;
    [SerializeField] Camera camera;
    [SerializeField] Transform mainMenuObjects;
    [SerializeField] MusicManager musicManager;
    [SerializeField] CardManager cardManager;

    public static bool hasPlayed = false;
    public static event Action OnStartGame;
    bool isPlaying;
    Vector2 shadowOffset;
    Coroutine typingCoroutine;
    bool typing = false;
    bool pendingSkip = false;
    int currentDialogue = 0;
    private async void Start()
    {
        gameCanvas.alpha = 0;
        deck.color = new Color(1, 1, 1, 0);
        deckShadow.color = new Color(0, 0, 0, 0);
        deckOverlayShadow.color = new Color(1, 1, 1, 0);
        cardUnderlay.gameObject.SetActive(false);
        cardOverlay.gameObject.SetActive(true);
        cardOverlayShadow.gameObject.SetActive(true);
        shadow.gameObject.SetActive(true);
        cardBackground.gameObject.SetActive(true);
        cardCharacter.gameObject.SetActive(true);
        dialogueBox.gameObject.SetActive(false);
        cardBlocker.gameObject.SetActive(false);

        mask.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2, -67);
        cardCharacter.rectTransform.anchoredPosition = new Vector2(130.921f, -211.1947f);
        RectTransform cardRect = cardCharacter.rectTransform;

        dialogueBox.rectTransform.localScale = new Vector3(1, 1, 1);
        dialogueShadow.rectTransform.localScale = new Vector3(1, 1, 1);
        dialogueText.rectTransform.localScale = new Vector3(1, 1, 1);
        dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 435);
        dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 132);
        dialogueBox.rectTransform.anchoredPosition = new Vector2(310.9737f, 132.7086f);
        dialogueShadow.rectTransform.anchoredPosition = new Vector2(52f, 53.97f);
        nextButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(116, -115);

        nextButton.onClick.AddListener(OnNextButtonClick);

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
            if (!CardManager.tutorial) StartCoroutine(musicManager.SwitchMusic(1, 0.2f, 1.0f));

            await Waiter.Wait(card.transform.DOScale(CardManager.tutorial ? Vector3.one : Vector3.zero, cardZoomDuration), destroyCancellationToken);
            if (CardManager.tutorial)
            {
                PlayTutorial();
            } else
            {
                if (!Application.isPlaying) return;   // never touch the scene in edit mode
                card.transform.localScale = Vector3.one;
                card.gameObject.SetActive(false);
            }
        }
        catch (OperationCanceledException) { }

    }

    async void PlayTutorial()
    {
        cardBlocker.gameObject.SetActive(true);
        cardUnderlay.color = new Color(0, 0, 0, 0);
        cardUnderlay.gameObject.SetActive(true);
        cardUnderlay.DOFade(0.9f, 0.3f);
        cardOverlay.transform.DOLocalMove(new Vector2(2000, 0), 0.3f);
        cardOverlay.transform.DOLocalRotate(new Vector3(0, 0, -30), 0.3f);
        cardBackground.transform.DOLocalMove(new Vector2(2000, 0), 0.3f);
        cardBackground.transform.DOLocalRotate(new Vector3(0, 0, -30), 0.3f);
        cardOverlayShadow.transform.DOLocalMove(new Vector2(2000, 0), 0.3f);
        cardOverlayShadow.transform.DOLocalRotate(new Vector3(0, 0, -30), 0.3f);
        shadow.transform.DOLocalMove(new Vector2(2000, 0), 0.3f);
        shadow.transform.DOLocalRotate(new Vector3(0, 0, -30), 0.3f);

        await Waiter.Seconds(0.5f, destroyCancellationToken);

        PlayDialogue(currentDialogue);
    }

    async Task PlayDialogue(int dialogue)
    {
        dialogueBox.gameObject.SetActive(true);

        switch (dialogue)
        {
            case 0:
                mask.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2, -67);
                cardCharacter.rectTransform.anchoredPosition = new Vector2(130.921f, -211.1947f);
                cardCharacter.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                foreach (Card card in cardManager.handCards)
                {
                    card.SetRenderPriority(4);
                }
                cardBlocker.gameObject.SetActive(true);

                dialogueText.text = "Welcome! Hehee! We'll have lots of fun playing, let me show you around!";
                dialogueBox.rectTransform.localScale = new Vector3(1, 1, 1);
                dialogueShadow.rectTransform.localScale = new Vector3(1, 1, 1);
                dialogueText.rectTransform.localScale = new Vector3(1, 1, 1);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 435);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 132);
                dialogueBox.rectTransform.anchoredPosition = new Vector2(310.9737f, 132.7086f);
                dialogueShadow.rectTransform.anchoredPosition = new Vector2(52f, 53.97f);
                nextButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(116, -115);
                break;
            case 1:
                dialogueBox.gameObject.SetActive(false);
                ResetSortOrder();
                luckbar.transform.SetAsLastSibling();
                foreach (Card card in cardManager.handCards)
                {
                    card.SetRenderPriority(4);
                }
                cardBlocker.gameObject.SetActive(true);

                mask.GetComponent<RectTransform>().anchoredPosition = new Vector2(629, -36);
                cardCharacter.rectTransform.rotation = Quaternion.Euler(0, 0, 58.4f);
                cardCharacter.rectTransform.anchoredPosition = new Vector2(1000, -210);
                await Waiter.Wait(cardCharacter.rectTransform.DOAnchorPos(new Vector2(213, -210), 0.2f), destroyCancellationToken);

                dialogueText.text = "Look at this, this is the luck bar. It stores all your luck from 0 to 100.";
                dialogueBox.rectTransform.localScale = new Vector3(-1, 1, 1);
                dialogueShadow.rectTransform.localScale = new Vector3(-1, 1, 1);
                dialogueText.rectTransform.localScale = new Vector3(-1, 1, 1);
                nextButton.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 460.7363f);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 128.4483f);
                dialogueBox.rectTransform.anchoredPosition = new Vector2(274.1936f, 139.7756f);
                dialogueShadow.rectTransform.anchoredPosition = new Vector2(408.8f, 54.2f);
                nextButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(129f, -118f);
                dialogueBox.gameObject.SetActive(true);
                break;
            case 2:
                dialogueBox.gameObject.SetActive(false);
                ResetSortOrder();
                luckbar.transform.SetAsLastSibling();
                foreach (Card card in cardManager.handCards)
                {
                    card.SetRenderPriority(4);
                }
                cardBlocker.gameObject.SetActive(true);

                mask.GetComponent<RectTransform>().anchoredPosition = new Vector2(629, -36);
                cardCharacter.rectTransform.rotation = Quaternion.Euler(0, 0, 58.4f);
                cardCharacter.rectTransform.anchoredPosition = new Vector2(213, -210);

                dialogueText.text = "When it gets to 0, thats when the game ends. and theres no finish line, so survive as long as possible!";
                dialogueBox.rectTransform.localScale = new Vector3(-1, 1, 1);
                dialogueShadow.rectTransform.localScale = new Vector3(-1, 1, 1);
                dialogueText.rectTransform.localScale = new Vector3(-1, 1, 1);
                nextButton.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 465.5788f);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 159.5786f);
                dialogueBox.rectTransform.anchoredPosition = new Vector2(271.7723f, 155.3408f);
                dialogueShadow.rectTransform.anchoredPosition = new Vector2(413.6f, 53.9f);
                nextButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(132.5f, -122.8f);
                dialogueBox.gameObject.SetActive(true);
                break;
            case 3:
                dialogueBox.gameObject.SetActive(false);
                ResetSortOrder();
                luckbar.transform.SetAsLastSibling();
                goldenChance.transform.SetAsLastSibling();
                foreach (Card card in cardManager.handCards)
                {
                    card.SetRenderPriority(4);
                    if (card.isGold) card.SetRenderPriority(7);
                }
                cardBlocker.gameObject.SetActive(true);

                mask.GetComponent<RectTransform>().DOAnchorPos(new Vector2(629, -140.6f), 0.2f);
                cardCharacter.rectTransform.DOAnchorPos(new Vector2(213, -266.8f), 0.2f);
                cardCharacter.rectTransform.anchoredPosition = new Vector2(213, -210);
                await Waiter.Wait(cardCharacter.rectTransform.DORotate(new Vector3(0, 0, 128.4f), 0.2f), destroyCancellationToken);

                dialogueText.text = "Your luck affects your golden chance, which is the chance you get a golden card (we’ll need that for later)";
                dialogueBox.rectTransform.localScale = new Vector3(-1, 1, 1);
                dialogueShadow.rectTransform.localScale = new Vector3(-1, 1, 1);
                dialogueText.rectTransform.localScale = new Vector3(-1, 1, 1);
                nextButton.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 466.5615f);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 169.4052f);
                dialogueBox.rectTransform.anchoredPosition = new Vector2(314.5087f, -65.0867f);
                dialogueShadow.rectTransform.anchoredPosition = new Vector2(414.7f, 53.9f);
                nextButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(132.5f, 157f);
                dialogueBox.gameObject.SetActive(true);
                break;
            case 4:
                dialogueBox.gameObject.SetActive(false);
                ResetSortOrder();
                luckbar.transform.SetAsLastSibling();
                luckNumber.transform.SetAsLastSibling();
                foreach (Card card in cardManager.handCards)
                {
                    card.SetRenderPriority(7);
                }
                cardBlocker.gameObject.SetActive(true);

                mask.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2, 120);
                cardCharacter.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                cardCharacter.rectTransform.anchoredPosition = new Vector2(131f, -933f);
                await Waiter.Wait(cardCharacter.rectTransform.DOAnchorPos(new Vector2(131f, -333f), 0.2f), destroyCancellationToken);

                dialogueText.text = "Now let’s talk about cards. You can play cards that add up exactly to the lucky number, this will increase your luck by that amount.";
                dialogueBox.rectTransform.localScale = new Vector3(1, 1, 1);
                dialogueShadow.rectTransform.localScale = new Vector3(1, 1, 1);
                dialogueText.rectTransform.localScale = new Vector3(1, 1, 1);
                nextButton.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 546.08f);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 198.65f);
                dialogueBox.rectTransform.anchoredPosition = new Vector2(357f, 221f);
                dialogueShadow.rectTransform.anchoredPosition = new Vector2(52f, 53.97f);
                nextButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(170, -158);
                dialogueBox.gameObject.SetActive(true);
                break;
            case 5:
                dialogueBox.gameObject.SetActive(false);
                ResetSortOrder();
                luckbar.transform.SetAsLastSibling();
                luckNumber.transform.SetAsLastSibling();
                foreach (Card card in cardManager.handCards)
                {
                    card.SetRenderPriority(11);
                }
                cardBlocker.gameObject.SetActive(true);

                mask.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2, 120);
                cardCharacter.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                cardCharacter.rectTransform.anchoredPosition = new Vector2(131f, -333f);

                dialogueText.text = "Try playing cards that add up!";
                dialogueBox.rectTransform.localScale = new Vector3(1, 1, 1);
                dialogueShadow.rectTransform.localScale = new Vector3(1, 1, 1);
                dialogueText.rectTransform.localScale = new Vector3(1, 1, 1);
                nextButton.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 392.99f);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 99.59f);
                dialogueBox.rectTransform.anchoredPosition = new Vector2(280.45f, 171.47f);
                dialogueShadow.rectTransform.anchoredPosition = new Vector2(52f, 53.97f);
                nextButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(95, -107);
                dialogueBox.gameObject.SetActive(true);
                break;
            case 6:
                mask.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-2, -67), 0.5f);
                cardCharacter.rectTransform.DORotate(Vector3.zero, 0.5f);
                cardCharacter.rectTransform.DOAnchorPos(new Vector2(130.921f, -211.1947f), 0.5f);
                foreach (Card card in cardManager.handCards)
                {
                    card.SetRenderPriority(4);
                }
                cardBlocker.gameObject.SetActive(true);

                dialogueText.text = "Oops I think I ran out of time! There's still more about COMBOs and GOLDEN CARDs, you can find how to play them in the Manual. Press Tab to toggle it on and off! Have fun playing! Hehee!";
                dialogueBox.rectTransform.localScale = new Vector3(1, 1, 1);
                dialogueShadow.rectTransform.localScale = new Vector3(1, 1, 1);
                dialogueText.rectTransform.localScale = new Vector3(1, 1, 1);
                nextButton.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 534.07f);
                dialogueBox.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 253.6f);
                dialogueBox.rectTransform.anchoredPosition = new Vector2(346.56f, 193.5f);
                dialogueShadow.rectTransform.anchoredPosition = new Vector2(52f, 53.97f);
                nextButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(162, -169);
                break;
            case 7:
                dialogueBox.gameObject.SetActive(false);

                cardOverlay.transform.DOLocalMove(new Vector2(0, 0), 0.3f);
                cardOverlay.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f);
                cardBackground.transform.DOLocalMove(new Vector2(0, 0), 0.3f);
                cardBackground.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f);
                cardOverlayShadow.transform.DOLocalMove(new Vector2(0, 0), 0.3f);
                cardOverlayShadow.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f);
                shadow.transform.DOLocalMove(new Vector2(0, 0), 0.3f);
                shadow.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f);
                CardManager.tutorial = false;
                await Waiter.Seconds(0.3f, destroyCancellationToken);

                card.transform.DOScale(cardEndSize, cardZoomDuration);
                shadow.DOFade(shadowEndAlpha, cardZoomDuration);
                shadow.transform.DOLocalMove(endShadowOffset, cardZoomDuration);
                StartCoroutine(musicManager.SwitchMusic(2, cardZoomDuration, 0));
                await Waiter.Seconds(cardZoomDuration, destroyCancellationToken);

                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

                return;
        }

        ShowText(dialogueText, dialogueText.text);
    }

    void OnNextButtonClick()
    {
        if (!typing)
        {
            currentDialogue += 1;
            PlayDialogue(currentDialogue);
            pendingSkip = false;
        }
        else
        {
            pendingSkip = true;
        }
    }

    public void ShowText(TextMeshProUGUI textComponent, string fullText)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(textComponent, fullText));
    }

    private IEnumerator TypeText(TextMeshProUGUI textComponent, string fullText)
    {
        typing = true;
        textComponent.text = fullText;
        textComponent.maxVisibleCharacters = 0;
        textComponent.ForceMeshUpdate();

        int totalCharacters = textComponent.textInfo.characterCount;

        for (int i = 0; i <= totalCharacters; i++)
        {
            textComponent.maxVisibleCharacters = i + 1;
            if (pendingSkip || i >= totalCharacters)
            {
                typing = false;
                textComponent.maxVisibleCharacters = totalCharacters;
                pendingSkip = false;
                break;
            }

            float delay;
            switch (textComponent.textInfo.characterInfo[i].character)
            {
                case ',':
                case ';':
                case ':':
                    delay = 0.25f;
                    break;

                case '.':
                    delay = 0.5f;
                    break;

                case '!':
                case '?':
                    delay = 0.5f;
                    break;

                default:
                    delay = 0.04f;
                    break;
            }
            yield return new WaitForSeconds(delay);
        }
    }

    void ResetSortOrder()
    {
        playHand.transform.SetSiblingIndex(1);
        failPenalty.transform.SetSiblingIndex(2);
        luckNumber.transform.SetSiblingIndex(3);
        luckbar.transform.SetSiblingIndex(4);
        goldenChance.transform.SetSiblingIndex(5);
        sum.transform.SetSiblingIndex(6);
        cardUnderlay.transform.SetAsLastSibling();
    }
}
