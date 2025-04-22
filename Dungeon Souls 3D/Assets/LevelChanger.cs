using System.Collections;
using UnityEngine;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] Transform merchants;
    [SerializeField] Transform enemies;
    [SerializeField] PlayerController player;
    [SerializeField] GameObject dungeon1;
    [SerializeField] GameObject dungeon2;
    [SerializeField] GameObject bossRoom;
    [SerializeField] GameObject bossRoom2;
	[SerializeField] GameObject boss2;

	public bool nextLevel = false;

    public void ChangeLevel()
    {
		//Reset necessary stats when going to next level
        player.hasBossKey = false;
        player.hasMapFragment = false;
        FindFirstObjectByType<PlayerInventory>().mapIcon.SetActive(false);
		FindFirstObjectByType<PlayerInventory>().minimapCover.SetActive(true);
        dungeon1.SetActive(false);
		//Clear all enemies and merchants
		foreach (Transform child in merchants)
		{
			Destroy(child.gameObject);
		}
		foreach (Transform child in enemies)
		{
			Destroy(child.gameObject);
		}
		//Spawn new bossroom and new dungeon
		dungeon2.SetActive(true);
        bossRoom.SetActive(false);
		bossRoom2.SetActive(true);

		player.currentSpawnPoint = null;
		SpawnpointManager spawnpointManager= FindAnyObjectByType<SpawnpointManager>();
		spawnpointManager.allSpawnPoints = null;
		StartCoroutine(spawnpointManager.DelayedGetSpawnPoints());
		nextLevel = true;

		StartCoroutine(DelayedUpdates());
	}

    IEnumerator DelayedUpdates()
    {
        yield return new WaitForSeconds(2f);
		Time.timeScale = 0f;
		//Reset player position
		player.transform.position = new Vector3(0, 0, 0);
		yield return new WaitForSeconds(0.5f);
		Time.timeScale = 1f;
		foreach (Transform child in enemies)
		{
			//Scale up all the new enemies in the new level
			child.GetComponent<EnemyController>().health *= 2;
			child.GetComponent<EnemyController>().xpDropped *= 3;
			child.GetComponent<EnemyController>().damage *= 2;
			print("Child Stats Boosted" + child.gameObject.name);
		}

		boss2.SetActive(true);
		player.currentSpawnPoint = FindFirstObjectByType<SpawnPoint>();
	}
}

