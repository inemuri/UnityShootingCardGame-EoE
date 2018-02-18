using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour {

    public List<GameObject> cardDatabase;
    
    public void LoadCards()
    {

        foreach (CardDisplay cd in GetComponentsInChildren<CardDisplay>())
        {
            cd.helpEnable = false;
            cd.selectEnable = false;
            cardDatabase.Add(cd.gameObject);
        }
        cardDatabase.Sort((x, y) => x.GetComponent<CardDisplay>().cid.CompareTo(y.GetComponent<CardDisplay>().cid));

    }
}
