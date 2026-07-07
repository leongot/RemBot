using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsecutoreProgramma : MonoBehaviour
{
    public static EsecutoreProgramma Instance;

    public RobotController robot;

    private bool programmaInEsecuzione = false;

    public bool ProgrammaInEsecuzione
    {
        get { return programmaInEsecuzione; }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void AvviaProgramma()
    {
        if (programmaInEsecuzione)
            return;

        if (robot == null)
        {
            Debug.LogError("[Esecutore] RobotController non assegnato.");
            return;
        }

        if (GestoreBlocchi.Instance == null || GestoreBlocchi.Instance.areaPrincipaleCodice == null)
        {
            Debug.LogError("[Esecutore] Area principale codice non trovata.");
            return;
        }

        programmaInEsecuzione = true;

        Debug.Log("[Esecutore] Compilazione ed esecuzione avviata...");
        StartCoroutine(EseguiInsiemeBlocchi(GestoreBlocchi.Instance.areaPrincipaleCodice));
    }

    public void FermaProgramma()
    {
        StopAllCoroutines();
        programmaInEsecuzione = false;

<<<<<<< HEAD
<<<<<<< HEAD
        if (robot != null) robot.RipristinaPosizione();

    }

    // Perche' una corotine? Tanto gestisce l'esecuzione dell'interno programma dell'utente.
    // Non e' meglio eseguirlo all'interno di Update
=======
        if (robot != null)
            robot.RipristinaPosizione();
    }

>>>>>>> parent of ec14606 (Merge branch 'alt')
=======
        if (robot != null)
            robot.RipristinaPosizione();
    }

>>>>>>> parent of ec14606 (Merge branch 'alt')
    private IEnumerator EseguiInsiemeBlocchi(Transform contenitore)
    {
        if (contenitore == null)
            yield break;

        foreach (Transform blocco in contenitore)
        {
            InfoBlocco info = blocco.GetComponent<InfoBlocco>();

            if (info == null)
                continue;

            switch (info.tipo)
            {
                case TipoBlocco.Avanti:
                    yield return robot.MuoviAvanti();
                    break;

                case TipoBlocco.Indietro:
                    yield return robot.MuoviIndietro();
                    break;

                case TipoBlocco.Sinistra:
                    yield return robot.RuotaSinistra();
                    break;

                case TipoBlocco.Destra:
                    yield return robot.RuotaDestra();
                    break;

                case TipoBlocco.Spara:
                    yield return robot.Spara();
                    break;

                case TipoBlocco.If:
                {
                    Transform slotCondIf = TrovaSlotCondizioneDiretto(blocco);
                    List<SlotComandi> slotComandiIf = TrovaSlotComandiDiretti(blocco);

                    if (slotCondIf == null)
                    {
                        Debug.LogWarning("[Esecutore] IF senza condizione.");
                        break;
                    }

                    if (slotComandiIf.Count == 0)
                    {
                        Debug.LogWarning("[Esecutore] IF senza pancia THEN.");
                        break;
                    }

                    Transform slotThen = slotComandiIf[0].transform;
                    Transform slotElse = null;

                    if (slotComandiIf.Count >= 2)
                        slotElse = slotComandiIf[1].transform;

                    bool condizioneVera = ValutaCondizioneDelloSlot(slotCondIf);

                    if (condizioneVera)
                    {
                        Debug.Log("[Esecutore] IF vero: eseguo THEN.");
                        yield return EseguiInsiemeBlocchi(slotThen);
                    }
                    else
                    {
                        Debug.Log("[Esecutore] IF falso: eseguo ELSE.");

                        if (slotElse != null)
                            yield return EseguiInsiemeBlocchi(slotElse);
                    }

                    break;
                }

                case TipoBlocco.While:
                {
                    Transform slotCondWhile = TrovaSlotCondizioneDiretto(blocco);
                    List<SlotComandi> slotComandiWhile = TrovaSlotComandiDiretti(blocco);

                    if (slotCondWhile == null)
                    {
                        Debug.LogWarning("[Esecutore] WHILE senza condizione.");
                        break;
                    }

                    if (slotComandiWhile.Count == 0)
                    {
                        Debug.LogWarning("[Esecutore] WHILE senza pancia comandi.");
                        break;
                    }

                    Transform slotPanciaWhile = slotComandiWhile[0].transform;

                    int limiteSicurezza = 0;

                    while (ValutaCondizioneDelloSlot(slotCondWhile) && limiteSicurezza < 100000)
                    {
                        yield return EseguiInsiemeBlocchi(slotPanciaWhile);

                        limiteSicurezza++;

                        // Evita che Unity si blocchi se dentro al WHILE non c'è nessun comando che aspetta tempo.
                        yield return null;
                    }

                    if (limiteSicurezza >= 100000)
                    {
                        Debug.LogError("[Esecutore] Loop infinito rilevato nel WHILE!");
                    }

                    break;
                }
            }
        }

        if (contenitore == GestoreBlocchi.Instance.areaPrincipaleCodice)
        {
            programmaInEsecuzione = false;
            Debug.Log("[Esecutore] Programma terminato.");

            if (GestoreBlocchi.Instance != null)
                GestoreBlocchi.Instance.ProgrammaTerminato();
        }
    }

    private bool ValutaCondizioneDelloSlot(Transform slotCondizione)
    {
        if (slotCondizione == null)
            return false;

        if (slotCondizione.childCount == 0)
            return false;

        InfoBlocco infoCondizione = slotCondizione.GetChild(0).GetComponent<InfoBlocco>();

        if (infoCondizione == null)
            return false;

        return robot.VerificaCondizione(infoCondizione.tipo);
    }

    private Transform TrovaSlotCondizioneDiretto(Transform blocco)
    {
        SlotCondizione[] tuttiGliSlot = blocco.GetComponentsInChildren<SlotCondizione>(true);

        foreach (SlotCondizione slot in tuttiGliSlot)
        {
            if (AppartieneAlBloccoCorrente(slot.transform, blocco))
                return slot.transform;
        }

        return null;
    }

    private List<SlotComandi> TrovaSlotComandiDiretti(Transform blocco)
    {
        SlotComandi[] tuttiGliSlot = blocco.GetComponentsInChildren<SlotComandi>(true);
        List<SlotComandi> slotDiretti = new List<SlotComandi>();

        foreach (SlotComandi slot in tuttiGliSlot)
        {
            if (AppartieneAlBloccoCorrente(slot.transform, blocco))
                slotDiretti.Add(slot);
        }

        return slotDiretti;
    }

    private bool AppartieneAlBloccoCorrente(Transform elemento, Transform bloccoCorrente)
    {
        Transform parent = elemento.parent;

        while (parent != null && parent != bloccoCorrente)
        {
            // Se salendo trovo un altro InfoBlocco,
            // vuol dire che questo slot appartiene a un blocco annidato,
            // non al blocco corrente.
            if (parent.GetComponent<InfoBlocco>() != null)
                return false;

            parent = parent.parent;
        }

        return parent == bloccoCorrente;
    }
}
