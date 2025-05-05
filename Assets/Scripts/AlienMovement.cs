using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AlienMovement : MonoBehaviour
{
    [SerializeField] protected float speed;
    [SerializeField] protected float acceleration;
    [SerializeField] protected float stoppingDistance = 0.5f;
    private Rigidbody rb;
    protected Vector3 target;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void FixedUpdate()
    {
        Move();
    }

    protected void Move()
    {
        Vector3 direction = (target - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target);
        if (distance > stoppingDistance)
        {
            Vector3 desiredVelocity = direction * acceleration * distance;
            Vector3 velocityChange = desiredVelocity - rb.linearVelocity;
            rb.AddForce(velocityChange * acceleration, ForceMode.Acceleration);
        }
        else
        {
            OnTargetReached();
        }
    }

    protected virtual void OnTargetReached()
    {
        
    }
}
