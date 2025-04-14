using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
	[Header("Dungeon Settings")]
	[SerializeField] private int minRooms = 15;
	[SerializeField] private int maxRooms = 25;
	[SerializeField] private int roomSpacing = 10; // Distance between rooms
	[SerializeField] private float roomSize = 5f; // Each room is 5x5 as specified
	[SerializeField] private int maxPathLength = 8; // How many rooms long a path can be
	[SerializeField] private float branchingProbability = 0.3f; // Probability to create branching paths

	[Header("Room Prefabs")]
	[SerializeField] private GameObject roomPrefab; // Base room prefab
	[SerializeField] private Transform roomsParent; // Parent object to hold all rooms

	// Room Type Chances (adjust as needed)
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

	// Internal variables
	private List<RoomNode> allRooms = new List<RoomNode>();
	private Dictionary<Vector2Int, RoomNode> roomGrid = new Dictionary<Vector2Int, RoomNode>();
	private RoomNode startRoom;
	private RoomNode endRoom;
	private RoomNode mapRoom;
	private RoomNode keyRoom;
	private int totalRooms;
	private int roomsFirstHalf;
	private int roomsSecondHalf;

	// Directions for room connections
	private Vector2Int[] directions = new Vector2Int[] {
		new Vector2Int(0, 1),  // North
        new Vector2Int(1, 0),  // East
        new Vector2Int(0, -1), // South
        new Vector2Int(-1, 0)  // West
    };

	// Nested class to represent a room in the dungeon
	private class RoomNode
	{
		public GameObject roomObject;
		public ProceduralRoom roomComponent;
		public Vector2Int gridPosition;
		public RoomType roomType;
		public List<RoomNode> connectedRooms = new List<RoomNode>();
		public bool visited = false;
		public int distanceFromStart = 0;

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

	// Start is called before first frame update
	void Start()
	{
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

		Debug.Log($"Generating dungeon with {totalRooms} rooms");

		// Create the initial layout (start at origin)
		Vector2Int startPosition = Vector2Int.zero;
		startRoom = CreateRoom(startPosition, RoomType.SpawnPoint);
		allRooms.Add(startRoom);
		roomGrid[startPosition] = startRoom;

		// Build the main path first
		List<RoomNode> mainPath = GenerateMainPath(startRoom, maxPathLength);
		endRoom = mainPath[mainPath.Count - 1];
		endRoom.roomType = RoomType.SpawnPoint; // Set the last room as a spawn room (exit)

		// Add branching paths
		GenerateBranchingPaths();

		// Ensure we have the minimum number of rooms
		while (allRooms.Count < minRooms)
		{
			TryAddBranch();
		}

		// Place special rooms (Map in first half, Key in second half)
		PlaceSpecialRooms();

		// Instantiate the actual room objects and set up their connections
		InstantiateRooms();

		Debug.Log($"Generated {allRooms.Count} rooms in total");
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
		int attemptLimit = 100; // Prevent infinite loops
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

		// Connect the rooms
		ConnectRooms(branchSource, newRoom);

		// 30% chance to continue branching
		if (Random.value < branchingProbability)
		{
			int branchLength = Random.Range(1, 4); // Short branches
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

				// Connect the rooms
				ConnectRooms(current, nextRoom);

				current = nextRoom;
			}
		}

		return true;
	}

	private List<Vector2Int> GetValidDirections(Vector2Int position)
	{
		List<Vector2Int> validDirections = new List<Vector2Int>();

		foreach (Vector2Int dir in directions)
		{
			Vector2Int neighborPos = position + dir;

			// Check if this position is already occupied
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
					// This is a valid neighboring room
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

	private int ManhattanDistance(Vector2Int a, Vector2Int b)
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
			if (room.roomType != RoomType.SpawnPoint) // Don't replace spawn points
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
		for (int i = allRooms.Count / 2; i < allRooms.Count - 1; i++)  // Skip the end room
		{
			RoomNode room = allRooms[i];
			if (room.roomType != RoomType.SpawnPoint) // Don't replace spawn points
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
				// Configure the room
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
		// Reset all doors first
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
	}

	// Method to regenerate the dungeon (can be called from editor or other scripts)
	public void RegenerateDungeon()
	{
		ClearDungeon();
		GenerateDungeon();
	}
}