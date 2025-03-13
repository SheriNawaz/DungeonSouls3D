using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private PlayerController player;
    
    void Start()
    {
        player = GetComponentInParent<PlayerController>();    
    }

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Ground")
        {
            player.isGrounded = true;
            player.animator.SetBool("Jumping", false);
            StartCoroutine(player.RegenStam());
        }
	}
}
