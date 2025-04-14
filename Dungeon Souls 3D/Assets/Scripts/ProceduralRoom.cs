using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum RoomType
{
	Normal,
	Treasure,
	Blacksmith,
	Prison,
	Map,
	Key,
	SpawnPoint,
	Library
}

[System.Serializable]
public class RoomPropSet
{
	public string name;
	public GameObject[] propPrefabs;
	[Range(0f, 1f)]
	public float propDensity = 0.2f;
	public bool forceCenterProp = false;
	public GameObject centerPropPrefab;
}

[System.Serializable]
public class WallPropSet
{
	public string name;
	public GameObject[] wallPropPrefabs;
	[Range(0f, 1f)]
	public float wallPropDensity = 0.2f;
	[Range(0f, 1f)]
	public float heightVariation = 0.2f; // Controls vertical position variation
	public float wallOffset = 0.05f; // Distance from the wall
}

[System.Serializable]
public class RoofPropSet
{
	public string name;
	public GameObject[] roofPropPrefabs;
	[Range(0f, 1f)]
	public float roofPropDensity = 0.1f;
	public float heightOffset = 0.1f; // Distance below ceiling
	public bool allowChandeliers = true;
	public GameObject chandelierPrefab;
}

public class ProceduralRoom : MonoBehaviour
{
	[Header("Room Type")]
	public RoomType roomType = RoomType.Normal;

	[Header("Floor Settings")]
	[SerializeField] GameObject[] floorPrefabs = new GameObject[3];
	[SerializeField] float[] floorProbabilities = new float[] { 0.45f, 0.45f, 0.1f };

	[Header("Wall Settings")]
	[SerializeField] GameObject[] wallPrefabs = new GameObject[4];

	[Header("Door Settings")]
	public bool hasNorthDoor = false;
	public bool hasSouthDoor = false;
	public bool hasEastDoor = false;
	public bool hasWestDoor = false;

	[Header("Prop Settings")]
	[SerializeField] float propSpacing = 1.0f;
	[SerializeField] bool avoidDoorways = true;
	[SerializeField] float doorwayAvoidanceRadius = 2.0f;

	[Header("Room Settings")]
	public float roomWidth = 10f;
	public float roomLength = 10f;
	[SerializeField] float wallAlignmentOffset = 0.01f;

	[Header("Room Type Props")]
	[SerializeField] private RoomPropSet normalRoomProps;
	[SerializeField] private RoomPropSet treasureRoomProps;
	[SerializeField] private RoomPropSet blacksmithRoomProps;
	[SerializeField] private RoomPropSet prisonRoomProps;
	[SerializeField] private RoomPropSet mapRoomProps;
	[SerializeField] private RoomPropSet keyRoomProps;
	[SerializeField] private RoomPropSet spawnPointRoomProps;
	[SerializeField] private RoomPropSet libraryRoomProps;

	[Header("Roof Props")]
	[SerializeField] private RoofPropSet normalRoofProps;
	[SerializeField] private RoofPropSet treasureRoofProps;
	[SerializeField] private RoofPropSet blacksmithRoofProps;
	[SerializeField] private RoofPropSet prisonRoofProps;
	[SerializeField] private RoofPropSet mapRoofProps;
	[SerializeField] private RoofPropSet keyRoofProps;
	[SerializeField] private RoofPropSet spawnPointRoofProps;
	[SerializeField] private RoofPropSet libraryRoofProps;

	[Header("Wall Props")]
	[SerializeField] private WallPropSet wallProps;
	[SerializeField] private bool enableWallProps = true;
	[Range(0f, 1f)]
	[SerializeField] private float wallPropChance = 0.3f; // Chance for a wall to have props
	[SerializeField] private float minWallPropDistance = 1.5f; // Minimum distance between wall props

	[Header("Roof Settings")]
	[SerializeField] private GameObject[] roofPrefabs = new GameObject[3];
	[SerializeField] float[] roofProbabilities = new float[] { 0.45f, 0.45f, 0.1f };
	[SerializeField] private float roofHeight = 4.0f;
	[SerializeField] private bool enableRoof = true;
	[SerializeField] private bool enableRoofProps = true;
	[SerializeField] private float minRoofPropDistance = 2.0f; // Minimum distance between roof props


