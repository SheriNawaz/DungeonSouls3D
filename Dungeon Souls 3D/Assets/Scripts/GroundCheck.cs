using UnityEngine;

public class GroundCheck : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Ground")
		{
			//When the player collides with ground update isGrounded
			PlayerController player = GetComponentInParent<PlayerController>();
			player.isGrounded = true;
		}
	}
}
