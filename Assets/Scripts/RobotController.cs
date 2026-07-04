using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Impostazioni Griglia")]
    public float dimensioneCella = 2f; // Lunghezza di un passo sulla griglia
    public float velocitaMovimento = 4f;
    public float velocitaRotazione = 360f;
    public LayerMask layerOstacoli; // Configura i layer di Muri e Nemici

    [Header("Animazioni")]
    public Animator animator; 

    [Header("Regolazione Occhi (Sensore)")]
    [Tooltip("Quanto in alto si trovano gli occhi rispetto alla base del robot")]
    public float altezzaOcchi = 1.2f; 
    [Tooltip("Quanto in avanti deve partire il raggio per uscire dalla faccia")]
    public float offsetAvantiOcchi = 0.4f;
    
    // CORREZIONE 1: Il parametro corretto dell'asset è "Walk_Anim"
    private const string PARAM_CAMMINA = "Walk_Anim";

    private bool staEseguendo = false;
    private Vector3 posizioneIniziale;
    private Quaternion rotazioneIniziale;

    private void Awake()
    {
        // Salva lo stato iniziale per il Reset
        posizioneIniziale = transform.position;
        rotazioneIniziale = transform.rotation;

        // CORREZIONE 2: Poiché lo script è sul Padre e l'Animator è sul Figlio,
        // usiamo GetComponentInChildren, altrimenti l'animator resterà NULL!
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // CORREZIONE 3: Comunica all'Animator che il robot è "Aperto" all'avvio.
        // Questo sblocca lo stato arancione "anim_open" e lo fa passare all'Idle.
        if (animator != null)
        {
            animator.SetBool("Open_Anim", true);
        }
    }

    public void RipristinaPosizione()
    {
        StopAllCoroutines();
        transform.position = posizioneIniziale;
        transform.rotation = rotazioneIniziale;

        if (animator != null) 
        {
            animator.SetBool(PARAM_CAMMINA, false);
            animator.SetBool("Open_Anim", true);
        }

        staEseguendo = false;
    }

    // --- AZIONI ---

    public IEnumerator MuoviAvanti() => Muovi(transform.forward);
    public IEnumerator MuoviIndietro() => Muovi(-transform.forward);
    public IEnumerator RuotaSinistra() => Ruota(-90f);
    public IEnumerator RuotaDestra() => Ruota(90f);

    public IEnumerator Spara()
    {
        Debug.Log("[Robot] PEW PEW! Ha sparato!");
        yield return new WaitForSeconds(0.5f); 
    }

    private void Update()
    {
        // Spostiamo l'origine in ALTO e poi in AVANTI (moltiplicato per l'orientamento del robot)
        Vector3 origineRaggio = transform.position + (Vector3.up * altezzaOcchi) + (transform.forward * offsetAvantiOcchi);
        
        // Disegna la linea rossa che punta sempre davanti al robot
        Debug.DrawRay(origineRaggio, transform.forward * dimensioneCella, Color.red);
    }
    private IEnumerator Muovi(Vector3 direzione)
    {
        Vector3 targetPos = transform.position + (direzione * dimensioneCella);
        
        Vector3 origineRaggio = transform.position + (Vector3.up * altezzaOcchi) + (transform.forward * offsetAvantiOcchi);

        if (Physics.Raycast(origineRaggio, direzione, dimensioneCella, layerOstacoli))
        {
            Debug.LogWarning("[Robot] Movimento bloccato da un ostacolo!");
            yield break; // Interrompe il movimento
        }

        if (animator != null) 
        {
            animator.SetBool(PARAM_CAMMINA, true);
        }

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, velocitaMovimento * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        if (animator != null) 
        {
            animator.SetBool(PARAM_CAMMINA, false);
        }
    }

    private IEnumerator Ruota(float angolo)
    {
        Quaternion targetRot = transform.rotation * Quaternion.Euler(0, angolo, 0);
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, velocitaRotazione * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRot;
    }

    // --- CONDIZIONI ---

    public bool VerificaCondizione(TipoBlocco condizione)
    {
        RaycastHit hit;
        // CORREZIONE: Alziamo il raggio anche qui per la verifica dei blocchi IF/WHILE
        Vector3 origineRaggio = transform.position + (Vector3.up * altezzaOcchi) + (transform.forward * offsetAvantiOcchi);
        
        bool riscontraQualcosa = Physics.Raycast(origineRaggio, transform.forward, out hit, dimensioneCella, layerOstacoli);

        switch (condizione)
        {
            case TipoBlocco.Muro:
                return riscontraQualcosa && hit.collider.CompareTag("Muro");
            
            case TipoBlocco.Nemico:
                return riscontraQualcosa && hit.collider.CompareTag("Nemico");
            
            case TipoBlocco.Libero:
                return !riscontraQualcosa;

            default:
                return false;
        }
    }
}