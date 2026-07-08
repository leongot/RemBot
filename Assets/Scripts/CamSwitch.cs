using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject startMenu;


    void Start()
    {
        mainCamera.SetActive(true);
        playerCamera.SetActive(false);
    }

    public void NewGameStart()
    {
        mainCamera.SetActive(false);
        playerCamera.SetActive(true);

        if (startMenu != null)
            startMenu.SetActive(true);
    }

    public void Reset()
    {
        mainCamera.SetActive(true);
        playerCamera.SetActive(false);

        if (startMenu != null)
            startMenu.SetActive(false);
    }
}