using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using System.Collections;

namespace Jayden
{

    public class AIPathFindingView : MonoBehaviour
    {
        [SerializeField] public Vector2 viewDistance = new Vector2(5, 5);
        public Vector2 scaledViewDistance;
        public int characterY = 10000000;
        //[SerializeField] public Vector3[,,] seenCells; // Allows me to map out a world with coordinates easily // Unused for stability and support
        [SerializeField] public List<Vector3> seenCells = new List<Vector3>(); // Allows me to map out a world with coordinates easily
        [SerializeField] public List<Vector3> seenItemPositions = new List<Vector3>();
        [SerializeField] public List<LayerMask> viewableLayers = new List<LayerMask>();
        [SerializeField] public List<Vector3> lockedPositions = new List<Vector3>();
        [SerializeField] public List<ItemData> seenItems = new List<ItemData>();
        [SerializeField] public List<Vector3> currentLookingAt = new List<Vector3>();
        [SerializeField] public ItemData itemInFront = null;

        private void Start()
        {
            scaledViewDistance = new Vector2(Mathf.CeilToInt(viewDistance.x), Mathf.CeilToInt(viewDistance.y));
            StartCoroutine(View()); // Starts the view that contantly runs
        }


        private void FixedUpdate()
        {
            SetCharacterY();
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
                                if ((!NewUnity.ContainsV3(checkPositions, pos) && (!NewUnity.ContainsV3(seenCells, pos) || NewUnity.ContainsV3(seenItemPositions, pos))))
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
                    for (int i = 0; i < checkPositions.Count; i++)
                    {
                        if (!NewUnity.ContainsV3(AIGrid.instance.unwalkableGrid, checkPositions[i])) // Unwalkable can't contain anything or be interacted with, so no need to check
                        {
                            foreach (LayerMask layer in viewableLayers)
                            {
                                // Checks the cell on the layer
                                RaycastHit hit2;
                                bool hitDetction2 = Physics.BoxCast(checkPositions[i], AIGrid.instance.scaledCellSize / 2, checkPositions[i], out hit2, Quaternion.identity, Mathf.Infinity, layer);
                                NewUnity.DrawBoxCastBox(checkPositions[i], AIGrid.instance.scaledCellSize / 2, Quaternion.identity, checkPositions[i], Mathf.Infinity, Color.white);
                                if (hitDetction2)
                                {
                                    ItemData itemData = hit2.collider.GetComponent<ItemData>();
                                    if (itemData != null)
                                    {
                                        if (!NewUnity.ContainsV3(itemData.position, checkPositions[i]))
                                        {
                                            itemData.position.Add(checkPositions[i]);
                                        }

                                        if (!seenItems.Contains(itemData))
                                        {
                                            seenItems.Add(itemData);
                                        }
                                        foreach (Vector3 pos in itemData.position)
                                        {
                                            if (!NewUnity.ContainsV3(seenItemPositions, pos)) seenItemPositions.Add(pos);
                                            if (!NewUnity.ContainsV3(lockedPositions, pos)) lockedPositions.Add(pos);
                                        }
                                        if (true)
                                        {
                                            if (!itemFound && itemInFront != itemData)
                                            {
                                                itemInFront = itemData;
                                            }
                                            if (itemData != null) itemFound = true;
                                        }

                                    }
                                }
                                else if (NewUnity.ContainsV3(seenItemPositions, checkPositions[i])) // Removes item from list if it is no longer there
                                {
                                    seenItemPositions.Remove(checkPositions[i]);
                                    List<ItemData> tmpItems = seenItems;
                                    for (int j = 0; j < seenItems.Count; j++)
                                    {
                                        if (NewUnity.ContainsV3(seenItems[j].position, checkPositions[i])) tmpItems.Remove(seenItems[j]);
                                    }
                                    seenItems.Clear();
                                    seenItems = tmpItems;


                                }
                            }

                        }
                        //seenCells[(int)checkPositions[i].x, (int)checkPositions[i].y, (int)checkPositions[i].z] = (checkPositions[i]);
                        if (!NewUnity.ContainsV3(seenCells, checkPositions[i])) seenCells.Add(checkPositions[i]);
                    }
                    currentLookingAt.Clear();
                    currentLookingAt = checkPositions;
                }
                catch (Exception e)
                {
                    //Debug.Log(e);
                }

                yield return null;
                //Debug.Log("Active");
            }

        }
    }
}