using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

struct CardData
{
    public int id;
    public bool isGold;

    public CardData(int _id, bool _isGold)
    {
        id = _id;
        isGold = _isGold;
    }
}

public class CardManager : MonoBehaviour
{

    public enum FeedBackShow{ Number, Suite, Penalty }

    [SerializeField] Card card;

    List<Card> handCards = new List<Card>();
    List<Card> selectedCards = new List<Card>();
    [SerializeField] int handCardNum = 5;

    //List<Card> playingCards = new List<Card>();
    [SerializeField] Button playHandButton;

    [Header("Spawn / Layout")]
    [SerializeField] Transform deckPos;
    [SerializeField] float staggerDelay = 0.08f;
    [SerializeField] private Vector2 initSpawnPoint = Vector2.zero;
    [SerializeField] private Vector2 spawnOffset = new Vector2(1f, 0f);
    [SerializeField] Transform playMidTargetPos;
    [SerializeField] private Vector3 placementOffset = new Vector3(1f, 0f);
    [SerializeField] Transform offScreenTarget;
    [SerializeField] Vector3 liftOffset = new Vector2(0f, 2f);
    [SerializeField] PopMover sumOut;
    [SerializeField] PopMover clover;
    [SerializeField] Transform cloverSpawn, cloverBarPos, barPos;
    [SerializeField] Sprite clubsSprite, heartsSprite, spadesSprite, diamondsSprite;
    [SerializeField] TweenMover barMover;
    [SerializeField] PopMover luckyNumMover, sumPop;
    [SerializeField] LossMenu lossMenu;
    [SerializeField]
    AudioClip placeCardClip, drawCardClip, luckGainClip, luckLossClip, shakeClip, numbersCombineClip,
        cardOutClip;


    List<PopMover> cloversSpawned = new List<PopMover>();


    Vector2 deckSpawnPoint => deckPos.transform.position;

    Vector3 outSumOriginalPos;

    int luckyNumber;

    int luck;
    int failStreak = 0;

    [SerializeField] int baseLuck = 50;
    [SerializeField] int maxLuck = 100;
    [SerializeField] int reward = 5;
    [SerializeField] int basePenalty = 1;
    [SerializeField] float baseGoldChance = 10;
    [SerializeField] float maxGoldChance = 30;

    [SerializeField] TextMeshProUGUI luckyNumberText, luckText, failPenaltyText, goldenChanceText, sumText;
    [SerializeField] Image luckbarImage;

    List<CardData> pairPool = new List<CardData>();
    List<CardData> straightPool = new List<CardData>();
    List<CardData> flushPool = new List<CardData>();

    [SerializeField] Color failColor, successColor;


    bool isPlaying;


    void Start()
    {
        playHandButton.onClick.AddListener(PlayCardDeck);
        playHandButton.interactable = false;
        playHandButton.GetComponent<OffsetContentOnClick>().OffsetOn();
    }


    private void OnEnable()
    {
        MainMenuManager.OnStartGame += StartGame;
    }

    private void OnDisable()
    {
        MainMenuManager.OnStartGame -= StartGame;
    }



    async void StartGame()
    {
        try
        {
            luck = baseLuck;
            GenerateLuckyNumber(new List<Card>());
            UpdateUI(true);
            sumText.text = "0";
            outSumOriginalPos = sumOut.transform.position;
            await Delay(1f);                       // was Task.Delay(1000)
            DrawHand();
        }
        catch (OperationCanceledException) { }
    }

    Task Delay(float seconds) =>
    Task.Delay(TimeSpan.FromSeconds(seconds), destroyCancellationToken);

    async Task Tw(Tween t)
    {
        await t.AsyncWaitForCompletion();
        destroyCancellationToken.ThrowIfCancellationRequested();
    }

    void ThrowIfStopped()
    {
        destroyCancellationToken.ThrowIfCancellationRequested();
    }




