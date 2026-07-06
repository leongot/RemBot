using UnityEngine;

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

    Transform slotAttivoComandi;
    Transform slotAttivoCondizione;

    private void Awake()
    {
        Instance = this;
        slotAttivoComandi = areaPrincipaleCodice;
    }

    public void ImpostaSlotComandi(Transform nuovoSlot)
    {
        slotAttivoComandi = nuovoSlot;
        // NON azzeriamo più slotAttivoCondizione qui:
        // il click sulla casella gialla potrebbe propagarsi alla pancia
        // e azzerarlo subito dopo averlo impostato.
        Debug.Log("[GestoreBlocchi] Slot comandi attivo: " + nuovoSlot.name);
    }

    public void ImpostaSlotCondizione(Transform nuovoSlot)
    {
        slotAttivoCondizione = nuovoSlot;
        Debug.Log("[GestoreBlocchi] Slot condizione attivo: " + nuovoSlot.name);
    }

    public void InstanziaComando(GameObject prefab)
    {
        if (slotAttivoComandi == null)
            slotAttivoComandi = areaPrincipaleCodice;

        GameObject nuovoComando = Instantiate(prefab, slotAttivoComandi);
        nuovoComando.transform.localScale = Vector3.one;

        // Torna all'area principale e deseleziona la condizione
        slotAttivoComandi = areaPrincipaleCodice;
        slotAttivoCondizione = null;
    }

    public void InstanziaCondizione(GameObject prefab)
    {
        Debug.Log("[GestoreBlocchi] InstanziaCondizione chiamato. slotAttivoCondizione: "
            + (slotAttivoCondizione != null ? slotAttivoCondizione.name : "NULL"));

        if (slotAttivoCondizione != null)
        {
            foreach (Transform figlio in slotAttivoCondizione)
                Destroy(figlio.gameObject);

            GameObject nuovaCondizione = Instantiate(prefab, slotAttivoCondizione);
            nuovaCondizione.transform.localScale = Vector3.one;

            slotAttivoCondizione = null;
            PannelloCondizioni.Instance.Nascondi();
        }
        else
        {
            Debug.LogWarning("[GestoreBlocchi] slotAttivoCondizione è NULL! Clicca prima su una casella gialla.");
        }
    }

    // Aggiungi queste funzioni in fondo alla classe GestoreBlocchi.cs

public void TastoStart()
{
    // Avvia l'interprete
    if (EsecutoreProgramma.Instance != null)
    {
        EsecutoreProgramma.Instance.AvviaProgramma();
    }
}

public void TastoReset()
{
    // 1. Ferma l'esecuzione e resetta il robot
    if (EsecutoreProgramma.Instance != null)
    {
        EsecutoreProgramma.Instance.FermaProgramma();
    }

    // 2. Svuota l'area del codice distruggendo i blocchi inseriti
    foreach (Transform blocco in areaPrincipaleCodice)
    {
        Destroy(blocco.gameObject);
    }

    // 3. Ripristina gli slot attivi di default
    slotAttivoComandi = areaPrincipaleCodice; 
    slotAttivoCondizione = null;
    
    Debug.Log("[GestoreBlocchi] Area codice resettata.");
}

    public void BottoneAvanti()   => InstanziaComando(prefabAvanti);
    public void BottoneSinistra() => InstanziaComando(prefabSinistra);
    public void BottoneDestra()   => InstanziaComando(prefabDestra);
    public void BottoneIndietro() => InstanziaComando(prefabIndietro);
    public void BottoneSpara()    => InstanziaComando(prefabSpara);
    public void BottoneWhile()    => InstanziaComando(prefabWhile);
    public void BottoneIf()       => InstanziaComando(prefabIf);

    public void BottoneLibero()  => InstanziaCondizione(prefabLibero);
    public void BottoneMuro()    => InstanziaCondizione(prefabMuro);
    public void BottoneNemico()  => InstanziaCondizione(prefabNemico);
}
