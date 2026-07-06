using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    float stepLength = 2f,
          movementSpeed = 4f,
          rotationSpeed = 360f;

    [SerializeField]
    LayerMask mask;

    [Header("Sensore Rilevamento Muro")]
    public float distanzaRilevamentoMuro = 6f;

    [Header("Collisione Movimento")]
    public float raggioControllo = 0.35f;

    [Header("Sensor")]
    [SerializeField]
    Transform sensor;

    [SerializeField, Range(0f, 1f)]
    float raySphereRadius = 0.4f;
    
    [SerializeField, Range(1f, 0f)]
    float raySphereDistance = 8f;

#if DEBUG
    [SerializeField]
    public bool showRaySphere = true;
#endif

    [Header("Shooting")]
    [SerializeField]
    GameObject proiettilePrefab;

    public float distanzaSparo = 10f;
    public float raggioSensoreSparo = 0.4f;
    public float tempoSparo = 0.5f;

    [Header("Animation")]
    [Header("Audio Morte")]
    [SerializeField] private AudioClip deathSound; 

    [Header("Animazioni")]
    public Animator animator;

    [Header("Regolazione Occhi Fallback")]
    public float altezzaOcchi = 1.2f;
    public float offsetAvantiOcchi = 0.4f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip prepareToShootSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private float prepareDelay = 0.4f;
    [SerializeField] private float reloadDelay = 0.8f;

    [Header("Vita")]
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int currentHealth;

    [Header("Audio Movimento")]
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private float intervalloSuonoPasso = 0.25f;

    [Header("Audio Warning")]
    [SerializeField] private AudioSource warningAudioSource;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private float warningCooldown = 1f;

    private float prossimoWarningTime = 0f;

    private const string PARAM_CAMMINA = "Walk_Anim";
    private const string TRIGGER_COLPITO = "Colpito";

    private bool robotColpito = false;

    public bool IsColpito
    {
        get { return robotColpito; }
    }

    public int CurrentHealth
    {
        get { return currentHealth; }
    }

    public int MaxHealth
    {
        get { return maxHealth; }
    }

    Vector3 posizioneIniziale;
    Quaternion rotazioneIniziale;


    bool ValidateAction(out RaycastHit info, float radius, float distance) => Physics.SphereCast(
            sensor.position,
            radius,
            sensor.forward,
            out info,
            distance,
            mask
    );

    void Awake()
    {
        posizioneIniziale = transform.position;
        rotazioneIniziale = transform.rotation;

        currentHealth = maxHealth;
    }

    private void Start()
    {
        animator.SetBool("Open_Anim", true);   
        if (animator != null)
            animator.SetBool("Open_Anim", true);
    }

    private void Update()
    {
        ControllaWarningContinuo();
    }

    private void PlaySound(AudioClip clip)
    {
        if (robotColpito)
            return;

        if (audioSource == null)
            return;

        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    public void FullReset()
    private void PlayWalkSound()
    {
        if (robotColpito)
            return;

        if (audioSource == null)
            return;

        if (walkSound == null)
            return;

        audioSource.PlayOneShot(walkSound);
    }

    private void PlayWarningSound()
    {
        if (robotColpito)
            return;

        if (warningAudioSource == null)
            return;

        if (warningSound == null)
            return;

        if (Time.time < prossimoWarningTime)
            return;

        warningAudioSource.PlayOneShot(warningSound);
        prossimoWarningTime = Time.time + warningCooldown;
    }

    public void RipristinaPosizione()
    {
        StopAllCoroutines();

        robotColpito = false;
        currentHealth = maxHealth;
        prossimoWarningTime = 0f;

        audioSource.Stop();
        
        transform.SetPositionAndRotation(posizioneIniziale, rotazioneIniziale);
        
        if (audioSource != null)
            audioSource.Stop();

        if (warningAudioSource != null && warningAudioSource != audioSource)
            warningAudioSource.Stop();

        animator.SetBool(PARAM_CAMMINA, false);
        animator.ResetTrigger(TRIGGER_COLPITO);
        animator.SetBool("Open_Anim", true);
        
    }

    // --- AZIONI ---

    public IEnumerator MuoviAvanti()
    {
        yield return Muovi(transform .forward);
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

        /*
        RaycastHit hit;

        if (!RilevaNemicoPerSparo(out hit))
        {
            Debug.Log("[Robot] Nessun nemico davanti. Non sparo.");
            yield return new WaitForSeconds(tempoSparo);
            yield break;
        }
        */

        if (!ValidateAction(out RaycastHit info), )
        {
            Debug.Log("[Robot] Nessun nemico davanti. Non sparo.");
            //yield return new WaitForSeconds(tempoSparo);
            yield break;
        }

        if (robotColpito)
            yield break;

        PlaySound(prepareToShootSound);
        yield return new WaitForSeconds(prepareDelay);

        if (robotColpito)
            yield break;

        Vector3 origineSparo = GetOrigineOcchi();

        Vector3 puntoMira = info.collider.bounds.center;

        Vector3 direzioneSparo = puntoMira - origineSparo;

        if (direzioneSparo.sqrMagnitude < 0.001f)
            yield break;

        Quaternion rotazioneSparo = Quaternion.LookRotation(direzioneSparo.normalized);

        Instantiate(
            proiettilePrefab,
            origineSparo,
            rotazioneSparo
        );

        Debug.Log("[Robot] Sparo verso nemico: " + info.collider.name);

        PlaySound(shootSound);
        yield return new WaitForSeconds(tempoSparo);

        if (robotColpito)
            yield break;

        PlaySound(reloadSound);
        yield return new WaitForSeconds(reloadDelay);
    }

    // --- SENSORI ---

    private Vector3 GetOrigineOcchi()
    {
        if (sensor != null)
            return sensor.position;

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

    private bool RilevaNemicoPerCondizione()
    {
        /*
        Vector3 origine = GetOrigineOcchi();

        bool haColpito = Physics.SphereCast(
            origine,
            raggioSensoreNemico,
            transform.forward,
            out hit,
            distanzaRilevamentoNemico,
            mask,
            QueryTriggerInteraction.Ignore
        );
        */

        bool hit = ValidateAction(out RaycastHit info);


        if (!hit)
        {
            Debug.Log("[Sensore Nemico] Non colpisco nessun collider nemico.");
            return false;
        }

        Debug.Log(
            "[Sensore Nemico] Colpito: " + info.collider.name +
            " | Tag: " + info.collider.tag +
            " | Root Tag: " + info.collider.transform.root.tag +
            " | Layer: " + LayerMask.LayerToName(info.collider.gameObject.layer) +
            " | IsTrigger: " + info.collider.isTrigger
        );

        return info.collider.CompareTag("Nemico") ||
               info.collider.transform.root.CompareTag("Nemico");
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
            mask,
            QueryTriggerInteraction.Ignore
        );

        if (!haColpito)
            return false;

        return hit.collider.CompareTag("Nemico") ||
               hit.collider.transform.root.CompareTag("Nemico");
    }

    private bool RilevaNemicoPerWarning(out RaycastHit hit)
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
            return false;

        return hit.collider.CompareTag("Nemico") ||
               hit.collider.transform.root.CompareTag("Nemico");
    }

    private bool RilevaMuroVicinoPerWarning(out RaycastHit hit)
    {
        Vector3 origineRaggio = GetOrigineSensoreMovimento(transform.forward);

        bool haColpito = Physics.SphereCast(
            origineRaggio,
            raggioControllo,
            transform.forward,
            out hit,
            distanzaRilevamentoMuro,
            layerOstacoli,
            QueryTriggerInteraction.Ignore
        );

        if (!haColpito)
            return false;

        return hit.collider.CompareTag("Muro") ||
               hit.collider.transform.root.CompareTag("Muro");
    }

    private void ControllaWarningContinuo()
    {
        if (robotColpito)
            return;

        RaycastHit hitNemico;
        RaycastHit hitMuro;

        bool vedeNemico = RilevaNemicoPerWarning(out hitNemico);
        bool vedeMuroVicino = RilevaMuroVicinoPerWarning(out hitMuro);

        if (vedeNemico || vedeMuroVicino)
        {
            PlayWarningSound();

            if (vedeNemico)
            {
                Debug.Log(
                    "[Warning Continuo] Nemico davanti: " + hitNemico.collider.name +
                    " | Distanza: " + hitNemico.distance
                );
            }
            else if (vedeMuroVicino)
            {
                Debug.Log(
                    "[Warning Continuo] Muro vicino davanti: " + hitMuro.collider.name +
                    " | Distanza: " + hitMuro.distance
                );
            }
        }
    }

    // --- MOVIMENTO ---

    private IEnumerator Muovi(Vector3 direzione)
    {
        if (robotColpito)
            yield break;

        Vector3 targetPos = transform.position + direzione * stepLength;

        Vector3 origineRaggio = GetOrigineSensoreMovimento(direzione);

        RaycastHit hit;

        if (Physics.SphereCast(
            origineRaggio,
            raggioControllo,
            direzione,
            out hit,
            stepLength,
            mask,
            QueryTriggerInteraction.Ignore))
        {
            Debug.LogWarning("[Robot] Movimento bloccato da: " + hit.collider.name);
            yield break;
        }

        if (animator != null)
            animator.SetBool(PARAM_CAMMINA, true);

        float prossimoSuonoPasso = 0f;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            if (robotColpito)
                yield break;

            if (Time.time >= prossimoSuonoPasso)
            {
                PlayWalkSound();
                prossimoSuonoPasso = Time.time + intervalloSuonoPasso;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                movementSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPos;

        if (animator != null)
            animator.SetBool(PARAM_CAMMINA, false);
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
                rotationSpeed * Time.deltaTime
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
            {
                return RilevaNemicoPerCondizione(out hit);
            }

            case TipoBlocco.Muro:
            {
                Vector3 origineRaggio = GetOrigineSensoreMovimento(transform.forward);

                bool vedeMuro = Physics.SphereCast(
                    origineRaggio,
                    raggioControllo,
                    transform.forward,
                    out hit,
                    stepLength,
                    mask,
                    QueryTriggerInteraction.Ignore
                );

                bool muroDavanti =
                    vedeMuro &&
                    (hit.collider.CompareTag("Muro") ||
                     hit.collider.transform.root.CompareTag("Muro"));

                return muroDavanti;
            }

            case TipoBlocco.Libero:
            {
                Vector3 origineRaggio = GetOrigineSensoreMovimento(transform.forward);

                bool trovaOstacolo = Physics.SphereCast(
                    origineRaggio,
                    raggioControllo,
                    transform.forward,
                    out hit,
                    stepLength,
                    mask,
                    QueryTriggerInteraction.Ignore
                );

                if (trovaOstacolo)
                {
                    Debug.Log(
                        "[Sensore Libero] Strada bloccata da: " + hit.collider.name +
                        " | Tag: " + hit.collider.tag +
                        " | Root Tag: " + hit.collider.transform.root.tag +
                        " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer)
                    );
                }
                else
                {
                    Debug.Log("[Sensore Libero] Strada libera.");
                }

                return !trovaOstacolo;
            }

            default:
                return false;
        }
    }

    // --- DANNO ---

    public void ColpitoDaLaser(int danno)
    {
        if (robotColpito)
            return;

        currentHealth -= danno;

        Debug.LogWarning("[Robot] Colpito dal laser! Danno: " + danno + " | Vita rimasta: " + currentHealth);

        if (currentHealth > 0)
        {
            // Il robot ha preso danno, ma è ancora vivo.
            return;
        }

        currentHealth = 0;
        robotColpito = true;

        StopAllCoroutines();

        if (audioSource != null)
        {
            audioSource.Stop();

            if (deathSound != null)
                audioSource.PlayOneShot(deathSound);
        }

        if (warningAudioSource != null && warningAudioSource != audioSource)
            warningAudioSource.Stop();

        if (animator != null)
        {
            animator.SetBool(PARAM_CAMMINA, false);
            animator.SetBool("Open_Anim", false);

            animator.ResetTrigger(TRIGGER_COLPITO);
            animator.SetTrigger(TRIGGER_COLPITO);
        }

        Debug.LogWarning("[Robot] Vita esaurita. Robot disattivato.");
    }

    // --- DEBUG VISIVO ---

    private void OnDrawGizmosSelected()
    {
        if (!mostraDebugSensoreNemico)
            return;
        }

        // Se arriva qui, la vita è finita.
        currentHealth = 0;
        robotColpito = true;

        StopAllCoroutines();

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (animator != null)
        {
            animator.SetBool(PARAM_CAMMINA, false);
            animator.SetBool("Open_Anim", false);

            animator.ResetTrigger(TRIGGER_COLPITO);
            animator.SetTrigger(TRIGGER_COLPITO);
        }

        Debug.LogWarning("[Robot] Vita esaurita. Robot disattivato.");
    }

#if DEBUG
    void OnDrawGizmosSelected()
    {
        if (!showRaySphere) return;

        Vector3 spottigEnd = sensor.position + transform.forward * raySphereDistance;
        Vector3 shotingEnd = sensor.position + transform.forward * distanzaSparo;

        Gizmos.color = Color.magenta;
        //Gizmos.DrawWireSphere(sensor.position, raySphereDistance);
        Gizmos.DrawWireSphere(spottigEnd, raySphereDistance);
        Gizmos.DrawLine(sensor.position, spottigEnd);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(shotingEnd, raggioSensoreSparo);
        Gizmos.DrawLine(sensor.position, shotingEnd);
    }
#endif
}