    void GenerateLuckyNumber(List<Card> cards, Combo combo = new Combo())
    {
        luckyNumber = cards.Count > 0 ? combo.baseNumber : UnityEngine.Random.Range(1, 14);
    }


    public void DrawHand()
    {
        //Card spawned = SpawnCard(11, true).GetComponent<Card>();
        //AssignPos(spawned, 0, 0 * staggerDelay);
        //Card spawned2 = SpawnCard(10, false).GetComponent<Card>();
        //AssignPos(spawned2, 1, 1 * staggerDelay);
        //Card spawned3 = SpawnCard(9, false).GetComponent<Card>();
        //AssignPos(spawned3, 2, 2 * staggerDelay);
        //Card spawned4 = SpawnCard(8, false).GetComponent<Card>();
        //AssignPos(spawned4, 3, 3 * staggerDelay);
        //Card spawned5 = SpawnCard(7, false).GetComponent<Card>();
        //AssignPos(spawned5, 4, 4 * staggerDelay);

        for (int i = 0; i < handCardNum; i++)
        {
            Card spawned = SpawnCard().GetComponent<Card>();
            AssignPos(spawned, i, i * staggerDelay);
            AudioManager.Play(drawCardClip, AudioChannel.Sfx, true,i * staggerDelay);
        }
    }

    GameObject SpawnCard(int id = -1, bool? isGold = null)
    {
        Card _card = Instantiate(card, deckSpawnPoint, Quaternion.identity);

        _card.id = id > -1 ? id : GetUniqueId();
        _card.isGold = UnityEngine.Random.Range(0, 100) < GetGoldenChance();

        if ((isGold ?? _card.isGold) && handCards.Count != handCardNum)
        {
            CardData card = GenerateCardFromPools();
            _card.id = card.id;
            _card.isGold = card.isGold;
        }

        _card.InitCard();

        _card.Clicked += HandleCardClicked;
        handCards.Add(_card);
        return _card.gameObject;
    }

    List<Card> GetLuckyNumberCards()
    {
        List<Card> cards = new List<Card>();
        
        foreach (Card card in handCards)
        {
            if (card.number == luckyNumber) cards.Add(card);
        }

        return cards;
    }

    CardData GenerateCardFromPools()
    {
        //if (handCards.Count == handCardNum) return null;

        List<Card> cards = GetLuckyNumberCards();
        Func<int, int, int> mod = (n, m) => ((n % m) + m) % m;
        for (int i = 0; i <= 3; i++) pairPool.Add(new CardData(i * 13 + luckyNumber, true));
        for (int i = luckyNumber - 4; i <= luckyNumber + 4; i++) straightPool.Add(new CardData(UnityEngine.Random.Range(0, 4) * 13 + mod(i - 1, 13), true));
        if (cards.Count > 0)
        {
            for (int c = 0; c < cards.Count; c++)
            {
                for (int i = 0; i < 13; i++) flushPool.Add(new CardData((int)cards[c].suit * 13 + i, true));
            }
        }

        List<List<CardData>> pools = new List<List<CardData>> { pairPool, straightPool, flushPool };
        (List<CardData> pool, CardData card) cardData = GetCardFromPools(pools);

        int j = 0;
        while (j < handCards.Count)
        {
            if (handCards[j].id == cardData.card.id)
            {
                cardData.pool.RemoveAll(e => e.id == cardData.card.id);
                cardData = GetCardFromPools(pools);

                j = 0;
            }
            else j++;
        }

        CardData card = cardData.card;
        return card;
    }

    (List<CardData> pool, CardData card) GetCardFromPools(List<List<CardData>> pools)
    {
        List<List<CardData>> validPools = pools.Where(e => e.Count > 0).ToList();
        List<CardData> pool = validPools[UnityEngine.Random.Range(0, validPools.Count)];
        CardData card = pool[UnityEngine.Random.Range(0, pool.Count)];
        return (pool, card);
    }