	private float lastWidth;
	private float lastLength;
	private RoomType lastRoomType;
	private Vector3 roomOrigin;
	private System.Random random;
	private List<Vector3> doorPositions = new List<Vector3>();

	[SerializeField] private RoomGridSystem gridSystem;

	public void Start()
	{
		random = new System.Random();
		roomOrigin = transform.position;
		lastRoomType = roomType;

		// Make sure we have the grid system component
		if (gridSystem == null)
		{
			gridSystem = GetComponent<RoomGridSystem>();
			if (gridSystem == null)
			{
				gridSystem = gameObject.AddComponent<RoomGridSystem>();
			}
		}

		GenerateRoom();
		lastWidth = roomWidth;
		lastLength = roomLength;
	}

	private void Update()
	{
		if (!Mathf.Approximately(roomWidth, lastWidth) || !Mathf.Approximately(roomLength, lastLength) || roomType != lastRoomType)
		{
			RegenerateRoom();
			lastWidth = roomWidth;
			lastLength = roomLength;
			lastRoomType = roomType;
		}
	}

	private void GenerateRoom()
	{
		doorPositions.Clear();
		CreateFloor();

		// Initialize the grid system with room dimensions
		Renderer floorRenderer = floorPrefabs[0].GetComponent<Renderer>();
		float tileSize = floorRenderer.bounds.size.x; // Assuming square tiles
		gridSystem.Initialize(roomOrigin, roomWidth, roomLength, tileSize);

		CreateWalls();

		// Mark walls in the grid system
		gridSystem.MarkWallsInGrid(transform, wallPrefabs);

		if (enableRoof)
		{
			CreateRoof();
		}

		PlaceRoomTypeProps();

		// Place wall props
		PlaceWallProps();

		// Place roof props if enabled
		if (enableRoof && enableRoofProps)
		{
			PlaceRoofProps();
		}
	}

	private int SelectFloorTile()
	{
		float value = (float)random.NextDouble();
		float cumulative = 0;

		for (int i = 0; i < floorProbabilities.Length; i++)
		{
			cumulative += floorProbabilities[i];
			if (value <= cumulative)
			{
				return i;
			}
		}

		return 0;
	}

	private int SelectWallTile()
	{
		return random.Next(0, 3);
	}

	private void CreateFloor()
	{
		Renderer renderer = floorPrefabs[0].GetComponent<Renderer>();

		Vector3 tileSize = renderer.bounds.size;
		int fullTilesX = Mathf.FloorToInt(roomWidth);
		int fullTilesZ = Mathf.FloorToInt(roomLength);

		float remainderX = roomWidth - fullTilesX;
		float remainderZ = roomLength - fullTilesZ;

		for (int x = 0; x < fullTilesX; x++)
		{
			for (int z = 0; z < fullTilesZ; z++)
			{
				Vector3 position = roomOrigin + new Vector3(x * tileSize.x, 0, z * tileSize.z);
				int tileType = SelectFloorTile();
				Instantiate(floorPrefabs[tileType], position, Quaternion.identity, transform);
			}
		}

		if (remainderX > 0)
		{
			for (int z = 0; z < fullTilesZ; z++)
			{
				float scaledTileWidth = tileSize.x * remainderX;
				Vector3 position = roomOrigin + new Vector3(fullTilesX * tileSize.x - (tileSize.x / 2) + (scaledTileWidth / 2), 0, z * tileSize.z);
				int tileType = SelectFloorTile();
				GameObject tile = Instantiate(floorPrefabs[tileType], position, Quaternion.identity, transform);
				tile.transform.localScale = new Vector3(remainderX, 1, 1);
			}
		}

		if (remainderZ > 0)
		{
			for (int x = 0; x < fullTilesX; x++)
			{
				float scaledTileLength = tileSize.z * remainderZ;
				Vector3 position = roomOrigin + new Vector3(x * tileSize.x, 0, fullTilesZ * tileSize.z - (tileSize.z / 2) + (scaledTileLength / 2));
				int tileType = SelectFloorTile();
				GameObject tile = Instantiate(floorPrefabs[tileType], position, Quaternion.identity, transform);
				tile.transform.localScale = new Vector3(1, 1, remainderZ);
			}
		}

		if (remainderX > 0 && remainderZ > 0)
		{
			float scaledTileWidth = tileSize.x * remainderX;
			float scaledTileLength = tileSize.z * remainderZ;
			Vector3 position = roomOrigin + new Vector3(
				fullTilesX * tileSize.x - (tileSize.x / 2) + (scaledTileWidth / 2),
				0,
				fullTilesZ * tileSize.z - (tileSize.z / 2) + (scaledTileLength / 2)
			);
			int tileType = SelectFloorTile();
			GameObject tile = Instantiate(floorPrefabs[tileType], position, Quaternion.identity, transform);
			tile.transform.localScale = new Vector3(remainderX, 1, remainderZ);
		}
	}

