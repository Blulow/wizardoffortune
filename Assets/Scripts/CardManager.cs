using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    [SerializeField] Card card;

    [SerializeField] private Vector2 initSpawnPoint = Vector2.zero;
    [SerializeField] private Vector2 spawnOffset = new Vector2(1f, 0f);

    List<Card> handCards = new List<Card>();
    List<Card> selectedCards = new List<Card>();
    [SerializeField] int handCardNum = 5;

    //List<Card> playingCards = new List<Card>();
    [SerializeField] Button playHandButton;

    int luckyNumber;

    int luck;
    int failStreak = 0;

    [SerializeField] int baseLuck = 50;
    [SerializeField] int maxLuck = 100;
    [SerializeField] int reward = 5;
    [SerializeField] int basePenalty = 2;
    [SerializeField] float baseGoldChance = 10;
    [SerializeField] float maxGoldChance = 30;

    [SerializeField] TextMeshProUGUI luckyNumberText, luckBarText, failPenaltyText, goldenChanceText, sumText;

    List<Card> pairPool = new List<Card>();
    List<Card> straightPool = new List<Card>();
    List<Card> flushPool = new List<Card>();

    void Start()
    {
        luck = baseLuck;

        playHandButton.onClick.AddListener(PlayCardDeck);
        playHandButton.interactable = false;
        DrawHand();

        GenerateLuckyNumber(new List<Card>());
        UpdateUI();
    }

    void GenerateLuckyNumber(List<Card> cards, Combo combo = new Combo())
    {
        luckyNumber = cards.Count > 0 ? combo.baseNumber : UnityEngine.Random.Range(1, 14);
    }

    public void DrawHand()
    {
        for (int i = 0; i < handCardNum; i++)
        {
            GameObject spawnedCard = SpawnCard();
            AssignPos(spawnedCard, i);
        }
    }

    GameObject SpawnCard(int id = -1, bool? isGold = null)
    {
        Card _card = Instantiate(card, Vector3.zero, Quaternion.identity);

        _card.id = GetUniqueId();
        _card.isGold = UnityEngine.Random.Range(0, 100) < GetGoldenChance();

        if ((isGold ?? _card.isGold) && id == -1 && handCards.Count == handCardNum) return GenerateCardFromPools().gameObject;

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

    Card GenerateCardFromPools()
    {
        //if (handCards.Count == handCardNum) return null;

        List<Card> cards = GetLuckyNumberCards();

        for (int i = 0; i <= 3; i++) pairPool.Add(new Card(i * 13 + luckyNumber, true));
        for (int i = luckyNumber - 4; i <= luckyNumber + 4; i++) straightPool.Add(new Card(UnityEngine.Random.Range(0, 4) * 13 + (i - 1) % 13, true));
        if (cards.Count > 0)
        {
            for (int c = 0; c < cards.Count; c++)
            {
                for (int i = 0; i < 13; i++) flushPool.Add(new Card((int)cards[c].suit * 13 + i, true));
            }
        }

        (List<Card> pool, Card card) cardData = GetCardFromPools();

        int j = 0;
        while (j < handCards.Count)
        {
            if (handCards[j].id == cardData.card.id)
            {
                cardData.pool.RemoveAll(e => e.id == cardData.card.id);
                cardData = GetCardFromPools();

                j = 0;
            }
            else j++;
        }

        Card card = cardData.card;
        return card;
    }

    (List<Card> pool, Card card) GetCardFromPools()
    {
        List<List<Card>> validPools = new List<List<Card>> { pairPool, straightPool, flushPool }.Where(e => e.Count > 0).ToList();
        List<Card> pool = validPools[UnityEngine.Random.Range(0, validPools.Count)];
        Card card = pool[UnityEngine.Random.Range(0, pool.Count)];
        return (pool, card);
    }

    void HandleCardClicked(Card c)
    {
        if (c.IsSelected)
        {
            c.SetSelected(false);
            selectedCards.Remove(c);
        }
        else
        {
            c.SetSelected(true);
            selectedCards.Add(c);
        }
        playHandButton.interactable = selectedCards.Count > 0;

        int sum = 0;
        foreach (Card card in selectedCards) sum += card.number;
        sumText.text = "Sum: " + sum;
    }

    void RemoveCard(Card c) // call this when playing or discarding
    {
        c.Clicked -= HandleCardClicked;
        selectedCards.Remove(c);
    }

    void ReplaceCardSlot(Card card)
    {
        GameObject newCard = SpawnCard();
        newCard.transform.position = card.transform.position;
    }

    void PlayCardDeck()
    {
        var cardsToPlay = new List<Card>(selectedCards);

        if (CentralMechanics.GetCardSum(cardsToPlay) == luckyNumber)
        {
            luck = Mathf.Min(maxLuck, luck + reward);
            failStreak = 0;
        }

        Combo combo = CentralMechanics.GetCombo(cardsToPlay);

        if (HasGolden(cardsToPlay) && combo.hasCombo && HasLuckyNumber(cardsToPlay))
        {
            GenerateLuckyNumber(cardsToPlay, combo);
        }

        if (CentralMechanics.GetCardSum(cardsToPlay) != luckyNumber && !combo.hasCombo)
        {
            int penalty = GetPenalty() * cardsToPlay.Count;

            luck = Mathf.Max(0, luck - penalty);

            failStreak++;
        }

        UpdateUI();

        selectedCards.Clear();
        foreach (Card card in cardsToPlay)
        {
            ReplaceCardSlot(card);
            card.Clicked -= HandleCardClicked;
            card.gameObject.SetActive(false);
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

    bool ContainsId(List<Card> cards, int id)
    {
        foreach(Card card in cards) if (card.id == id) return true;
        return false;
    }

    void AssignPos(GameObject go, int index)
    {
        int step = (index + 1) / 2;
        int sign = (index % 2 == 1) ? 1 : -1;
        Vector2 spawnPos = initSpawnPoint + spawnOffset * (step * sign);
        go.transform.position = spawnPos;
    }

    void UpdateUI()
    {
        luckyNumberText.text = "Lucky Number: " + luckyNumber;
        luckBarText.text = "Luck: " + luck + " / " + maxLuck;
        failPenaltyText.text = "Failure Penalty: " + GetPenalty();
        goldenChanceText.text = $"Golden Card Chance: {GetGoldenChance():F1}%";
    }
}
