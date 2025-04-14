using UnityEngine;

public class GroundCheck : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Ground")
		{
			PlayerController player = GetComponentInParent<PlayerController>();
			player.isGrounded = true;
		}
	}
}
