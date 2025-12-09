using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Jayden
{
    [RequireComponent(typeof(AIPathFindingView))]
    public class AIPathFindingSmartGround : AIPathFindingGround
    {




        [SerializeField] protected AIGoals currentTask = AIGoals.goToGoal;
        [SerializeField] protected ItemData.ItemTypes fetchingItem = ItemData.ItemTypes.nothing;

        [SerializeField] protected List<ItemData.ItemTypes> inventory = new List<ItemData.ItemTypes>();
        [SerializeField] protected List<ItemData.ItemTypes> neededItems = new List<ItemData.ItemTypes>();

        [SerializeField] protected List<ListBuffer> pathCellPositionsBuffer = new List<ListBuffer>();

        AIPathFindingView aIPathFindingView;

        bool startWander = false;

        /// <summary>
        /// Different goals/tasks for the AI agent
        /// </summary>
        public enum AIGoals
        {
            goToGoal,
            wander,
            fetch
        }

        protected override void Start()
        {
            aIPathFindingView = GetComponent<AIPathFindingView>();
            base.Start();
        }


        /// <summary>
        /// Determines what to do based on the current goal/task
        /// </summary>
        protected override void FixedUpdate()
        {

            ItemBehaviour(); // Doesn't matter what task is happening, should run this at the start always, hence out of switch statement
            switch (currentTask)
            {
                default:
                    base.FixedUpdate();
                    break;
                case AIGoals.goToGoal:
                    base.FixedUpdate();
                    break;
                case AIGoals.fetch:
                    ItemBehaviour();
                    CheckForItems();
                    FollowPath();
                    break;
                case AIGoals.wander:
                    CheckForItems();
                    Wander();
                    FollowPath();
                    break;
            }

        }

        protected override void CheckGoal()
        {
            base.CheckGoal();
        }

        protected override void Update()
        {
            base.Update();
        }


        /// <summary>
        /// Checks if a needed item has been seen
        /// </summary>
        protected virtual void CheckForItems()
        {
            bool foundMatch = false;
            foreach (ItemData.ItemTypes item in neededItems)
            {
                ItemNeeded(item);
                foundMatch = true;
            }
            if (!foundMatch && neededItems.Count > 0)
            {
                currentTask = AIGoals.wander;
            }
        }






        /// <summary>
        /// Wanders around to fill in the unviewed areas
        /// </summary>
        protected virtual void Wander()
        {
            //if (Array.IndexOf(aIPathFindingView.seenCells, pathTo) != -1)
            if (startWander || NewUnity.ContainsV3(aIPathFindingView.seenCells, pathTo)) // Prevents a new wander spot from being selected when it doesn't need to be.
            {
                startWander = false;

                // Ensures the code doesn't try going outside of the supported area in the grid
                int[] xBounds = new int[2] { AIGrid.instance.grid.GetLowerBound(0), AIGrid.instance.grid.GetUpperBound(0) };
                int[] zBounds = new int[2] { AIGrid.instance.grid.GetLowerBound(2), AIGrid.instance.grid.GetUpperBound(2) };
                int[] yBounds = new int[2] { AIGrid.instance.grid.GetLowerBound(1), AIGrid.instance.grid.GetUpperBound(1) };

                List<Vector3> checkedPositions = new List<Vector3>();

                CalculatedPathData pathDataCore = null;

                // Stores variables locally to prevent issues from changing while function is running
                int localcharacterY = characterY;
                Vector3 localPosition = transform.position;

                // Goal is to fill out the viewed area so random works the most optimally

                while (checkedPositions.Count < (xBounds[0] + xBounds[1]) * (zBounds[0] + zBounds[1]))
                {
                    if (localcharacterY > yBounds[0] && localcharacterY < yBounds[1])
                    {
                        AIGridCell tmpCell = AIGrid.instance.grid[UnityEngine.Random.Range(xBounds[0], xBounds[1]), localcharacterY, UnityEngine.Random.Range(zBounds[0], zBounds[1])];
                        if (!NewUnity.ContainsV3(checkedPositions, tmpCell.position))
                        {
                            checkedPositions.Add(tmpCell.position);
                            //if (Array.IndexOf(aIPathFindingView.seenCells, tmpCell.position) != -1 && (tmpCell.state == AIGrid.GridStates.walkable || tmpCell.state == AIGrid.GridStates.stairs))
                            if (!NewUnity.ContainsV3(aIPathFindingView.seenCells, tmpCell.position) && (tmpCell.state == AIGrid.GridStates.walkable || tmpCell.state == AIGrid.GridStates.stairs))
                            {

                                CalculatedPathData pathData = null;

                                // Tries multiple times to account for errors
                                for (int i = 0; i < 5; i++)
                                {
                                    try
                                    {
                                        pathData = AIPathFindingCore.CalculatePathCore(tmpCell.position, localPosition, 0, aIPathFindingView.lockedPositions);
                                    }
                                    catch { }
                                    if (pathData != null && !pathData.failure) break;
                                }
                                if (pathData != null && !pathData.failure && pathData.pathFound)
                                {
                                    pathDataCore = pathData;
                                    pathDataCore.goal = tmpCell.position;
                                    break;
                                }
                            }
                        }
                    }
                    else break;


                }

                if (pathDataCore != null) // Something has been found, variables should be set
                {
                    pathTo = pathDataCore.goal;
                    pathCellPositions = pathDataCore.pathCellPositions;
                }

                else // Failure to find somewhere, should go again
                {
                    // Idk, maybe explode
                    startWander = true;
                }

            }
            return;
        }


        /// <summary>
        /// Attempts to fetch a needed item if it has been seen before continuing or going around if faster
        /// </summary>
        /// <param name="fetchingItemType"></param>
        protected virtual void ItemNeeded(ItemData.ItemTypes fetchingItemType)
        {
            ItemData shortestKey = null;
            float shortestKeyCost = float.MaxValue;

            CalculatedPathData pathData1 = null;

            if (pathCellPositionsBuffer.Count == 0) // Only runs when there is nothing in the buffer used for fetching
            {

                // Checks if the needed item has been seen, path finds to it if it has been.
                foreach (ItemData item in aIPathFindingView.seenItems)
                {
                    if (item.type == fetchingItemType)
                    {
                        CalculatedPathData pathData = null;
                        for (int i = 0; i < 5; i++)
                        {
                            try
                            {
                                pathData = AIPathFindingCore.CalculatePathCore(item.position[0], transform.position, 0);
                            }
                            catch { }
                            if (pathData != null && !pathData.failure) break;
                        }
                        if (pathData != null && pathData.pathFound)
                        {
                            if ((shortestKey == null || shortestKeyCost < pathData.cost)) // Gets the shortest path if multiple of the item has been seen
                            {
                                shortestKey = item;
                                pathData1 = pathData;
                            }
                        }

                    }
                }

                // Gets the path for back to where it needs to go for the goal
                CalculatedPathData pathData2 = null;
                if (shortestKey != null)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            pathData2 = AIPathFindingCore.CalculatePathCore(pathTo, shortestKey.position[0], 0);
                        }
                        catch { }
                        if (pathData2 != null && !pathData2.failure) break;
                    }
                }
                else // If none of the needed item was found, agent should start wandering to find one
                {
                    currentTask = AIGoals.wander;
                    startWander = true;
                    return;
                }

                // Adds the positions of the item in front of the agent to a list for blocked off areas
                List<Vector3> invalidPoints = aIPathFindingView.lockedPositions;
                foreach (Vector3 pos in aIPathFindingView.itemInFront.position)
                {
                    invalidPoints.Add(pos);
                }


                // Checks if there is a faster way to get to the goal instead of getting the item and going back
                CalculatedPathData pathData3 = null;

                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        pathData3 = AIPathFindingCore.CalculatePathCore(pathTo, transform.position, 0, invalidPoints);
                    }
                    catch { }
                    if (pathData3 != null && !pathData3.failure) break;
                }

                List<AIGridCell> gridPathData = new List<AIGridCell>();
                List<ListBuffer> listBuffer = new List<ListBuffer>();
                CalculatedPathData pathData4 = null;

                if (pathData3 == null || !pathData3.pathFound) // Sets the cost of the alternate path high if one isn't found so the previous path is chosen
                {
                    pathData4 = new CalculatedPathData();
                    pathData4.cost = float.MaxValue;
                }
                else
                {
                    pathData4 = pathData3;
                }


                // Checks the length of attempting each path
                if (pathData2 != null && pathData2.cost + shortestKeyCost < pathData4.cost) // If collecting the item is faster
                {
                    ListBuffer listBuffer1 = new ListBuffer();
                    listBuffer1.aiGridCellB = pathData1.pathCells;
                    listBuffer.Add(listBuffer1);
                    listBuffer1 = new ListBuffer();
                    listBuffer1.aiGridCellB = pathData2.pathCells;
                    listBuffer.Add(listBuffer1);
                }
                else if (pathData3 != null && pathData3.pathFound) // If the alternate path exists and is faster
                {
                    ListBuffer listBuffer1 = new ListBuffer();
                    listBuffer1.aiGridCellB = pathData3.pathCells;
                    listBuffer.Add(listBuffer1);
                }
                else // If no alternate path is avalaible but collecting would take way too long
                {
                    currentTask = AIGoals.wander;
                    startWander = true;
                    return;
                }


                // Sets variables and adds them to lists for following
                List<ListBuffer> listBuffer4 = new List<ListBuffer>();

                foreach (ListBuffer buffer in listBuffer)
                {
                    List<Vector3> tmpList2 = new List<Vector3>();
                    ListBuffer listBuffer3 = new ListBuffer();
                    foreach (AIGridCell cell in buffer.aiGridCellB)
                    {
                        tmpList2.Add(cell.position);
                    }
                    listBuffer3.vector3B = tmpList2;
                    listBuffer4.Add(listBuffer3);

                }
                currentTask = AIGoals.fetch;
                pathCellPositionsBuffer.Clear();
                pathCellPositionsBuffer = listBuffer4;
                pathCellPositions.Clear();
            }


        }


        /// <summary>
        /// Item specific behaviour when interacting with them
        /// </summary>
        protected virtual void ItemBehaviour()
        {
            if (aIPathFindingView.itemInFront == null) return;
            else
            {
                if (aIPathFindingView.itemInFront.type == ItemData.ItemTypes.key) // Collects the item if it is a key
                {
                    InteractItem(true);
                }
                else if (aIPathFindingView.itemInFront.type == ItemData.ItemTypes.door)
                {
                    if (inventory.Contains(ItemData.ItemTypes.key)) // Checks if the agent's inventory contains a key to use on the door, using it if avalaible
                    {
                        foreach (Vector3 pos in aIPathFindingView.itemInFront.position)
                        {
                            aIPathFindingView.lockedPositions.Remove(pos);
                        }
                        InteractItem(false, AIGoals.fetch, ItemData.ItemTypes.key);
                    }
                    else // Marks a key as needed for the door if not in inventory
                    {
                        if (!neededItems.Contains(ItemData.ItemTypes.key)) neededItems.Add(ItemData.ItemTypes.key);
                        if (currentTask != AIGoals.wander) currentTask = AIGoals.fetch;
                        pathCellPositions.Clear();
                    }
                }
            }
            return;
        }


        /// <summary>
        /// Removes an item when collected. Removes item from inventory if one is needed
        /// </summary>
        /// <param name="collect"></param>
        /// <param name="goal"></param>
        /// <param name="useItem"></param>
        protected virtual void InteractItem(bool collect, AIGoals task = AIGoals.goToGoal, ItemData.ItemTypes useItem = ItemData.ItemTypes.nothing)
        {
            if (collect) inventory.Add(aIPathFindingView.itemInFront.type);

            ActiveToggle.instance.ToggleActive(aIPathFindingView.itemInFront.gameObject, false);
            currentTask = task;
            if (useItem != ItemData.ItemTypes.nothing)
            {
                inventory.Remove(useItem);
            }
            return;
        }


        /// <summary>
        /// Constantly checks a triangle in front and records what is seen
        /// </summary>
        /// <returns></returns>



        private void OnDrawGizmos() // Used for map visualisation for performance purposes
        {
            // Cell type visualisation through Gizmos

            if (AIGrid.instance != null)
            {
                if (AIGrid.instance.showVisualisations)
                {
                    for (int i = 0; i < aIPathFindingView.currentLookingAt.Count; i += 1)
                    {
                        if (i > 0) Gizmos.color = new Color(1, 0, 1);
                        else Gizmos.color = new Color(0, 0, 0); // First is black for debugging purposes
                        Gizmos.DrawCube(aIPathFindingView.currentLookingAt[i], AIGrid.instance.scaledCellSize);
                    }
                }

            }

        }




        protected override void FollowPath()
        {
            // If the path finding is empty, the buffer is checked and used if avaliable
            if (pathCellPositions.Count < 1)
            {
                if (pathCellPositionsBuffer.Count > 0)
                {
                    pathCellPositions = pathCellPositionsBuffer[0].vector3B;
                    pathCellPositionsBuffer.RemoveAt(0);
                }
            }

            base.FollowPath();

            // Visualisations
            try
            {
                clearCalculatedPathVisualisation.Invoke();
            }
            catch { }
            try
            {
                foreach (Vector3 pos1 in pathCellPositions)
                {
                    //if (enableMyVisualisations) VisualisationSetter.instance.SpawnVisualisation(pos1, AIGrid.instance.scaledCellSize, VisualisationSetter.VisualisationStates.calculatedPath, gameObject);
                }
            }
            catch { }
        }

        protected override void ColliderJump(Collision collision)
        {

            if (!NewUnity.ContainsLM(aIPathFindingView.viewableLayers, collision.gameObject.layer)) // Prevents character from climibing doors and other items
            {
                Debug.Log(collision.gameObject.layer);
                base.ColliderJump(collision);
            }
        }

        protected override IEnumerator CalculatePath() // Uses the core path finding functionality for more consistent behaviour with other functions and better functionality tie in
        {
            if (!awaitCalculation)
            {
                if (pathTo == Vector3.zero) SetPathTo();
                awaitCalculation = true;
                pathCalculated = true;
                CalculatedPathData data = new CalculatedPathData();
                //Debug.Log("Start Calculation");



                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        data = AIPathFindingCore.CalculatePathCore(pathTo, transform.position, 0, aIPathFindingView.lockedPositions);
                        Debug.Log($"{pathCellPositions.Count} {data.pathCellPositions.Count} {characterY}");
                    }
                    catch { }
                    if (data != null && !data.failure)
                    {
                        pathCellPositionsBuffer.Clear();
                        pathCellPositions.Clear();
                        pathCellPositions = data.pathCellPositions;
                        //Debug.Log($"{pathCellPositions.Count} {data.pathCellPositions.Count} {characterY}");
                        pathCalculated = true;
                        //Debug.Log("Calculation Success");
                        break;
                    }
                    yield return null;
                }
                if (data == null || data.failure)
                {
                    pathCalculated = false;
                    //Debug.Log("Calculation Failure");
                }

                yield return null;


                awaitCalculation = false;
            }
            yield return null;




        }

    }

}