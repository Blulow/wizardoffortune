using NUnit.Framework;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManualManager : MonoBehaviour
{
    [SerializeField] GameObject[] manuals;
    [SerializeField] Button nextButton, prevButton, closeButton;
    [SerializeField] TextMeshProUGUI pagesText;
    GameObject showingManual;
    int index = 0;
    
    void TurnPage(bool next)
    {
        SetPage(next);
        if (showingManual != null) showingManual.SetActive(false);
        showingManual = manuals[index];
        showingManual.SetActive(true);
        pagesText.text = "(" + (index + 1) + "/" + manuals.Count() + ")";
    }

    private void Start()
    {
        showingManual = manuals[index];
        showingManual?.SetActive(true);
        pagesText.text = "(" + (index + 1) + "/" + manuals.Count() + ")";
    }


    private void OnEnable()
    {
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PreviousPage);
    }

    void NextPage()
    {
        TurnPage(true);
    }

    void PreviousPage()
    {
        TurnPage(false);
    }

    void SetPage(bool next)
    {
        if (next)
        {
            if (index == manuals.Count() - 1)
            {
                index = 0;
            }
            else
            {
                index++;
            }
        }
        else
        {
            if (index == 0)
            {
                index = manuals.Count() - 1;
            }
            else
            {
                index--;
            }
        }
    }

}
