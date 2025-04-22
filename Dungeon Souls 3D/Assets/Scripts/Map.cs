using UnityEngine;

public class Map : MonoBehaviour
{
	private void LateUpdate()
	{
		//Make sure the map center is the dungeon center
		DungeonGenerator dungeon = FindFirstObjectByType<DungeonGenerator>();
		Vector2 dungeonPos = dungeon.CalculateDungeonCenter();
		transform.position = new Vector3(dungeonPos.x, transform.position.y, dungeonPos.y);
	}
}
