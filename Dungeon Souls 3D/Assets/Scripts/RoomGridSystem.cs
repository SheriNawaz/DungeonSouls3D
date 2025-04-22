using System;
using System.Collections.Generic;
using UnityEngine;

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

public class RoomGridSystem : MonoBehaviour
{
	[SerializeField] private float cellSize = 1f;
	[SerializeField] private bool showGrid = true;
	[SerializeField] private bool respectRoomBoundaries = true; 

	private Cell[,] grid;
	private Vector3 roomOrigin;
	private float roomWidth;
	private float roomLength;
	private float floorTileSize;

	// Store the actual room bounds for grid constraint
	private float minX;
	private float maxX;
	private float minZ;
	private float maxZ;

	public void Initialize(Vector3 origin, float width, float length, float tileSize)
	{
		roomOrigin = origin;
		roomWidth = width;
		roomLength = length;
		floorTileSize = tileSize;

		// Calculate room dimensions in world units
		float actualWidth = roomWidth * floorTileSize;
		float actualLength = roomLength * floorTileSize;

		// Calculate room boundaries
		float offsetX = -(floorTileSize / 2) + roomOrigin.x;
		float offsetZ = -(floorTileSize / 2) + roomOrigin.z;

		minX = offsetX;
		maxX = offsetX + actualWidth;
		minZ = offsetZ;
		maxZ = offsetZ + actualLength;

		CreateGrid();
	}

	public Cell[,] GetGrid()
	{
		return grid;
	}

	public bool HasGrid()
	{
		return grid != null;
	}

	public float GetCellSize()
	{
		return cellSize;
	}

	private void CreateGrid()
	{
		float actualWidth = roomWidth * floorTileSize;
		float actualLength = roomLength * floorTileSize;

		// Calculate how many cells fit within the room dimensions 
		int gridWidth = Mathf.FloorToInt(actualWidth / cellSize);
		int gridLength = Mathf.FloorToInt(actualLength / cellSize);

		grid = new Cell[gridWidth, gridLength];

		float offsetX = -(floorTileSize / 2) + roomOrigin.x;
		float offsetZ = -(floorTileSize / 2) + roomOrigin.z;

		// Center grid in room
		float centerOffsetX = (actualWidth - (gridWidth * cellSize)) / 2;
		float centerOffsetZ = (actualLength - (gridLength * cellSize)) / 2;

		for (int x = 0; x < gridWidth; x++)
		{
			for (int z = 0; z < gridLength; z++)
			{
				Vector3 worldPos = new Vector3(
					offsetX + centerOffsetX + x * cellSize + (cellSize / 2),
					roomOrigin.y,
					offsetZ + centerOffsetZ + z * cellSize + (cellSize / 2)
				);

				// Make sure position is within room bounds
				if (!respectRoomBoundaries || IsPositionWithinRoomBounds(worldPos))
				{
					grid[x, z] = new Cell(worldPos);
				}
			}
		}
	}

	private bool IsPositionWithinRoomBounds(Vector3 position)
	{
		// Add some padding to avoid cells right at the edge
		float margin = cellSize * 0.05f;
		return position.x >= minX + margin &&
			   position.x <= maxX - margin &&
			   position.z >= minZ + margin &&
			   position.z <= maxZ - margin;
	}

	public void MarkWallsInGrid(Transform roomTransform, GameObject[] wallPrefabs)
	{
		foreach (Transform child in roomTransform)
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
				bool isDoorway = child.gameObject.name.Contains(wallPrefabs[3].name); // Index 3 is doorway

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

	public List<Cell> GetAvailableCells(List<Vector3> doorPositions, bool avoidDoorways, float doorwayAvoidanceRadius)
	{
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

		return availableCells;
	}

	public void RemoveNearbyCells(List<Cell> cells, Vector3 position, float radius)
	{
		for (int i = cells.Count - 1; i >= 0; i--)
		{
			if (Vector3.Distance(cells[i].worldPosition, position) <= radius)
			{
				cells.RemoveAt(i);
			}
		}
	}

	public Vector2Int WorldToGrid(Vector3 worldPos)
	{
		float offsetX = -(floorTileSize / 2) + roomOrigin.x;
		float offsetZ = -(floorTileSize / 2) + roomOrigin.z;

		// Account for grid centering in the room
		float actualWidth = roomWidth * floorTileSize;
		float actualLength = roomLength * floorTileSize;
		int gridWidth = Mathf.FloorToInt(actualWidth / cellSize);
		int gridLength = Mathf.FloorToInt(actualLength / cellSize);
		float centerOffsetX = (actualWidth - (gridWidth * cellSize)) / 2;
		float centerOffsetZ = (actualLength - (gridLength * cellSize)) / 2;

		int x = Mathf.FloorToInt((worldPos.x - offsetX - centerOffsetX) / cellSize);
		int z = Mathf.FloorToInt((worldPos.z - offsetZ - centerOffsetZ) / cellSize);

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

		// Draw room boundaries for debugging
		Gizmos.color = Color.red;
		Vector3 center = new Vector3((minX + maxX) / 2, roomOrigin.y, (minZ + maxZ) / 2);
		Vector3 size = new Vector3(maxX - minX, 0.05f, maxZ - minZ);
		Gizmos.DrawWireCube(center, size);

		// Draw grid cells
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
				Vector3 size2 = new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f);
				Gizmos.DrawWireCube(pos, size2);
			}
		}
	}

	public void DrawDoorRadiusGizmos(List<Vector3> doorPositions, float doorwayAvoidanceRadius)
	{
		if (doorPositions == null || doorPositions.Count == 0) return;

		Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
		foreach (Vector3 doorPos in doorPositions)
		{
			Gizmos.DrawSphere(doorPos, doorwayAvoidanceRadius);
		}
	}

	public void MarkCellAsOccupied(Vector3 worldPosition)
	{
		Vector2Int gridPos = WorldToGrid(worldPosition);
		Cell cell = GetCell(gridPos.x, gridPos.y);
		if (cell != null)
		{
			cell.isOccupied = true;
		}
	}

}