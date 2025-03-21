using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FuelLeftUI : MonoBehaviour
{
    Jetpack jetpack;

    void Start()
    {
        jetpack = FindObjectOfType<Jetpack>();
    }

    void Update()
    {
        GetComponent<TMP_Text>().text = "Fuel Left: " + (int)((jetpack.FuelLeft / jetpack.Duration)*100) + "%";
    }
}
