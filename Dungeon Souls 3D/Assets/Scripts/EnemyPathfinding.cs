using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{ 
	//A star algorithm for enemy pathfinding 
	[Header("Movement Settings")]
	[SerializeField] private float moveSpeed = 2f;
	[SerializeField] private float rotationSpeed = 5f;
	[SerializeField] private float stoppingDistance = 1.5f;
	[SerializeField] private float pathRecalculationTime = 0.5f;
	[SerializeField] private float enemyAvoidanceRadius = 1.0f;

	[Header("Detection Settings")]
	[SerializeField] private float detectionRange = 10f;
	[SerializeField] private float fieldOfView = 90f;
	[SerializeField] private LayerMask obstacleLayer;
	[SerializeField] private LayerMask playerLayer;

	[Header("Attack Settings")]
	[SerializeField] private float attackRadius = 2.0f;

	[Header("Pathfinding Settings")]
	[SerializeField] private bool strictlyAvoidOccupiedCells = true; // If true, will never choose an occupied cell
	[SerializeField] private float occupiedCellPenalty = 10f; // Higher penalty makes occupied cells less attractive

	[Header("Debug")]
	[SerializeField] private bool showDetection = true;
	[SerializeField] private bool showPath = false;

	// Attack state
	[HideInInspector] public bool canAttack = false;

	// A* Pathfinding variables
	private List<Cell> path = new List<Cell>();
	private int currentPathIndex = 0;
	private bool isPathValid = false;
	private bool isChasing = false;

	// Component references
	private ProceduralRoom currentRoom;
	private Transform playerTransform;
	private Coroutine pathfindingCoroutine;

	// Enemy state
	private Vector3 lastKnownPlayerPosition;
	private bool canSeePlayer = false;

	// Path optimization
	private int pathfindingAttempts = 0;
	private const int maxPathfindingAttempts = 3;
	private float pathfindingTimeout = 0f;
	private const float pathfindingTimeoutDuration = 2f;

	public Animator animator;

	public void Start()
	{
		playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
		animator = GetComponent<Animator>();
		UpdateCurrentRoom();

		if (currentRoom != null)
		{
			pathfindingCoroutine = StartCoroutine(PathfindingRoutine());
		}
	}

	private void Update()
	{
		if (currentRoom == null)
		{
			UpdateCurrentRoom();
			return;
		}

		CheckPlayerVisibility();

		CheckAttackRange();

		if (isPathValid && path.Count > 0 && !canAttack)
		{
			MoveAlongPath();
			if (animator != null)
			{
				animator.SetFloat("speed", 1f);
			}
		}
		else
		{
			if (animator != null)
			{
				animator.SetFloat("speed", 0f);
			}

			if (pathfindingTimeout > 0)
			{
				// Handle timeout for pathfinding
				pathfindingTimeout -= Time.deltaTime;
			}
		}

		// Check if the current path contains occupied cells and recalculate if needed
		if (isPathValid && path.Count > 0 && currentPathIndex < path.Count)
		{
			// If the next cell in the path is occupied, recalculate path
			Cell nextCell = path[currentPathIndex];
			if (nextCell.isOccupied && strictlyAvoidOccupiedCells)
			{
				if (pathfindingCoroutine != null)
				{
					StopCoroutine(pathfindingCoroutine);
					pathfindingCoroutine = StartCoroutine(PathfindingRoutine());
				}
			}
		}
	}

	private void CheckAttackRange()
	{
		if (playerTransform == null) return;

		float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

		// Check if player is within attack radius
		if (distanceToPlayer <= attackRadius && canSeePlayer)
		{
			canAttack = true;

			// Face player
			Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
			if (directionToPlayer != Vector3.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
			}
		}
		else
		{
			canAttack = false;
		}
	}

	private void UpdateCurrentRoom()
	{
		// Find the room that contains this enemy
		ProceduralRoom[] rooms = FindObjectsOfType<ProceduralRoom>();

		foreach (ProceduralRoom room in rooms)
		{
			// Make sure the room has a grid before checking
			if (room.GetComponent<RoomGridSystem>() == null || !room.GetComponent<RoomGridSystem>().HasGrid())
			{
				continue;
			}

			if (IsPositionInRoom(transform.position, room))
			{
				currentRoom = room;
				return;
			}
		}
	}

	private bool IsPositionInRoom(Vector3 position, ProceduralRoom room)
	{
		// check if room has a valid grid
		if (!room.GetComponent<RoomGridSystem>().HasGrid())
		{
			return false;
		}

		// Convert world position to grid position
		Vector2Int gridPos = room.WorldToGrid(position);

		// Check if the position is within the grid bounds
		Cell cell = room.GetCell(gridPos.x, gridPos.y);
		return cell != null && cell.isWalkable;
	}

	private void CheckPlayerVisibility()
	{
		if (playerTransform == null) return;

		Vector3 directionToPlayer = playerTransform.position - transform.position;
		float distanceToPlayer = directionToPlayer.magnitude;

		// Check if player is within detection range
		if (distanceToPlayer <= detectionRange)
		{
			// Check if player is within field of view
			float angle = Vector3.Angle(transform.forward, directionToPlayer);
			if (angle <= fieldOfView / 2)
			{
				// Check if there's a clear line of sight
				RaycastHit hit;
				if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out hit, detectionRange, obstacleLayer | playerLayer))
				{
					// If raycast hits player
					if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0)
					{
						canSeePlayer = true;
						lastKnownPlayerPosition = playerTransform.position;

						if (!isChasing)
						{
							isChasing = true;
							// Recalculate path immediately when player is spotted
							if (pathfindingCoroutine != null)
							{
								StopCoroutine(pathfindingCoroutine);
							}
							pathfindingCoroutine = StartCoroutine(PathfindingRoutine());
						}
						return;
					}
				}
			}
		}

		canSeePlayer = false;
	}

	private IEnumerator PathfindingRoutine()
	{
		while (true)
		{
			if (pathfindingTimeout <= 0)
			{
				pathfindingAttempts = 0;
			}

			Vector3 targetPosition;

			if (isChasing && playerTransform != null)
			{
				targetPosition = canSeePlayer ? playerTransform.position : lastKnownPlayerPosition;

				CalculatePath(transform.position, targetPosition);

				if (!canSeePlayer && Vector3.Distance(transform.position, lastKnownPlayerPosition) < stoppingDistance)
				{
					isChasing = false;
				}
			}
			else
			{
				isPathValid = false;
				path.Clear();
			}
			yield return new WaitForSeconds(pathRecalculationTime);
		}
	}

	private void CalculatePath(Vector3 startPos, Vector3 targetPos)
	{
		if (pathfindingTimeout > 0 || currentRoom == null)
		{
			return;
		}

		Vector2Int startNode = currentRoom.WorldToGrid(startPos);
		Vector2Int targetNode = currentRoom.WorldToGrid(targetPos);

		Cell startCell = currentRoom.GetCell(startNode.x, startNode.y);
		Cell targetCell = currentRoom.GetCell(targetNode.x, targetNode.y);

		if (startCell == null || targetCell == null)
		{
			isPathValid = false;
			path.Clear();
			return;
		}

		if (!targetCell.isWalkable || (strictlyAvoidOccupiedCells && targetCell.isOccupied))
		{
			Cell alternativeTarget = FindNearestWalkableCell(targetCell);
			if (alternativeTarget != null)
			{
				targetCell = alternativeTarget;
			}
			else
			{
				isPathValid = false;
				path.Clear();
				return;
			}
		}
		List<Cell> newPath = FindPath(startCell, targetCell);

		if (newPath != null && newPath.Count > 0)
		{
			path = newPath;
			currentPathIndex = 0;
			isPathValid = true;
			pathfindingAttempts = 0;
		}
		else
		{
			isPathValid = false;

			pathfindingAttempts++;

			if (pathfindingAttempts >= maxPathfindingAttempts)
			{
				pathfindingTimeout = pathfindingTimeoutDuration;
			}
		}
	}

	private Cell FindNearestWalkableCell(Cell targetCell)
	{
		int maxRadius = 5;
		Vector2Int cellPos = currentRoom.WorldToGrid(targetCell.worldPosition);
		for (int radius = 1; radius <= maxRadius; radius++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				for (int z = -radius; z <= radius; z++)
				{
					if (Mathf.Abs(x) == radius || Mathf.Abs(z) == radius)
					{
						Cell cell = currentRoom.GetCell(cellPos.x + x, cellPos.y + z);
						if (cell != null && cell.isWalkable && (!strictlyAvoidOccupiedCells || !cell.isOccupied))
						{
							return cell;
						}
					}
				}
			}
		}

		// If we can't find a non-occupied cell and we're strictly avoiding them,
		// but we need a cell anyway, try again without the occupied restriction
		if (strictlyAvoidOccupiedCells)
		{
			for (int radius = 1; radius <= maxRadius; radius++)
			{
				for (int x = -radius; x <= radius; x++)
				{
					for (int z = -radius; z <= radius; z++)
					{
						if (Mathf.Abs(x) == radius || Mathf.Abs(z) == radius)
						{
							Cell cell = currentRoom.GetCell(cellPos.x + x, cellPos.y + z);
							if (cell != null && cell.isWalkable)
							{
								return cell;
							}
						}
					}
				}
			}
		}

		return null; // No suitable cell found
	}

	private List<Cell> FindPath(Cell startCell, Cell targetCell)
	{
		List<Cell> openSet = new List<Cell>();
		HashSet<Cell> closedSet = new HashSet<Cell>();
		Dictionary<Cell, Cell> cameFrom = new Dictionary<Cell, Cell>();
		Dictionary<Cell, float> gScore = new Dictionary<Cell, float>();
		Dictionary<Cell, float> fScore = new Dictionary<Cell, float>();

		gScore[startCell] = 0;
		fScore[startCell] = HeuristicCost(startCell, targetCell);
		openSet.Add(startCell);

		while (openSet.Count > 0)
		{
			Cell current = openSet[0];
			for (int i = 1; i < openSet.Count; i++)
			{
				if (fScore.ContainsKey(openSet[i]) && fScore[openSet[i]] < fScore[current])
				{
					current = openSet[i];
				}
			}

			if (current == targetCell)
			{
				return ReconstructPath(cameFrom, current);
			}

			openSet.Remove(current);
			closedSet.Add(current);

			// Check neighbors
			List<Cell> neighbors = GetNeighbors(current);
			foreach (Cell neighbor in neighbors)
			{
				if (closedSet.Contains(neighbor) || !neighbor.isWalkable ||
					(strictlyAvoidOccupiedCells && neighbor.isOccupied))
				{
					continue;
				}

				// Calculate movement cost to this neighbor
				float movementCost = Vector3.Distance(current.worldPosition, neighbor.worldPosition);

				// Apply penalty for occupied cells if we're not strictly avoiding them
				if (!strictlyAvoidOccupiedCells && neighbor.isOccupied)
				{
					movementCost += occupiedCellPenalty;
				}

				float tentativeGScore = gScore[current] + movementCost;

				if (!openSet.Contains(neighbor))
				{
					openSet.Add(neighbor);
				}
				else if (gScore.ContainsKey(neighbor) && tentativeGScore >= gScore[neighbor])
				{
					continue;
				}

				cameFrom[neighbor] = current;
				gScore[neighbor] = tentativeGScore;
				fScore[neighbor] = gScore[neighbor] + HeuristicCost(neighbor, targetCell);
			}
		}

		// If we've strictly avoided occupied cells but couldn't find a path,
		// retry with less strict constraints if this is our last attempt
		if (strictlyAvoidOccupiedCells && pathfindingAttempts >= maxPathfindingAttempts - 1)
		{
			return FindPathWithOccupiedCells(startCell, targetCell);
		}

		return null;
	}

	private List<Cell> FindPathWithOccupiedCells(Cell startCell, Cell targetCell)
	{
		// This is a backup pathfinding method that allows traversal through occupied cells
		// when necessary, applying penalties to make them less preferable
		List<Cell> openSet = new List<Cell>();
		HashSet<Cell> closedSet = new HashSet<Cell>();
		Dictionary<Cell, Cell> cameFrom = new Dictionary<Cell, Cell>();
		Dictionary<Cell, float> gScore = new Dictionary<Cell, float>();
		Dictionary<Cell, float> fScore = new Dictionary<Cell, float>();

		gScore[startCell] = 0;
		fScore[startCell] = HeuristicCost(startCell, targetCell);
		openSet.Add(startCell);

		while (openSet.Count > 0)
		{
			Cell current = openSet[0];
			for (int i = 1; i < openSet.Count; i++)
			{
				if (fScore.ContainsKey(openSet[i]) && fScore[openSet[i]] < fScore[current])
				{
					current = openSet[i];
				}
			}

			if (current == targetCell)
			{
				return ReconstructPath(cameFrom, current);
			}

			openSet.Remove(current);
			closedSet.Add(current);

			// Check all neighbors, including occupied ones
			List<Cell> allNeighbors = GetAllNeighbors(current);
			foreach (Cell neighbor in allNeighbors)
			{
				if (closedSet.Contains(neighbor) || !neighbor.isWalkable)
				{
					continue;
				}

				// Calculate movement cost to this neighbor
				float movementCost = Vector3.Distance(current.worldPosition, neighbor.worldPosition);

				// Apply high penalty for occupied cells
				if (neighbor.isOccupied)
				{
					movementCost += occupiedCellPenalty * 2; // Double penalty in backup pathfinding
				}

				float tentativeGScore = gScore[current] + movementCost;

				if (!openSet.Contains(neighbor))
				{
					openSet.Add(neighbor);
				}
				else if (gScore.ContainsKey(neighbor) && tentativeGScore >= gScore[neighbor])
				{
					continue;
				}

				cameFrom[neighbor] = current;
				gScore[neighbor] = tentativeGScore;
				fScore[neighbor] = gScore[neighbor] + HeuristicCost(neighbor, targetCell);
			}
		}

		return null;
	}

	private List<Cell> GetNeighbors(Cell cell)
	{
		List<Cell> neighbors = new List<Cell>();
		if (currentRoom == null) return neighbors;

		Vector2Int cellPos = currentRoom.WorldToGrid(cell.worldPosition);

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				if (x == 0 && z == 0) continue;

				Cell neighbor = currentRoom.GetCell(cellPos.x + x, cellPos.y + z);

				if (neighbor != null && neighbor.isWalkable &&
					(!strictlyAvoidOccupiedCells || !neighbor.isOccupied))
				{
					if (IsCellClearOfEnemies(neighbor))
					{
						neighbors.Add(neighbor);
					}
				}
			}
		}

		return neighbors;
	}

	private List<Cell> GetAllNeighbors(Cell cell)
	{
		// This version includes occupied cells for the backup pathfinding
		List<Cell> neighbors = new List<Cell>();
		if (currentRoom == null) return neighbors;

		Vector2Int cellPos = currentRoom.WorldToGrid(cell.worldPosition);

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				if (x == 0 && z == 0) continue;

				Cell neighbor = currentRoom.GetCell(cellPos.x + x, cellPos.y + z);

				if (neighbor != null && neighbor.isWalkable)
				{
					if (IsCellClearOfEnemies(neighbor))
					{
						neighbors.Add(neighbor);
					}
				}
			}
		}

		return neighbors;
	}

	private bool IsCellClearOfEnemies(Cell cell)
	{
		EnemyPathfinding[] enemies = FindObjectsOfType<EnemyPathfinding>();

		foreach (var enemy in enemies)
		{
			if (enemy == this) continue;

			if (Vector3.Distance(enemy.transform.position, cell.worldPosition) < enemyAvoidanceRadius)
			{
				return false;
			}
		}

		return true;
	}

	private float HeuristicCost(Cell a, Cell b)
	{
		if (currentRoom == null) return float.MaxValue;

		Vector2Int posA = currentRoom.WorldToGrid(a.worldPosition);
		Vector2Int posB = currentRoom.WorldToGrid(b.worldPosition);
		return Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.y - posB.y);
	}

	private List<Cell> ReconstructPath(Dictionary<Cell, Cell> cameFrom, Cell current)
	{
		List<Cell> totalPath = new List<Cell> { current };

		while (cameFrom.ContainsKey(current))
		{
			current = cameFrom[current];
			totalPath.Insert(0, current);
		}

		return SmoothPath(totalPath);
	}

	private List<Cell> SmoothPath(List<Cell> originalPath)
	{
		if (originalPath.Count <= 2)
			return originalPath;

		List<Cell> smoothPath = new List<Cell>();
		smoothPath.Add(originalPath[0]);

		int current = 0;
		while (current < originalPath.Count - 1)
		{
			int furthest = current + 1;

			for (int i = furthest + 1; i < originalPath.Count; i++)
			{
				if (HasClearLineOfSight(originalPath[current], originalPath[i]) &&
					IsLineClearOfOccupiedCells(originalPath[current], originalPath[i]))
				{
					furthest = i;
				}
			}

			current = furthest;
			smoothPath.Add(originalPath[current]);
		}

		return smoothPath;
	}

	private bool HasClearLineOfSight(Cell from, Cell to)
	{
		Vector3 direction = to.worldPosition - from.worldPosition;
		float distance = direction.magnitude;

		RaycastHit hit;
		if (Physics.Raycast(from.worldPosition + Vector3.up * 0.5f, direction.normalized, out hit, distance, obstacleLayer))
		{
			return false;
		}

		return true;
	}

	private bool IsLineClearOfOccupiedCells(Cell from, Cell to)
	{
		if (!strictlyAvoidOccupiedCells) return true;

		// Get all cells along the line
		List<Cell> cellsOnLine = GetCellsOnLine(from, to);

		// Check if any are occupied
		foreach (Cell cell in cellsOnLine)
		{
			if (cell.isOccupied)
			{
				return false;
			}
		}

		return true;
	}

	private List<Cell> GetCellsOnLine(Cell from, Cell to)
	{
		List<Cell> cells = new List<Cell>();

		Vector2Int fromGrid = currentRoom.WorldToGrid(from.worldPosition);
		Vector2Int toGrid = currentRoom.WorldToGrid(to.worldPosition);

		// Use Bresenham's line algorithm to find grid cells along the line
		int x0 = fromGrid.x;
		int y0 = fromGrid.y;
		int x1 = toGrid.x;
		int y1 = toGrid.y;

		int dx = Mathf.Abs(x1 - x0);
		int dy = Mathf.Abs(y1 - y0);
		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;
		int err = dx - dy;

		while (true)
		{
			Cell cell = currentRoom.GetCell(x0, y0);
			if (cell != null)
			{
				cells.Add(cell);
			}

			if (x0 == x1 && y0 == y1) break;

			int e2 = 2 * err;
			if (e2 > -dy)
			{
				err -= dy;
				x0 += sx;
			}
			if (e2 < dx)
			{
				err += dx;
				y0 += sy;
			}
		}

		return cells;
	}

	private void MoveAlongPath()
	{
		if (path.Count == 0 || currentPathIndex >= path.Count)
		{
			return;
		}

		Cell targetCell = path[currentPathIndex];
		Vector3 targetPosition = targetCell.worldPosition;

		float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

		if (distanceToTarget < stoppingDistance)
		{
			currentPathIndex++;

			if (currentPathIndex >= path.Count)
			{
				if (isChasing && canSeePlayer && pathfindingCoroutine != null)
				{
					StopCoroutine(pathfindingCoroutine);
					pathfindingCoroutine = StartCoroutine(PathfindingRoutine());
				}
				return;
			}
		}

		// Check if the next cell is now occupied
		if (currentPathIndex < path.Count)
		{
			Cell nextCell = path[currentPathIndex];
			if (nextCell.isOccupied && strictlyAvoidOccupiedCells)
			{
				if (pathfindingCoroutine != null)
				{
					StopCoroutine(pathfindingCoroutine);
					pathfindingCoroutine = StartCoroutine(PathfindingRoutine());
				}
				return;
			}
		}

		Vector3 direction = (targetPosition - transform.position).normalized;
		transform.position += direction * moveSpeed * Time.deltaTime;

		if (direction != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(direction);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRadius);

		if (showDetection)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, detectionRange);
		}

		if (showPath && path != null && path.Count > 0)
		{
			Gizmos.color = Color.green;
			for (int i = 0; i < path.Count - 1; i++)
			{
				Gizmos.DrawLine(path[i].worldPosition + Vector3.up * 0.1f,
								path[i + 1].worldPosition + Vector3.up * 0.1f);
			}

			// Highlight current target point
			if (currentPathIndex < path.Count)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(path[currentPathIndex].worldPosition + Vector3.up * 0.1f, 0.2f);
			}
		}
	}
}