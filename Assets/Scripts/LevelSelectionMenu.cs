using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelectionMenu : MonoBehaviour
{
    [Header("Bottoni Livelli")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;

    [Header("Testi Livelli")]
    [SerializeField] private TMP_Text level1Text;
    [SerializeField] private TMP_Text level2Text;
    [SerializeField] private TMP_Text level3Text;

    [Header("Record Text")]
    [SerializeField] private TMP_Text level1RecordText;
    [SerializeField] private TMP_Text level2RecordText;
    [SerializeField] private TMP_Text level3RecordText;

    [Header("Lock Panel Livelli")]
    [SerializeField] private GameObject level1LockPanel;
    [SerializeField] private GameObject level2LockPanel;
    [SerializeField] private GameObject level3LockPanel;

    public void Initialize()
    {
        UpdateLevelButtons();
    }

    private void Start()
    {
        UpdateLevelButtons();
    }

    public void StartLevel(int levelNumber)
    {
        if (!LevelSaveSystem.IsLevelUnlocked(levelNumber))
        {
            Debug.Log("[LevelSelectionMenu] Livello " + levelNumber + " bloccato.");
            return;
        }

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

        Debug.Log("[LevelSelectionMenu] Progressi resettati. Record e lock panel aggiornati.");
    }

    public void UpdateLevelButtons()
    {
        SetLevelState(level1Button, level1Text, level1RecordText, level1LockPanel, 1);
        SetLevelState(level2Button, level2Text, level2RecordText, level2LockPanel, 2);
        SetLevelState(level3Button, level3Text, level3RecordText, level3LockPanel, 3);
    }

    private void SetLevelState(
        Button button,
        TMP_Text levelText,
        TMP_Text recordText,
        GameObject lockPanel,
        int levelNumber)
    {
        bool unlocked = LevelSaveSystem.IsLevelUnlocked(levelNumber);

      
        if (levelNumber == 1)
            unlocked = true;

        if (button != null)
        {
            button.gameObject.SetActive(true);
            button.interactable = unlocked;

            Image buttonImage = button.GetComponent<Image>();

            if (buttonImage != null)
            {
                buttonImage.enabled = unlocked;
            }
        }

        if (levelText != null)
        {
            if (unlocked)
                levelText.text = "LEVEL " + levelNumber;
            else
                levelText.text = "LEVEL " + levelNumber + " LOCKED";
        }

        if (recordText != null)
        {
            int record = LevelSaveSystem.GetRecordScore(levelNumber);
            recordText.text = "RECORD: " + record;
        }

        if (lockPanel != null)
        {
            lockPanel.SetActive(!unlocked);
        }

        Debug.Log(
            "[LevelSelectionMenu] Level " + levelNumber +
            " | unlocked: " + unlocked +
            " | record: " + LevelSaveSystem.GetRecordScore(levelNumber) +
            " | lockPanel: " + (lockPanel != null ? lockPanel.name : "NULL")
        );
    }
}