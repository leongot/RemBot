using System.Collections;
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

    [Header("Audio Fine Partita")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private float volumeAudioFinePartita = 1f;

    [Header("Ritardi UI")]
    [SerializeField] private float ritardoSchermataVittoria = 1f;

    private bool partitaFinita = false;

    public bool PartitaFinita
    {
        get { return partitaFinita; }
    }

    private void Awake()
    {
        Instance = this;

        Time.timeScale = 1f;

        if (uiAudioSource == null)
            uiAudioSource = GetComponent<AudioSource>();

        if (uiAudioSource == null)
            uiAudioSource = gameObject.AddComponent<AudioSource>();

        uiAudioSource.playOnAwake = false;

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

        PlayAudioFinePartita(gameOverSound);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            Debug.LogError("[GameUIManager] Game Over Panel non assegnato.");

        Time.timeScale = 0f;
    }

    public void MostraLevelComplete()
    {
        if (partitaFinita)
            return;

        partitaFinita = true;

        Debug.Log("[GameUIManager] Vittoria raggiunta. Avvio suono e mostro pannello tra " + ritardoSchermataVittoria + " secondo/i.");

        PlayAudioFinePartita(levelCompleteSound);

    
        StartCoroutine(MostraLevelCompleteDopoRitardo());

    }
        private void PlayAudioFinePartita(AudioClip clip)
        {
            if (uiAudioSource == null)
                return;

            if (clip == null)
                return;

            uiAudioSource.PlayOneShot(clip, volumeAudioFinePartita);
    }

    private IEnumerator MostraLevelCompleteDopoRitardo()
    {
        yield return new WaitForSecondsRealtime(ritardoSchermataVittoria);

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            Debug.Log("[GameUIManager] Level Complete Panel mostrato dopo il ritardo.");
        }
        else
        {
            Debug.LogError("[GameUIManager] Impossibile mostrare Level Complete: panel non assegnato.");
        }

        Time.timeScale = 0f;
    }

    public void RiprovaLivello()
    {
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

        Time.timeScale = 1f;
        SceneManager.LoadScene(nomeScenaProssimoLivello);
    }

    public void EsciDalGioco()
    {
        Time.timeScale = 1f;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}