using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Shroud : MonoBehaviour
{
    [Header("Scene References")]
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

    // internal states
    private float currentGasTime = 0.0f;
    private List<AIGridCell> reachableGasCells = new List<AIGridCell>();
    private float maximumReachableGasCost = 0.0f;
    private bool gasHasBeenInitialized = false;

    private void Start()
    {
        if (shroudSourcePos != null)
        {
            StartShroud(); // automatic for now, later will be triggered manually
        }
    }

    private void Update()
    {
        if (!gasHasBeenInitialized)
        {
            return;
        }

        if (shroudSpreadSpeed <= 0.0f)
        {
            return;
        }
        
        float deltaTime = Time.deltaTime;
        currentGasTime += shroudSpreadSpeed * deltaTime; // shroud timer

        // clamp to configurable maximum if we decide to put any
        if (maxShroudCost > 0.0f && currentGasTime > maxShroudCost)
        {
            currentGasTime = maxShroudCost;
        }

        UpdateGlobalFogFromGas();
        UpdateLocalThickness();
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

        reachableGasCells = DijkstraPathfinder.instance
            .CalculateGasDistanceField(shroudSourcePos.position, maxShroudCost);
        
        maximumReachableGasCost = 0.0f;
        for (int i = 0; i < reachableGasCells.Count; i++)
        {
            AIGridCell cell = reachableGasCells[i];
            if (cell == null)
            {
                continue;
            }

            if (cell.gCost != float.MaxValue && cell.gCost > maximumReachableGasCost)
            {
                // getting the max reachable cost to normalize global spread
                maximumReachableGasCost = cell.gCost; 
            }
        }
        currentGasTime = 0.0f;
        gasHasBeenInitialized = true;
    }

    /// <summary>
    /// Updates the shroud distance / density based on how far it has spread over the entire level/grid
    ///as the waves propagates through the level
    /// </summary>
    private void UpdateGlobalFogFromGas()
    {
        if (shroudMaterial == null)
        {
            return;
        }

        float globalSpreadNormalized = 0.0f;
        if (maximumReachableGasCost > 0.0f)
        {
            globalSpreadNormalized = Mathf.Clamp01(currentGasTime / maximumReachableGasCost);
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

        float costDelta = currentGasTime - arrivalCost;

        float normalizedInsideAmount = 0.0f;

        if (costDelta <= 0.0f)
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
                normalizedInsideAmount = Mathf.Clamp01(costDelta / playerShroudWrapCost);
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
        AIGrid gridComponent = AIGrid.instance;
        if (gridComponent == null || gridComponent.grid == null)
        {
            return null;
        }

        Vector3 localPosition = worldPosition - gridComponent.transform.position;

        int xIndex = Mathf.Clamp(Mathf.RoundToInt(localPosition.x), 0, gridComponent.grid.GetLength(0) - 1);
        int yIndex = Mathf.Clamp(Mathf.RoundToInt(localPosition.y), 0, gridComponent.grid.GetLength(1) - 1);
        int zIndex = Mathf.Clamp(Mathf.RoundToInt(localPosition.z), 0, gridComponent.grid.GetLength(2) - 1);

        AIGridCell gridCell = gridComponent.grid[xIndex, yIndex, zIndex];
        return gridCell;
    }
}
