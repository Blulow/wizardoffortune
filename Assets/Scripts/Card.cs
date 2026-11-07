using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour, IInteractable
{
    public int id = 0;


    [SerializeField] TextMeshProUGUI cardText;
    [SerializeField] SpriteRenderer cardSprite;
    [SerializeField] GameObject selectIndicator;


    public event Action<Card> Clicked;
    public bool IsSelected { get; private set; }

    bool selected = false;
    public int number { get; private set; }

    public enum Suit { DIAMONDS, CLUBS, HEARTS, SPADES }
    public Suit suit { get; private set; }




    int IInteractable.Priority => cardSprite.rendererPriority;

    public bool isGold;

    public void InitCard()
    {
        number = ParseIdToNumber(id);
        suit = ParseIdToSuit(id);
        cardText.text = number.ToString() + " of " + suit.ToString();
        if (isGold)
        {
            cardSprite.color = Color.gold;
        }
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

    void IInteractable.OnHoverEnter()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + .2f);
    }

    void IInteractable.OnHoverExit()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y - .2f);
    }

    public void SetSelected(bool value)
    {
        IsSelected = value;
        selectIndicator.gameObject.SetActive(value);
    }



    void IInteractable.OnClick() => Clicked?.Invoke(this);

}
