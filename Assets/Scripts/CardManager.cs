using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CardManager : MonoBehaviour
{
    [SerializeField] Card card;
    [SerializeField] private Vector2 initSpawnPoint = Vector2.zero;
    [SerializeField] private Vector2 spawnOffset = new Vector2(1f, 0f);
    List<int> handCards = new List<int>();
    List <Card> selectedCards = new List<Card>();  
    [SerializeField] int handCardNum = 5;
    List<int> playingCards = new List<int>();
    [SerializeField] Button playHandButton;


    void Start()
    {
        playHandButton.onClick.AddListener(PlayCardDeck);
        playHandButton.interactable = false;
        DrawHand();
    }

    public void DrawHand()
    {
        for (int i = 0; i < handCardNum; i++)
        {
            GameObject spawnedCard = SpawnCard();
            AssignPos(spawnedCard, i);
        }
    }

    GameObject SpawnCard()
    {
        Card _card = Instantiate(card, Vector3.zero, Quaternion.identity);
        _card.isGold = UnityEngine.Random.Range(0, 2) == 1;
        _card.id = GetUniqueId();
        _card.InitCard();
        _card.Clicked += HandleCardClicked;
        handCards.Add(_card.id);
        return _card.gameObject;
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

    }

    void RemoveCard(Card c) // call this when playing or discarding
    {
        c.Clicked -= HandleCardClicked;
        selectedCards.Remove(c);
    }

    void ReplaceCardSlot(Card card)
    {
        GameObject newCard =  SpawnCard();
        newCard.transform.position = card.transform.position;
    }

    void PlayCardDeck()
    {
        var cardsToPlay = new List<Card>(selectedCards);
        selectedCards.Clear();

        foreach (Card card in cardsToPlay) 
        {
            ReplaceCardSlot(card);
            card.Clicked -= HandleCardClicked;
            card.gameObject.SetActive(false);
        }
    }

    int GetUniqueId()
    {
        int id;
        do
        {
            id = UnityEngine.Random.Range(0, 52);
        } while (handCards.Contains(id));
        return id;
    }

    void AssignPos(GameObject go, int index)
    {
        int step = (index + 1) / 2;
        int sign = (index % 2 == 1) ? 1 : -1;
        Vector2 spawnPos = initSpawnPoint + spawnOffset * (step * sign);
        go.transform.position = spawnPos;
    }
}
