using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ProceduralRoom : MonoBehaviour
{
	[Header("Floor Settings")]
	[SerializeField] GameObject[] floorPrefabs = new GameObject[3];
	[SerializeField] float[] floorProbabilities = new float[] { 0.45f, 0.45f, 0.1f };

	[Header("Wall Settings")]
	[SerializeField] GameObject[] wallPrefabs = new GameObject[4]; 

	[Header("Door Settings")]
	[SerializeField] bool hasNorthDoor = false;
	[SerializeField] bool hasSouthDoor = false;
	[SerializeField] bool hasEastDoor = false;
	[SerializeField] bool hasWestDoor = false;
	[Range(0f, 1f)]
	[SerializeField] float secondDoorwayChance = 0.5f; 
	[SerializeField] float doorwayWidth = 1.5f;

	[Header("Prop Settings")]
	[SerializeField] GameObject[] propPrefabs; 
	[Range(0f, 1f)]
	[SerializeField] float propDensity = 0.2f; 
	[SerializeField] float propSpacing = 1.0f; 
	[SerializeField] bool avoidDoorways = true; 
	[SerializeField] float doorwayAvoidanceRadius = 2.0f; 

	[Header("Room Settings")]
	[SerializeField] float roomWidth = 10f;
	[SerializeField] float roomLength = 10f;
	[SerializeField] float cellSize = 1f;
	[SerializeField] bool showGrid = true;
	[SerializeField] float wallAlignmentOffset = 0.01f;  

	private float lastWidth;
	private float lastLength;
	private Cell[,] grid;
	private Vector3 roomOrigin;
	private System.Random random;

	private List<Vector3> doorPositions = new List<Vector3>();

	[System.Serializable]
	public class Cell
	{
		public Vector3 worldPosition;
		public bool isWalkable = true;
		public bool isOccupied = false;

		public Cell(Vector3 position)
		{
			worldPosition = position;
		}
	}

	private void Start()
	{
		random = new System.Random();
		roomOrigin = transform.position;

		ValidatePrefabs();

		GenerateRoom();
		lastWidth = roomWidth;
		lastLength = roomLength;
	}

	private void ValidatePrefabs()
	{
		if (floorPrefabs.Length < 3)
		{
			Debug.LogError("Not enough floor prefabs assigned! Need 3 variations.");
			return;
		}

		if (wallPrefabs.Length < 4)
		{
			Debug.LogError("Not enough wall prefabs assigned! Need 3 wall variations + 1 doorway (index 3).");
			return;
		}

		if (propPrefabs == null || propPrefabs.Length == 0)
		{
			Debug.LogWarning("No prop prefabs assigned. Props will not be generated.");
		}

		float sum = 0;
		foreach (var prob in floorProbabilities)
		{
			sum += prob;
		}

		if (Mathf.Approximately(sum, 0))
		{
			floorProbabilities = new float[] { 0.45f, 0.45f, 0.1f };
		}
		else if (!Mathf.Approximately(sum, 1.0f))
		{
			for (int i = 0; i < floorProbabilities.Length; i++)
			{
				floorProbabilities[i] /= sum;
			}
		}
	}

	private void Update()
	{
		if (!Mathf.Approximately(roomWidth, lastWidth) || !Mathf.Approximately(roomLength, lastLength))
		{
			RegenerateRoom();
			lastWidth = roomWidth;
			lastLength = roomLength;
		}
	}

	private void GenerateRoom()
	{
		doorPositions.Clear();
		CreateFloor();
		CreateGrid();
		CreateWalls();
		MarkWallsInGrid();
		PlaceProps();
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
		if (floorPrefabs[0] == null)
		{
			Debug.LogError("Floor prefab[0] is not assigned!");
			return;
		}

		Renderer renderer = floorPrefabs[0].GetComponent<Renderer>();
		if (renderer == null)
		{
			Debug.LogError("Floor prefab does not have a Renderer component!");
			return;
		}

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

	private void CreateGrid()
	{
		Renderer renderer = floorPrefabs[0].GetComponent<Renderer>();
		Vector3 tileSize = renderer.bounds.size;

		float actualWidth = roomWidth * tileSize.x;
		float actualLength = roomLength * tileSize.z;

		int gridWidth = Mathf.CeilToInt(actualWidth / cellSize);
		int gridLength = Mathf.CeilToInt(actualLength / cellSize);

		grid = new Cell[gridWidth, gridLength];

		float offsetX = -(tileSize.x / 2) + roomOrigin.x;
		float offsetZ = -(tileSize.z / 2) + roomOrigin.z;

		for (int x = 0; x < gridWidth; x++)
		{
			for (int z = 0; z < gridLength; z++)
			{
				Vector3 worldPos = new Vector3(
					offsetX + x * cellSize + (cellSize / 2),
					roomOrigin.y,
					offsetZ + z * cellSize + (cellSize / 2)
				);
				grid[x, z] = new Cell(worldPos);
			}
		}
	}

	private void CreateWalls()
	{
		if (wallPrefabs[0] == null || wallPrefabs[3] == null)
		{
			Debug.LogError("Wall prefabs are not properly assigned!");
			return;
		}

		Renderer floorRenderer = floorPrefabs[0].GetComponent<Renderer>();
		Renderer wallRenderer = wallPrefabs[0].GetComponent<Renderer>();

		if (floorRenderer == null || wallRenderer == null)
		{
			Debug.LogError("Prefabs are missing Renderer components!");
			return;
		}

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
		List<int> doorIndices = new List<int>();

		if (hasDoor)
		{
			int centerIndex = wallCount / 2;
			doorIndices.Add(centerIndex);

			if ((float)random.NextDouble() < secondDoorwayChance)
			{
				int secondDoorIndex;
				do
				{
					secondDoorIndex = random.Next(0, wallCount);
				} while (secondDoorIndex == centerIndex);

				doorIndices.Add(secondDoorIndex);
			}
		}

		for (int i = 0; i < wallCount; i++)
		{
			int wallType;

			if (doorIndices.Contains(i))
			{
				wallType = 3; 
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

	private void MarkWallsInGrid()
	{
		foreach (Transform child in transform)
		{
			bool isWallOrDoor = false;
			for (int i = 0; i < wallPrefabs.Length; i++)
			{
				if (child.gameObject.name.Contains(wallPrefabs[i].name))
				{
					isWallOrDoor = true;
					break;
				}
			}

			if (isWallOrDoor && child.gameObject.GetComponent<Renderer>() != null)
			{
				Bounds objBounds = child.gameObject.GetComponent<Renderer>().bounds;
				bool isDoorway = child.gameObject.name.Contains(wallPrefabs[3].name); 

				for (int x = 0; x < grid.GetLength(0); x++)
				{
					for (int z = 0; z < grid.GetLength(1); z++)
					{
						if (grid[x, z] == null) continue;

						Vector3 cellPos = grid[x, z].worldPosition;
						Bounds cellBounds = new Bounds(cellPos, new Vector3(cellSize, 1, cellSize));

						if (cellBounds.Intersects(objBounds))
						{
							grid[x, z].isWalkable = isDoorway;
						}
					}
				}
			}
		}
	}

	private void PlaceProps()
	{
		if (propPrefabs == null || propPrefabs.Length == 0)
		{
			return;
		}

		Transform propsParent = new GameObject("Props").transform;
		propsParent.SetParent(transform);

		List<Cell> availableCells = new List<Cell>();
		for (int x = 0; x < grid.GetLength(0); x++)
		{
			for (int z = 0; z < grid.GetLength(1); z++)
			{
				Cell cell = grid[x, z];
				if (cell != null && cell.isWalkable && !cell.isOccupied)
				{
					bool isTooCloseToADoor = false;
					if (avoidDoorways)
					{
						foreach (Vector3 doorPos in doorPositions)
						{
							if (Vector3.Distance(cell.worldPosition, doorPos) < doorwayAvoidanceRadius)
							{
								isTooCloseToADoor = true;
								break;
							}
						}
					}

					if (!isTooCloseToADoor)
					{
						availableCells.Add(cell);
					}
				}
			}
		}

		int propsToPlace = Mathf.FloorToInt(availableCells.Count * propDensity);

		for (int i = 0; i < propsToPlace; i++)
		{
			if (availableCells.Count == 0)
			{
				break;
			}

			int cellIndex = random.Next(0, availableCells.Count);
			Cell selectedCell = availableCells[cellIndex];

			int propIndex = random.Next(0, propPrefabs.Length);
			GameObject propPrefab = propPrefabs[propIndex];

			if (propPrefab != null)
			{
				Vector3 propPosition = selectedCell.worldPosition;
				float randomRotation = (float)random.NextDouble() * 360f;
				GameObject prop = Instantiate(propPrefab, propPosition, Quaternion.Euler(0, randomRotation, 0), propsParent);
				selectedCell.isOccupied = true;
				RemoveNearbyCells(availableCells, selectedCell.worldPosition, propSpacing);
			}
		}
	}

	private void RemoveNearbyCells(List<Cell> cells, Vector3 position, float radius)
	{
		for (int i = cells.Count - 1; i >= 0; i--)
		{
			if (Vector3.Distance(cells[i].worldPosition, position) <= radius)
			{
				cells.RemoveAt(i);
			}
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
		Renderer renderer = floorPrefabs[0].GetComponent<Renderer>();
		Vector3 tileSize = renderer.bounds.size;
		float offsetX = -(tileSize.x / 2) + roomOrigin.x;
		float offsetZ = -(tileSize.z / 2) + roomOrigin.z;

		int x = Mathf.FloorToInt((worldPos.x - offsetX) / cellSize);
		int z = Mathf.FloorToInt((worldPos.z - offsetZ) / cellSize);

		return new Vector2Int(x, z);
	}

	public Cell GetCell(int x, int z)
	{
		if (x >= 0 && x < grid.GetLength(0) && z >= 0 && z < grid.GetLength(1))
		{
			return grid[x, z];
		}
		return null;
	}

	private void OnDrawGizmos()
	{
		if (!showGrid || grid == null) return;

		for (int x = 0; x < grid.GetLength(0); x++)
		{
			for (int z = 0; z < grid.GetLength(1); z++)
			{
				if (grid[x, z] == null) continue;
				if (!grid[x, z].isWalkable)
				{
					Gizmos.color = Color.red;
				}
				else if (grid[x, z].isOccupied)
				{
					Gizmos.color = Color.blue;
				}
				else
				{
					Gizmos.color = Color.green;
				}

				Vector3 pos = grid[x, z].worldPosition;
				Vector3 size = new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f);
				Gizmos.DrawWireCube(pos, size);
			}
		}

		if (avoidDoorways && Application.isPlaying)
		{
			Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); 
			foreach (Vector3 doorPos in doorPositions)
			{
				Gizmos.DrawSphere(doorPos, doorwayAvoidanceRadius);
			}
		}
	}
}