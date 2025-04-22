using System.Collections.Generic;
using UnityEngine;
using static DungeonGenerator;

public enum EnemyType
{
	Basic,
	Advanced
}

[System.Serializable]
public class EnemyPrefabSet
{
	public GameObject basicEnemyPrefab;
	public GameObject advancedEnemyPrefab;
}

public class EnemySpawner : MonoBehaviour
{
	[Header("Enemy Prefabs")]
	[SerializeField] private EnemyPrefabSet enemyPrefabs;

	[Header("Enemy Spawning Settings")]
	[SerializeField] private Transform enemiesParent;
	[SerializeField] private Vector2 basicEnemyRange = new Vector2(1, 3); // Min-Max basic enemies (skeletons)
	[SerializeField] private Vector2 advancedEnemyRange = new Vector2(1, 2); // Min-Max advanced enemies (big monsters)
	[SerializeField] private float enemySpawnHeightOffset = 0.1f; // Height above floor
	[SerializeField][Range(0f, 1f)] private float advancedEnemyChance = 0.7f; // Probability of advanced enemies in the second half

	// Reference to the dungeon generator
	private DungeonGenerator dungeonGenerator;

	private void Start()
	{
		dungeonGenerator = GetComponent<DungeonGenerator>();
	}

	public void SpawnEnemiesInDungeon()
	{
		if (enemiesParent == null)
		{
			enemiesParent = new GameObject("Enemies").transform;
			enemiesParent.SetParent(transform);
		}
		else
		{
			// Clear any existing enemies
			for (int i = enemiesParent.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(enemiesParent.GetChild(i).gameObject);
			}
		}

		// Get rooms from DungeonGenerator
		if (dungeonGenerator == null) return;

		List<RoomNode> allRooms = GetAllRoomsFromDungeonGenerator();
		if (allRooms == null || allRooms.Count == 0) return;

		int totalRooms = allRooms.Count;
		int firstQuarterEnd = totalRooms / 4;
		int secondQuarterEnd = totalRooms / 2;

		for (int i = 0; i < allRooms.Count; i++)
		{
			RoomNode room = allRooms[i];

			// Skip the start room, boss room, and map/key rooms
			if (room.roomType == RoomType.SpawnPoint ||
				room.roomType == RoomType.BossRoom ||
				room.roomType == RoomType.Map ||
				room.roomType == RoomType.Key)
			{
				continue;
			}

			ProceduralRoom proceduralRoom = room.roomComponent;
			if (proceduralRoom == null) continue;

			if (i < firstQuarterEnd)
			{
				// First quarter of rooms spawn 1 enemy
				SpawnEnemyInRoom(proceduralRoom, 1, 0);
			}
			else if (i < secondQuarterEnd)
			{
				// Second quarter of rooms spawn a range of basic enemies
				int basicCount = Mathf.RoundToInt(Random.Range(basicEnemyRange.x, basicEnemyRange.y));
				SpawnEnemyInRoom(proceduralRoom, basicCount, 0);
			}
			else
			{
				// Final half of rooms spawn big monsters or basic enemy hordes
				if (Random.value < advancedEnemyChance)
				{
					int advancedCount = Mathf.RoundToInt(Random.Range(advancedEnemyRange.x, advancedEnemyRange.y));

					// Sometimes add a few basic enemies along with big
					int basicCount = Random.value < 0.3f ? Mathf.RoundToInt(Random.Range(1, 2)) : 0;

					SpawnEnemyInRoom(proceduralRoom, basicCount, advancedCount);
				}
				else
				{
					// Spawn basic enemies 
					int basicCount = Mathf.RoundToInt(Random.Range(basicEnemyRange.x, basicEnemyRange.y));
					SpawnEnemyInRoom(proceduralRoom, basicCount, 0);
				}
			}
		}
	}

	private void SpawnEnemyInRoom(ProceduralRoom room, int basicCount, int advancedCount)
	{
		//Spawn enemies in available cells
		if (room == null ||
			(basicCount <= 0 && advancedCount <= 0) ||
			enemyPrefabs.basicEnemyPrefab == null ||
			(advancedCount > 0 && enemyPrefabs.advancedEnemyPrefab == null))
		{
			return;
		}

		RoomGridSystem gridSystem = room.GetComponent<RoomGridSystem>();
		if (gridSystem == null) return;

		List<Vector3> doorPositions = GetDoorPositionsFromRoom(room);
		List<Cell> availableCells = gridSystem.GetAvailableCells(doorPositions, true, 2f);

		if (availableCells == null || availableCells.Count == 0) return;

		// Spawn big enemies first as they need more space
		for (int i = 0; i < advancedCount; i++)
		{
			if (availableCells.Count == 0) break;

			int randomIndex = Random.Range(0, availableCells.Count);
			Vector3 spawnPos = availableCells[randomIndex].worldPosition + Vector3.up * enemySpawnHeightOffset;

			GameObject enemy = Instantiate(enemyPrefabs.advancedEnemyPrefab, spawnPos, Quaternion.identity, enemiesParent);
			gridSystem.RemoveNearbyCells(availableCells, spawnPos, 2f);
		}

		// Spawn skeletons after advanced enemies
		for (int i = 0; i < basicCount; i++)
		{
			if (availableCells.Count == 0) break;

			int randomIndex = Random.Range(0, availableCells.Count);
			Vector3 spawnPos = availableCells[randomIndex].worldPosition + Vector3.up * enemySpawnHeightOffset;

			GameObject enemy = Instantiate(enemyPrefabs.basicEnemyPrefab, spawnPos, Quaternion.identity, enemiesParent);
			gridSystem.RemoveNearbyCells(availableCells, spawnPos, 1.5f);
		}
	}

	private List<Vector3> GetDoorPositionsFromRoom(ProceduralRoom room)
	{
		List<Vector3> doorPositions = new List<Vector3>();

		Vector3 roomPosition = room.transform.position;
		float roomWidth = room.roomWidth;
		float roomLength = room.roomLength;

		if (room.hasNorthDoor)
		{
			doorPositions.Add(new Vector3(roomPosition.x, roomPosition.y, roomPosition.z + roomLength / 2));
		}

		if (room.hasSouthDoor)
		{
			doorPositions.Add(new Vector3(roomPosition.x, roomPosition.y, roomPosition.z - roomLength / 2));
		}

		if (room.hasEastDoor)
		{
			doorPositions.Add(new Vector3(roomPosition.x + roomWidth / 2, roomPosition.y, roomPosition.z));
		}

		if (room.hasWestDoor)
		{
			doorPositions.Add(new Vector3(roomPosition.x - roomWidth / 2, roomPosition.y, roomPosition.z));
		}

		return doorPositions;
	}

	private List<RoomNode> GetAllRoomsFromDungeonGenerator()
	{
		System.Reflection.FieldInfo fieldInfo = typeof(DungeonGenerator).GetField("allRooms",
												System.Reflection.BindingFlags.NonPublic |
												System.Reflection.BindingFlags.Instance);

		if (fieldInfo != null)
		{
			return fieldInfo.GetValue(dungeonGenerator) as List<RoomNode>;
		}

		return null;
	}
}