using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Impostazioni Griglia")]
    public float dimensioneCella = 2f;
    public float velocitaMovimento = 4f;
    public float velocitaRotazione = 360f;
    public LayerMask layerOstacoli;

    [Header("Sensore Rilevamento Muro")]
    public float distanzaRilevamentoMuro = 6f;

    [Header("Collisione Movimento")]
    public float raggioControllo = 0.35f;
    [SerializeField] private float distanzaBloccoMovimento = 6f;

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

    [Header("Audio Morte")]
    [SerializeField] private AudioClip deathSound;

    [Header("Animazioni")]
    public Animator animator;

    [Header("Regolazione Occhi Fallback")]
    public float altezzaOcchi = 1.2f;
    public float offsetAvantiOcchi = 0.4f;

    [Header("Audio Sparo")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip prepareToShootSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private float prepareDelay = 0.4f;
    [SerializeField] private float reloadDelay = 0.8f;

    [Header("Vita")]
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int currentHealth;

    [Header("Game Over Caduta")]
    [SerializeField] private bool gameOverSeCade = true;
    [SerializeField] private float quotaYGameOver = -100f;

    [Header("Controllo Terreno")]
    [SerializeField] private LayerMask layerTerreno;
    [SerializeField] private float raggioControlloTerreno = 0.35f;
    [SerializeField] private float altezzaControlloTerreno = 0.5f;
    [SerializeField] private float distanzaControlloTerreno = 1.2f;

    [Header("Traslazione Caduta")]
    [SerializeField] private float durataTraslazioneCaduta = 0.7f;
    [SerializeField] private float velocitaTraslazioneAvanti = 3f;
    [SerializeField] private float velocitaTraslazioneBasso = 2f;
    [SerializeField] private float velocitaFisicaInizialeCaduta = 2f;

    [Header("Audio Movimento")]
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private float intervalloSuonoPasso = 0.25f;

    [Header("Audio Warning")]
    [SerializeField] private AudioSource warningAudioSource;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private float warningCooldown = 1f;

    [Header("Game Over Morte")]
    [SerializeField] private float ritardoGameOverDopoMorte = 5f;

    private float prossimoWarningTime = 0f;

    private const string PARAM_CAMMINA = "Walk_Anim";
    private const string TRIGGER_COLPITO = "Colpito";

    private bool robotColpito = false;
    private bool staCadendo = false;
    private bool gameOverCadutaMostrato = false;

    private Rigidbody rb;
    private RigidbodyConstraints vincoliInizialiRb;
    private bool rbKinematicIniziale;
    private bool rbUseGravityIniziale;

    public bool IsColpito
    {
        get { return robotColpito; }
    }

    public bool StaCadendo
    {
        get { return staCadendo; }
    }

    public int CurrentHealth
    {
        get { return currentHealth; }
    }

    public int MaxHealth
    {
        get { return maxHealth; }
    }

    private bool staEseguendo = false;

    private Vector3 posizioneIniziale;
    private Quaternion rotazioneIniziale;

    private void Awake()
    {
        posizioneIniziale = transform.position;
        rotazioneIniziale = transform.rotation;

        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            vincoliInizialiRb = rb.constraints;
            rbKinematicIniziale = rb.isKinematic;
            rbUseGravityIniziale = rb.useGravity;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (warningAudioSource == null)
            warningAudioSource = audioSource;
    }

    private void Start()
    {
        if (animator != null)
            animator.SetBool("Open_Anim", true);
    }

    private void Update()
    {
        ControllaWarningContinuo();
        ControllaGameOverCaduta();
    }

    private void LateUpdate()
    {
        ControllaTerrenoSotto();
    }

    // --- CADUTA ---

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Muro"))
            Muori("Wall hit");
    }

    private void ControllaTerrenoSotto()
    {
        if (!gameOverSeCade)
            return;

        if (robotColpito || staCadendo)
            return;

        if (!HaTerrenoSotto(transform.position))
        {
            IniziaMorteDaCaduta();
        }
    }

    private bool HaTerrenoSotto(Vector3 posizione)
    {
        Vector3 origine = posizione + Vector3.up * altezzaControlloTerreno;

        return Physics.SphereCast(
            origine,
            raggioControlloTerreno,
            Vector3.down,
            out RaycastHit hit,
            distanzaControlloTerreno,
            layerTerreno,
            QueryTriggerInteraction.Ignore
        );
    }

    private void IniziaMorteDaCaduta()
    {
        if (staCadendo)
            return;

        staCadendo = true;
        robotColpito = true;
        currentHealth = 0;

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

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Durante la traslazione manuale il Rigidbody non deve combattere contro i collider del bordo.
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        StartCoroutine(TraslazioneCadutaPoiFisica());

        Debug.LogWarning("[Robot] Ha perso il terreno sotto: avvio traslazione diagonale di caduta.");
    }

    private IEnumerator TraslazioneCadutaPoiFisica()
    {
        float tempo = 0f;
        Vector3 direzioneAvanti = transform.forward.normalized;

        while (tempo < durataTraslazioneCaduta)
        {
            Vector3 spostamento =
                direzioneAvanti * velocitaTraslazioneAvanti +
                Vector3.down * velocitaTraslazioneBasso;

            transform.position += spostamento * Time.deltaTime;

            tempo += Time.deltaTime;
            yield return null;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;

            rb.linearVelocity = Vector3.down * velocitaFisicaInizialeCaduta;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }

        Debug.Log("[Robot] Traslazione caduta finita: ora il Rigidbody cade liberamente.");
    }

    private void ControllaGameOverCaduta()
    {
        if (!gameOverSeCade)
            return;

        if (gameOverCadutaMostrato)
            return;

        if (transform.position.y > quotaYGameOver)
            return;

        gameOverCadutaMostrato = true;

        Debug.LogWarning(
            "[Robot] Sono sotto quota " + quotaYGameOver +
            ". Posizione Y attuale: " + transform.position.y +
            ". Provo a mostrare Game Over."
        );

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.MostraGameOver();
        }
        else
        {
            Debug.LogError(
                "[Robot] GameUIManager.Instance è NULL. " +
                "Controlla che GameUIManager sia su un GameObject attivo nella scena, " +
                "per esempio Gestore Gioco."
            );
        }
    }

    
    private void PlaySound(AudioClip clip)
    {
        if (robotColpito || staCadendo)
            return;

        if (audioSource == null)
            return;

        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    private void PlayWalkSound()
    {
        if (robotColpito || staCadendo)
            return;

        if (audioSource == null)
            return;

        if (walkSound == null)
            return;

        audioSource.PlayOneShot(walkSound);
    }

    private void PlayWarningSound()
    {
        if (robotColpito || staCadendo)
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
        staCadendo = false;
        gameOverCadutaMostrato = false;
        currentHealth = maxHealth;
        prossimoWarningTime = 0f;

        if (audioSource != null)
            audioSource.Stop();

        if (warningAudioSource != null && warningAudioSource != audioSource)
            warningAudioSource.Stop();

        if (rb != null)
        {
            rb.isKinematic = rbKinematicIniziale;
            rb.useGravity = rbUseGravityIniziale;
            rb.constraints = vincoliInizialiRb;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = posizioneIniziale;
        transform.rotation = rotazioneIniziale;

        if (animator != null)
        {
            animator.SetBool(PARAM_CAMMINA, false);
            animator.ResetTrigger(TRIGGER_COLPITO);
            animator.SetBool("Open_Anim", true);
        }

        staEseguendo = false;
    }

   

    public IEnumerator MuoviAvanti()
    {
        if (robotColpito || staCadendo)
            yield break;

        yield return Muovi(transform.forward);
    }

    public IEnumerator MuoviIndietro()
    {
        if (robotColpito || staCadendo)
            yield break;

        yield return Ruota(180f);

        if (robotColpito || staCadendo)
            yield break;

        yield return Muovi(transform.forward);
    }

    public IEnumerator RuotaSinistra()
    {
        if (robotColpito || staCadendo)
            yield break;

        yield return Ruota(-90f);
    }

    public IEnumerator RuotaDestra()
    {
        if (robotColpito || staCadendo)
            yield break;

        yield return Ruota(90f);
    }

    public IEnumerator Spara()
    {
        if (robotColpito || staCadendo)
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

        if (robotColpito || staCadendo)
            yield break;

        PlaySound(prepareToShootSound);
        yield return new WaitForSeconds(prepareDelay);

        if (robotColpito || staCadendo)
            yield break;

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

        PlaySound(shootSound);
        yield return new WaitForSeconds(tempoSparo);

        if (robotColpito || staCadendo)
            yield break;

        PlaySound(reloadSound);
        yield return new WaitForSeconds(reloadDelay);
    }

   

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
            " | IsTrigger: " + hit.collider.isTrigger +
            " | Distanza: " + hit.distance
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
        if (robotColpito || staCadendo)
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

    

    private IEnumerator Muovi(Vector3 direzione)
    {
        if (robotColpito || staCadendo)
            yield break;

        Vector3 targetPos = transform.position + direzione * dimensioneCella;
        Vector3 origineRaggio = GetOrigineSensoreMovimento(direzione);

        RaycastHit hit;

        if (Physics.SphereCast(
            origineRaggio,
            raggioControllo,
            direzione,
            out hit,
            distanzaBloccoMovimento,
            layerOstacoli,
            QueryTriggerInteraction.Ignore))
        {
            Debug.LogWarning(
                "[Robot] Movimento bloccato da: " + hit.collider.name +
                " | Distanza hit: " + hit.distance +
                " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer)
            );

            yield break;
        }

        if (animator != null)
            animator.SetBool(PARAM_CAMMINA, true);

        float prossimoSuonoPasso = 0f;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            if (robotColpito || staCadendo)
                yield break;

            if (Time.time >= prossimoSuonoPasso)
            {
                PlayWalkSound();
                prossimoSuonoPasso = Time.time + intervalloSuonoPasso;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                velocitaMovimento * Time.deltaTime
            );

            yield return null;
        }

        if (robotColpito || staCadendo)
            yield break;

        transform.position = targetPos;

        if (animator != null)
            animator.SetBool(PARAM_CAMMINA, false);
    }

    private IEnumerator Ruota(float angolo)
    {
        if (robotColpito || staCadendo)
            yield break;

        Quaternion targetRot = transform.rotation * Quaternion.Euler(0f, angolo, 0f);

        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            if (robotColpito || staCadendo)
                yield break;

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                velocitaRotazione * Time.deltaTime
            );

            yield return null;
        }

        if (robotColpito || staCadendo)
            yield break;

        transform.rotation = targetRot;
    }

   

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
                    distanzaRilevamentoMuro,
                    layerOstacoli,
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
                    distanzaBloccoMovimento,
                    layerOstacoli,
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

   

    public void ColpitoDaLaser(int danno)
    {
        if (robotColpito || staCadendo)
            return;

        currentHealth -= danno;

        Debug.LogWarning("[Robot] Colpito dal laser! Danno: " + danno + " | Vita rimasta: " + currentHealth);

        if (currentHealth > 0)
        {
            return;
        }

        Muori("danno laser");
    }

    private void Muori(string motivo)
    {
        if (robotColpito)
            return;

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

            Debug.LogWarning("[Robot] Morte robot. Motivo: " + motivo);

            StartCoroutine(MostraGameOverDopoRitardo());
    }

    private IEnumerator MostraGameOverDopoRitardo()
    {
        yield return new WaitForSeconds(ritardoGameOverDopoMorte);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.MostraGameOver();
        }
        else
        {
            Debug.LogError("[Robot] GameUIManager.Instance è NULL. Non posso mostrare Game Over.");
        }
    }

    

    private void OnDrawGizmosSelected()
    {
        if (mostraDebugSensoreNemico)
        {
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

            Vector3 origineMovimento = transform.position
                + Vector3.up * altezzaOcchi
                + transform.forward * offsetAvantiOcchi;

            Vector3 fineMuro = origineMovimento + transform.forward * dimensioneCella;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origineMovimento, raggioControllo);
            Gizmos.DrawWireSphere(fineMuro, raggioControllo);
            Gizmos.DrawLine(origineMovimento, fineMuro);
        }

        // Debug controllo terreno sotto il robot.
        Vector3 origineTerreno = transform.position + Vector3.up * altezzaControlloTerreno;
        Vector3 fineTerreno = origineTerreno + Vector3.down * distanzaControlloTerreno;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origineTerreno, raggioControlloTerreno);
        Gizmos.DrawLine(origineTerreno, fineTerreno);
        Gizmos.DrawWireSphere(fineTerreno, raggioControlloTerreno);
    }
}
