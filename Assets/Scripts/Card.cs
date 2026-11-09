using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour, IInteractable
{
    public int id = 0;

    [SerializeField] SpriteRenderer cardSprite, numberSprite, suitSprite;
    [SerializeField] Sprite goldenSprite;

    [SerializeField] PopMover numberShow;
    [SerializeField] PopMover suitShow;
    [SerializeField] Transform feedbackSpawnPoint;

    [SerializeField] Collider2D collider;
    public CardMover Mover { get; private set; }
    void Awake() => Mover = GetComponent<CardMover>();

    public event Action<Card> Clicked;
    public event Action<bool> OnHover;
    public event Action OnPlayed;

    public bool IsSelected { get; private set; }

    public int number { get; private set; }

    public enum Suit { DIAMONDS, CLUBS, HEARTS, SPADES }
    public Suit suit { get; private set; }

    int IInteractable.Priority => cardSprite.rendererPriority;

    public bool isGold;

    public void CardPlayed()
    {

        collider.enabled = false;
        OnPlayed?.Invoke();
    }


    public void InitCard(int _id = -1, bool _isGold = false)
    {
        number = ParseIdToNumber(_id > -1 ? _id : id);
        suit = ParseIdToSuit(_id > -1 ? _id : id);
        //cardText.text = number.ToString() + " of " + suit.ToString();
        numberSprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/CardSprites/Numbers/card_number_{number}.png");
        suitSprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/CardSprites/Suits/card_suit_{(int)suit + 1}.png");
        if (isGold || _isGold)
        {
            //cardSprite.sprite = goldenSprite;
            GetComponent<CardMover>().frontSprite = goldenSprite;
            numberSprite.color = new Color32(150, 128, 89, 255);
            suitSprite.color = new Color32(150, 128, 89, 255);
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
        //transform.position = new Vector2(transform.position.x, transform.position.y + .2f);
        OnHover?.Invoke(true);

    }

    void IInteractable.OnHoverExit()
    {
        
        //transform.position = new Vector2(transform.position.x, transform.position.y - .2f);
        OnHover?.Invoke(false);
    }

    public void SetSelected(bool value)
    {
        IsSelected = value;
    }


    void ToggleSelected()
    {
        SetSelected(!IsSelected);
        Clicked?.Invoke(this);
    }


    public TweenMover ShowNumber(int number)
    {
        PopMover mover = Instantiate(numberShow, feedbackSpawnPoint.position, Quaternion.identity);
        if (mover != null) mover.PlayPop(0f,number.ToString());
        else Debug.LogWarning("numberShow prefab has no TextPopReveal on its label", numberShow);

        return mover;
    }

    public TweenMover ShowSuit(Sprite sprite)
    {
        PopMover mover = Instantiate(suitShow, feedbackSpawnPoint.position, Quaternion.identity);
        SpriteRenderer spriteRen  = mover.GetComponent<SpriteRenderer>();
        spriteRen.sprite = sprite;
        mover.PlayPop();
        return mover;

    }


    void IInteractable.OnClick() => ToggleSelected();

    public void SetRenderPriority(int r)
    {
        cardSprite.sortingOrder = r;
        numberSprite.sortingOrder = r + 1;
        suitSprite.sortingOrder = r + 1;
        //Debug.Log(cardSprite.soe);
    }

}
