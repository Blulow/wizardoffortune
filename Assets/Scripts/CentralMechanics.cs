using NUnit.Framework;
using NUnit.Framework.Internal.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

[System.Serializable]
public struct Combo
{
    public enum ComboType { NONE, FOAK, TOAK, PAIR, STRT, FLSH }

    public bool hasCombo;
    public ComboType type;
    public int baseNumber;

    public Combo(bool _hasCombo = false, ComboType _type = ComboType.NONE, int _baseNumber = -1)
    {
        hasCombo = _hasCombo;
        type = _type;
        baseNumber = _baseNumber;
    }
}

public static class CentralMechanics
{
    public static int GetCardSum(List<Card> playerCards)
    {
        int sum = 0;
        foreach (Card card in playerCards)
        {
            sum += card.number;
        }

        return sum;
    }

    public static Combo GetCombo(List<Card> playerCards)
    {
        Combo combo = new Combo();
        
        Dictionary<int, int> values = new Dictionary<int, int>();
        Dictionary<Card.Suit, int> suits = new Dictionary<Card.Suit, int>();

        foreach (Card card in playerCards)
        {
            if (values.ContainsKey(card.number))
            {
                values[card.number]++;
            }
            else values[card.number] = 1;

            if (suits.ContainsKey(card.suit))
            {
                suits[card.suit]++;
            }
            else suits[card.suit] = 1;
        }

        (int baseNumber, bool isConsecutive) consecutive = GetConsecutive(values.Keys.ToList<int>());

        if (playerCards.Count == 4 && values.ContainsValue(4))
        {
            Debug.Log("4 of a kind");
            combo.hasCombo = true;
            combo.type = Combo.ComboType.FOAK;
            combo.baseNumber = values.Keys.First();
        }
        else if (playerCards.Count == 3 && values.ContainsValue(3))
        {
            Debug.Log("3 of a kind");
            combo.hasCombo = true;
            combo.type = Combo.ComboType.TOAK;
            combo.baseNumber = values.Keys.First();
        }
        else if (playerCards.Count == 2 && values.ContainsValue(2))
        {
            Debug.Log("pair");
            combo.hasCombo = true;
            combo.type = Combo.ComboType.PAIR;
            combo.baseNumber = values.Keys.First();
        }
        else if (playerCards.Count >= 3 && OnlyContainsValue<int, int>(values, 1) && consecutive.isConsecutive)
        {
            Debug.Log("straight");
            combo.hasCombo = true;
            combo.type = Combo.ComboType.STRT;
            combo.baseNumber = consecutive.baseNumber;
        }
        else if (suits.Count == 1 && HasValueGreaterOrEqualThan<Card.Suit, int>(suits, 3))
        {
            Debug.Log("flush");
            combo.hasCombo = true;
            combo.type = Combo.ComboType.FLSH;
            combo.baseNumber = values.Keys.Min();
        }

        return combo;
    }

    static (int baseNumber, bool isConsecutive) GetConsecutive(List<int> values, int minVal = 1, int maxVal = 13)
    {
        List<int> sorted = new List<int>(values);
        sorted.Sort();
        int totalRange = maxVal - minVal + 1;

        int gaps = 0;
        int baseNumber = -1;

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            int diff = sorted[i + 1] - sorted[i];

            if (diff == 1) continue;
            else if (diff == totalRange - sorted.Count + 1)
            {
                gaps++;
                baseNumber = sorted[i + 1];
            }
            else return ( baseNumber, isConsecutive: false );
        }

        if (baseNumber == -1) baseNumber = sorted[0];

        return (baseNumber, isConsecutive: gaps <= 1 );
    }

    static bool OnlyContainsValue<TKey, TValue>(Dictionary<TKey, int> dict, int value)
    {
        foreach (KeyValuePair<TKey, int> pair in dict)
        {
            if (pair.Value != value)
            {
                return false;
            }
        }

        return true;
    }

    static bool HasValueGreaterOrEqualThan<TKey, TValue>(Dictionary<TKey, int> dict, int value)
    {
        foreach (KeyValuePair<TKey, int> pair in dict)
        {
            if (pair.Value >= value)
            {
                return true;
            }
        }

        return false;
    }
}
