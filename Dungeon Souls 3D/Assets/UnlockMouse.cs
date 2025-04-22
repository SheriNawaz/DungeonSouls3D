using UnityEngine;

public class UnlockMouse : MonoBehaviour
{
	private void Start()
	{
		Cursor.lockState = CursorLockMode.None;
	}
}