    void HandleCardClicked(Card c)
    {
        if (!c.IsSelected)
        {
            selectedCards.Remove(c);
        }
        else
        {
            selectedCards.Add(c);
        }
        AudioManager.Play(cardOutClip, AudioChannel.Sfx);
        playHandButton.interactable = selectedCards.Count > 0;
        playHandButton.GetComponent<OffsetContentOnClick>().SetOffsetOnOff(!playHandButton.interactable);

        int sum = 0;
        foreach (Card card in selectedCards) sum += card.number;
        sumPop.PlayPop(0f, sum.ToString());
    }

    void LayoutHand(int firstNewIndex = int.MaxValue)
    {
        for (int i = 0; i < handCards.Count; i++)
        {
            float delay = i >= firstNewIndex ? (i - firstNewIndex) * staggerDelay : 0f;
            AudioManager.Play(drawCardClip, AudioChannel.Sfx, true, delay);
            handCards[i].Mover.SetSlot(GetSlotPosition(i), delay, 0 ,true, i >= firstNewIndex);
            handCards[i].SetRenderPriority(5 - i);
        }
    }


    void RefillHand()
    {
        int firstNew = handCards.Count;

        while (handCards.Count < handCardNum)
        {
            Card c = SpawnCard().GetComponent<Card>();
            c.transform.position = deckSpawnPoint; // every new card starts at the same spot
        }

        LayoutHand(firstNew);
    }


    async Task GetRidOfPlayedCards(List<Card> cards)
    {
        Tween tween = null;
        for (int i = 0; i < cards.Count; i++)
            tween = cards[i].Mover.SetSlot(offScreenTarget.position, 0.1f * i + 0.5f, 0f, true, true, false);

        if (tween != null) await Tw(tween);        // was tween.AsyncWaitForCompletion()
    }

    async Task ShakeCards(
        List<Card> cards,
        List<Func<Card, TweenMover>> feedbacks,
        Func<Card, TweenMover> mergeFeedback = null,
        int outCome = -1)
    {
        if (cards.Count == 0) return;

        await Delay(0.5f);
        if (!Application.isPlaying) return;   // never touch the scene in edit mode

        if (cards.Count > 1)
        {
            var popups = new List<TweenMover>();
            foreach (Card c in cards)
            {
                foreach (var feedback in feedbacks) feedback(c);

                if (mergeFeedback != null)
                {
                    TweenMover popup = mergeFeedback(c);
                    if (popup != null) popups.Add(popup);
                }
                AudioManager.Play(shakeClip, AudioChannel.Sfx, true);
                await Waiter.Wait(c.Mover.Shake());
                if (!Application.isPlaying) return;   // never touch the scene in edit mode

            }
            if (outCome != 0)
            {
                await MergeObject(popups, outCome);
                if (!Application.isPlaying) return;   // never touch the scene in edit mode

            }
        }
        else
        {
            AudioManager.Play(shakeClip, AudioChannel.Sfx);
            Debug.Log("Outcome: " + outCome);
            Card card = cards[0];
            foreach (var feedback in feedbacks) feedback(card);
            var pop = PopOutcome(outCome);
            await Waiter.Wait(card.Mover.Shake());
            if (!Application.isPlaying) return;   // never touch the scene in edit mode
            await pop;
        }
    }


    async Task HandCardsDelete()
    {
        Tween tween = null;
        foreach (Card c in handCards)
        {
            Vector2 knockDir = UnityEngine.Random.insideUnitCircle.normalized;
            tween = c.GetComponent<CardMover>().ApplyKnockback(0f, knockDir);
        }
        if (tween != null) await Tw(tween);        // was tween.AsyncWaitForCompletion()
    }




