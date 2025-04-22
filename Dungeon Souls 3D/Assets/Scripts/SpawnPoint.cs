using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	//Class to mark spawnpoints as visited
    public bool visited = false;
    public int number;

	public void MarkAsVisited()
	{
		visited = true;
	}
}
