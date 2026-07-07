using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionMenu : MonoBehaviour
{
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;

    public void Initialize() => UpdateLevelButtons();

    public void StartLevel(int levelNumber)
    {
        if (!LevelSaveSystem.IsLevelUnlocked(levelNumber))
            return;

        SceneManager.LoadScene(levelNumber);
    }

    public void MarkLevelCompleted(int levelNumber)
    {
        LevelSaveSystem.MarkLevelCompleted(levelNumber);
        UpdateLevelButtons();
    }

    public void ResetProgress()
    {
        LevelSaveSystem.ResetAllProgress();
        UpdateLevelButtons();
    }

    public void UpdateLevelButtons()
    {
        SetButtonInteractable(level1Button, 1);
        SetButtonInteractable(level2Button, 2);
        SetButtonInteractable(level3Button, 3);
    }

    private void SetButtonInteractable(Button button, int levelNumber)
    {
        if (button != null)
            button.interactable = LevelSaveSystem.IsLevelUnlocked(levelNumber);
    }
}
