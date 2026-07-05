using System;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Assemblies;

public class Player : MonoBehaviour
{
    [SerializeField]
    CharacterController controller;

    [SerializeField]
    float movementSpeed = 4.0f,
          rotationSpeed = 360.0f,
          stepLength    = 2.0f;

    [SerializeField]
    Transform sensor;

#if DEBUG
    [SerializeField, Range(0f, 1f)]
    float sphereCastRadius = 0.2f;

    [SerializeField, Range(0f, 2f)]
    float sphereCastDistance = 1.0f;
#endif

    [SerializeField]
    LayerMask mask;

    [SerializeField]
    Animator animator;

    Vector2 initialPosition;  
    Quaternion initialRotation;

    bool ValidateAction() => Physics.SphereCast(sensor.position,
                                                sphereCastRadius,
                                                sensor.forward,
                                                out RaycastHit _,
                                                sphereCastDistance,
                                                mask);


    public void StartNewGame(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);

        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void Move()
    {
        if (!ValidateAction()) return;

        Vector2 finalPosition = transform.position + (transform.forward * stepLength);
        controller.Move(finalPosition);
    }


}
