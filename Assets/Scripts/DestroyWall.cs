using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyWall : MonoBehaviour
{

    [SerializeField] private GameObject wallToDestroy;
    [SerializeField] private Transform player; 
    [SerializeField] private float destroyDistance = 3f;

    private void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("Player is not assigned in DestroyWall script!");
            return;
        }


        float distanceToWall = Vector3.Distance(player.position, wallToDestroy.transform.position);


        if (distanceToWall <= destroyDistance && Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q Pressed! Destroying Wall.");
            Destroy(wallToDestroy);
        }
    }
}