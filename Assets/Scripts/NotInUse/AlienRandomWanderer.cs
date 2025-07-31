using UnityEngine;
using System.Collections.Generic;
public class AlienRandomWanderer : AlienMovement
{
    [SerializeField] private float wanderRadius;
    [SerializeField] private float waitTime;
    private float timer;

    protected override void Awake()
    {
        base.Awake();
        //PickNewTarget();
    }

    protected override void FixedUpdate()
    {
        PickNewTarget();
        base.FixedUpdate();
    }

    private void PickNewTarget()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-wanderRadius, wanderRadius), 0f, Random.Range(-wanderRadius, wanderRadius));
        target = transform.position + randomDirection;
    }

    protected override void OnTargetReached()
    { 
        timer += Time.deltaTime;
        if (timer >= waitTime)
        {
            PickNewTarget();
            timer = 0;
        }
    }
}
