using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuLevelLoader : MonoBehaviour
{
    public static MenuLevelLoader Instance;

    [Header("Schermata Caricamento")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image loadingBarFill;
    [SerializeField] private TMP_Text loadingText;

    [Header("Impostazioni")]
    [SerializeField] private float durataMinimaCaricamento = 4f;

    [Header("Audio Caricamento")]
    [SerializeField] private AudioSource loadingAudioSource;
    [SerializeField] private AudioClip loadingSound;
    [SerializeField] private float loadingVolume = 1f;

    private bool caricamentoInCorso = false;

    private void Awake()
    {
        Instance = this;

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 0f;

        if (loadingText != null)
            loadingText.text = "0%";
    }

    public void CaricaLivelloConLoading(string nomeScena)
    {
        if (caricamentoInCorso)
            return;

        if (string.IsNullOrEmpty(nomeScena))
        {
            Debug.LogError("[MenuLevelLoader] Nome scena non assegnato.");
            return;
        }

        StartCoroutine(CaricamentoRealeScena(nomeScena));
    }

    private IEnumerator CaricamentoRealeScena(string nomeScena)
    {
        caricamentoInCorso = true;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 0f;

        if (loadingText != null)
            loadingText.text = "0%";

        PlayLoadingSound();

        AsyncOperation operazione = SceneManager.LoadSceneAsync(nomeScena);

        if (operazione == null)
        {
            Debug.LogError("[MenuLevelLoader] Impossibile caricare la scena: " + nomeScena);
            yield break;
        }

        // Blocca l'ingresso immediato nella scena.
        // Così puoi mostrare il 100% prima del cambio scena.
        operazione.allowSceneActivation = false;

        float tempoPassato = 0f;

        while (!operazione.isDone)
        {
            tempoPassato += Time.deltaTime;

            // Unity arriva massimo a 0.9 finché allowSceneActivation è false.
            float progressoReale = Mathf.Clamp01(operazione.progress / 0.9f);

            // Serve per evitare che scene leggere passino da 0 a 100 istantaneamente.
            float progressoTempo = Mathf.Clamp01(tempoPassato / durataMinimaCaricamento);

            // La barra segue il caricamento reale, ma rispetta anche una durata minima visiva.
            float progressoFinale = Mathf.Min(progressoReale, progressoTempo);

            if (loadingBarFill != null)
                loadingBarFill.fillAmount = progressoFinale;

            if (loadingText != null)
                loadingText.text = Mathf.RoundToInt(progressoFinale * 100f) + "%";

            // Quando la scena è caricata e la durata minima è finita,
            // posso mostrare 100% e attivare la scena.
            if (operazione.progress >= 0.9f && tempoPassato >= durataMinimaCaricamento)
            {
                if (loadingBarFill != null)
                    loadingBarFill.fillAmount = 1f;

                if (loadingText != null)
                    loadingText.text = "100%";

                yield return new WaitForSeconds(0.3f);

                StopLoadingSound();

                operazione.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private void PlayLoadingSound()
    {
        if (loadingAudioSource == null)
            return;

        if (loadingSound == null)
            return;

        loadingAudioSource.clip = loadingSound;
        loadingAudioSource.volume = loadingVolume;
        loadingAudioSource.loop = false;
        loadingAudioSource.Play();
    }

    private void StopLoadingSound()
    {
        if (loadingAudioSource == null)
            return;

        loadingAudioSource.Stop();
        loadingAudioSource.loop = false;
    }
}