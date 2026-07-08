using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject startMenu;

    [SerializeField]
    Transform gridBorderGenerator;

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

        for (int i = 0; i < gridBorderGenerator.childCount; i++)
            gridBorderGenerator.transform.GetChild(i).GetComponent<Renderer>().enabled = false;
        
    }

    public void Reset()
    {
        mainCamera.SetActive(true);
        playerCamera.SetActive(false);

        if (startMenu != null)
            startMenu.SetActive(false);

        for (int i = 0; i < gridBorderGenerator.childCount; i++)
            gridBorderGenerator.transform.GetChild(i).GetComponent<Renderer>().enabled = true;
    }
}