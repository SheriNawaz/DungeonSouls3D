using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject stats;
	[SerializeField] GameObject shop;
	[SerializeField] GameObject spawnMenu;
	[SerializeField] Image shopView1;
	[SerializeField] Image shopView2;
	[SerializeField] GameObject map;
	[SerializeField] GameObject mainCam;
	[SerializeField] GameObject lockCam;
	[SerializeField] GameObject mapUI;

	public static bool paused = false;

	private void Start()
	{
		LockCursor();
	}

	void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && paused)
		{
			Resume();
		}
		else if(Input.GetKeyDown(KeyCode.Escape) && !paused)
		{
			Pause();
		}
	}

	public void Pause()
	{
		stats.SetActive(false);
		pauseMenu.SetActive(true);
		shop.SetActive(false);
		spawnMenu.SetActive(false);
		paused = true;
		Time.timeScale = 0f;
		UnlockCursor();
	}

	public void ViewMap()
	{
		pauseMenu.SetActive(false);
		shop.SetActive(false);
		stats.SetActive(false);
		spawnMenu.SetActive(false);
		mapUI.SetActive(true);
		paused = true;
		map.SetActive(true);
		mainCam.SetActive(false);
		lockCam.SetActive(false);
		Time.timeScale = 0f;
		UnlockCursor();
	}

	public void Resume()
	{
		pauseMenu.SetActive(false);
		shop.SetActive(false);
		stats.SetActive(true);
		spawnMenu.SetActive(false);
		mapUI.SetActive(false);
		paused = false;
		map.SetActive(false);
		mainCam.SetActive(true);
		lockCam.SetActive(false);
		Time.timeScale = 1f;
		LockCursor();
	}

	public void EnterShop()
	{
		stats.SetActive(false);
		pauseMenu.SetActive(false);
		shop.SetActive(true);
		spawnMenu.SetActive(false);
		paused = true;
		Time.timeScale = 0f;
		UnlockCursor();
	}

	public void EnterSpawn()
	{
		stats.SetActive(false);
		pauseMenu.SetActive(false);
		shop.SetActive(false);
		spawnMenu.SetActive(true);
		paused = true;
		Time.timeScale = 0f;
		UnlockCursor();
	}

	private void LockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void UnlockCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}
