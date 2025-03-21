using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableOnHitboxSweep : MonoBehaviour
{
    [SerializeField]
    HitboxSweep Sweeper;

    [SerializeField]
    float Duration;

    Maid maid = new();

    private void Awake()
    {
        Sweeper.HitboxSwept += (int n) => {
            maid.Cleanup();
            gameObject.SetActive(true);
            maid.GiveCoroutine(this, StartCoroutine(WaitThenDisable()));
        };
        gameObject.SetActive(false);
    }

    IEnumerator WaitThenDisable()
    {
        yield return new WaitForSeconds(Duration);
        gameObject.SetActive(false);
    }
}
