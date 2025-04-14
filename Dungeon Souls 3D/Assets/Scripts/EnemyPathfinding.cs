using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
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

	[Header("Debug")]
	[SerializeField] private bool showDetection = true;

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

	private void Start()
	{
		// Find the player
		playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
		if (playerTransform == null)
		{
			Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
		}

		// Get animator component
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			Debug.LogWarning("Animator component not found on enemy!");
		}

		// Find initial room
		UpdateCurrentRoom();

		// Start pathfinding coroutine
		pathfindingCoroutine = StartCoroutine(PathfindingRoutine());
	}

	private void Update()
	{
		if (currentRoom == null)
		{
			UpdateCurrentRoom();
			return;
		}

		// Check if player is visible
		CheckPlayerVisibility();

		// Check if player is within attack range
		CheckAttackRange();

		// Move along path if we have one and not in attack range
		if (isPathValid && path.Count > 0 && !canAttack)
		{
			MoveAlongPath();
			// Set animator speed parameter to 1 when moving
			if (animator != null)
			{
				animator.SetFloat("speed", 1f);
			}
		}
		else
		{
			// Set animator speed parameter to 0 when stationary
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
	}

	private void CheckAttackRange()
	{
		if (playerTransform == null) return;

		float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

		// Check if player is within attack radius
		if (distanceToPlayer <= attackRadius && canSeePlayer)
		{
			canAttack = true;

			// Make sure we're facing the player when in attack mode
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
			if (IsPositionInRoom(transform.position, room))
			{
				currentRoom = room;
				return;
			}
		}
	}

	private bool IsPositionInRoom(Vector3 position, ProceduralRoom room)
	{
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
					// If we hit the player
					if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0)
					{
						canSeePlayer = true;
						lastKnownPlayerPosition = playerTransform.position;

						if (!isChasing)
						{
							isChasing = true;
							// Recalculate path immediately when player is spotted
							StopCoroutine(pathfindingCoroutine);
							pathfindingCoroutine = StartCoroutine(PathfindingRoutine());
						}
						return;
					}
				}
			}
		}

		// If we got here, we can't see the player
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

			if (isChasing)
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
		if (pathfindingTimeout > 0)
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

		if (!targetCell.isWalkable || targetCell.isOccupied)
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
						if (cell != null && cell.isWalkable && !cell.isOccupied)
						{
							return cell;
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
				if (closedSet.Contains(neighbor) || !neighbor.isWalkable || neighbor.isOccupied)
				{
					continue;
				}

				float tentativeGScore = gScore[current] + Vector3.Distance(current.worldPosition, neighbor.worldPosition);

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
		Vector2Int cellPos = currentRoom.WorldToGrid(cell.worldPosition);

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				if (x == 0 && z == 0) continue;

				Cell neighbor = currentRoom.GetCell(cellPos.x + x, cellPos.y + z);

				if (neighbor != null && neighbor.isWalkable && !neighbor.isOccupied)
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
				if (HasClearLineOfSight(originalPath[current], originalPath[i]))
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
				if (isChasing && canSeePlayer)
				{
					StopCoroutine(pathfindingCoroutine);
					pathfindingCoroutine = StartCoroutine(PathfindingRoutine());
				}
				return;
			}
		}
		if (currentPathIndex < path.Count)
		{
			Cell nextCell = path[currentPathIndex];
			if (nextCell.isOccupied)
			{
				if (isChasing)
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
		// Display attack radius
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRadius);

		// Display detection range
		if (showDetection)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, detectionRange);
		}
	}
}