	private void CreateWalls()
	{

		Renderer floorRenderer = floorPrefabs[0].GetComponent<Renderer>();
		Renderer wallRenderer = wallPrefabs[0].GetComponent<Renderer>();

		Vector3 tileSize = floorRenderer.bounds.size;
		Vector3 wallSize = wallRenderer.bounds.size;

		float actualWidth = roomWidth * tileSize.x;
		float actualLength = roomLength * tileSize.z;

		int wallCountX = Mathf.CeilToInt(roomWidth);
		int wallCountZ = Mathf.CeilToInt(roomLength);

		float scaleX = actualWidth / wallCountX;
		float scaleZ = actualLength / wallCountZ;

		float floorOffsetX = -(tileSize.x / 2) + roomOrigin.x;
		float floorOffsetZ = -(tileSize.z / 2) + roomOrigin.z;

		CreateWallSection(wallCountX, scaleX, floorOffsetX, floorOffsetZ, actualWidth, actualLength, wallSize, WallSide.South, hasSouthDoor);
		CreateWallSection(wallCountX, scaleX, floorOffsetX, floorOffsetZ, actualWidth, actualLength, wallSize, WallSide.North, hasNorthDoor);
		CreateWallSection(wallCountZ, scaleZ, floorOffsetX, floorOffsetZ, actualWidth, actualLength, wallSize, WallSide.West, hasWestDoor);
		CreateWallSection(wallCountZ, scaleZ, floorOffsetX, floorOffsetZ, actualWidth, actualLength, wallSize, WallSide.East, hasEastDoor);
	}

	private enum WallSide
	{
		North,
		South,
		East,
		West
	}

	private void CreateWallSection(int wallCount, float scale, float floorOffsetX, float floorOffsetZ, float actualWidth, float actualLength, Vector3 wallSize, WallSide side, bool hasDoor)
	{
		// Only one door and always in the middle
		int doorIndex = -1;
		if (hasDoor)
		{
			doorIndex = wallCount / 2; // This ensures the door is in the middle
		}

		for (int i = 0; i < wallCount; i++)
		{
			int wallType;

			// If this is where the door should be placed
			if (i == doorIndex)
			{
				wallType = 3; // Doorway
			}
			else
			{
				wallType = SelectWallTile();
			}

			Vector3 position;
			Quaternion rotation;

			switch (side)
			{
				case WallSide.South:
					position = new Vector3(
						i * scale + (scale / 2) + floorOffsetX,
						roomOrigin.y,
						floorOffsetZ - (wallSize.z / 2) + wallAlignmentOffset
					);
					rotation = Quaternion.identity;
					break;

				case WallSide.North:
					position = new Vector3(
						i * scale + (scale / 2) + floorOffsetX,
						roomOrigin.y,
						floorOffsetZ + actualLength + (wallSize.z / 2) - wallAlignmentOffset
					);
					rotation = Quaternion.Euler(0, 180, 0);
					break;

				case WallSide.West:
					position = new Vector3(
						floorOffsetX - (wallSize.z / 2) + wallAlignmentOffset,
						roomOrigin.y,
						i * scale + (scale / 2) + floorOffsetZ
					);
					rotation = Quaternion.Euler(0, 90, 0);
					break;

				case WallSide.East:
					position = new Vector3(
						floorOffsetX + actualWidth + (wallSize.z / 2) - wallAlignmentOffset,
						roomOrigin.y,
						i * scale + (scale / 2) + floorOffsetZ
					);
					rotation = Quaternion.Euler(0, 270, 0);
					break;

				default:
					continue;
			}

			GameObject wall = Instantiate(wallPrefabs[wallType], position, rotation, transform);
			wall.transform.localScale = new Vector3(scale / wallSize.x, 1, 1);

			if (wallType == 3)
			{
				doorPositions.Add(position);
			}
		}
	}

