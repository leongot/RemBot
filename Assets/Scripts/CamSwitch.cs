using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject startMenu;

    private void Awake()
    {
        mainCamera.SetActive(true);
        playerCamera.SetActive(false);

        if (startMenu != null)
            startMenu.SetActive(true);
    }

    public void NewLevelStart()
    {
        mainCamera.SetActive(false);
        playerCamera.SetActive(true);

        if (startMenu != null)
            startMenu.SetActive(false);
    }

    public void ResetLevel()
    {
        mainCamera.SetActive(true);
        playerCamera.SetActive(false);
    }
}