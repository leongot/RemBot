using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelButtonUI : MonoBehaviour
{
    [Header("Livello")]
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private string sceneName;

    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text recordText;
    [SerializeField] private GameObject lockedIcon;

    [Header("Colori")]
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.25f, 0.25f, 0.25f, 1f);

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
    }

    private void Start()
    {
        AggiornaUI();
    }

    public void AggiornaUI()
    {
        bool unlocked = LevelSaveSystem.IsLevelUnlocked(levelNumber);
        int record = LevelSaveSystem.GetRecordScore(levelNumber);

        if (button != null)
            button.interactable = unlocked;

        if (buttonImage != null)
            buttonImage.color = unlocked ? unlockedColor : lockedColor;

        if (levelText != null)
        {
            if (unlocked)
                levelText.text = "LEVEL " + levelNumber;
            else
                levelText.text = "LEVEL " + levelNumber + " LOCKED";
        }

        if (recordText != null)
        {
            if (unlocked)
                recordText.text = "RECORD: " + record;
            else
                recordText.text = "LOCKED";
        }

        if (lockedIcon != null)
            lockedIcon.SetActive(!unlocked);
    }

    public void ApriLivello()
    {
        if (!LevelSaveSystem.IsLevelUnlocked(levelNumber))
        {
            Debug.Log("[LevelButtonUI] Livello bloccato: " + levelNumber);
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[LevelButtonUI] Scene name non assegnato per livello " + levelNumber);
            return;
        }

        if (MenuLevelLoader.Instance != null)
            MenuLevelLoader.Instance.CaricaLivelloConLoading(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}