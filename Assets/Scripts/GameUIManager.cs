using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;

    private bool partitaFinita = false;

    public bool PartitaFinita
    {
        get { return partitaFinita; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameUIManager] Esiste già un'altra Instance. Sostituisco con questa.");
        }

        Instance = this;

        Debug.Log("[GameUIManager] Awake eseguito su: " + gameObject.name + ". Instance impostata.");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[GameUIManager] Game Over Panel NON assegnato nell'Inspector.");
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[GameUIManager] Level Complete Panel non assegnato. Va bene se non lo usi ancora.");
        }
    }

    public void MostraGameOver()
    {
        if (partitaFinita)
        {
            Debug.LogWarning("[GameUIManager] MostraGameOver chiamato, ma partitaFinita è già true.");
            return;
        }

        partitaFinita = true;

        Debug.Log("[GameUIManager] MostraGameOver chiamato.");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            Debug.Log(
                "[GameUIManager] Game Over Panel attivato. " +
                "activeSelf: " + gameOverPanel.activeSelf +
                " | activeInHierarchy: " + gameOverPanel.activeInHierarchy
            );

            if (!gameOverPanel.activeInHierarchy)
            {
                Debug.LogError(
                    "[GameUIManager] Il Game Over Panel è stato attivato, " +
                    "ma non è visibile in gerarchia. Probabilmente ha un parent disattivato, " +
                    "per esempio Canvas o un pannello padre."
                );
            }
        }
        else
        {
            Debug.LogError("[GameUIManager] Impossibile mostrare Game Over: Game Over Panel non assegnato.");
        }
    }

    public void MostraLevelComplete()
    {
        if (partitaFinita)
        {
            Debug.LogWarning("[GameUIManager] MostraLevelComplete chiamato, ma partitaFinita è già true.");
            return;
        }

        partitaFinita = true;

        Debug.Log("[GameUIManager] MostraLevelComplete chiamato.");

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            if (!levelCompletePanel.activeInHierarchy)
            {
                Debug.LogError(
                    "[GameUIManager] Il Level Complete Panel è stato attivato, " +
                    "ma non è visibile in gerarchia. Probabilmente ha un parent disattivato."
                );
            }
        }
        else
        {
            Debug.LogError("[GameUIManager] Impossibile mostrare Level Complete: panel non assegnato.");
        }
    }

    public void RiprovaLivello()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TornaAlMenu(string nomeScenaMenu)
    {
        SceneManager.LoadScene(nomeScenaMenu);
    }

    public void ProssimoLivello(string nomeScenaProssimoLivello)
    {
        SceneManager.LoadScene(nomeScenaProssimoLivello);
    }
}
