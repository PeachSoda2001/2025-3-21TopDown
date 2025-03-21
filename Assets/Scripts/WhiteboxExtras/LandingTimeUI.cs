using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LandingTimeUI : MonoBehaviour
{
    CharacterMovement movement;
    float timeSinceLanding = 0;


    void Awake()
    {
        movement = FindObjectOfType<CharacterMovement>();
    }

    private void OnEnable()
    {
        movement.Landed += ResetLandingTime;
    }

    private void OnDisable()
    {
        movement.Landed -= ResetLandingTime;
    }

    void ResetLandingTime()
    {
        timeSinceLanding = 0;
    }

    void Update()
    {
        if (!movement.IsOnGround()) {
            timeSinceLanding += Time.deltaTime;
        }

        GetComponent<TMP_Text>().text = "Time Since Landing: " + timeSinceLanding.ToString("0.##");
    }
}
