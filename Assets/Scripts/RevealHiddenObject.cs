using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealHiddenObject : MonoBehaviour
{

    [SerializeField] private GameObject DestroyWall;
    [SerializeField] private GameObject Hidden;
    [SerializeField] private GameObject DestroyWall1;
    [SerializeField] private GameObject WinningStage;


    private void Update()
    {
        if (DestroyWall == null)
        {
            Debug.Log("Wall destroyed! Revealing hidden objects.");

            Hidden.SetActive(true);
            
        }
        if (DestroyWall1 == null)
        {
            WinningStage.SetActive(true);
        }

        if (DestroyWall == null && DestroyWall1 == null)
        {
            Destroy(this);
        }
    }
}
