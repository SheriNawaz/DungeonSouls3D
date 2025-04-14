using UnityEngine;

public class LockOnIndicator : MonoBehaviour
{
	public void On()
	{
		GetComponent<MeshRenderer>().enabled = true;
	}

	public void Off()
	{
		GetComponent<MeshRenderer>().enabled = false;
	}
}