	private void PlaceRoomTypeProps()
	{
		RoomPropSet currentPropSet = GetCurrentRoomPropSet();

		

		if ((currentPropSet.propPrefabs == null || currentPropSet.propPrefabs.Length == 0) &&
			!(currentPropSet.forceCenterProp && currentPropSet.centerPropPrefab != null))
		{
			return;
		}

		Transform propsParent = new GameObject($"{roomType}Props").transform;
		propsParent.SetParent(transform);

		// Get available cells from the grid system
		List<Cell> availableCells = gridSystem.GetAvailableCells(doorPositions, avoidDoorways, doorwayAvoidanceRadius);

		if (availableCells == null || availableCells.Count == 0)
		{
			if (currentPropSet.forceCenterProp && currentPropSet.centerPropPrefab != null)
			{
				Renderer floorRenderer = floorPrefabs[0].GetComponent<Renderer>();
				Vector3 tileSize = floorRenderer.bounds.size;
				float actualWidth = roomWidth * tileSize.x;
				float actualLength = roomLength * tileSize.z;
				Vector3 roomCenter = new Vector3(
					roomOrigin.x + (actualWidth / 2),
					roomOrigin.y,
					roomOrigin.z + (actualLength / 2)
				);
				Cell centerCell = new Cell(roomCenter);
				availableCells = new List<Cell> { centerCell };
			}
			else
			{
				return; // No cells, no center prop needed, exit
			}
		}

		// If this room type requires a center prop, place it first
		if (currentPropSet.forceCenterProp && currentPropSet.centerPropPrefab != null)
		{
			// Calculate room center based on floor tiles
			Renderer floorRenderer = floorPrefabs[0].GetComponent<Renderer>();
			Vector3 tileSize = floorRenderer.bounds.size;

			float actualWidth = roomWidth * tileSize.x;
			float actualLength = roomLength * tileSize.z;

			Vector3 roomCenter = new Vector3(
				roomOrigin.x + (actualWidth / 2),
				roomOrigin.y,
				roomOrigin.z + (actualLength / 2)
			);

			// Create the center prop
			GameObject centerProp = Instantiate(currentPropSet.centerPropPrefab, roomCenter, Quaternion.identity, propsParent);

			// Remove cells near the center prop - use a smaller radius to ensure we still have cells for other props
			float centerPropRadius = propSpacing * 1.5f;
			gridSystem.RemoveNearbyCells(availableCells, roomCenter, centerPropRadius);
		}

		// Only place regular props if we have prop prefabs
		if (currentPropSet.propPrefabs != null && currentPropSet.propPrefabs.Length > 0 && availableCells.Count > 0)
		{
			int propsToPlace = Mathf.Max(1, Mathf.FloorToInt(availableCells.Count * currentPropSet.propDensity));

			for (int i = 0; i < propsToPlace; i++)
			{
				if (availableCells.Count == 0)
				{
					break;
				}

				int cellIndex = random.Next(0, availableCells.Count);
				Cell selectedCell = availableCells[cellIndex];
				availableCells.RemoveAt(cellIndex); // Remove the cell from options immediately

				if (currentPropSet.propPrefabs.Length == 0)
				{
					continue;
				}

				int propIndex = random.Next(0, currentPropSet.propPrefabs.Length);
				GameObject propPrefab = currentPropSet.propPrefabs[propIndex];

				if (propPrefab != null)
				{
					Vector3 propPosition = selectedCell.worldPosition;
					float randomRotation = (float)random.NextDouble() * 360f;
					GameObject prop = Instantiate(propPrefab, propPosition, Quaternion.Euler(0, randomRotation, 0), propsParent);

					// Mark nearby cells as unavailable
					gridSystem.RemoveNearbyCells(availableCells, propPosition, propSpacing);
				}
			}
		}
	}

