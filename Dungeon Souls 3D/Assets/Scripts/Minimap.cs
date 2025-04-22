using UnityEngine;

public class Minimap : MonoBehaviour
{
    PlayerController player;

	private void Start()
	{
		player = FindFirstObjectByType<PlayerController>();
	}

	private void LateUpdate()
	{
		//Update position of minimap to be around player
		transform.position = new Vector3(player.transform.position.x, 20f, player.transform.position.z);
	}
}
