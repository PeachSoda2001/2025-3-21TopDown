using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAni : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController controller;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float speed = controller.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }
}