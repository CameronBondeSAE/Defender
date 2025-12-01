using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// a Pathfinder using Dijkstra’s algorithm, used for gas spreads:
///     - explores from a start cell outward in cost order
///     - returns the minimum travel cost to every reachable cell
/// </summary>
public class DijkstraPathfinder : MonoBehaviour
{
     public static DijkstraPathfinder instance;

    [Header("Neighbour Settings")]
    public bool allowVerticalNeighbours = true; 
    public float baseMovementCost = 1.0f;
    public float maxMovementCost;

    [Header("Gas Blocking Params")]
    public LayerMask gasBlockerMask = ~0; // everything for now, since our level is one layer
    public Vector3 gasBlockCheck = new Vector3(0.5f, 0.5f, 0.5f); // half sized cell boxes to cast to check for walls


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    #region Find Shortest Path

    /// <summary>
    /// Dijkstra's algorithm:
    /// - edges connect neighbouring cells and each has a cost (movement cost)
    /// - Dijkstra's algo starts from the start node, then:
    ///   1. sets its cost to 0
    ///   2. repeatedly picks the currently known node with smallest cost
    ///   3. updating neighbors: if going through this node gives the neighbour a cheaper cost,
    ///        update that neighbour's cost and remember that it came from this current node
    ///   4. once the target node is picked as the smallest cost node,
    ///      then the algorithm will know it has the most optimal cost to it
    ///   5. if no path is found, returns an empty list
    /// </summary>
    /// <param name="startWorldPosition">World position of start point.</param>
    /// <param name="endWorldPosition">World position of end point.</param>
    /// 
    public List<Vector3> CalculateShortestPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        AIGrid gridComponent = AIGrid.instance;
        if (gridComponent == null || gridComponent.grid == null)
        {
            Debug.LogWarning("AIGridDijkstraPathfinder: AIGrid.instance or AIGrid.grid is null.");
            return new List<Vector3>();
        }

        AIGridCell startCell = GetClosestValidCellFromWorldPosition(startWorldPosition);
        AIGridCell endCell = GetClosestValidCellFromWorldPosition(endWorldPosition);

        if (startCell == null || endCell == null)
        {
            Debug.LogWarning("AIGridDijkstraPathfinder: Could not resolve start or end cell.");
            return new List<Vector3>();
        }
        ResetGridCosts();
        
        startCell.gCost = 0.0f; // = distance so far

        List<AIGridCell> openSet = new List<AIGridCell>();
        HashSet<AIGridCell> closedSet = new HashSet<AIGridCell>();
        Dictionary<AIGridCell, AIGridCell> cameFrom = new Dictionary<AIGridCell, AIGridCell>();

        openSet.Add(startCell);

