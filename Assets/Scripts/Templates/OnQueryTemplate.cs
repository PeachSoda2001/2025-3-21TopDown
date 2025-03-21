using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OnQueryTemplate : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How to get the objects to act on.")]
    CharacterQueryBehavior Querier;

    readonly Maid maid = new();

    private void OnEnable()
    {
        Querier.Hit += ActOnQuery;
    }

    private void OnDisable()
    {
        Querier.Hit -= ActOnQuery;
    }

    private void ActOnQuery(List<(Collider, Vector3)> objectsQueried)
    {
        foreach ((Collider collider, Vector3 pointHit) in objectsQueried)
        {
            DoSomething(collider.gameObject, collider, pointHit);
        }
    }

    // DoSomething is called for each object found by the query

    // objectQueried is a game object found by the query

    // colliderOnObject is the collider the query found, on the game object

    // pointQueriedAt is where the query found the object
    // for raycasts and projectiles, this is the position the query hit the object at
    private void DoSomething(
        GameObject objectQueried, 
        Collider colliderOnObject, 
        Vector3 pointQueriedAt
    )
    {
        Debug.Log($"{objectQueried.name} Queried!");
        // your code here!
    }
}
