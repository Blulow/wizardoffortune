using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int id = 0; // 0-12 13-25 26-38 39-51

    public int number { get; private set; }

    public enum Suit { DIAMONDS, CLUBS, HEARTS, SPADES }
    public Suit suit { get; private set; }

    public void InitCard()
    {
        number = ParseIdToNumber(id);
        suit = ParseIdToSuit(id);
    }

    int ParseIdToNumber(int id)
    {
        return id % 13 + 1;
    }

    Suit ParseIdToSuit(int id)
    {
        switch (Mathf.FloorToInt(id / 13))
        {
            case 0:
                return Suit.DIAMONDS;
                break;
            case 1:
                return Suit.CLUBS;
                break;
            case 2:
                return Suit.HEARTS;
                break;
            case 3:
                return Suit.SPADES;
                break;
        }

        return Suit.DIAMONDS;
    }
}
