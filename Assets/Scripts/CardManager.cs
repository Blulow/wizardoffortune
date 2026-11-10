using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.GPUSort;

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


    [SerializeField] Sprite clubsSprite, heartsSprite, spadesSprite, diamondsSprite;

    Vector2 deckSpawnPoint => deckPos.transform.position;


    int luckyNumber;

    int luck;
    int failStreak = 0;

    [SerializeField] int baseLuck = 50;
    [SerializeField] int maxLuck = 100;
    [SerializeField] int reward = 5;
    [SerializeField] int basePenalty = 2;
    [SerializeField] float baseGoldChance = 10;
    [SerializeField] float maxGoldChance = 30;

    [SerializeField] TextMeshProUGUI luckyNumberText, luckText, failPenaltyText, goldenChanceText, sumText;
    [SerializeField] Image luckbarImage;

    List<CardData> pairPool = new List<CardData>();
    List<CardData> straightPool = new List<CardData>();
    List<CardData> flushPool = new List<CardData>();

    void Start()
    {
        luck = baseLuck;

        playHandButton.onClick.AddListener(PlayCardDeck);
        playHandButton.interactable = false;
        playHandButton.GetComponent<OffsetContentOnClick>().OffsetOn();
        DrawHand();
        GenerateLuckyNumber(new List<Card>());
        UpdateUI();
        sumText.text = "0";
    }

    void GenerateLuckyNumber(List<Card> cards, Combo combo = new Combo())
    {
        luckyNumber = cards.Count > 0 ? combo.baseNumber : UnityEngine.Random.Range(1, 14);
    }


    public void DrawHand()
    {
        //Card spawned = SpawnCard(11, false).GetComponent<Card>();
        //AssignPos(spawned, 0, 0 * staggerDelay);
        //Card spawned2 = SpawnCard(10, false).GetComponent<Card>();
        //AssignPos(spawned2, 1, 1 * staggerDelay);
        //Card spawned3 = SpawnCard(9, false).GetComponent<Card>();
        //AssignPos(spawned3, 2, 2 * staggerDelay);
        //Card spawned4 = SpawnCard(8, false).GetComponent<Card>();
        //AssignPos(spawned4, 3, 3 * staggerDelay);
        //Card spawned5 = SpawnCard(23, true).GetComponent<Card>();
        //AssignPos(spawned5, 4, 4 * staggerDelay);

        for (int i = 0; i < handCardNum; i++)
        {
            Card spawned = SpawnCard().GetComponent<Card>();
            AssignPos(spawned, i, i * staggerDelay);
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
        playHandButton.interactable = selectedCards.Count > 0;
        playHandButton.GetComponent<OffsetContentOnClick>().SetOffsetOnOff(!playHandButton.interactable);

        int sum = 0;
        foreach (Card card in selectedCards) sum += card.number;
        sumText.text = sum.ToString();
    }

    void LayoutHand(int firstNewIndex = int.MaxValue)
    {
        for (int i = 0; i < handCards.Count; i++)
        {
            float delay = i >= firstNewIndex ? (i - firstNewIndex) * staggerDelay : 0f;
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
        for (int i  = 0; i < cards.Count; i++) 
        {
            tween = cards[i].Mover.SetSlot(offScreenTarget.position, 0.3f * i + 2f, 0f, true, true, false);

        }
        await tween.AsyncWaitForCompletion();

    }

    async Task<List<TweenMover>> ShakeCards(List<Card> cards, Func<Card, TweenMover> showFeedback)
    {
        var popups = new List<TweenMover>();
        Tween tween;

        await Task.Delay(TimeSpan.FromSeconds(1f));

        foreach (Card c in cards)
        {
            TweenMover popup = showFeedback(c);
            if (popup != null) popups.Add(popup);
            tween = c.Mover.Shake();
            await tween.AsyncWaitForCompletion();
        }

        return popups;
    }

    bool isPlaying;

    async void PlayCardDeck()
    {
        if (isPlaying) return;
        isPlaying = true;
        playHandButton.interactable = false;
        playHandButton.GetComponent<OffsetContentOnClick>().OffsetOn();

        try
        {
            var cardsToPlay = new List<Card>(selectedCards);
            var feedbackQueue = new List<Func<Card, TweenMover>>();

            foreach (Card card in cardsToPlay)
                card.CardPlayed();

            // Lucky number sum
            bool sumMatches = CentralMechanics.GetCardSum(cardsToPlay) == luckyNumber;
            if (sumMatches)
            {
                feedbackQueue.Add(c => c.ShowNumber(c.number));
                luck = Mathf.Min(maxLuck, luck + reward);
                failStreak = 0;
            }

            // Combo
            Combo combo = CentralMechanics.GetCombo(cardsToPlay);

            if (combo.type == Combo.ComboType.STRT)
                cardsToPlay = SortCircular(cardsToPlay, combo.baseNumber);

            if (combo.type == Combo.ComboType.FLSH)
                feedbackQueue.Add(c => c.ShowSuit(GetSuitSprite(c.suit)));
            else if (combo.type != Combo.ComboType.NONE)
                feedbackQueue.Add(c => c.ShowNumber(c.number));

            // Golden card regenerates the lucky number
            if (HasGolden(cardsToPlay) && combo.hasCombo && HasLuckyNumber(cardsToPlay))
                GenerateLuckyNumber(cardsToPlay, combo);

            // Move to the play area
            await MoveCardsToPlay(cardsToPlay);

            // Penalty (computed after the move, same as your original order)
            if (CentralMechanics.GetCardSum(cardsToPlay) != luckyNumber && !combo.hasCombo)
            {
                int penaltyPerCard = GetPenalty();          // capture before failStreak changes
                feedbackQueue.Add(c => c.ShowNumber(-penaltyPerCard));
                luck = Mathf.Max(0, luck - penaltyPerCard * cardsToPlay.Count);
                failStreak++;
            }

            UpdateUI();

            List<TweenMover> mover = new List<TweenMover>();

            foreach (var feedback in feedbackQueue)
            mover = await ShakeCards(cardsToPlay, feedback);
            

            await GetRidOfPlayedCards(cardsToPlay);

            foreach (Card card in cardsToPlay)
                card.CardPlayed();

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
            RefillHand();
        }
        catch (Exception e)
        {
            Debug.LogException(e);   // async void would otherwise hide this
        }
        finally
        {
            isPlaying = false;
            playHandButton.interactable = selectedCards.Count > 0;
            playHandButton.GetComponent<OffsetContentOnClick>().SetOffsetOnOff(!playHandButton.interactable);
        }
    }

    float GetGoldenChance()
    {
        return Mathf.Clamp(baseGoldChance * luck / baseLuck, 0, maxGoldChance);
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

    Sprite GetSuitSprite(Card.Suit suit) => suit switch
    {
        Card.Suit.DIAMONDS => diamondsSprite,
        Card.Suit.CLUBS => clubsSprite,
        Card.Suit.HEARTS => heartsSprite,
        Card.Suit.SPADES => spadesSprite,
        _ => null
    };


    void AssignPos(Card c, int index, float delay = 0f)
    {
        c.transform.position = deckSpawnPoint;                 // everything starts at one spot
        c.Mover.SetSlot(GetSlotPosition(index), delay, 0, true,true);   // then slides to its slot
        c.SetRenderPriority(5 - index);
    }
    void UpdateUI()
    {
        luckyNumberText.text = luckyNumber.ToString();
        luckText.text = luck.ToString();
        luckbarImage.fillAmount = Mathf.Clamp01((float)luck / (float)maxLuck);
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
            }
            if (newest != null) await TweenMover.Wait(newest);
        }
    }
}
