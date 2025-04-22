using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpawnpointManager : MonoBehaviour
{
    public SpawnPoint[] allSpawnPoints;
	public Button spawnButton;
	public GameObject spawnView;

	private void Start()
	{
		StartCoroutine(DelayedGetSpawnPoints());
	}

	public IEnumerator DelayedGetSpawnPoints()
	{
		//Get spawn points AFTER dungeon generated
		yield return new WaitForEndOfFrame();
		yield return null;
		GetSpawnPoints();
	}

	public void GetSpawnPoints()
	{
		//Find all objects that contain SpawnPoint and store in array
		allSpawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
		for (int i = 0; i < allSpawnPoints.Length; i++)
		{
			int number = i + 1;
			allSpawnPoints[i].number = number;
		}
	}

	public void CreateSpawnUI()
	{
		foreach (Transform child in spawnView.transform)
		{
			Destroy(child.gameObject);
		}
		foreach(var spawnPoint in allSpawnPoints)
		{
			if (spawnPoint.visited)
			{
				//Create buttons for each spawnpoint player has visited
				Button spawnButtonInstance = Instantiate(spawnButton, spawnView.transform);
				TMP_Text buttonText = spawnButtonInstance.GetComponentInChildren<TMP_Text>();
				buttonText.text = "SpawnPoint " + spawnPoint.number;
			}
		}
	}

	public SpawnPoint GetSpawnPointByNumber(int number)
	{
		return allSpawnPoints.FirstOrDefault(sp => sp.number == number);
	}

	public void Teleport()
	{
		//Teleport between spawn rooms
		GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
		string buttonText = selectedObject.GetComponentInChildren<TMP_Text>().text; 
		SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
		int spawnNumber = int.Parse(buttonText.Substring(buttonText.LastIndexOf(" ") + 1));
		print(spawnNumber);
		foreach (var point in spawns)
		{
			if(spawnNumber == point.number)
			{
				print(spawnNumber + " Teleported");
				FindFirstObjectByType<PlayerController>().transform.position = point.transform.position;
				FindFirstObjectByType<PlayerController>().currentSpawnPoint = point;
				break;
			}
		}
	}
}
