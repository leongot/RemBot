using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Scene")]
    [SerializeField] private string nomeScenaMenu = "MainMenu";
    [SerializeField] private string nomeScenaProssimoLivello = "";

    private bool partitaFinita = false;

    public bool PartitaFinita
    {
        get { return partitaFinita; }
    }

    private void Awake()
    {
        Instance = this;

        // Importantissimo: quando carichi o ricarichi una scena,
        // il tempo deve tornare normale.
        Time.timeScale = 1f;

        Debug.Log("[GameUIManager] Awake eseguito su: " + gameObject.name);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        else
            Debug.LogError("[GameUIManager] Game Over Panel NON assegnato.");

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        else
            Debug.LogWarning("[GameUIManager] Level Complete Panel non assegnato.");
    }

    public void MostraGameOver()
    {
        if (partitaFinita)
            return;

        partitaFinita = true;

        Debug.Log("[GameUIManager] Mostro Game Over e blocco il gioco.");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            Debug.LogError("[GameUIManager] Game Over Panel non assegnato.");

        // Blocca il gioco sotto al pannello.
        Time.timeScale = 0f;
    }

    public void MostraLevelComplete()
    {
        if (partitaFinita)
            return;

        partitaFinita = true;

        Debug.Log("[GameUIManager] Mostro Level Complete e blocco il gioco.");

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);
        else
            Debug.LogError("[GameUIManager] Level Complete Panel non assegnato.");

        // Blocca il gioco sotto al pannello.
        Time.timeScale = 0f;
    }

    public void RiprovaLivello()
    {
        Debug.Log("[GameUIManager] Ricarico il livello corrente.");

        // Prima di cambiare scena, riattiva il tempo.
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TornaAlMenu()
    {
        if (string.IsNullOrEmpty(nomeScenaMenu))
        {
            Debug.LogError("[GameUIManager] Nome scena menu non assegnato.");
            return;
        }

        Debug.Log("[GameUIManager] Torno al menu: " + nomeScenaMenu);

        // Prima di cambiare scena, riattiva il tempo.
        Time.timeScale = 1f;

        SceneManager.LoadScene(nomeScenaMenu);
    }

    public void ProssimoLivello()
    {
        if (string.IsNullOrEmpty(nomeScenaProssimoLivello))
        {
            Debug.LogWarning("[GameUIManager] Nome scena prossimo livello non assegnato.");
            return;
        }

        Debug.Log("[GameUIManager] Carico prossimo livello: " + nomeScenaProssimoLivello);

        // Prima di cambiare scena, riattiva il tempo.
        Time.timeScale = 1f;

        SceneManager.LoadScene(nomeScenaProssimoLivello);
    }

    public void EsciDalGioco()
    {
        Debug.Log("[GameUIManager] Esco dal gioco.");

        Time.timeScale = 1f;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}