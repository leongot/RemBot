using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GestoreBlocchi : MonoBehaviour
{
    public static GestoreBlocchi Instance;

    [Header("PREFAB COMANDI (Azzurri)")]
    public GameObject prefabAvanti;
    public GameObject prefabSinistra;
    public GameObject prefabDestra;
    public GameObject prefabIndietro;
    public GameObject prefabSpara;

    [Header("PREFAB CONTROLLO")]
    public GameObject prefabWhile;
    public GameObject prefabIf;

    [Header("PREFAB CONDIZIONI (Gialle)")]
    public GameObject prefabLibero;
    public GameObject prefabMuro;
    public GameObject prefabNemico;

    [Header("Riferimenti Aree Principali")]
    public Transform areaPrincipaleCodice;

    [HideInInspector] public Transform slotAttivoComandi;
    [HideInInspector] public Transform slotAttivoCondizione;

    [Header("Schermata Caricamento")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image loadingBarFill;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private float durataCaricamento = 4f;

    [Header("Audio Caricamento")]
    [SerializeField] private AudioSource loadingAudioSource;
    [SerializeField] private AudioClip loadingSound;
    [SerializeField] private float loadingVolume = 1f;

    [Header("Camera Start")]
    [SerializeField] private CameraSwitcher cameraSwitcher;
    [SerializeField] private float attesaDopoSwitchCamera = 2f;

    [Header("Score")]
    [SerializeField] private TMP_Text recordScoreText;
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private int scoreMassimo = 1000;

    [Header("Livello")]
    [SerializeField] private int livelloCorrente = 1;
    [SerializeField] private int numeroTotaleLivelli = 3;

    [Header("Audio Pulsanti")]
    [SerializeField] private AudioSource uiAudioSource;

    [SerializeField] private AudioClip suonoComando;
    [SerializeField] private AudioClip suonoControllo;
    [SerializeField] private AudioClip suonoCondizione;
    [SerializeField] private AudioClip suonoStart;
    [SerializeField] private AudioClip suonoReset;
    [SerializeField] private AudioClip suonoSlotCondizione;
    [SerializeField] private AudioClip suonoSlotComando;

    [SerializeField] private float volumePulsanti = 1f;

    [Header("Ritardo Start")]
    [SerializeField] private float ritardoDopoClickStart = 0.5f;

    private int numeroIstruzioniInserite = 0;
    private int scoreAttuale = 0;
    private int recordScore = 0;
    private bool livelloCompletato = false;

    private const string RECORD_SCORE_KEY = "RECORD_SCORE";

    private bool caricamentoInCorso = false;
    private bool programmaBloccato = false;

    public bool ProgrammaBloccato
    {
        get { return programmaBloccato; }
    }

    public int NumeroIstruzioniInserite
    {
        get { return numeroIstruzioniInserite; }
    }

    private void Awake()
    {
        Instance = this;
        slotAttivoComandi = areaPrincipaleCodice;

        recordScore = LevelSaveSystem.GetRecordScore(livelloCorrente);
        scoreAttuale = 0;
        
        AggiornaScoreUI();
        numeroIstruzioniInserite = 0;

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 0f;

        if (loadingText != null)
            loadingText.text = "0%";

        if (uiAudioSource == null)
            uiAudioSource = GetComponent<AudioSource>();

        AggiornaScoreUI();
    }

    public void ImpostaSlotComandi(Transform nuovoSlot)
    {
        if (programmaBloccato)
        {
            Debug.Log("[GestoreBlocchi] Programma bloccato: non puoi più selezionare slot.");
            return;
        }

        PlayButtonSound(suonoSlotComando);

        slotAttivoComandi = nuovoSlot;
        Debug.Log("[GestoreBlocchi] Slot comandi attivo: " + nuovoSlot.name);
    }

    public void ImpostaSlotCondizione(Transform nuovoSlot)
    {
        if (programmaBloccato)
        {
            Debug.Log("[GestoreBlocchi] Programma bloccato: non puoi più cambiare condizioni.");
            return;
        }

        PlayButtonSound(suonoSlotCondizione);

        slotAttivoCondizione = nuovoSlot;
        Debug.Log("[GestoreBlocchi] Slot condizione attivo: " + nuovoSlot.name);
    }

    public void InstanziaComando(GameObject prefab)
    {
        if (programmaBloccato)
        {
            Debug.Log("[GestoreBlocchi] Programma già avviato: comando non aggiunto.");
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning("[GestoreBlocchi] Prefab comando non assegnato.");
            return;
        }

        if (slotAttivoComandi == null)
            slotAttivoComandi = areaPrincipaleCodice;

        GameObject nuovoComando = Instantiate(prefab, slotAttivoComandi);
        nuovoComando.transform.localScale = Vector3.one;

    
        numeroIstruzioniInserite++;
        scoreAttuale = 0;

        
        slotAttivoComandi = areaPrincipaleCodice;
        slotAttivoCondizione = null;

        AggiornaScoreUI();
    }

    public void InstanziaCondizione(GameObject prefab)
    {
        if (programmaBloccato)
        {
            Debug.Log("[GestoreBlocchi] Programma già avviato: condizione non aggiunta.");
            return;
        }

        Debug.Log("[GestoreBlocchi] InstanziaCondizione chiamato. slotAttivoCondizione: "
            + (slotAttivoCondizione != null ? slotAttivoCondizione.name : "NULL"));

        if (prefab == null)
        {
            Debug.LogWarning("[GestoreBlocchi] Prefab condizione non assegnato.");
            return;
        }

        if (slotAttivoCondizione != null)
        {
            foreach (Transform figlio in slotAttivoCondizione)
                Destroy(figlio.gameObject);

            GameObject nuovaCondizione = Instantiate(prefab, slotAttivoCondizione);
            nuovaCondizione.transform.localScale = Vector3.one;

            slotAttivoCondizione = null;

            if (PannelloCondizioni.Instance != null)
                PannelloCondizioni.Instance.Nascondi();
        }
        else
        {
            Debug.LogWarning("[GestoreBlocchi] slotAttivoCondizione è NULL! Clicca prima su una casella gialla.");
        }

        // Le condizioni non aumentano numeroIstruzioniInserite.
        AggiornaScoreUI();
    }

    private void PlayButtonSound(AudioClip clip)
    {
        if (uiAudioSource == null)
            return;

        if (clip == null)
            return;

        uiAudioSource.PlayOneShot(clip, volumePulsanti);
    }

    public void TastoStart()
    {
        PlayButtonSound(suonoStart);

        if (programmaBloccato || caricamentoInCorso)
        {
            Debug.Log("[GestoreBlocchi] START già premuto.");
            return;
        }

        
        scoreAttuale = 0;
        AggiornaScoreUI();

        StartCoroutine(StartConCaricamento());
    }

    public void TastoResetProgramma()
    {
        PlayButtonSound(suonoReset);

        if (programmaBloccato)
        {
            Debug.Log("[GestoreBlocchi] Non puoi resettare solo il programma dopo START. Usa Reset Level.");
            return;
        }

        foreach (Transform blocco in areaPrincipaleCodice)
        {
            Destroy(blocco.gameObject);
        }

        slotAttivoComandi = areaPrincipaleCodice;
        slotAttivoCondizione = null;

        numeroIstruzioniInserite = 0;
        scoreAttuale = 0;

        if (PannelloCondizioni.Instance != null)
            PannelloCondizioni.Instance.Nascondi();

        AggiornaScoreUI();

        cameraSwitcher.Reset();

        Debug.Log("[GestoreBlocchi] Area codice resettata.");
    }

    public void TastoResetScena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator StartConCaricamento()
    {
        programmaBloccato = true;
        caricamentoInCorso = true;

        slotAttivoComandi = null;
        slotAttivoCondizione = null;

        if (PannelloCondizioni.Instance != null)
            PannelloCondizioni.Instance.Nascondi();
            
        yield return new WaitForSecondsRealtime(ritardoDopoClickStart);

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

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (cameraSwitcher != null)
        {
            cameraSwitcher.NewGameStart();
        }
        else
        {
            Debug.LogWarning("[GestoreBlocchi] CameraSwitcher non assegnato.");
        }

        yield return new WaitForSeconds(attesaDopoSwitchCamera);

        caricamentoInCorso = false;

        if (EsecutoreProgramma.Instance != null)
        {
            EsecutoreProgramma.Instance.AvviaProgramma();
        }
        else
        {
            Debug.LogError("[GestoreBlocchi] EsecutoreProgramma.Instance non trovato.");
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

    public int CalcolaScore()
    {
        int score = scoreMassimo - numeroIstruzioniInserite;

        if (score < 0)
            score = 0;

        return score;
    }

    public void ProgrammaTerminato()
    {
        scoreAttuale = CalcolaScore();

        AggiornaScoreUI();

        Debug.Log(
            "[GestoreBlocchi] Programma terminato. " +
            "Score calcolato: " + scoreAttuale +
            " | Il livello NON è ancora completato finché non raggiungi il goal."
        );
    }

    private void AggiornaScoreUI()
    {
        if (recordScoreText != null)
        {
            recordScoreText.text = "RECORD SCORE: " + recordScore;
        }

        if (currentScoreText != null)
        {
            currentScoreText.text = "SCORE: " + scoreAttuale;
        }

        Debug.Log(
            "[Score] Istruzioni: " + numeroIstruzioniInserite +
            " | Score attuale: " + scoreAttuale +
            " | Record: " + recordScore
        );
    }

    public void CompletaLivelloDaGoal()
    {
        if (livelloCompletato)
            return;

        livelloCompletato = true;

        scoreAttuale = CalcolaScore();

        LevelSaveSystem.SaveRecordScore(livelloCorrente, scoreAttuale);

        LevelSaveSystem.MarkLevelCompleted(livelloCorrente);

        recordScore = LevelSaveSystem.GetRecordScore(livelloCorrente);

        AggiornaScoreUI();

        Debug.Log(
            "[GestoreBlocchi] GOAL raggiunto. Livello completato: " + livelloCorrente +
            " | Score: " + scoreAttuale +
            " | Record: " + recordScore
        );
    }

    public void BottoneAvanti()
    {
        PlayButtonSound(suonoComando);
        InstanziaComando(prefabAvanti);
    }

    public void BottoneSinistra()
    {
        PlayButtonSound(suonoComando);
        InstanziaComando(prefabSinistra);
    }

    public void BottoneDestra()
    {
        PlayButtonSound(suonoComando);
        InstanziaComando(prefabDestra);
    }

    public void BottoneIndietro()
    {
        PlayButtonSound(suonoComando);
        InstanziaComando(prefabIndietro);
    }

    public void BottoneSpara()
    {
        PlayButtonSound(suonoComando);
        InstanziaComando(prefabSpara);
    }

    public void BottoneWhile()
    {
        PlayButtonSound(suonoControllo);
        InstanziaComando(prefabWhile);
    }

    public void BottoneIf()
    {
        PlayButtonSound(suonoControllo);
        InstanziaComando(prefabIf);
    }

    public void BottoneLibero()
    {
        PlayButtonSound(suonoCondizione);
        InstanziaCondizione(prefabLibero);
    }

    public void BottoneMuro()
    {
        PlayButtonSound(suonoCondizione);
        InstanziaCondizione(prefabMuro);
    }

    public void BottoneNemico()
    {
        PlayButtonSound(suonoCondizione);
        InstanziaCondizione(prefabNemico);
    }
}