	// New method for placing wall props
	private void PlaceWallProps()
	{
		if (!enableWallProps || wallProps == null || wallProps.wallPropPrefabs == null || wallProps.wallPropPrefabs.Length == 0)
		{
			return;
		}

		Transform wallPropsParent = new GameObject("WallProps").transform;
		wallPropsParent.SetParent(transform);

		// Get wall positions
		List<WallSegment> wallSegments = GetWallSegments();

		// Track all placed wall prop positions to maintain minimum distances
		List<Vector3> placedWallPropPositions = new List<Vector3>();

		foreach (WallSegment segment in wallSegments)
		{
			// Random chance to place props on this wall segment
			if (random.NextDouble() > wallPropChance)
			{
				continue;
			}

			int propsToPlace = Mathf.Max(1, Mathf.FloorToInt(segment.length * wallProps.wallPropDensity));

			// Maximum attempts to place props to avoid infinite loop
			int maxAttempts = propsToPlace * 3;
			int attempts = 0;

			for (int i = 0; i < propsToPlace && attempts < maxAttempts; attempts++)
			{
				if (wallProps.wallPropPrefabs.Length == 0)
				{
					continue;
				}

				// Select a random wall prop
				int propIndex = random.Next(0, wallProps.wallPropPrefabs.Length);
				GameObject propPrefab = wallProps.wallPropPrefabs[propIndex];

				if (propPrefab == null)
				{
					continue;
				}

				// Calculate position along the wall
				float randomPos = (float)random.NextDouble();

				// Avoid placing props at the exact edges of walls
				randomPos = Mathf.Lerp(0.1f, 0.9f, randomPos) * segment.length;

				// Add height variation
				float heightVar = ((float)random.NextDouble() * 2 - 1) * wallProps.heightVariation;

				Vector3 propPosition = segment.start + segment.direction * randomPos;

				// Position props higher on the wall (2-2.5 units from floor)
				propPosition.y += 2.2f + heightVar;

				// Offset from wall slightly
				propPosition += segment.normal * wallProps.wallOffset;

				// Check if position is too close to other wall props
				bool tooCloseToOtherProp = false;
				foreach (Vector3 existingPropPos in placedWallPropPositions)
				{
					if (Vector3.Distance(propPosition, existingPropPos) < minWallPropDistance)
					{
						tooCloseToOtherProp = true;
						break;
					}
				}

				if (tooCloseToOtherProp)
				{
					continue; // Skip this position and try again
				}

				// Create the prop
				Quaternion propRotation = Quaternion.LookRotation(segment.normal);
				GameObject prop = Instantiate(propPrefab, propPosition, propRotation, wallPropsParent);

				// Track this prop position
				placedWallPropPositions.Add(propPosition);

				// Check if prop is too close to a door before instantiating
				bool tooCloseToADoor = false;
				float increasedDoorAvoidance = doorwayAvoidanceRadius * 1.5f; // Increased radius for wall props

				foreach (Vector3 doorPos in doorPositions)
				{
					// Use horizontal distance check to better avoid doors
					Vector3 propHorizontal = new Vector3(propPosition.x, 0, propPosition.z);
					Vector3 doorHorizontal = new Vector3(doorPos.x, 0, doorPos.z);

					if (Vector3.Distance(propHorizontal, doorHorizontal) < increasedDoorAvoidance)
					{
						tooCloseToADoor = true;
						break;
					}
				}

				if (tooCloseToADoor)
				{
					Destroy(prop);
					placedWallPropPositions.Remove(propPosition); // Remove from tracking
					continue; // Try another placement
				}

				// Successfully placed a prop
				i++;
			}
		}
	}

	// Struct to represent a wall segment
	private struct WallSegment
	{
		public Vector3 start;
		public Vector3 end;
		public Vector3 direction;
		public Vector3 normal;
		public float length;
		public WallSide side;

