using UnityEngine;
using Jayden;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
using System;
using System.Collections;


public class AlienHandOffAI : AlienAI
{
    public int characterY = 10000000;
    [SerializeField] public Vector2 viewDistance = new Vector2(5, 5);
    public Vector2 scaledViewDistance;

    [SerializeField] public List<LayerMask> enemyLayers = new List<LayerMask>();

    public AlienHandOffAI seenEnemy;

    public Vector3 originalDestination;

    public bool seekOtherEnemy = false;

    public bool handingOff = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        scaledViewDistance = new Vector2(Mathf.CeilToInt(viewDistance.x), Mathf.CeilToInt(viewDistance.y));
        StartCoroutine(View()); // Starts the view that contantly runs
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
    protected void FixedUpdate()
    {
        if (!IsServer)
            return;
        SetCharacterY();

        if (currentState is ReturnState && seenEnemy != null && seenEnemy.currentState is ReturnState)
        {
            if (!seekOtherEnemy)
            {
                seekOtherEnemy = true;
                // originalDestination = lastDestination;
            }
            MoveTo(seenEnemy.transform.position);
        }

        // base.FixedUpdate();

    }
    public virtual void SetCharacterY()
    {
        characterY = (int)Mathf.Floor(transform.position.y);
        return;
    }



    public virtual IEnumerator View()
    {
        while (true)
        {
            if (seenEnemy == null)
            {
                try
                {
                    int adjacent1Count = 0;


                    List<Vector3> checkPositions = new List<Vector3>();
                    try
                    {

                        // TRIGONOMETRY!!!!!
                        // Gets the view area in a triangle based on maths

                        float triAngle = Mathf.Atan(scaledViewDistance.x / scaledViewDistance.y); // "I should rename this angle varaible, I just used it in the wrong spot. Oh I know, it is for triangles so I'll put a tri prefix before it.........wait"


                        for (int adjacent = 1; adjacent < scaledViewDistance.y; adjacent += 1)
                        {

                            int opposite = Mathf.CeilToInt(Mathf.Tan(triAngle) * adjacent);
                            for (int oppositeX = -opposite; oppositeX <= opposite; oppositeX += 1)
                            {
                                Vector3 pos = new Vector3(Mathf.FloorToInt(transform.position.x + adjacent * Mathf.Sin(transform.rotation.y)), characterY, Mathf.FloorToInt(transform.position.z + oppositeX * Mathf.Cos(transform.rotation.y)));
                                if ((!NewUnity.ContainsV3(checkPositions, pos)))
                                {
                                    checkPositions.Add(pos);
                                }
                                else if (adjacent == 1) // Ensures the front row/right in front of the agent is always seen and checked
                                {
                                    checkPositions.Add(pos);
                                    adjacent1Count++;

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Debug.LogError(ex);
                    }



                    bool itemFound = false;

                    // Checks if any of the seen cells contained an item
                    AlienHandOffAI closestSeenEnemy = null;
                    for (int i = 0; i < checkPositions.Count; i++)
                    {
                        if (!NewUnity.ContainsV3(AIGrid.instance.unwalkableGrid, checkPositions[i])) // Unwalkable can't contain anything or be interacted with, so no need to check
                        {
                            foreach (LayerMask layer in enemyLayers)
                            {
                                // Checks the cell on the layer
                                RaycastHit hit2;
                                bool hitDetction2 = Physics.BoxCast(checkPositions[i], AIGrid.instance.scaledCellSize / 2, checkPositions[i], out hit2, Quaternion.identity, Mathf.Infinity, layer);
                                NewUnity.DrawBoxCastBox(checkPositions[i], AIGrid.instance.scaledCellSize / 2, Quaternion.identity, checkPositions[i], Mathf.Infinity, Color.white);
                                if (hitDetction2)
                                {
                                    AlienHandOffAI seenEnemyTmp = hit2.collider.GetComponent<AlienHandOffAI>();
                                    if (seenEnemyTmp != null)
                                    {
                                        if (closestSeenEnemy == null) closestSeenEnemy = seenEnemyTmp;
                                        else if (Vector3.Distance(transform.position, closestSeenEnemy.transform.position) > Vector3.Distance(transform.position, seenEnemyTmp.transform.position)) closestSeenEnemy = seenEnemyTmp;

                                    }
                                }

                            }

                        }
                        //seenCells[(int)checkPositions[i].x, (int)checkPositions[i].y, (int)checkPositions[i].z] = (checkPositions[i]);
                    }

                    seenEnemy = closestSeenEnemy;

                }
                catch (Exception e)
                {
                    //Debug.Log(e);
                }

                for (int i = 0; i < 10; i++)
                {
                    yield return null;  
                }
                //Debug.Log("Active");
            }


        }

    }

    void InitiateHandOff()
    {
        seenEnemy.handingOff = true;
        if (!handingOff)
        {
            handingOff = true;
            CalculatedPathData myPathData = AIPathFindingCore.CalculatePathCore(originalDestination, transform.position, 0);
            CalculatedPathData theirPathData = AIPathFindingCore.CalculatePathCore(seenEnemy.originalDestination, seenEnemy.transform.position, 0);
            if (myPathData.cost < theirPathData.cost)
            {
                HandOff(seenEnemy, this);
            }
            else
            {
                HandOff(this, seenEnemy);
            }
        }
        seenEnemy = null;
        seekOtherEnemy = false;
    }


    void HandOff(AlienHandOffAI handingOff, AlienHandOffAI handingTo)
    {
        foreach (AIBase civ in handingOff.escortingCivs)
        {
            civ.escortingAlien = handingTo;
            handingTo.escortingCivs.Add(civ);
            civ.ChangeState(new FollowState(handingTo.currentTargetCiv, handingTo.transform));
        }
        handingOff.escortingCivs.Clear();
        MoveTo(handingTo.originalDestination);
        handingOff.ChangeState(new SearchState(handingOff));
        return;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (seenEnemy != null && collision != null)
        {
            AlienHandOffAI hitAlien = collision.gameObject.GetComponent<AlienHandOffAI>();
            if (hitAlien != null && hitAlien == seenEnemy)
            {
                InitiateHandOff();
            }
        }
    }

}

