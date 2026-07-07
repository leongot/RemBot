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
    [SerializeField] private float durataCaricamento = 4f;

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
            Debug.LogError("[MenuLevelLoader] Nome scena vuoto.");
            return;
        }

        StartCoroutine(CaricamentoMenu(nomeScena));
    }

    private IEnumerator CaricamentoMenu(string nomeScena)
    {
        caricamentoInCorso = true;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 0f;

        if (loadingText != null)
            loadingText.text = "0%";

        PlayLoadingSound();

        float tempo = 0f;

        while (tempo < durataCaricamento)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / durataCaricamento);
            int percentuale = Mathf.RoundToInt(progresso * 100f);

            if (loadingBarFill != null)
                loadingBarFill.fillAmount = progresso;

            if (loadingText != null)
                loadingText.text = percentuale + "%";

            yield return null;
        }

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 1f;

        if (loadingText != null)
            loadingText.text = "100%";

        yield return new WaitForSeconds(0.2f);

        StopLoadingSound();

        SceneManager.LoadScene(nomeScena);
    }

    private void PlayLoadingSound()
    {
        if (loadingAudioSource == null)
            return;

        if (loadingSound == null)
            return;

        loadingAudioSource.clip = loadingSound;
        loadingAudioSource.volume = loadingVolume;
        loadingAudioSource.loop = true;
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