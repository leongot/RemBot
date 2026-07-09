using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Impostazioni Griglia / Movimento")]
    public float dimensioneCella = 12f;
    public float velocitaMovimento = 4f;
    public float velocitaRotazione = 360f;
    public LayerMask layerOstacoli;

    [Header("Sensore Muro - Condizioni")]
    [Tooltip("Distanza corta usata da IF(MURO), WHILE(LIBERO) e warning muro. Non metterla a 12.")]
    public float distanzaRilevamentoMuro = 1.2f;

    [Header("Vecchia Collisione Movimento - Non più usata per il muro")]
    public float raggioControllo = 0.35f;
    [SerializeField] private float distanzaBloccoMovimento = 2f;

    [Header("Sensore Muro Corpo")]
    [SerializeField] private float altezzaSensoreMuro = 0.8f;
    [SerializeField] private float offsetAvantiSensoreMuro = 0f;
    [SerializeField] private float raggioSensoreMuro = 0.3f;
    [SerializeField] private float distanzaStopMuro = 0.8f;
    [SerializeField] private float margineAnticipoMuro = 0.15f;
    [SerializeField] private float durataMemoriaMuro = 0.8f;

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

    private bool ultimoMovimentoFermatoDaMuro = false;
    private float tempoUltimoBloccoMuro = -999f;

    private Rigidbody rb;
    private RigidbodyConstraints vincoliInizialiRb;
    private bool rbKinematicIniziale;
    private bool rbUseGravityIniziale;

    private Vector3 posizioneIniziale;
    private Quaternion rotazioneIniziale;

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

    // ---------------------------------------------------------
    // AUDIO
    // ---------------------------------------------------------

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

    // ---------------------------------------------------------
    // RESET ROBOT
    // ---------------------------------------------------------

    public void RipristinaPosizione()
    {
        StopAllCoroutines();

        robotColpito = false;
        staCadendo = false;
        gameOverCadutaMostrato = false;
        currentHealth = maxHealth;
        prossimoWarningTime = 0f;

        ultimoMovimentoFermatoDaMuro = false;
        tempoUltimoBloccoMuro = -999f;

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
    }

    // ---------------------------------------------------------
    // MOVIMENTO PUBBLICO
    // ---------------------------------------------------------

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

    // ---------------------------------------------------------
    // SPARO
    // ---------------------------------------------------------

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

    // ---------------------------------------------------------
    // ORIGINI SENSORI
    // ---------------------------------------------------------

    private Vector3 GetOrigineOcchi()
    {
        if (eyePoint != null)
            return eyePoint.position;

        return transform.position
            + Vector3.up * altezzaOcchi
            + transform.forward * offsetAvantiOcchi;
    }

    private Vector3 NormalizzaDirezioneOrizzontale(Vector3 direzione)
    {
        direzione.y = 0f;

        if (direzione.sqrMagnitude < 0.001f)
        {
            direzione = transform.forward;
            direzione.y = 0f;
        }

        if (direzione.sqrMagnitude < 0.001f)
            return Vector3.forward;

        return direzione.normalized;
    }

    private Vector3 GetOrigineSensoreMuro(Vector3 direzione)
    {
        direzione = NormalizzaDirezioneOrizzontale(direzione);

        return transform.position
            + Vector3.up * altezzaSensoreMuro
            + direzione * offsetAvantiSensoreMuro;
    }

    // ---------------------------------------------------------
    // SENSORE MURO CORRETTO
    // ---------------------------------------------------------

    private bool ColliderEMuro(Collider col)
    {
        if (col == null)
            return false;

        return col.CompareTag("Muro") ||
               col.transform.root.CompareTag("Muro");
    }

    private bool ColliderDavantiAlSensore(Collider col, Vector3 origine, Vector3 direzione)
    {
        if (col == null)
            return false;

        direzione = NormalizzaDirezioneOrizzontale(direzione);

        Vector3 puntoPiuVicino = col.ClosestPoint(origine);
        Vector3 versoCollider = puntoPiuVicino - origine;

        if (versoCollider.sqrMagnitude < 0.0001f)
            return true;

        float davanti = Vector3.Dot(direzione, versoCollider.normalized);

        return davanti >= -0.05f;
    }

    private bool MemoriaMuroValida()
    {
        return ultimoMovimentoFermatoDaMuro &&
               Time.time - tempoUltimoBloccoMuro <= durataMemoriaMuro;
    }

    private void RegistraBloccoMuro()
    {
        ultimoMovimentoFermatoDaMuro = true;
        tempoUltimoBloccoMuro = Time.time;
    }

    private void CancellaMemoriaMuro()
    {
        ultimoMovimentoFermatoDaMuro = false;
        tempoUltimoBloccoMuro = -999f;
    }

    private bool RilevaMuroDavanti(Vector3 direzione, float distanza, out RaycastHit hit)
    {
        hit = new RaycastHit();

        direzione = NormalizzaDirezioneOrizzontale(direzione);
        Vector3 origine = GetOrigineSensoreMuro(direzione);

        Collider[] collidersVicini = Physics.OverlapSphere(
            origine,
            raggioSensoreMuro,
            layerOstacoli,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider col in collidersVicini)
        {
            if (col == null)
                continue;

            if (col.isTrigger)
                continue;

            if (!ColliderEMuro(col))
                continue;

            if (!ColliderDavantiAlSensore(col, origine, direzione))
                continue;

            Debug.LogWarning(
                "[Sensore Muro] Muro già vicino/sovrapposto: " + col.name +
                " | Origine sensore: " + origine
            );

            return true;
        }

        bool haColpito = Physics.SphereCast(
            origine,
            raggioSensoreMuro,
            direzione,
            out hit,
            distanza,
            layerOstacoli,
            QueryTriggerInteraction.Ignore
        );

        if (!haColpito)
            return false;

        bool eMuro = ColliderEMuro(hit.collider);

        Debug.Log(
            "[Sensore Muro] Hit: " + hit.collider.name +
            " | È muro: " + eMuro +
            " | Distanza hit: " + hit.distance +
            " | Distanza controllo: " + distanza +
            " | Origine: " + origine
        );

        return eMuro;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Muro"))
            Muori("Wall hit");
    }


    // ---------------------------------------------------------
    // SENSORI NEMICO
    // ---------------------------------------------------------

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
        return RilevaMuroDavanti(
            transform.forward,
            distanzaRilevamentoMuro,
            out hit
        );
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
            else if (vedeMuroVicino && hitMuro.collider != null)
            {
                Debug.Log(
                    "[Warning Continuo] Muro vicino davanti: " + hitMuro.collider.name +
                    " | Distanza: " + hitMuro.distance
                );
            }
        }
    }

    // ---------------------------------------------------------
    // MOVIMENTO CORRETTO CON CONTROLLO MURO CONTINUO
    // ---------------------------------------------------------

    private IEnumerator Muovi(Vector3 direzione)
    {
        if (robotColpito || staCadendo)
            yield break;

        direzione = NormalizzaDirezioneOrizzontale(direzione);

        CancellaMemoriaMuro();

        Vector3 targetPos = transform.position + direzione * dimensioneCella;
        bool movimentoBloccatoDaMuro = false;

        if (animator != null)
            animator.SetBool(PARAM_CAMMINA, true);

        float prossimoSuonoPasso = 0f;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            if (robotColpito || staCadendo)
            {
                if (animator != null)
                    animator.SetBool(PARAM_CAMMINA, false);

                yield break;
            }

            float movimentoQuestoFrame = velocitaMovimento * Time.deltaTime;

            float distanzaControlloMuro =
                distanzaStopMuro +
                movimentoQuestoFrame +
                margineAnticipoMuro;

            RaycastHit hit;

            if (RilevaMuroDavanti(direzione, distanzaControlloMuro, out hit))
            {
                movimentoBloccatoDaMuro = true;
                RegistraBloccoMuro();

                Debug.LogWarning(
                    "[Robot] Movimento fermato prima del muro." +
                    " | Muro: " + (hit.collider != null ? hit.collider.name : "muro vicino/sovrapposto") +
                    " | Distanza controllo: " + distanzaControlloMuro
                );

                break;
            }

            if (Time.time >= prossimoSuonoPasso)
            {
                PlayWalkSound();
                prossimoSuonoPasso = Time.time + intervalloSuonoPasso;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                movimentoQuestoFrame
            );

            yield return null;
        }

        if (!movimentoBloccatoDaMuro && !robotColpito && !staCadendo)
        {
            transform.position = targetPos;
            CancellaMemoriaMuro();
        }

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

        CancellaMemoriaMuro();
    }

    // ---------------------------------------------------------
    // CONDIZIONI BLOCCHI
    // ---------------------------------------------------------

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
                if (MemoriaMuroValida())
                {
                    Debug.Log("[Condizione MURO] Vera per memoria: il movimento precedente è stato fermato da un muro.");
                    return true;
                }

                bool muroDavanti = RilevaMuroDavanti(
                    transform.forward,
                    distanzaRilevamentoMuro,
                    out hit
                );

                Debug.Log("[Condizione MURO] Risultato: " + muroDavanti);

                return muroDavanti;
            }

            case TipoBlocco.Libero:
            {
                if (MemoriaMuroValida())
                {
                    Debug.Log("[Condizione LIBERO] Falsa per memoria: il movimento precedente è stato fermato da un muro.");
                    return false;
                }

                bool muroDavanti = RilevaMuroDavanti(
                    transform.forward,
                    distanzaRilevamentoMuro,
                    out hit
                );

                bool libero = !muroDavanti;

                Debug.Log("[Condizione LIBERO] Risultato: " + libero);

                return libero;
            }

            case TipoBlocco.Sempre:
            {
                Debug.Log("[Condizione SEMPRE] Vera.");
                return true;
            }

            default:
                return false;
        }
    }

    // ---------------------------------------------------------
    // VITA / MORTE
    // ---------------------------------------------------------

    public void ColpitoDaLaser(int danno)
    {
        if (robotColpito || staCadendo)
            return;

        currentHealth -= danno;

        Debug.LogWarning("[Robot] Colpito dal laser! Danno: " + danno + " | Vita rimasta: " + currentHealth);

        if (currentHealth > 0)
            return;

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

    // ---------------------------------------------------------
    // CADUTA
    // ---------------------------------------------------------

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

            rb.isKinematic = true;
            rb.useGravity = false;
        }

        StartCoroutine(TraslazioneCadutaPoiFisica());

        Debug.LogWarning("[Robot] Ha perso il terreno sotto: avvio traslazione diagonale di caduta.");
    }

    private IEnumerator TraslazioneCadutaPoiFisica()
    {
        float tempo = 0f;
        Vector3 direzioneAvanti = NormalizzaDirezioneOrizzontale(transform.forward);

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
                "Controlla che GameUIManager sia su un GameObject attivo nella scena."
            );
        }
    }

    // ---------------------------------------------------------
    // GIZMOS
    // ---------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        if (mostraDebugSensoreNemico)
        {
            Vector3 origineNemico;

            if (eyePoint != null)
            {
                origineNemico = eyePoint.position;
            }
            else
            {
                origineNemico = transform.position
                    + Vector3.up * altezzaOcchi
                    + transform.forward * offsetAvantiOcchi;
            }

            Vector3 fineRilevamentoNemico = origineNemico + transform.forward * distanzaRilevamentoNemico;
            Vector3 fineSparo = origineNemico + transform.forward * distanzaSparo;

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(origineNemico, raggioSensoreNemico);
            Gizmos.DrawWireSphere(fineRilevamentoNemico, raggioSensoreNemico);
            Gizmos.DrawLine(origineNemico, fineRilevamentoNemico);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(fineSparo, raggioSensoreSparo);
            Gizmos.DrawLine(origineNemico, fineSparo);
        }

        Vector3 direzioneMuro = transform.forward;
        direzioneMuro.y = 0f;

        if (direzioneMuro.sqrMagnitude < 0.001f)
            direzioneMuro = Vector3.forward;

        direzioneMuro.Normalize();

        Vector3 origineMuro =
            transform.position +
            Vector3.up * altezzaSensoreMuro +
            direzioneMuro * offsetAvantiSensoreMuro;

        Vector3 fineMuro = origineMuro + direzioneMuro * distanzaRilevamentoMuro;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origineMuro, raggioSensoreMuro);
        Gizmos.DrawLine(origineMuro, fineMuro);
        Gizmos.DrawWireSphere(fineMuro, raggioSensoreMuro);

        Vector3 fineStopMuro = origineMuro + direzioneMuro * distanzaStopMuro;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(fineStopMuro, raggioSensoreMuro);

        Vector3 origineTerreno = transform.position + Vector3.up * altezzaControlloTerreno;
        Vector3 fineTerreno = origineTerreno + Vector3.down * distanzaControlloTerreno;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origineTerreno, raggioControlloTerreno);
        Gizmos.DrawLine(origineTerreno, fineTerreno);
        Gizmos.DrawWireSphere(fineTerreno, raggioControlloTerreno);
    }
}