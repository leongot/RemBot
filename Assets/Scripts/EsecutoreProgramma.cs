using System;
using System.Collections;
using UnityEngine;

public class EsecutoreProgramma : MonoBehaviour
{
    public static EsecutoreProgramma Instance;

    public RobotController robot;
    private bool programmaInEsecuzione = false;

    [SerializeField]
    GameObject topDownCamera;

    [SerializeField]
    GameObject followCamera;
    
    private void Awake()
    {  
        Instance = this;
        StartNewGame();
    }

    private void StartNewGame()
    {
        topDownCamera.SetActive(true);
        //robot.StartNewGame(spawnPoints.position);
    }
    public void AvviaProgramma()
    {
        if (programmaInEsecuzione) return;
        
        followCamera.SetActive(true);

        Debug.Log("[Esecutore] Compilazione ed esecuzione avviata...");
        StartCoroutine(EseguiInsiemeBlocchi(GestoreBlocchi.Instance.areaPrincipaleCodice));
    }

    public void FermaProgramma()
    {
        StopAllCoroutines();
        programmaInEsecuzione = false;
        if (robot != null) robot.FullReset();
    }

    private bool ValutaCondizioneDelloSlot(Transform slotCondizione)
    {
        // Se lo slot non ha figli, l'utente non ha messo nessuna condizione gialla
        if (slotCondizione.childCount == 0) return false;

        InfoBlocco infoCondizione = slotCondizione.GetChild(0).GetComponent<InfoBlocco>();
        if (infoCondizione == null) return false;

        return robot.VerificaCondizione(infoCondizione.tipo);
    }

    // Perche' una corotine? Tanto gestisce l'esecuzione dell'interno programma dell'utente.
    // Non e' meglio eseguirlo all'interno di Update
    private IEnumerator EseguiInsiemeBlocchi(Transform contenitore)
    {
        programmaInEsecuzione = true;

        // Scorre tutti i blocchi inseriti nell'area corrente
        foreach (Transform blocco in contenitore)
        {
            InfoBlocco info = blocco.GetComponent<InfoBlocco>();
            if (info == null) continue; // Salta slot vuoti o sfondi

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
                    // Recuperiamo la condizione e la pancia dei comandi
                    Transform slotCondIf = blocco.GetComponentInChildren<SlotCondizione>().transform;
                    Transform slotComandiIf = blocco.GetComponentInChildren<SlotComandi>().transform;
                    
                    if (ValutaCondizioneDelloSlot(slotCondIf))
                    {
                        yield return EseguiInsiemeBlocchi(slotComandiIf);
                    }
                    break;

                case TipoBlocco.While:
                    Transform slotCondWhile = blocco.GetComponentInChildren<SlotCondizione>().transform;
                    Transform slotComandiWhile = blocco.GetComponentInChildren<SlotComandi>().transform;

                    int limiteSicurezza = 0; // Evita crash da loop infiniti
                    while (ValutaCondizioneDelloSlot(slotCondWhile) && limiteSicurezza < 100000
                    )
                    {
                        yield return EseguiInsiemeBlocchi(slotComandiWhile);
                        limiteSicurezza++;
                    }
                    if (limiteSicurezza >= 100000) Debug.LogError("[Esecutore] Loop infinito rilevato nel WHILE!");
                    break;
            }
        }

        if (contenitore == GestoreBlocchi.Instance.areaPrincipaleCodice)
        {
            programmaInEsecuzione = false;
            Debug.Log("[Esecutore] Programma terminato.");
        }
    }
}