        while (openSet.Count > 0)
        {
            AIGridCell currentCell = GetCellWithLowestCost(openSet);

            if (currentCell == endCell)
            {
                break;
            }

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            List<AIGridCell> neighbourCells = GetNeighbourCells(currentCell);
            for (int i = 0; i < neighbourCells.Count; i++)
            {
                AIGridCell neighbourCell = neighbourCells[i];

                if (neighbourCell == null)
                {
                    continue;
                }
                
                // so my shroud ONLY permeate vertically/stays on the same floor, 
                // because Jayden treats unwalkable as having at least 3 stacked cells high
                // and our level walls aren't that tall
                if (Mathf.Abs(neighbourCell.position.y - currentCell.position.y) > 0.1f)
                {
                    continue;
                }
                // treating air as unwalkable too, so it doesn't seep over/past walls
                /* if (neighbourCell.state != AIGrid.GridStates.walkable &&
                   neighbourCell.state != AIGrid.GridStates.stairs)
                {
                   continue;
                }*/
                
                // only step through walkables
                if (!IsWalkableCell(neighbourCell))
                {
                    continue;
                }
                
                Vector3Int currentIndex   = GetCellIndex(currentCell);
                Vector3Int neighbourIndex = GetCellIndex(neighbourCell);

                bool isDiagonal =
                    currentIndex.x != neighbourIndex.x &&
                    currentIndex.z != neighbourIndex.z;

                if (isDiagonal)
                {
                    // horizontal step only
                    int side1X = neighbourIndex.x;
                    int side1Z = currentIndex.z;

                    // vertical step only
                    int side2X = currentIndex.x;
                    int side2Z = neighbourIndex.z;

                    // clamp ust in case
                    side1X = Mathf.Clamp(side1X, 0, gridComponent.grid.GetLength(0) - 1);
                    side2X = Mathf.Clamp(side2X, 0, gridComponent.grid.GetLength(0) - 1);
                    side1Z = Mathf.Clamp(side1Z, 0, gridComponent.grid.GetLength(2) - 1);
                    side2Z = Mathf.Clamp(side2Z, 0, gridComponent.grid.GetLength(2) - 1);

                    AIGridCell sideCell1 = gridComponent.grid[side1X, currentIndex.y, side1Z];
                    AIGridCell sideCell2 = gridComponent.grid[side2X, currentIndex.y, side2Z];

                    // if either side is not walkable, block diagonal
                    if (!IsWalkableCell(sideCell1) || !IsWalkableCell(sideCell2))
                    {
                        continue;
                    }
                }
                
                // don't step through actual walls
               /* Vector3 from = currentCell.position;
                Vector3 to = neighbourCell.position;
                Vector3 direction = to - from;
                float distance = direction.magnitude;

                if (distance > 0.001f)
                {
                    direction /= distance;

                    // cast a small box between the two cell centres
                    if (Physics.BoxCast(
                            from,
                            gasBlockCheck,
                            direction,
                            out RaycastHit hitInfo,
                            Quaternion.identity,
                            distance,
                            gasBlockerMask))
                    {
                        // if there's some/any geometry between the two cells, treat as blocked
                        continue;
                    }
                } */

                if (closedSet.Contains(neighbourCell))
                {
                    continue;
                }

                float movementCost = baseMovementCost + neighbourCell.eCost;
                float candidateNewCost = currentCell.gCost + movementCost;

                if (maxMovementCost > 0.0f && candidateNewCost > maxMovementCost)
                {
                    // if this cell is outside the shroud radius, skip
                    continue;
                }
                if (candidateNewCost < neighbourCell.gCost)
                {
                    neighbourCell.gCost = candidateNewCost;

                    if (!openSet.Contains(neighbourCell))
                    {
                        openSet.Add(neighbourCell);
                    }

                    cameFrom[neighbourCell] = currentCell;
                }
            }
        }

