using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
	[Header("Dungeon Settings")]
	[SerializeField] private int minRooms = 15;
	[SerializeField] private int maxRooms = 25;
	[SerializeField] private int roomSpacing = 10; 
	[SerializeField] private float roomSize = 5f;
	[SerializeField] private int maxPathLength = 8; 
	[SerializeField] private float branchingProbability = 0.3f; 
	[SerializeField] private int additionalSpawnPoints = 5; 

	[Header("Room Prefabs")]
	[SerializeField] private GameObject roomPrefab;
	[SerializeField] private Transform roomsParent;
	[SerializeField] private GameObject merchantPrefab; 
	[SerializeField] private Transform merchantsParent; 

	[Header("Room Type Chances")]
	[Range(0f, 1f)]
	[SerializeField] private float normalRoomChance = 0.6f;
	[Range(0f, 1f)]
	[SerializeField] private float treasureRoomChance = 0.15f;
	[Range(0f, 1f)]
	[SerializeField] private float blacksmithRoomChance = 0.1f;
	[Range(0f, 1f)]
	[SerializeField] private float prisonRoomChance = 0.1f;
	[Range(0f, 1f)]
	[SerializeField] private float libraryRoomChance = 0.05f;

	private List<RoomNode> allRooms = new List<RoomNode>();
	private Dictionary<Vector2Int, RoomNode> roomGrid = new Dictionary<Vector2Int, RoomNode>();
	private RoomNode startRoom;
	private List<RoomNode> spawnRooms = new List<RoomNode>();
	private RoomNode endRoom;
	private RoomNode mapRoom;
	private RoomNode keyRoom;
	private int totalRooms;
	private int roomsFirstHalf;
	private int roomsSecondHalf;
	private EnemySpawner enemySpawner;

	// Directions for room connections
	private Vector2Int[] directions = new Vector2Int[] {
		new Vector2Int(0, 1),  // North
        new Vector2Int(1, 0),  // East
        new Vector2Int(0, -1), // South
        new Vector2Int(-1, 0)  // West
    };

	// Nested class to represent a room in the dungeon
	public class RoomNode
	{
		public GameObject roomObject;
		public ProceduralRoom roomComponent;
		public Vector2Int gridPosition;
		public RoomType roomType;
		public List<RoomNode> connectedRooms = new List<RoomNode>();
		public bool visited = false;
		public int distanceFromStart = 0;
		public GameObject merchantObject; 

		public RoomNode(Vector2Int position, RoomType type)
		{
			gridPosition = position;
			roomType = type;
		}

		public void AddConnection(RoomNode other)
		{
			if (!connectedRooms.Contains(other))
			{
				connectedRooms.Add(other);
			}
		}
	}

	void Start()
	{
		enemySpawner = GetComponent<EnemySpawner>();

		GenerateDungeon();
	}

	public void GenerateDungeon()
	{
		// Clear any existing rooms
		ClearDungeon();

		// Determine total number of rooms
		totalRooms = Random.Range(minRooms, maxRooms + 1);
		roomsFirstHalf = totalRooms / 2;
		roomsSecondHalf = totalRooms - roomsFirstHalf;

		// Create the initial layout (start at origin)
		Vector2Int startPosition = Vector2Int.zero;
		startRoom = CreateRoom(startPosition, RoomType.SpawnPoint);
		allRooms.Add(startRoom);
		roomGrid[startPosition] = startRoom;
		spawnRooms.Add(startRoom);
		// Build the main path first
		List<RoomNode> mainPath = GenerateMainPath(startRoom, maxPathLength);

		// Add branching paths
		GenerateBranchingPaths();

		// Ensure we have the minimum number of rooms
		while (allRooms.Count < minRooms)
		{
			TryAddBranch();
		}

		// After all rooms are generated, find the farthest room to be the boss room
		PlaceBossRoom();

		// Add additional spawn points distributed around the dungeon
		AddAdditionalSpawnPoints();

		// Place special rooms 
		PlaceSpecialRooms();

		// Instantiate the actual room objects and set up their connections
		InstantiateRooms();

		// Spawn merchants at all spawn points
		SpawnMerchantsAtSpawnPoints();

		StartCoroutine(DelayedEnemySpawning());
	}

	private void SpawnMerchantsAtSpawnPoints()
	{
		foreach (RoomNode spawnRoom in spawnRooms)
		{
			if (spawnRoom.roomObject != null)
			{
				// Get the room's world position
				Vector3 roomPosition = spawnRoom.roomObject.transform.position;

				Vector3 merchantPosition = new Vector3(
					roomPosition.x,
					roomPosition.y,
					roomPosition.z
				);

				GameObject merchant = Instantiate(merchantPrefab, merchantPosition, Quaternion.identity);

				merchant.transform.SetParent(merchantsParent);

				spawnRoom.merchantObject = merchant;

			}
		}
	}

	private IEnumerator DelayedEnemySpawning()
	{
		yield return new WaitForEndOfFrame();

		yield return null;

		if (enemySpawner != null)
		{
			enemySpawner.SpawnEnemiesInDungeon();
		}
	}

	private List<RoomNode> GenerateMainPath(RoomNode startNode, int maxLength)
	{
		List<RoomNode> path = new List<RoomNode> { startNode };
		RoomNode currentNode = startNode;
		int pathLength = Random.Range(maxLength / 2, maxLength);

		for (int i = 0; i < pathLength; i++)
		{
			List<Vector2Int> validDirections = GetValidDirections(currentNode.gridPosition);
			if (validDirections.Count == 0)
			{
				break; // No valid directions to expand
			}

			Vector2Int direction = validDirections[Random.Range(0, validDirections.Count)];
			Vector2Int newPosition = currentNode.gridPosition + direction;

			RoomNode newRoom = CreateRoom(newPosition, GetRandomRoomType());
			path.Add(newRoom);
			allRooms.Add(newRoom);
			roomGrid[newPosition] = newRoom;

			// Connect the rooms
			ConnectRooms(currentNode, newRoom);

			// Update current node
			currentNode = newRoom;
			currentNode.distanceFromStart = i + 1;
		}

		return path;
	}

	private void GenerateBranchingPaths()
	{
		int currentRoomCount = allRooms.Count;
		int attemptLimit = 100; 
		int attempts = 0;

		while (allRooms.Count < totalRooms && attempts < attemptLimit)
		{
			attempts++;
			TryAddBranch();
		}
	}

	private bool TryAddBranch()
	{
		// Select a random room to branch from
		RoomNode branchSource = allRooms[Random.Range(0, allRooms.Count)];

		List<Vector2Int> validDirections = GetValidDirections(branchSource.gridPosition);
		if (validDirections.Count == 0)
		{
			return false; // No valid directions to expand
		}

		Vector2Int direction = validDirections[Random.Range(0, validDirections.Count)];
		Vector2Int newPosition = branchSource.gridPosition + direction;

		// Create a new room
		RoomNode newRoom = CreateRoom(newPosition, GetRandomRoomType());
		allRooms.Add(newRoom);
		roomGrid[newPosition] = newRoom;

		// Set the distance from start based on the branching source
		newRoom.distanceFromStart = branchSource.distanceFromStart + 1;

		// Connect the rooms
		ConnectRooms(branchSource, newRoom);

		// 30% chance to continue branching
		if (Random.value < branchingProbability)
		{
			int branchLength = Random.Range(1, 4);
			RoomNode current = newRoom;

			for (int i = 0; i < branchLength; i++)
			{
				validDirections = GetValidDirections(current.gridPosition);
				if (validDirections.Count == 0)
				{
					break;
				}

				direction = validDirections[Random.Range(0, validDirections.Count)];
				newPosition = current.gridPosition + direction;

				RoomNode nextRoom = CreateRoom(newPosition, GetRandomRoomType());
				allRooms.Add(nextRoom);
				roomGrid[newPosition] = nextRoom;
				nextRoom.distanceFromStart = current.distanceFromStart + 1;

				ConnectRooms(current, nextRoom);

				current = nextRoom;
			}
		}

		return true;
	}

	private void AddAdditionalSpawnPoints()
	{
		CalculateDistancesFromStart();

		// Get a list of rooms sorted by their distance from center
		Vector2 dungeonCenter = CalculateDungeonCenterPosition();
		List<RoomNode> roomsByCenterDistance = new List<RoomNode>(allRooms);

		// Sort rooms by their distance from the dungeon center 
		roomsByCenterDistance.Sort((a, b) => {
			float distA = Vector2.Distance(
				new Vector2(a.gridPosition.x, a.gridPosition.y),
				new Vector2(dungeonCenter.x / roomSpacing, dungeonCenter.y / roomSpacing));

			float distB = Vector2.Distance(
				new Vector2(b.gridPosition.x, b.gridPosition.y),
				new Vector2(dungeonCenter.x / roomSpacing, dungeonCenter.y / roomSpacing));

			return distB.CompareTo(distA); 
		});

		int targetSpawnPoints = additionalSpawnPoints;

		// Find rooms that are far from each other
		HashSet<RoomNode> selectedRooms = new HashSet<RoomNode>();
		selectedRooms.Add(startRoom); // Add the main spawn point

		// try to add rooms while maintaining maximum distance
		int minDistance = 4; // Start with a high minimum distance

		while (selectedRooms.Count < targetSpawnPoints + 1 && minDistance > 0)
		{
			// Try to find rooms at the current minimum distance
			bool addedAny = false;

			foreach (RoomNode candidate in roomsByCenterDistance)
			{
				// Skip rooms that are already special
				if (candidate.roomType == RoomType.SpawnPoint ||
					candidate.roomType == RoomType.BossRoom ||
					candidate.roomType == RoomType.Map ||
					candidate.roomType == RoomType.Key ||
					selectedRooms.Contains(candidate))
				{
					continue;
				}

				// Check if this room is far enough from existing spawn points
				bool isFarEnough = true;
				foreach (RoomNode existingSpawn in selectedRooms)
				{
					if (ManhattanDistance(candidate.gridPosition, existingSpawn.gridPosition) < minDistance)
					{
						isFarEnough = false;
						break;
					}
				}

				if (isFarEnough)
				{
					candidate.roomType = RoomType.SpawnPoint;
					spawnRooms.Add(candidate);
					selectedRooms.Add(candidate);
					addedAny = true;

					// If target reached, break out
					if (selectedRooms.Count >= targetSpawnPoints + 1)
					{
						break;
					}
				}
			}

			// If we couldn't add any rooms at this distance, reduce the minimum distance
			if (!addedAny)
			{
				minDistance--;
			}
		}

		if (selectedRooms.Count < targetSpawnPoints + 1)
		{

			// Convert any non-special rooms to spawn points
			foreach (RoomNode candidate in allRooms)
			{
				if (selectedRooms.Count >= targetSpawnPoints + 1)
				{
					break;
				}

				// Skip rooms that are already special
				if (candidate.roomType == RoomType.SpawnPoint ||
					candidate.roomType == RoomType.BossRoom ||
					candidate.roomType == RoomType.Map ||
					candidate.roomType == RoomType.Key ||
					selectedRooms.Contains(candidate))
				{
					continue;
				}

				candidate.roomType = RoomType.SpawnPoint;
				spawnRooms.Add(candidate);
				selectedRooms.Add(candidate);
			}
		}

	}

	private Vector2 CalculateDungeonCenterPosition()
	{
		if (allRooms.Count == 0)
			return Vector2.zero;

		int minX = int.MaxValue;
		int maxX = int.MinValue;
		int minY = int.MaxValue;
		int maxY = int.MinValue;

		foreach (RoomNode room in allRooms)
		{
			minX = Mathf.Min(minX, room.gridPosition.x);
			maxX = Mathf.Max(maxX, room.gridPosition.x);
			minY = Mathf.Min(minY, room.gridPosition.y);
			maxY = Mathf.Max(maxY, room.gridPosition.y);
		}

		float centerX = (minX + maxX) / 2f;
		float centerY = (minY + maxY) / 2f;

		Vector3 worldCenter = new Vector3(
			centerX * roomSpacing,
			0,
			centerY * roomSpacing
		);

		return new Vector2(worldCenter.x, worldCenter.z);
	}

	// find the farthest room and set it as  boss room
	private void PlaceBossRoom()
	{
		// Calculate distances from start room for all rooms using BFS
		CalculateDistancesFromStart();

		// Get all rooms that have only one connection (dead ends)
		List<RoomNode> deadEnds = allRooms.Where(room =>
			room != startRoom &&
			room.connectedRooms.Count == 1 &&
			room.roomType != RoomType.SpawnPoint).ToList();

		if (deadEnds.Count > 0)
		{
			// Find the dead end with largest distance from start
			deadEnds.Sort((a, b) => b.distanceFromStart.CompareTo(a.distanceFromStart));
			endRoom = deadEnds[0];
		}
		else
		{
			// If no dead ends, get the furthest room that's not the start
			allRooms.Sort((a, b) => b.distanceFromStart.CompareTo(a.distanceFromStart));
			foreach (RoomNode room in allRooms)
			{
				if (room != startRoom && room.roomType != RoomType.SpawnPoint)
				{
					endRoom = room;
					break;
				}
			}
		}

		// Set the room type to boss room
		if (endRoom != null)
		{
			endRoom.roomType = RoomType.BossRoom;
		}
	}

	//  method to calculate accurate distances from start for all rooms
	private void CalculateDistancesFromStart()
	{
		// Reset all distances and visited flags
		foreach (RoomNode room in allRooms)
		{
			room.distanceFromStart = int.MaxValue;
			room.visited = false;
		}

		// BFS to calculate shortest path distances
		Queue<RoomNode> queue = new Queue<RoomNode>();
		startRoom.distanceFromStart = 0;
		startRoom.visited = true;
		queue.Enqueue(startRoom);

		while (queue.Count > 0)
		{
			RoomNode current = queue.Dequeue();

			foreach (RoomNode neighbor in current.connectedRooms)
			{
				if (!neighbor.visited)
				{
					neighbor.visited = true;
					neighbor.distanceFromStart = current.distanceFromStart + 1;
					queue.Enqueue(neighbor);
				}
			}
		}

		// Reset visited flags for later use
		foreach (RoomNode room in allRooms)
		{
			room.visited = false;
		}
	}

	private List<Vector2Int> GetValidDirections(Vector2Int position)
	{
		List<Vector2Int> validDirections = new List<Vector2Int>();

		foreach (Vector2Int dir in directions)
		{
			Vector2Int neighborPos = position + dir;

			// Check if  position is already occupied
			if (roomGrid.ContainsKey(neighborPos))
			{
				continue;
			}

			// Make sure we don't create rooms that are too close to existing rooms
			bool tooClose = false;
			foreach (Vector2Int existingPos in roomGrid.Keys)
			{
				if (existingPos != position && ManhattanDistance(neighborPos, existingPos) == 1)
				{
					// valid neighboring room
				}
				else if (neighborPos == existingPos)
				{
					tooClose = true;
					break;
				}
			}

			if (!tooClose)
			{
				validDirections.Add(dir);
			}
		}

		return validDirections;
	}

	private int ManhattanDistance(Vector2Int a, Vector2Int b) //Get distance between 2 rooms
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}

	private RoomNode CreateRoom(Vector2Int position, RoomType type)
	{
		return new RoomNode(position, type);
	}

	private void ConnectRooms(RoomNode roomA, RoomNode roomB)
	{
		roomA.AddConnection(roomB);
		roomB.AddConnection(roomA);
	}

	private RoomType GetRandomRoomType()
	{
		float totalChance = normalRoomChance + treasureRoomChance + blacksmithRoomChance + prisonRoomChance + libraryRoomChance;
		float rand = Random.Range(0, totalChance);
		float runningTotal = 0;

		runningTotal += normalRoomChance;
		if (rand <= runningTotal) return RoomType.Normal;

		runningTotal += treasureRoomChance;
		if (rand <= runningTotal) return RoomType.Treasure;

		runningTotal += blacksmithRoomChance;
		if (rand <= runningTotal) return RoomType.Blacksmith;

		runningTotal += prisonRoomChance;
		if (rand <= runningTotal) return RoomType.Prison;

		return RoomType.Library;
	}

	private void PlaceSpecialRooms()
	{
		// Sort rooms by distance from start
		allRooms.Sort((a, b) => a.distanceFromStart.CompareTo(b.distanceFromStart));

		// First half gets the map room
		List<RoomNode> firstHalfRooms = new List<RoomNode>();
		for (int i = 1; i < allRooms.Count / 2; i++)  // Skip the start room (index 0)
		{
			RoomNode room = allRooms[i];
			if (room.roomType != RoomType.SpawnPoint && room.roomType != RoomType.BossRoom) // Don't replace spawn points or boss rooms
			{
				firstHalfRooms.Add(room);
			}
		}

		if (firstHalfRooms.Count > 0)
		{
			mapRoom = firstHalfRooms[Random.Range(0, firstHalfRooms.Count)];
			mapRoom.roomType = RoomType.Map;
		}

		// Second half gets the key room
		List<RoomNode> secondHalfRooms = new List<RoomNode>();
		for (int i = allRooms.Count / 2; i < allRooms.Count; i++)
		{
			RoomNode room = allRooms[i];
			if (room.roomType != RoomType.SpawnPoint && room.roomType != RoomType.BossRoom) // Don't replace spawn points or boss rooms
			{
				secondHalfRooms.Add(room);
			}
		}

		if (secondHalfRooms.Count > 0)
		{
			keyRoom = secondHalfRooms[Random.Range(0, secondHalfRooms.Count)];
			keyRoom.roomType = RoomType.Key;
		}
	}

	private void InstantiateRooms()
	{
		if (roomsParent == null)
		{
			roomsParent = new GameObject("Rooms").transform;
			roomsParent.SetParent(transform);
		}

		foreach (RoomNode room in allRooms)
		{
			Vector3 worldPosition = new Vector3(
				room.gridPosition.x * roomSpacing,
				0,
				room.gridPosition.y * roomSpacing
			);

			room.roomObject = Instantiate(roomPrefab, worldPosition, Quaternion.identity, roomsParent);
			room.roomObject.name = $"Room_{room.gridPosition.x}_{room.gridPosition.y}_{room.roomType}";

			ProceduralRoom proceduralRoom = room.roomObject.GetComponent<ProceduralRoom>();
			if (proceduralRoom != null)
			{
				proceduralRoom.roomType = room.roomType;
				proceduralRoom.roomWidth = roomSize;
				proceduralRoom.roomLength = roomSize;

				// Set up door connections
				SetupDoors(room, proceduralRoom);

				room.roomComponent = proceduralRoom;
			}
		}
	}

	private void SetupDoors(RoomNode room, ProceduralRoom proceduralRoom)
	{
		proceduralRoom.hasNorthDoor = false;
		proceduralRoom.hasSouthDoor = false;
		proceduralRoom.hasEastDoor = false;
		proceduralRoom.hasWestDoor = false;

		// For each connected room, determine which door to open
		foreach (RoomNode connectedRoom in room.connectedRooms)
		{
			Vector2Int direction = connectedRoom.gridPosition - room.gridPosition;

			if (direction == new Vector2Int(0, 1)) // North
			{
				proceduralRoom.hasNorthDoor = true;
			}
			else if (direction == new Vector2Int(1, 0)) // East
			{
				proceduralRoom.hasEastDoor = true;
			}
			else if (direction == new Vector2Int(0, -1)) // South
			{
				proceduralRoom.hasSouthDoor = true;
			}
			else if (direction == new Vector2Int(-1, 0)) // West
			{
				proceduralRoom.hasWestDoor = true;
			}
		}
	}

	private void ClearDungeon()
	{
		if (roomsParent != null)
		{
			for (int i = roomsParent.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(roomsParent.GetChild(i).gameObject);
			}
		}

		allRooms.Clear();
		roomGrid.Clear();
		startRoom = null;
		endRoom = null;
		mapRoom = null;
		keyRoom = null;
		spawnRooms.Clear();
	}

	public void RegenerateDungeon()
	{
		ClearDungeon();
		GenerateDungeon();
	}

	public Vector2 CalculateDungeonCenter()
	{
		return CalculateDungeonCenterPosition();
	}
}