using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionMenu : MonoBehaviour
{
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;

    public void Initialize()
    {
        UpdateLevelButtons();
    }

    public void StartLevel(int levelNumber)
    {
        if (!LevelProgress.IsLevelUnlocked(levelNumber)) return;
        SceneManager.LoadScene(levelNumber);
    }

    public void MarkLevelCompleted(int levelNumber)
    {
        LevelProgress.SetLevelCompleted(levelNumber);
        UpdateLevelButtons();
    }

    public void ResetProgress()
    {
        LevelProgress.ResetProgress();
        UpdateLevelButtons();
    }

    public void UpdateLevelButtons()
    {
        if (level1Button != null) level1Button.interactable = LevelProgress.IsLevelUnlocked(1);
        if (level2Button != null) level2Button.interactable = LevelProgress.IsLevelUnlocked(2);
        if (level3Button != null) level3Button.interactable = LevelProgress.IsLevelUnlocked(3);
    }
}