    async void PlayCardDeck()
    {
       
        if (isPlaying) return;

        isPlaying = true;
        playHandButton.interactable = false;
        playHandButton.GetComponent<OffsetContentOnClick>().OffsetOn();

        try
        {
            foreach (Card c in handCards)
            {
                c.SetColl(false);
            }
            bool reduceLuck = false;
            bool addLuck = false;
            int combNum = 0;
            var cardsToPlay = new List<Card>(selectedCards);
            var feedbacks = new List<Func<Card, TweenMover>>();   // combo popups
            Func<Card, TweenMover> mergeFeedback = null;          // sum / penalty

            foreach (Card card in cardsToPlay)
                card.CardPlayed();

            // Lucky number sum
            if (CentralMechanics.GetCardSum(cardsToPlay) == luckyNumber)
            {
                addLuck = true;
                combNum = luckyNumber;
                mergeFeedback = c => c.ShowNumber(c.number, successColor);
            }

            // Combo
            Combo combo = CentralMechanics.GetCombo(cardsToPlay);

            if (combo.type == Combo.ComboType.STRT)
                cardsToPlay = SortCircular(cardsToPlay, combo.baseNumber);

            if (combo.type == Combo.ComboType.FLSH)
                feedbacks.Add(c => c.PopInfo(false));
            else if (combo.type != Combo.ComboType.NONE)
                feedbacks.Add(c => c.PopInfo(true));

            if (HasGolden(cardsToPlay) && combo.hasCombo && HasLuckyNumber(cardsToPlay))
            {
                GenerateLuckyNumber(cardsToPlay, combo);
                luckyNumMover.PlayPop(0f, luckyNumber.ToString());
                failStreak = 0;
            }

            await MoveCardsToPlay(cardsToPlay);
            if (!Application.isPlaying) return;   // never touch the scene in edit mode


            // Penalty
            if (CentralMechanics.GetCardSum(cardsToPlay) != luckyNumber && !combo.hasCombo)
            {
                reduceLuck = true;
                int penaltyPerCard = GetPenalty();
                mergeFeedback = c => c.ShowNumber(-penaltyPerCard, failColor);
                combNum = -penaltyPerCard * cardsToPlay.Count;
                failStreak++;
            }


            await ShakeCards(cardsToPlay, feedbacks, mergeFeedback, combNum);
            if (!Application.isPlaying) return;   // never touch the scene in edit mode


            if (addLuck)
            {
                await SpawnClovers(reward);
                if (!Application.isPlaying) return;   // never touch the scene in edit mode


            }
            else if (reduceLuck)
            {
                await DoDamage(combNum);
                if (!Application.isPlaying) return;   // never touch the scene in edit mode

            }



            UpdateUI();


            await GetRidOfPlayedCards(cardsToPlay);
            if (!Application.isPlaying) return;   // never touch the scene in edit mode
            foreach (Card card in cardsToPlay)
            {
                if (card != null)
                {
                    card.Clicked -= HandleCardClicked;
                    card.gameObject.SetActive(false);
                    handCards.Remove(card);
                }
            }

            selectedCards.Clear();
            sumText.text = "0";


            if (luck == 0)
            {
                await HandCardsDelete();
                if (!Application.isPlaying) return;   // never touch the scene in edit mode
                lossMenu.gameObject.SetActive(true);
                lossMenu.InitLossMenu();
            }
            else
            {
                RefillHand();
            }

            foreach (Card c in handCards)
            {
                c.SetColl(true);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            Debug.LogException(e);   // async void would otherwise hide this
        }
        if (!destroyCancellationToken.IsCancellationRequested)
        {
            isPlaying = false;
            playHandButton.interactable = selectedCards.Count > 0;
            playHandButton.GetComponent<OffsetContentOnClick>().SetOffsetOnOff(!playHandButton.interactable);
        }
    }


    async Task DoDamage(int reduceNum)
    {
        Vector3 vector3 = sumOut.transform.position;
        await Waiter.Wait(sumOut.ReachKnockback(barPos.transform.position, 0.5f));
        if (!Application.isPlaying) return;   // never touch the scene in edit mode
        AudioManager.Play(luckLossClip, AudioChannel.Sfx);
        int newLuckAmount = Mathf.Max(0, luck + reduceNum);
        SetValue(luckText, luck, newLuckAmount);

        luck = newLuckAmount;

        float height = Mathf.Clamp01((float)luck / (float)maxLuck) * 684.0f;
        DOVirtual.Float(luckbarImage.GetComponent<RectTransform>().rect.height, height, 0.5f, (value) =>
        {
            luckbarImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
        }).SetEase(Ease.OutQuad);

        Vector3 luckTextPos = luckText.GetComponent<RectTransform>().anchoredPosition;
        luckTextPos.y = height - 684.0f;
        luckText.GetComponent<RectTransform>().DOAnchorPos(luckTextPos, 0.5f);

        barMover.Shake(0f, 10f, 4f);
        await Waiter.Wait(sumOut.ApplyKnockback());
        if (!Application.isPlaying) return;   // never touch the scene in edit mode
        sumOut.transform.position = vector3;
        sumOut.gameObject.SetActive(false);

    }

    public void SetValue(TextMeshProUGUI label,float from, float target, float duration = 0.6f)
    {
        DOVirtual.Float(from, target, duration,
            v => label.text = Mathf.RoundToInt(v).ToString())
            .SetEase(Ease.OutCubic);
    }

    float GetGoldenChance()
    {
        return Mathf.Clamp(baseGoldChance * luck / baseLuck, 0, maxGoldChance);
    }


    async Task MergeObject(List<TweenMover> tweenMovers, int outCome)
    {
        
        await Task.WhenAll(tweenMovers.Select(m =>
            Waiter.Wait(m.ReachKnockback(sumOut.transform.position))));
        if (!Application.isPlaying) return;   // never touch the scene in edit mode

        var tasks = new List<Task>();

        if (tweenMovers.Count > 1)
        {
            AudioManager.Play(numbersCombineClip, AudioChannel.Sfx);
            foreach (TweenMover m in tweenMovers)
                tasks.Add(Waiter.Wait(m.ApplyKnockback()));

        }
        else
        {
            foreach (TweenMover m in tweenMovers)
                tasks.Add(Waiter.Wait(m.PlayFade(0f, 0.3f)));
        }
        tasks.Add(PopOutcome(outCome));  


        await Task.WhenAll(tasks);
    }


    async Task PopOutcome(int outCome)
    {
        sumOut.gameObject.SetActive(true);
        sumOut.GetComponent<TextMeshPro>().color = outCome > 0 ? successColor : failColor;
        Tween tweenMove = sumOut.PlayPop(0f, outCome.ToString());
        await Tw(tweenMove);                       // was tweenMove.AsyncWaitForCompletion()
    }


    int GetPenalty()
    {
        return basePenalty * (failStreak + 1);
    }

    bool HasGolden(List<Card> cards)
    {
        foreach (Card card in cards) if (card.isGold) return true;
        return false;
    }
    bool HasLuckyNumber(List<Card> cards)
    {
        foreach (Card card in cards) if (card.number == luckyNumber) return true;
        return false;
    }

    int GetUniqueId()
    {
        int id;
        do
        {
            id = UnityEngine.Random.Range(0, 52);
        }
        while (ContainsId(handCards, id));
        return id;
    }

    List<Card> SortCircular(List<Card> cards, int baseNumber, int minVal = 1, int maxVal = 13)
    {
        int range = maxVal - minVal + 1;
        int Offset(Card c) => ((c.number - baseNumber) % range + range) % range;

        var result = new List<Card>(cards);
        result.Sort((a, b) => Offset(a).CompareTo(Offset(b)));
        return result;
    }

    Vector2 GetSlotPosition(int index)
    {
        float center = (handCardNum - 1) * 0.5f;
        return initSpawnPoint + spawnOffset * (index - center);
    }


    bool ContainsId(List<Card> cards, int id)
    {
        foreach(Card card in cards) if (card.id == id) return true;
        return false;
    }


    void AssignPos(Card c, int index, float delay = 0f)
    {
        if (c == null || c.Mover == null) return;
        c.transform.position = deckSpawnPoint;                 // everything starts at one spot
        c.Mover.SetSlot(GetSlotPosition(index), delay, 0, true,true);   // then slides to its slot
        c.SetRenderPriority(5 - index);
    }
    void UpdateUI(bool start = false)
    {

        luckyNumberText.text = luckyNumber.ToString();
        luckText.text = luck.ToString();

        if (start)
        {
            float height = Mathf.Clamp01((float)luck / (float)maxLuck) * 684.0f;
            luckbarImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            Vector3 luckTextPos = luckText.GetComponent<RectTransform>().anchoredPosition;
            luckTextPos.y = height - 684.0f;
            luckText.GetComponent<RectTransform>().anchoredPosition = luckTextPos;
        }

        failPenaltyText.text = GetPenalty().ToString();
        goldenChanceText.text = $"{GetGoldenChance():F1}%";
    }
    async Task MoveCardsToPlay(List<Card> cards)
    {
        int n = cards.Count;
        for (int k = 0; k < n; k++)
        {
            Tween newest = null;
            for (int j = 0; j <= k; j++)
            {
                if (cards[j] == null) continue;
                Vector3 pos = playMidTargetPos.position + placementOffset * (j - k * 0.5f);
                newest = cards[j].Mover.SetSlot(pos, 0f, 0f);
                AudioManager.Play(placeCardClip, AudioChannel.Sfx, true);
            }
            if (newest != null) await Waiter.Wait(newest);
        }
    }

    async Task SpawnClovers(int cloverNum)
    {
        ThrowIfStopped();                       
        int toSpawn = cloverNum - cloversSpawned.Count;
        for (int i = 0; i < toSpawn; i++)
        {
            PopMover cloverSpawn = Instantiate(clover, sumOut.transform.position, Quaternion.identity);
            cloversSpawned.Add(cloverSpawn);
            cloverSpawn.gameObject.SetActive(false);
        }

        var tasks = new List<Task>();

        List<PopMover> popMovers = new List<PopMover>();

        for (int i = 0; i < cloverNum; i++)
        {
            PopMover c = cloversSpawned[i];  
            popMovers.Add(c);
            c.gameObject.SetActive(false);
            c.transform.position = sumOut.transform.position;
            c.gameObject.SetActive(true);
            tasks.Add(Waiter.Wait(c.PlayPop(i * 0.1f)));
            tasks.Add(Waiter.Wait(c.MoveTo(cloverSpawn.position, i*0.1f)));
        }

        await Task.WhenAll(tasks);
        if (!Application.isPlaying) return;   // never touch the scene in edit mode

        await Delay(0.25f);
        if (!Application.isPlaying) return;   // never touch the scene in edit mode

        int index = 0;

        foreach (PopMover pop in popMovers)
        {

            AudioManager.Play(luckGainClip, AudioChannel.Sfx, false,(luckGainClip.length * 0.2f)* index);
            index++;
            await Waiter.Wait(pop.MoveTo(cloverBarPos.position));
            if (!Application.isPlaying) return;   // never touch the scene in edit mode
            luck = Mathf.Min(maxLuck, luck + 1);
            luckText.text = luck.ToString();

            float height = Mathf.Clamp01((float)luck / (float)maxLuck) * 684.0f;
            DOVirtual.Float(luckbarImage.GetComponent<RectTransform>().rect.height, height, 0.5f, (value) =>
            {
                luckbarImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
            }).SetEase(Ease.OutQuad);

            Vector3 luckTextPos = luckText.GetComponent<RectTransform>().anchoredPosition;
            luckTextPos.y = height - 684.0f;
            luckText.GetComponent<RectTransform>().DOAnchorPos(luckTextPos, 0.5f);

            barMover.Shake(0f, 0.3f, 1f);
            pop.gameObject.SetActive(false);
        }

        sumOut.gameObject.SetActive(false);
    }

}
