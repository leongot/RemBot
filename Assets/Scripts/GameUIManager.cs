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
        Instance = this;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    public void MostraGameOver()
    {
        if (partitaFinita)
            return;

        partitaFinita = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("[GameUIManager] Game Over.");
    }

    public void MostraLevelComplete()
    {
        if (partitaFinita)
            return;

        partitaFinita = true;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        Debug.Log("[GameUIManager] Livello completato.");
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