		public WallSegment(Vector3 start, Vector3 end, Vector3 normal, WallSide side)
		{
			this.start = start;
			this.end = end;
			this.direction = (end - start).normalized;
			this.normal = normal;
			this.length = Vector3.Distance(start, end);
			this.side = side;
		}
	}

	// Method to get wall segments
	private List<WallSegment> GetWallSegments()
	{
		List<WallSegment> segments = new List<WallSegment>();

		Renderer floorRenderer = floorPrefabs[0].GetComponent<Renderer>();
		Vector3 tileSize = floorRenderer.bounds.size;

		float actualWidth = roomWidth * tileSize.x;
		float actualLength = roomLength * tileSize.z;

		float floorOffsetX = -(tileSize.x / 2) + roomOrigin.x;
		float floorOffsetZ = -(tileSize.z / 2) + roomOrigin.z;

		// South wall (Z-)
		Vector3 southStart = new Vector3(floorOffsetX, roomOrigin.y, floorOffsetZ);
		Vector3 southEnd = new Vector3(floorOffsetX + actualWidth, roomOrigin.y, floorOffsetZ);
		segments.Add(new WallSegment(southStart, southEnd, Vector3.forward, WallSide.South));

		// North wall (Z+)
		Vector3 northStart = new Vector3(floorOffsetX, roomOrigin.y, floorOffsetZ + actualLength);
		Vector3 northEnd = new Vector3(floorOffsetX + actualWidth, roomOrigin.y, floorOffsetZ + actualLength);
		segments.Add(new WallSegment(northStart, northEnd, Vector3.back, WallSide.North));

		// West wall (X-)
		Vector3 westStart = new Vector3(floorOffsetX, roomOrigin.y, floorOffsetZ);
		Vector3 westEnd = new Vector3(floorOffsetX, roomOrigin.y, floorOffsetZ + actualLength);
		segments.Add(new WallSegment(westStart, westEnd, Vector3.right, WallSide.West));

		// East wall (X+)
		Vector3 eastStart = new Vector3(floorOffsetX + actualWidth, roomOrigin.y, floorOffsetZ);
		Vector3 eastEnd = new Vector3(floorOffsetX + actualWidth, roomOrigin.y, floorOffsetZ + actualLength);
		segments.Add(new WallSegment(eastStart, eastEnd, Vector3.left, WallSide.East));

		return segments;
	}

	private RoomPropSet GetCurrentRoomPropSet()
	{
		switch (roomType)
		{
			case RoomType.Normal:
				return normalRoomProps;
			case RoomType.Treasure:
				return treasureRoomProps;
			case RoomType.Blacksmith:
				return blacksmithRoomProps;
			case RoomType.Prison:
				return prisonRoomProps;
			case RoomType.Map:
				return mapRoomProps;
			case RoomType.Key:
				return keyRoomProps;
			case RoomType.SpawnPoint:
				return spawnPointRoomProps;
			case RoomType.Library:
				return libraryRoomProps;
			default:
				return normalRoomProps;
		}
	}

	private RoofPropSet GetCurrentRoofPropSet()
	{
		switch (roomType)
		{
			case RoomType.Normal:
				return normalRoofProps;
			case RoomType.Treasure:
				return treasureRoofProps;
			case RoomType.Blacksmith:
				return blacksmithRoofProps;
			case RoomType.Prison:
				return prisonRoofProps;
			case RoomType.Map:
				return mapRoofProps;
			case RoomType.Key:
				return keyRoofProps;
			case RoomType.SpawnPoint:
				return spawnPointRoofProps;
			case RoomType.Library:
				return libraryRoofProps;
			default:
				return normalRoofProps;
		}
	}

	private void RegenerateRoom()
	{
		foreach (Transform child in transform)
		{
			Destroy(child.gameObject);
		}

		roomOrigin = transform.position;
		GenerateRoom();
	}

	public Vector2Int WorldToGrid(Vector3 worldPos)
	{
		return gridSystem.WorldToGrid(worldPos);
	}

	public Cell GetCell(int x, int z)
	{
		return gridSystem.GetCell(x, z);
	}

