using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
public class BackAndForthMovement : MonoBehaviour
{
    // TODO make these an array of transforms so students can make platforms that travel every which way.
    [SerializeField]
    Transform beginning;
    [SerializeField]
    Transform ending;

    [SerializeField]
    [Tooltip("How fast the platform moves.")]
    float speed = 1;

    [SerializeField]
    [Range(0, 1)]
    float offset = 0;


    // Update is called once per frame
    void Update()
    {
        float distanceBetween = (beginning.position - ending.position).magnitude;
        float t = (Mathf.Sin(speed/distanceBetween * Time.time + offset * 2 * Mathf.PI) + 1) / 2;

        transform.position = Vector3.Lerp(beginning.position, ending.position, t);
    }
}
