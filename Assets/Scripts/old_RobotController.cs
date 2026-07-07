using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class old_RobotController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    float stepLength = 2f,
          movementSpeed = 4f,
          rotationSpeed = 360f;

    [SerializeField]
    LayerMask mask;

    [Header("Sensor")]
    [SerializeField]
    Transform sensor;

    [SerializeField, Range(0f, 1f)]
    float raySphereRadius = 0.4f;      // Use this for both wallas and enemies

    [SerializeField, Range(1f, 0f)]
    float raySphereDistance = 8f;

#if UNITY_EDITOR
    [SerializeField]
    public bool showRaySphere = true;
#endif

    [Header("Shooting")]
    [SerializeField]
    GameObject proiettilePrefab;

    [SerializeField]
    float range = 10f,
          shootingDelay = 0.5f;

    [Header("Animation")]
    Animator animator;

    const string Animation_WalkAnim = "Walk_Anim",
                 Animation_Colpito = "Colpito",
                 Animation_Open = "Open_Anim";

    [Header("Regolazione Occhi Fallback")]
    public float altezzaOcchi = 0.8f;         // remove
    public float offsetAvantiOcchi = 0.4f;    // remove

    [Header("Audio")]
    [SerializeField] 
    AudioSource audioSource;

    [SerializeField]
    AudioClip[] audioClips;

    const string AudioClip_RobotFootsteps = "robot_footsteps",
                 AudioClip_RobotDeath = "robot_death",
                 AudioClip_RobotWarning = "robot_warning",
                 AudioClip_Robot


    /* OLD AUDIO FIELDS
    [Header("Audio Movimento")]
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private float intervalloSuonoPasso = 0.25f;

    [Header("Audio Warning")]
    [SerializeField] private AudioSource warningAudioSource;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private float warningCooldown = 1f;

    [SerializeField] private AudioClip prepareToShootSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    private float prossimoWarningTime = 0f;

    */

#if UNITY_EDITOR
    [SerializeField]
    float prepareSoundDelay = 0.4f,
          reloadSoundDelay  = 0.8f,
          stepSoundDelay    = 0.25f,
          warningSoundDelay = 1f;      
#endif

    [Header("Vita")]
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int currentHealth;


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

    void Awake()
    {
        posizioneIniziale = transform.position;
        rotazioneIniziale = transform.rotation;

        currentHealth = maxHealth;
    }

    private void Start()
    {
        animator.SetBool(Animation_Open, true);
    }

    private void Update()
    {
        ControllaWarningContinuo();
    }

    public void FullReset()
    {
        StopAllCoroutines();

        robotColpito = false;
        currentHealth = maxHealth;
        //prossimoWarningTime = 0f;

        audioSource.Stop();

        transform.SetPositionAndRotation(posizioneIniziale, rotazioneIniziale);


        //if (warningAudioSource != audioSource)
        //warningAudioSource.Stop();

        animator.SetBool(Animation_WalkAnim, false);
        animator.ResetTrigger(Animation_Colpito);
        animator.SetBool(Animation_Open, true);
    }


    /* OLD AUDIO METHODS
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

        if (Time.time < prossimoWarningTime)
            return;

        warningAudioSource.PlayOneShot(warningSound);
        prossimoWarningTime = Time.time + warningCooldown;
    }
    */

    IEnumerator PlayClipSound(string clipName)
    {
        AudioClip clip = null;
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].name == clipName)
            {
                clip = audioClips[i];
                break;
            }
        }

#if DEBUG
        if (clip == null)
        {
            Debug.LogError("[Audio]: Missing clip");
            yield break;
        }
#endif

        const int repetition = 3;
        if (clip.name == AudioClip_RobotWarning)
        {
            audioSource.PlayOneShot(clip);
            for (int i = 0; i < repetition; i++)
            {
                yield return new WaitForSeconds(Time.time + warningSoundDelay);
                audioSource.PlayOneShot(clip);
            }

            yield break;
        }
        else if (clip.name == AudioClip_RobotFootsteps)
        {
            yield return new WaitForSeconds(Time.time + stepSoundDelay);
        }

        audioSource.PlayOneShot(clip);
    }

    bool SensorScanHit(out RaycastHit info, float radius, float distance)
    {
        bool hit = Physics.SphereCast(
                            sensor.position,
                            radius,
                            sensor.forward,
                            out info,
                            distance,
                            mask
                   );
        
        if (info.collider.CompareTag("Muro") || info.collider.CompareTag("Nemico"))
        {
            StartCoroutine(PlayClipSound(AudioClip_RobotWarning));
            return true; 
        }

        return false;    
    }

    private IEnumerator Muovi(Vector3 direzione)
    {
        if (robotColpito)
            yield break;

        if (SensorScanHit(out RaycastHit info, raySphereRadius, raySphereDistance))
        { 
            Debug.LogWarning("[Robot] Movimento bloccato da: " + info.collider.name);
            yield break;
        }

        Vector3 movement = transform.position + direzione * stepLength;

        animator.SetBool(Animation_WalkAnim, true);

        while (Vector3.Distance(transform.position, movement) > 0.01f)
        {
            StartCoroutine(PlayClipSound(AudioClip_RobotFootsteps));

            transform.position = Vector3.MoveTowards(
                transform.position,
                movement,
                movementSpeed * Time.deltaTime
            );

            yield return null;
        }

        animator.SetBool(Animation_WalkAnim, false);
    }

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

        if (SensorScanHit(out RaycastHit info, raySphereRadius = 0.4f, range))
        {
            Debug.Log("[Robot] Nessun nemico davanti. Non sparo.");

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
        yield return new WaitForSeconds(shootingDelay);

        if (robotColpito)
            yield break;

        PlaySound(reloadSound);
        yield return new WaitForSeconds(reloadDelay);
    }
    
    /*
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
    */

    // --- MOVIMENTO ---



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

        bool hit = ValidateAction(out RaycastHit info, raySphereRadius = 0.4f, range);


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
                        raySphereRadius,
                        transform.forward,
                        out hit,
                        raySphereDistance,
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
                        raySphereRadius,
                        transform.forward,
                        out hit,
                        raySphereDistance,
                        mask
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
    /*
    private void OnDrawGizmosSelected()
    {
        if (!mostraDebugSensoreNemico)
            return;

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
    */

#if DEBUG
    void OnDrawGizmosSelected()
    {
        if (!showRaySphere) return;

        Vector3 spottigEnd = sensor.position + transform.forward * raySphereDistance;
        Vector3 shotingEnd = sensor.position + transform.forward * range;

        Gizmos.color = Color.magenta;
        //Gizmos.DrawWireSphere(sensor.position, raySphereDistance);
        Gizmos.DrawWireSphere(spottigEnd, raySphereDistance);
        Gizmos.DrawLine(sensor.position, spottigEnd);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(shotingEnd, range);
        Gizmos.DrawLine(sensor.position, shotingEnd);
    }
#endif
}