	private void CreateRoof()
	{
		if (roofPrefabs[0] == null)
		{
			return;
		}

		Renderer renderer = roofPrefabs[0].GetComponent<Renderer>();
		if (renderer == null)
		{
			return;
		}

		Vector3 tileSize = renderer.bounds.size;
		int fullTilesX = Mathf.FloorToInt(roomWidth);
		int fullTilesZ = Mathf.FloorToInt(roomLength);

		float remainderX = roomWidth - fullTilesX;
		float remainderZ = roomLength - fullTilesZ;

		Transform roofParent = new GameObject("Roof").transform;
		roofParent.SetParent(transform);
		roofParent.position = new Vector3(roomOrigin.x, roomOrigin.y + roofHeight, roomOrigin.z);

		// Create full-sized roof tiles
		for (int x = 0; x < fullTilesX; x++)
		{
			for (int z = 0; z < fullTilesZ; z++)
			{
				Vector3 position = roofParent.position + new Vector3(x * tileSize.x, 0, z * tileSize.z);
				int tileType = SelectRoofTile();
				GameObject tile = Instantiate(roofPrefabs[tileType], position, Quaternion.Euler(180, 0, 0), roofParent);
			}
		}

		// Create partial tiles for X remainder
		if (remainderX > 0)
		{
			for (int z = 0; z < fullTilesZ; z++)
			{
				float scaledTileWidth = tileSize.x * remainderX;
				Vector3 position = roofParent.position + new Vector3(fullTilesX * tileSize.x - (tileSize.x / 2) + (scaledTileWidth / 2), 0, z * tileSize.z);
				int tileType = SelectRoofTile();
				GameObject tile = Instantiate(roofPrefabs[tileType], position, Quaternion.Euler(180, 0, 0), roofParent);
				tile.transform.localScale = new Vector3(remainderX, 1, 1);
			}
		}

		// Create partial tiles for Z remainder
		if (remainderZ > 0)
		{
			for (int x = 0; x < fullTilesX; x++)
			{
				float scaledTileLength = tileSize.z * remainderZ;
				Vector3 position = roofParent.position + new Vector3(x * tileSize.x, 0, fullTilesZ * tileSize.z - (tileSize.z / 2) + (scaledTileLength / 2));
				int tileType = SelectRoofTile();
				GameObject tile = Instantiate(roofPrefabs[tileType], position, Quaternion.Euler(180, 0, 0), roofParent);
				tile.transform.localScale = new Vector3(1, 1, remainderZ);
			}
		}

		// Create corner tile for X and Z remainder
		if (remainderX > 0 && remainderZ > 0)
		{
			float scaledTileWidth = tileSize.x * remainderX;
			float scaledTileLength = tileSize.z * remainderZ;
			Vector3 position = roofParent.position + new Vector3(
				fullTilesX * tileSize.x - (tileSize.x / 2) + (scaledTileWidth / 2),
				0,
				fullTilesZ * tileSize.z - (tileSize.z / 2) + (scaledTileLength / 2)
			);
			int tileType = SelectRoofTile();
			GameObject tile = Instantiate(roofPrefabs[tileType], position, Quaternion.Euler(180, 0, 0), roofParent);
			tile.transform.localScale = new Vector3(remainderX, 1, remainderZ);
		}
	}

	private int SelectRoofTile()
	{
		float value = (float)random.NextDouble();
		float cumulative = 0;

		for (int i = 0; i < roofProbabilities.Length; i++)
		{
			cumulative += roofProbabilities[i];
			if (value <= cumulative)
			{
				return i;
			}
		}

		return 0;
	}

