using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Impostazioni Griglia")]
    public float dimensioneCella = 2f;
    public float velocitaMovimento = 4f;
    public float velocitaRotazione = 360f;
    public LayerMask layerOstacoli;

    [Header("Collisione Movimento")]
    public float raggioControllo = 0.35f;

    [Header("Sensore Nemico")]
    public Transform eyePoint;
    public LayerMask layerNemici;
    public float distanzaRilevamentoNemico = 8f;
    public float raggioSensoreNemico = 0.4f;
    public bool mostraDebugSensoreNemico = true;

    [Header("Sparo")]
    public GameObject proiettilePrefab;
    public float distanzaSparo = 10f;
    public float raggioSensoreSparo = 0.4f;
    public float tempoSparo = 0.5f;

    [Header("Animazioni")]
    public Animator animator;

    [Header("Regolazione Occhi Fallback")]
    public float altezzaOcchi = 1.2f;
    public float offsetAvantiOcchi = 0.4f;

    private const string PARAM_CAMMINA = "Walk_Anim";

    private bool robotColpito = false;
    private bool staEseguendo = false;

    private Vector3 posizioneIniziale;
    private Quaternion rotazioneIniziale;

    public void StartNewGame(Vector3 position)
    {
        transform.position = position;

        posizioneIniziale = transform.position;
        rotazioneIniziale = transform.rotation;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {

    }
   

    private void Start()
    {
        if (animator != null)
        {
            animator.SetBool("Open_Anim", true);
        }
    }

    public void RipristinaPosizione()
    {
        StopAllCoroutines();

        robotColpito = false;

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

    public IEnumerator MuoviAvanti()
    {
        yield return Muovi(transform.forward);
    }

    public IEnumerator MuoviIndietro()
    {
        if (robotColpito)
            yield break;

        yield return Ruota(180f);

        if (robotColpito)
            yield break;

        yield return Muovi(transform.forward);
    }

    public IEnumerator RuotaSinistra()
    {
        yield return Ruota(-90f);
    }

    public IEnumerator RuotaDestra()
    {
        yield return Ruota(90f);
    }

    public IEnumerator Spara()
    {
        if (robotColpito)
            yield break;

        if (proiettilePrefab == null)
        {
            Debug.LogError("[Robot] Proiettile prefab non assegnato.");
            yield break;
        }

        RaycastHit hit;

        if (!RilevaNemicoPerSparo(out hit))
        {
            Debug.Log("[Robot] Nessun nemico davanti. Non sparo.");
            yield return new WaitForSeconds(tempoSparo);
            yield break;
        }

        Vector3 origineSparo = GetOrigineOcchi();
        Vector3 puntoMira = hit.collider.bounds.center;

        Vector3 direzioneSparo = puntoMira - origineSparo;

        if (direzioneSparo.sqrMagnitude < 0.001f)
            yield break;

        Quaternion rotazioneSparo = Quaternion.LookRotation(direzioneSparo.normalized);

        Instantiate(
            proiettilePrefab,
            origineSparo,
            rotazioneSparo
        );

        Debug.Log("[Robot] Sparo verso nemico: " + hit.collider.name);

        yield return new WaitForSeconds(tempoSparo);
    }

    // --- SENSORI ---

    private Vector3 GetOrigineOcchi()
    {
        if (eyePoint != null)
            return eyePoint.position;

        return transform.position
            + Vector3.up * altezzaOcchi
            + transform.forward * offsetAvantiOcchi;
    }

    private Vector3 GetOrigineSensoreMovimento(Vector3 direzione)
    {
        return transform.position
            + Vector3.up * altezzaOcchi
            + direzione.normalized * offsetAvantiOcchi;
    }

    private bool RilevaNemicoPerCondizione(out RaycastHit hit)
    {
        Vector3 origine = GetOrigineOcchi();

        bool haColpito = Physics.SphereCast(
            origine,
            raggioSensoreNemico,
            transform.forward,
            out hit,
            distanzaRilevamentoNemico,
            layerNemici,
            QueryTriggerInteraction.Ignore
        );

        if (!haColpito)
        {
            Debug.Log("[Sensore Nemico] Non colpisco nessun collider nemico.");
            return false;
        }

        Debug.Log(
            "[Sensore Nemico] Colpito: " + hit.collider.name +
            " | Tag: " + hit.collider.tag +
            " | Root Tag: " + hit.collider.transform.root.tag +
            " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer) +
            " | IsTrigger: " + hit.collider.isTrigger
        );

        return hit.collider.CompareTag("Nemico") ||
               hit.collider.transform.root.CompareTag("Nemico");
    }

    private bool RilevaNemicoPerSparo(out RaycastHit hit)
    {
        Vector3 origine = GetOrigineOcchi();

        bool haColpito = Physics.SphereCast(
            origine,
            raggioSensoreSparo,
            transform.forward,
            out hit,
            distanzaSparo,
            layerNemici,
            QueryTriggerInteraction.Ignore
        );

        if (!haColpito)
            return false;

        return hit.collider.CompareTag("Nemico") ||
               hit.collider.transform.root.CompareTag("Nemico");
    }

    // --- MOVIMENTO ---

    private IEnumerator Muovi(Vector3 direzione)
    {
        if (robotColpito)
            yield break;

        Vector3 targetPos = transform.position + direzione * dimensioneCella;

        Vector3 origineRaggio = GetOrigineSensoreMovimento(direzione);

        RaycastHit hit;

        if (Physics.SphereCast(
            origineRaggio,
            raggioControllo,
            direzione,
            out hit,
            dimensioneCella,
            layerOstacoli,
            QueryTriggerInteraction.Ignore))
        {
            Debug.LogWarning("[Robot] Movimento bloccato da: " + hit.collider.name);
            yield break;
        }

        if (animator != null)
        {
            animator.SetBool(PARAM_CAMMINA, true);
        }

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            if (robotColpito)
                yield break;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                velocitaMovimento * Time.deltaTime
            );

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
        if (robotColpito)
            yield break;

        Quaternion targetRot = transform.rotation * Quaternion.Euler(0f, angolo, 0f);

        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            if (robotColpito)
                yield break;

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                velocitaRotazione * Time.deltaTime
            );

            yield return null;
        }

        transform.rotation = targetRot;
    }

    // --- CONDIZIONI ---

    public bool VerificaCondizione(TipoBlocco condizione)
    {
        RaycastHit hit;

        switch (condizione)
        {
            case TipoBlocco.Nemico:
                return RilevaNemicoPerCondizione(out hit);

            case TipoBlocco.Muro:
            {
                Vector3 origineRaggio = GetOrigineSensoreMovimento(transform.forward);

                bool vedeMuro = Physics.SphereCast(
                    origineRaggio,
                    raggioControllo,
                    transform.forward,
                    out hit,
                    dimensioneCella,
                    layerOstacoli,
                    QueryTriggerInteraction.Ignore
                );

                return vedeMuro &&
                       (hit.collider.CompareTag("Muro") ||
                        hit.collider.transform.root.CompareTag("Muro"));
            }

            case TipoBlocco.Libero:
            {
                Vector3 origineRaggio = GetOrigineSensoreMovimento(transform.forward);

                bool trovaOstacolo = Physics.SphereCast(
                    origineRaggio,
                    raggioControllo,
                    transform.forward,
                    out hit,
                    dimensioneCella,
                    layerOstacoli,
                    QueryTriggerInteraction.Ignore
                );

                return !trovaOstacolo;
            }

            default:
                return false;
        }
    }

    // --- DANNO ---

    public void ColpitoDaLaser(int danno)
    {
        Debug.LogWarning("[Robot] Colpito dal laser! Danno: " + danno);

        robotColpito = true;
        StopAllCoroutines();

        if (animator != null)
        {
            animator.SetBool(PARAM_CAMMINA, false);
        }
    }

    // --- DEBUG VISIVO ---

    private void OnDrawGizmosSelected()
    {
        if (!mostraDebugSensoreNemico)
            return;

        Vector3 origine;

        if (eyePoint != null)
        {
            origine = eyePoint.position;
        }
        else
        {
            origine = transform.position
                + Vector3.up * altezzaOcchi
                + transform.forward * offsetAvantiOcchi;
        }

        Vector3 fineRilevamento = origine + transform.forward * distanzaRilevamentoNemico;
        Vector3 fineSparo = origine + transform.forward * distanzaSparo;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(origine, raggioSensoreNemico);
        Gizmos.DrawWireSphere(fineRilevamento, raggioSensoreNemico);
        Gizmos.DrawLine(origine, fineRilevamento);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(fineSparo, raggioSensoreSparo);
        Gizmos.DrawLine(origine, fineSparo);
    }
}