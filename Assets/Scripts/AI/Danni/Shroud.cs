using System;
using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Serialization;

public class Shroud : MonoBehaviour
{
    [Header("Scene Refs")]
    public Transform shroudSourcePos;
    public Transform playerPos;
    public Material shroudMaterial;
    public float shroudSpreadSpeed = 1.0f;
    public float maxShroudCost = -1.0f;

    [Header("Shroud Params (material properties)")]
    // fields from the actual material, which this script will configure
    public string shroudAlphaName = ""; //empty for now     
    public string shroudDistanceName = "_RaymarchDistance"; 
    public string shroudDensityName = "_Density";
    public float baseShroudDistance = 10.0f;
    public float maxShroudDistance = 60.0f;
    public float baseGlobalDensity = 0.0f;
    public float maxGlobalDensity = 0.5f;
    public float baseAlphaAroundPlayer = 0.0f;
    public float maxAlphaAroundPlayer = 0.6f;
    public float playerShroudWrapCost = 2.0f; // how many cost units after arrival until the shroud fully envelopes the player
    
    [Header("Shroud Visual")]
    public GameObject gasPrefab;
    public Transform gasParent;
    public float gasHeightOffset = 0.05f;
    public float gasSpawnLeadTime = 0.1f;
    
    private HashSet<AIGridCell> gasCellsWithVisuals = new HashSet<AIGridCell>();

    // internal states
    private float currentGasTime = 0.0f;
    private List<AIGridCell> reachableGasCells = new List<AIGridCell>();
    private float maxReachableCost = 0.0f;
    private bool shroudInitialized = false;

    private void Start()
    {
        if (shroudSourcePos != null && shroudSourcePos != null)
        {
            StartShroud(); // automatic for now, later will be triggered manually
        }
    }

    private void Update()
    {
        if (!shroudInitialized) return;
        if (shroudSpreadSpeed <= 0.0f) return;
        float deltaTime = Time.deltaTime;
        currentGasTime += shroudSpreadSpeed * deltaTime; // shroud timer

        // clamp to configurable maximum if we decide to put any
        if (maxShroudCost > 0.0f && currentGasTime > maxShroudCost)
        {
            currentGasTime = maxShroudCost;
        }
        // UpdateDensityFromSpread();
        // UpdateLocalThickness();
        UpdateVisuals();
    }
    
    public void StartShroud()
    {
        Debug.Log("fog started");
        if (shroudSourcePos == null) return;
        if (DijkstraPathfinder.instance == null)
        {
            Debug.Log("no DijkstraPathfinder");
            return;
        }
        gasCellsWithVisuals.Clear();
        reachableGasCells = DijkstraPathfinder.instance
            .CalculateGasDistanceField(shroudSourcePos.position, maxShroudCost);
        
        maxReachableCost = 0.0f;
        for (int i = 0; i < reachableGasCells.Count; i++)
        {
            AIGridCell cell = reachableGasCells[i];
            if (cell == null)
            {
                continue;
            }

            if (cell.gCost != float.MaxValue && cell.gCost > maxReachableCost)
            {
                // getting the max reachable cost to normalize global spread
                maxReachableCost = cell.gCost; 
            }
        }
        currentGasTime = 0.0f;
        shroudInitialized = true;
    }

    /// <summary>
    /// Updates the shroud distance / density based on how far it has spread over the entire level/grid
    ///as the waves propagates through the level
    /// </summary>
    private void UpdateDensityFromSpread()
    {
        if (shroudMaterial == null)
        {
            return;
        }

        float globalSpreadNormalized = 0.0f;
        if (maxReachableCost > 0.0f)
        {
            globalSpreadNormalized = Mathf.Clamp01(currentGasTime / maxReachableCost);
        }

        float currentFogDistance = Mathf.Lerp(baseShroudDistance, maxShroudDistance, globalSpreadNormalized);
        float currentGlobalDensity = Mathf.Lerp(baseGlobalDensity, maxGlobalDensity, globalSpreadNormalized);

        if (!string.IsNullOrEmpty(shroudDistanceName))
        {
            shroudMaterial.SetFloat(shroudDistanceName, currentFogDistance);
        }

        if (!string.IsNullOrEmpty(shroudDensityName))
        {
            shroudMaterial.SetFloat(shroudDensityName, currentGlobalDensity);
        }
    }