	private void PlaceRoofProps()
	{
		RoofPropSet currentRoofPropSet = GetCurrentRoofPropSet();

		if (currentRoofPropSet == null)
		{
			return;
		}

		if ((currentRoofPropSet.roofPropPrefabs == null || currentRoofPropSet.roofPropPrefabs.Length == 0) &&
			!(currentRoofPropSet.allowChandeliers && currentRoofPropSet.chandelierPrefab != null))
		{
			return;
		}

		Transform roofPropsParent = new GameObject($"{roomType}RoofProps").transform;
		roofPropsParent.SetParent(transform);

		Renderer floorRenderer = floorPrefabs[0].GetComponent<Renderer>();
		Vector3 tileSize = floorRenderer.bounds.size;

		float actualWidth = roomWidth * tileSize.x;
		float actualLength = roomLength * tileSize.z;

		// Calculate room boundaries correctly
		// Using floorRenderer size to ensure consistent calculations
		float floorTileSize = floorRenderer.bounds.size.x;
		float floorOffsetX = -(floorTileSize / 2) + roomOrigin.x;
		float floorOffsetZ = -(floorTileSize / 2) + roomOrigin.z;

		float minX = floorOffsetX;
		float maxX = floorOffsetX + actualWidth;
		float minZ = floorOffsetZ;
		float maxZ = floorOffsetZ + actualLength;

		// Calculate true center position for the room
		Vector3 roomCenter = new Vector3(
			minX + (actualWidth / 2),
			roomOrigin.y + roofHeight - currentRoofPropSet.heightOffset,
			minZ + (actualLength / 2)
		);

		// Place chandelier if enabled
		if (currentRoofPropSet.allowChandeliers && currentRoofPropSet.chandelierPrefab != null)
		{
			// Use identity rotation for chandelier (no rotation)
			GameObject chandelier = Instantiate(
				currentRoofPropSet.chandelierPrefab,
				roomCenter,
				Quaternion.identity,
				roofPropsParent
			);

		}

		// Place other roof props
		if (currentRoofPropSet.roofPropPrefabs != null && currentRoofPropSet.roofPropPrefabs.Length > 0)
		{
			// Track placed prop positions for minimum distance
			List<Vector3> placedRoofPropPositions = new List<Vector3>();

			// Add center position if chandelier was placed
			if (currentRoofPropSet.allowChandeliers && currentRoofPropSet.chandelierPrefab != null)
			{
				placedRoofPropPositions.Add(roomCenter);
			}

			// Calculate how many props to place
			float roomArea = actualWidth * actualLength;
			int propsToPlace = Mathf.Max(1, Mathf.FloorToInt(roomArea * currentRoofPropSet.roofPropDensity));

			// Maximum attempts to place props to avoid infinite loop
			int maxAttempts = propsToPlace * 3;
			int attempts = 0;
			int propsPlaced = 0;

			while (propsPlaced < propsToPlace && attempts < maxAttempts)
			{
				attempts++;

				// Calculate random position WITHIN the room boundaries
				float randomX = Mathf.Lerp(minX + 1f, maxX - 1f, (float)random.NextDouble());
				float randomZ = Mathf.Lerp(minZ + 1f, maxZ - 1f, (float)random.NextDouble());

				Vector3 propPosition = new Vector3(
					randomX,
					roomOrigin.y + roofHeight - currentRoofPropSet.heightOffset,
					randomZ
				);

				// Avoid placing props too close to each other
				bool tooCloseToOther = false;
				foreach (Vector3 existingPropPos in placedRoofPropPositions)
				{
					if (Vector3.Distance(propPosition, existingPropPos) < minRoofPropDistance)
					{
						tooCloseToOther = true;
						break;
					}
				}

				// Avoid placing props too close to doors
				bool tooCloseToDoor = false;
				foreach (Vector3 doorPos in doorPositions)
				{
					Vector3 doorTopPos = new Vector3(doorPos.x, propPosition.y, doorPos.z);
					if (Vector3.Distance(propPosition, doorTopPos) < doorwayAvoidanceRadius * 1.5f)
					{
						tooCloseToDoor = true;
						break;
					}
				}

				if (tooCloseToOther || tooCloseToDoor)
				{
					continue;
				}

				int propIndex = random.Next(0, currentRoofPropSet.roofPropPrefabs.Length);
				GameObject propPrefab = currentRoofPropSet.roofPropPrefabs[propIndex];

				if (propPrefab != null)
				{
					float randomRotation = (float)random.NextDouble() * 360f;
					Quaternion propRotation = Quaternion.Euler(0, randomRotation, 0);
					GameObject prop = Instantiate(propPrefab, propPosition, propRotation, roofPropsParent);
					placedRoofPropPositions.Add(propPosition);
					propsPlaced++;
				}
			}
		}
	}
}