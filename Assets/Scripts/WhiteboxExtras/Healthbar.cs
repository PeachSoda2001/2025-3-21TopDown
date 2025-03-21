using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Healthbar : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The bar you want to scale to match the health")]
    RectTransform Bar;

    [SerializeField]
    [Tooltip("The health to read from")]
    Health HealthToMatch;

    bool shouldRun = false;

    void Awake()
    {
        Assert.IsNotNull(Bar, "Bar must be set for this healthbar.");
        Assert.IsNotNull(HealthToMatch, "Health must be set for this healthbar.");
        shouldRun = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!shouldRun)
        {
            return;
        }

        Bar.offsetMax = new Vector2(-(1 - HealthToMatch.Hitpoints / HealthToMatch.MaximumHitpoints), Bar.offsetMax.y); 
    }
}
