using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AlienWaypointPatrol : AlienMovement
{
    [Header("Movement Parameters")]
    public BoxCollider patrolArea;

    public int waypointCount = 10;
    private List<Vector3> patrolPoints = new List<Vector3>();
    private int currentPointIndex = 0;
    private bool isMoving = true;
    public float waitTime = 1f;
    //public AlienWaypointSpawner waypointSpawner;

    [Header("Color Lerp")] // because they are aliens:)
    public Renderer enemyRenderer;
    public Color color1;
    public Color color2;
    public float colorLerpSpeed = 2f;
    private float timer;
    
    void Start()
    {
        if (patrolArea == null)
        {
            Debug.LogError("Patrol area not assigned");
            return;
        }
    }
    
    protected override void FixedUpdate()
    {
        //base.FixedUpdate();
        GenerateRandomWaypoints();
        StartCoroutine(Patrol());
        timer = Mathf.PingPong(Time.time * colorLerpSpeed, 1f);
        enemyRenderer.material.color = Color.Lerp(color1, color2, timer);
    }
    
    void GenerateRandomWaypoints()
    {
        Bounds bounds = patrolArea.bounds;

        for (int i = 0; i < waypointCount; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                transform.position.y,  // freese Y-axis 
                Random.Range(bounds.min.z, bounds.max.z)
            );
            patrolPoints.Add(randomPoint);
        }

        // Start at first waypoint
        if (patrolPoints.Count > 0)
        {
            transform.position = patrolPoints[0];
        }
    }

    IEnumerator Patrol()
    {
        while (true)
        {
            if (isMoving && patrolPoints.Count > 0)
            {
                Vector3 target = patrolPoints[currentPointIndex];

                while (Vector3.Distance(transform.position, target) > 0.2f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                    yield return null;
                }

                yield return new WaitForSeconds(waitTime);
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
            }
        }
    }
}
