using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CharacterMechanics
{
    class FaceMousePosition : MonoBehaviour
    {
        CharacterMovement Movement => GetComponent<CharacterMovement>();

        private void OnEnable()
        {
            Movement.FacingDirectionMiddleware = FacingMiddleware.LookAtMouse(Movement);
        }

        private void OnDisable()
        {
            Movement.FacingDirectionMiddleware = FacingMiddleware.FaceMovementDirection(Movement);
        }

        private void Start()
        {
            OnEnable();
        }

    }
}
