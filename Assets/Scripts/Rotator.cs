using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Rotator : MonoBehaviour
{

    [SerializeField] private Vector3 _rotation;
    [SerializeField] private float _speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Z)) _rotation = new Vector3(0, 0, 1);
        else if (Input.GetKey(KeyCode.C)) _rotation = new Vector3(0, 0, -1);
        else _rotation = Vector3.zero;

        transform.Rotate(_rotation * _speed * Time.deltaTime);
    }
}
