using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject stats;

    public static bool paused = false;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && paused)
        {
            pauseMenu.SetActive(false);
            stats.SetActive(true);
			paused = false;
            Time.timeScale = 1f;
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && !paused)
        {
            stats.SetActive(false);
			pauseMenu.SetActive(true);
			paused = true;
			Time.timeScale = 0f;
		}
	}
}
