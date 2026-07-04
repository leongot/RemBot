using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject startMenu;

    private void Start()
    {
        mainCamera.SetActive(true);
        playerCamera.SetActive(false);

        if (startMenu != null)
            startMenu.SetActive(true);
    }

    public void StartGame()
    {
        mainCamera.SetActive(false);
        playerCamera.SetActive(true);

        if (startMenu != null)
            startMenu.SetActive(false);
    }
}