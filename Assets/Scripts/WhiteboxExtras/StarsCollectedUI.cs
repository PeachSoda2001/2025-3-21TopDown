using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StarsCollectedUI : MonoBehaviour
{
    GameObject[] stars;
    int collected = 0;

    void Awake()
    {
        stars = GameObject.FindGameObjectsWithTag("Star");
    }

    private void OnEnable()
    {
        foreach (GameObject star in stars)
        {
            star.GetComponent<ItemPickup>().Collected += incrementCollected;
        }
    }

    private void OnDisable()
    {
        foreach (GameObject star in stars)
        {
            star.GetComponent<ItemPickup>().Collected -= incrementCollected;
        }
    }

    private void incrementCollected(GameObject character)
    {
        collected++;
    }



    void Update()
    {
        GetComponent<TMP_Text>().text = "Stars Collected: " + collected + "/" + stars.Length;
    }
}