        List<Vector3> worldPath = ReconstructWorldPath(startCell, endCell, cameFrom);
        return worldPath;
    }

    #endregion

    #region Public Gas Functions

    /// <summary>
    /// expands outwards from the source along grid
    /// for each reachable AIGridCell, it stores the minimum cost in gCost
    /// if maximumTotalCost > 0, stop exploring when the next cost
    /// to explore would exceed this value
    /// <summary>
    public List<AIGridCell> CalculateGasDistanceField(Vector3 sourceWorldPosition, float maxMovementCost = -1.0f)
    {
        AIGrid gridComponent = AIGrid.instance;
        if (gridComponent == null || gridComponent.grid == null)
        {
            Debug.LogWarning("AIGrid.grid is null");
            return new List<AIGridCell>();
        }

        AIGridCell sourceCell = GetClosestValidCellFromWorldPosition(sourceWorldPosition);
        if (sourceCell == null)
        {
            Debug.LogWarning("can't find source cell for gas distance");
            return new List<AIGridCell>();
        }

        ResetGridCosts();

        sourceCell.gCost = 0.0f;

        List<AIGridCell> openSet = new List<AIGridCell>();
        HashSet<AIGridCell> closedSet = new HashSet<AIGridCell>();
        List<AIGridCell> visitedCells = new List<AIGridCell>();

        openSet.Add(sourceCell);

        while (openSet.Count > 0)
        {
            AIGridCell currentCell = GetCellWithLowestCost(openSet);
            openSet.Remove(currentCell);
            closedSet.Add(currentCell);
            visitedCells.Add(currentCell);
            
            // stop spreading if current cell cost exceeds maximumTotalCost (for limited radius spread)
            if (this.maxMovementCost > 0.0f && currentCell.gCost > maxMovementCost)
            {
                continue;
            }

            List<AIGridCell> neighbourCells = GetNeighbourCells(currentCell);
            for (int i = 0; i < neighbourCells.Count; i++)
            {
                AIGridCell neighbourCell = neighbourCells[i];

                if (neighbourCell == null)
                {
                    continue;
                }

                if (neighbourCell.state == AIGrid.GridStates.unwalkable)
                {
                    // gas don't pass through obstacles
                    continue;
                }

                if (closedSet.Contains(neighbourCell))
                {
                    continue;
                }

                float movementCost = baseMovementCost + neighbourCell.eCost;
                float candidateNewCost = currentCell.gCost + movementCost;
                
                if (this.maxMovementCost > 0.0f && candidateNewCost > this.maxMovementCost)
                {
                    continue;
                }

                if (candidateNewCost < neighbourCell.gCost)
                {
                    neighbourCell.gCost = candidateNewCost;

                    if (!openSet.Contains(neighbourCell))
                    {
                        openSet.Add(neighbourCell);
                    }
                }
            }
        }
        // now visitedCells has all cells the gas can reach
        return visitedCells;
    }

    /// <summary>
    /// This function is for visual expansion effect: 
    /// GetGasExpansionRing(currentTime, currentTime + 1) will give which cells should start showing gas in that frame
    /// </summary>
    /// <param name="minimumInclusiveCost">Minimum gCost to include.</param>
    /// <param name="maximumInclusiveCost">Maximum gCost to include.</param>
    /// <returns>List of AIGridCells whose gCost is between those values.</returns>
    public List<AIGridCell> GetGasExpansionRing(float minimumInclusiveCost, float maximumInclusiveCost)
    {
        AIGrid gridComponent = AIGrid.instance;
        List<AIGridCell> ringCells = new List<AIGridCell>();

        if (gridComponent == null || gridComponent.grid == null)
        {
            return ringCells;
        }

        int maximumX = gridComponent.grid.GetLength(0);
        int maximumY = gridComponent.grid.GetLength(1);
        int maximumZ = gridComponent.grid.GetLength(2);

        for (int xIndex = 0; xIndex < maximumX; xIndex++)
        {
            for (int yIndex = 0; yIndex < maximumY; yIndex++)
            {
                for (int zIndex = 0; zIndex < maximumZ; zIndex++)
                {
                    AIGridCell gridCell = gridComponent.grid[xIndex, yIndex, zIndex];
                    if (gridCell == null)
                    {
                        continue;
                    }

                    if (gridCell.gCost == float.MaxValue)
                    {
                        // if cell was never reached by the gas
                        continue;
                    }

                    if (gridCell.gCost >= minimumInclusiveCost && gridCell.gCost <= maximumInclusiveCost)
                    {
                        ringCells.Add(gridCell);
                    }
                }
            }
        }

        return ringCells;
    }

    #endregion

    #region Internal Helpers

    /// <summary>
    /// resets all gCost, hCost and fCost on cells
    /// </summary>
    private void ResetGridCosts()
    {
        AIGrid gridComponent = AIGrid.instance;
        if (gridComponent == null || gridComponent.grid == null)
        {
            return;
        }

        int maximumX = gridComponent.grid.GetLength(0);
        int maximumY = gridComponent.grid.GetLength(1);
        int maximumZ = gridComponent.grid.GetLength(2);

        for (int xIndex = 0; xIndex < maximumX; xIndex++)
        {
            for (int yIndex = 0; yIndex < maximumY; yIndex++)
            {
                for (int zIndex = 0; zIndex < maximumZ; zIndex++)
                {
                    AIGridCell gridCell = gridComponent.grid[xIndex, yIndex, zIndex];
                    if (gridCell == null)
                    {
                        continue;
                    }

                    gridCell.gCost = float.MaxValue;
                    gridCell.hCost = float.MaxValue;
                    gridCell.fCost = float.MaxValue;
                }
            }
        }
    }

    /// <summary>
    /// picks the AIGridCell with the lowest gCost from a list 
    /// </summary>
    private AIGridCell GetCellWithLowestCost(List<AIGridCell> cellList)
    {
        AIGridCell bestCell = null;
        float bestCost = float.MaxValue;

        for (int i = 0; i < cellList.Count; i++)
        {
            AIGridCell candidateCell = cellList[i];
            if (candidateCell.gCost < bestCost)
            {
                bestCost = candidateCell.gCost;
                bestCell = candidateCell;
            }
        }

        return bestCell;
    }

    /// <summary>
    /// converts a world position to the closest valid AIGridCell
    /// if the exact index is null/unwalkable this will still try to return it 
    /// because for gas it might still be relevant as a source/destination
    /// </summary>
    private AIGridCell GetClosestValidCellFromWorldPosition(Vector3 worldPosition)
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

    /// <summary>
    /// returns neighbouring cells for a given cell which will define how the gas spreads
    /// </summary>
    private List<AIGridCell> GetNeighbourCells(AIGridCell cell)
    {
        List<AIGridCell> neighbourCells = new List<AIGridCell>();

        AIGrid gridComponent = AIGrid.instance;
        if (gridComponent == null || gridComponent.grid == null)
        {
            return neighbourCells;
        }

        Vector3 localPosition = cell.position - gridComponent.transform.position;

        int xIndex = Mathf.RoundToInt(localPosition.x);
        int yIndex = Mathf.RoundToInt(localPosition.y);
        int zIndex = Mathf.RoundToInt(localPosition.z);

        int xStep = Mathf.Max(1, Mathf.CeilToInt(gridComponent.cellSize.x));
        int yStep = Mathf.Max(1, Mathf.CeilToInt(gridComponent.cellSize.y));
        int zStep = Mathf.Max(1, Mathf.CeilToInt(gridComponent.cellSize.z));

        int maximumX = gridComponent.grid.GetLength(0);
        int maximumY = gridComponent.grid.GetLength(1);
        int maximumZ = gridComponent.grid.GetLength(2);

        // X axis
        TryAddNeighbour(neighbourCells, xIndex + xStep, yIndex, zIndex, maximumX, maximumY, maximumZ);
        TryAddNeighbour(neighbourCells, xIndex - xStep, yIndex, zIndex, maximumX, maximumY, maximumZ);

        // Z axis
        TryAddNeighbour(neighbourCells, xIndex, yIndex, zIndex + zStep, maximumX, maximumY, maximumZ);
        TryAddNeighbour(neighbourCells, xIndex, yIndex, zIndex - zStep, maximumX, maximumY, maximumZ);

        if (allowVerticalNeighbours)
        {
            // Y axis
            TryAddNeighbour(neighbourCells, xIndex, yIndex + yStep, zIndex, maximumX, maximumY, maximumZ);
            TryAddNeighbour(neighbourCells, xIndex, yIndex - yStep, zIndex, maximumX, maximumY, maximumZ);
        }

        return neighbourCells;
    }

    private void TryAddNeighbour(List<AIGridCell> neighbourCells, int xIndex, int yIndex, int zIndex, int maximumX, int maximumY, int maximumZ)
    {
        if (xIndex < 0 || yIndex < 0 || zIndex < 0) return;
        if (xIndex >= maximumX || yIndex >= maximumY || zIndex >= maximumZ) return;

        AIGridCell gridCell = AIGrid.instance.grid[xIndex, yIndex, zIndex];
        if (gridCell != null)
        {
            neighbourCells.Add(gridCell);
        }
    }

    /// <summary>
    /// reconstructs the path from start to end using the
    /// cameFrom dictionary filled by CalculateShortestPath
    /// </summary>
    private List<Vector3> ReconstructWorldPath(
        AIGridCell startCell,
        AIGridCell endCell,
        Dictionary<AIGridCell, AIGridCell> cameFrom)
    {
        List<Vector3> worldPath = new List<Vector3>();

        if (startCell == null || endCell == null)
        {
            return worldPath;
        }

        if (!cameFrom.ContainsKey(endCell) && endCell != startCell)
        {
            // no path
            return worldPath;
        }

        AIGridCell currentCell = endCell;
        worldPath.Add(currentCell.position);

        while (currentCell != startCell)
        {
            if (!cameFrom.ContainsKey(currentCell))
            {
                break;
            }

            currentCell = cameFrom[currentCell];
            worldPath.Add(currentCell.position);
        }

        worldPath.Reverse();
        return worldPath;
    }
    
    private bool IsWalkableCell(AIGridCell cell)
    {
        if (cell == null)
        {
            return false;
        }

        return cell.state == AIGrid.GridStates.walkable;
    }

    /// <summary>
    /// convert a cell's world position back to grid indexes
    /// </summary>
    private Vector3Int GetCellIndex(AIGridCell cell)
    {
        AIGrid gridComponent = AIGrid.instance;
        Vector3 localPosition = cell.position - gridComponent.transform.position;

        int xIndex = Mathf.RoundToInt(localPosition.x);
        int yIndex = Mathf.RoundToInt(localPosition.y);
        int zIndex = Mathf.RoundToInt(localPosition.z);

        return new Vector3Int(xIndex, yIndex, zIndex);
    }


    #endregion
}