    /// <summary>
    /// this function updates the shroud's density depending on whether player is reached by it
    ///  I am using it to trigger alerts
    /// </summary>
    private void UpdateLocalThickness()
    {
        if (shroudMaterial == null)
        {
            return;
        }

        if (playerPos == null)
        {
            return;
        }

        AIGridCell playerCell = GetClosestCellFromWorldPosition(playerPos.position);
        if (playerCell == null)
        {
            return;
        }

        float arrivalCost = playerCell.gCost;
        if (arrivalCost == float.MaxValue)
        {
            SetFogAlpha(baseAlphaAroundPlayer);
            return;
        }

        float costDif = currentGasTime - arrivalCost;

        float normalizedInsideAmount = 0.0f;

        if (costDif <= 0.0f)
        {
            normalizedInsideAmount = 0.0f;
        }
        else
        {
            if (playerShroudWrapCost <= 0.0f)
            {
                normalizedInsideAmount = 1.0f;
            }
            else
            {
                normalizedInsideAmount = Mathf.Clamp01(costDif / playerShroudWrapCost);
            }
        }

        float targetAlpha = Mathf.Lerp(baseAlphaAroundPlayer, maxAlphaAroundPlayer, normalizedInsideAmount);
        SetFogAlpha(targetAlpha);
    }

    /// <summary>
    /// Setting the transparency of the fog on the material 
    /// </summary>
    private void SetFogAlpha(float alpha)
    {
        if (shroudMaterial == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(shroudAlphaName))
        {
            return;
        }

        shroudMaterial.SetFloat(shroudAlphaName, alpha);
    }

    /// <summary>
    /// converts world position into the closest AIGridCell index
    /// </summary>
    private AIGridCell GetClosestCellFromWorldPosition(Vector3 worldPosition)
    {
        AIGrid grid = AIGrid.instance;
        if (grid == null || grid.grid == null)
        {
            return null;
        }
        Vector3 localPosition = worldPosition - grid.transform.position;

        int xIndex = Mathf.Clamp(Mathf.RoundToInt(localPosition.x), 0, grid.grid.GetLength(0) - 1);
        int yIndex = Mathf.Clamp(Mathf.RoundToInt(localPosition.y), 0, grid.grid.GetLength(1) - 1);
        int zIndex = Mathf.Clamp(Mathf.RoundToInt(localPosition.z), 0, grid.grid.GetLength(2) - 1);

        AIGridCell gridCell = grid.grid[xIndex, yIndex, zIndex];
        return gridCell;
    }
    
    private void UpdateVisuals()
    {
        if (gasPrefab == null)
        {
            return;
        }

        if (reachableGasCells == null || reachableGasCells.Count == 0)
        {
            return;
        }

        // Take the shroud source Y as the "floor" we care about
        float referenceFloorY = shroudSourcePos != null
            ? shroudSourcePos.position.y
            : 0.0f;

        for (int i = 0; i < reachableGasCells.Count; i++)
        {
            AIGridCell cell = reachableGasCells[i];
            if (cell == null)
            {
                continue;
            }

            // only use walkable cells
            if (cell.state != AIGrid.GridStates.walkable)
            {
                continue;
            }

            // only spawn on the floor layer (same Y as source)
            if (Mathf.Abs(cell.position.y - referenceFloorY) > .5f)
            {
                continue;
            }

            if (cell.gCost == float.MaxValue)
            {
                continue;
            }

            if (gasCellsWithVisuals.Contains(cell))
            {
                continue;
            }

            if (currentGasTime + gasSpawnLeadTime >= cell.gCost)
            {
                Vector3 spawnPosition = cell.position;
                spawnPosition.y += gasHeightOffset;

                GameObject newGasObject = Instantiate(gasPrefab, spawnPosition, Quaternion.identity);
                if (gasParent != null)
                {
                    newGasObject.transform.SetParent(gasParent, true);
                }

                gasCellsWithVisuals.Add(cell);
            }
        }
    }
}
