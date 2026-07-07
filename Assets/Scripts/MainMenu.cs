using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private AudioSettings audioSettings;
    [SerializeField] private LevelSelectionMenu levelSelectionMenu;
    private void Start()
    {
        // Debug print the actual window dimensions on launch
        Debug.Log($"Game started with resolution: {Screen.width} x {Screen.height}");
        audioSettings?.Initialize();
        levelSelectionMenu?.Initialize();
    }

    // Audio delegations

    public void SetVolume(float sliderValue)
    {
        audioSettings?.SetVolume(sliderValue);
    }
    public void PlayOnFirstClick()
    {
        audioSettings?.PlayOnFirstClick();
    }

    public void PlayForwardSound()
    {
        audioSettings?.PlayForwardSound();
    }

    public void PlayBackwardSound()
    {
        audioSettings?.PlayBackwardSound();
    }

    // Level delegations (keeps existing UI wiring simple)

    public void StartLevel1() => levelSelectionMenu?.StartLevel(1);
    public void StartLevel2() => levelSelectionMenu?.StartLevel(2);
    public void StartLevel3() => levelSelectionMenu?.StartLevel(3);
    public void MarkLevelCompleted(int levelNumber) => levelSelectionMenu?.MarkLevelCompleted(levelNumber);
    public void ResetProgress() => levelSelectionMenu?.ResetProgress();
    
    public void ExitGame()
    {
        Application.Quit();
    }
} 