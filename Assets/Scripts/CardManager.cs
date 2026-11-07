using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [SerializeField] Card card;

    List<int> handCards = new List<int>();
    [SerializeField] int handCardNum = 5;

    List<int> playingCards = new List<int>();

    void Start()
    {
        DrawHand();
    }

    public void DrawHand()
    {
        for (int i = 0; i < handCardNum; i++)
        {
            SpawnCard();
        }
    }

    void SpawnCard(int index = 0)
    {
        Card _card = Instantiate(card, Vector2.zero, Quaternion.identity);

        _card.id = Random.Range(0, 52);
        _card.InitCard();

        handCards.Add(_card.id);
    }
}
