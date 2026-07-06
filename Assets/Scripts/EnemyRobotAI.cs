using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyRobotAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject laserPrefab;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float reachDistance = 0.5f;

    [Header("Vision")]
    [SerializeField] private float visionRange = 18f;
    [SerializeField] private float visionAngle = 80f;

    [Header("Shooting")]
    [SerializeField] private float fireRate = 0.7f;
    [SerializeField] private float aimHeight = 1.2f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float deathAnimationDuration = 2f;
    [SerializeField] private float delayAfterDeathAnimation = 0.5f;

    [Header("Audio Sparo")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private float reloadSoundDelay = 0.4f;
    [SerializeField] private AudioClip enemyDetectedSound;
    [SerializeField] private float delayPrimaDiAttaccare = 1f;
    [SerializeField] private AudioClip deathSound;

    [Header("Audio Movimento")]
    [SerializeField] private AudioSource walkAudioSource;
    [SerializeField] private AudioClip enemyWalkSound;
    
    private RobotController playerRobotController;
    private int currentHealth;
    private int currentPatrolIndex = 0;
    private float nextFireTime = 0f;
    private bool isDead = false;
    private bool playerRilevato = false;
    private float tempoRilevamentoPlayer = 0f;

    private const string PARAM_IS_RUNNING = "IsRunning";
    private const string PARAM_IS_SHOOTING = "IsShooting";
    private const string TRIGGER_DIE = "Die";

    private void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (walkAudioSource == null)
        walkAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (player != null)
        {
            playerRobotController = player.GetComponentInParent<RobotController>();
        }
    }
    
    private void Update()
    {
        if (isDead)
            return;

        if (playerRobotController != null && playerRobotController.IsColpito)
        {
            SetRunning(false);
            SetShooting(false);
            StopWalkSound();
            playerRilevato = false;
            return;
        }

        if (player == null)
        {
            playerRilevato = false;
            Patrol();
            return;
        }

        if (CanSeePlayer())
        {
            if (playerRilevato == false)
            {
                playerRilevato = true;
                tempoRilevamentoPlayer = Time.time;
                SetRunning(false);
                SetShooting(false);

                if (walkAudioSource != null) 
                walkAudioSource.Stop();
                
                StartCoroutine(PlayEnemyDetectedSound());
                Debug.Log("[EnemyRobotAI] Player rilevato. Attacco tra 1 secondo.");
            }

        if (Time.time - tempoRilevamentoPlayer < delayPrimaDiAttaccare)
        {
            SetRunning(false);
            SetShooting(false);
            StopWalkSound();
            LookAtPlayer();
            return;
        }

            AttackPlayer();
        }
        else
        {
            playerRilevato = false;
            Patrol();
        }
    }

    public void Initialize(Transform newPlayer, Transform[] newPatrolPoints)
    {
        player = newPlayer;
        patrolPoints = newPatrolPoints;
        currentPatrolIndex = 0;

        if (player != null)
        {
            playerRobotController = player.GetComponentInParent<RobotController>();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null)
            return;

        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    private void StartWalkSound()
    {
        if (isDead)
            return;

        if (walkAudioSource == null)
            return;

        if (enemyWalkSound == null)
            return;

        if (walkAudioSource.isPlaying)
            return;

        walkAudioSource.clip = enemyWalkSound;
        walkAudioSource.loop = true;
        walkAudioSource.Play();
    }

    private void StopWalkSound()
    {
        if (walkAudioSource == null)
            return;

        walkAudioSource.Stop();
        walkAudioSource.loop = false;
    }
    
    private IEnumerator PlayEnemyDetectedSound()
    {
        if (isDead)
            yield break;

        PlaySound(enemyDetectedSound);
    }

    private IEnumerator PlayReloadSoundAfterDelay()
    {
        yield return new WaitForSeconds(reloadSoundDelay);

        if (isDead)
            yield break;

        PlaySound(reloadSound);
    }


    private void Patrol()
    {
        SetShooting(false);

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            SetRunning(false);
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        if (targetPoint == null)
        {
            SetRunning(false);
            return;
        }

        SetRunning(true);
        StartWalkSound();

        Vector3 targetPosition = targetPoint.position;
        targetPosition.y = transform.position.y;

        RotateTowards(targetPosition);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance <= reachDistance)
        {
            currentPatrolIndex++;

            if (currentPatrolIndex >= patrolPoints.Length)
                currentPatrolIndex = 0;
        }
    }

    private void AttackPlayer()
    {
        SetRunning(false);
        SetShooting(true);

        StopWalkSound();

        LookAtPlayer();
        ShootAtPlayer();
    }

    private bool CanSeePlayer()
    {
        Vector3 origin;

        if (eyePoint != null)
            origin = eyePoint.position;
        else
            origin = transform.position + Vector3.up * 1.5f;

        Vector3 target = player.position + Vector3.up * aimHeight;
        Vector3 directionToPlayer = target - origin;

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > visionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > visionAngle * 0.5f)
            return false;

        if (Physics.Raycast(
            origin,
            directionToPlayer.normalized,
            out RaycastHit hit,
            visionRange,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            bool hitPlayer =
                hit.collider.CompareTag("Player") ||
                hit.collider.transform.root.CompareTag("Player");

            return hitPlayer;
        }

        return false;
    }

    private void LookAtPlayer()
    {
        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y;

        RotateTowards(targetPosition);
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void ShootAtPlayer()
{
    if (Time.time < nextFireTime)
        return;

    if (laserPrefab == null)
    {
        Debug.LogError("Laser prefab non assegnato su " + gameObject.name);
        return;
    }

    if (bulletSpawnPoint == null)
    {
        Debug.LogError("Bullet Spawn Point non assegnato su " + gameObject.name);
        return;
    }

    Vector3 target = player.position + Vector3.up * aimHeight;
    Vector3 shootDirection = target - bulletSpawnPoint.position;

    if (shootDirection.sqrMagnitude < 0.001f)
        return;

    Quaternion shootRotation = Quaternion.LookRotation(shootDirection.normalized);

    Instantiate(
        laserPrefab,
        bulletSpawnPoint.position,
        shootRotation
    );

    // Suono dello sparo
    PlaySound(shootSound);

    // Suono della ricarica dopo un piccolo delay
    StartCoroutine(PlayReloadSoundAfterDelay());

    nextFireTime = Time.time + fireRate;
}

    public void PrendiDanno(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log("[EnemyRobotAI] Danno ricevuto: " + damage + " | HP rimasti: " + currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
        }
    }

    private IEnumerator DieRoutine()
{
    isDead = true;
    
    SetShooting(false);
    SetRunning(false);

    StopWalkSound();
    PlaySound(deathSound);

    if (animator != null)
    {
        animator.SetBool(PARAM_IS_SHOOTING, false);
        animator.SetTrigger(TRIGGER_DIE);
    }

    Collider[] colliders = GetComponentsInChildren<Collider>();

    foreach (Collider col in colliders)
    {
        col.enabled = false;
    }

    // Aspetta la durata dell'animazione Die
    yield return new WaitForSeconds(deathAnimationDuration);

    // Aspetta ancora un delay extra dopo l'animazione
    yield return new WaitForSeconds(delayAfterDeathAnimation);

    Destroy(gameObject);
}

    private void SetRunning(bool running)
    {
        if (animator != null)
            animator.SetBool(PARAM_IS_RUNNING, running);
    }

    private void SetShooting(bool shooting)
    {
        if (animator != null)
            animator.SetBool(PARAM_IS_SHOOTING, shooting);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin;

        if (eyePoint != null)
            origin = eyePoint.position;
        else
            origin = transform.position + Vector3.up * 1.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, visionRange);

        Vector3 leftDirection = Quaternion.Euler(0f, -visionAngle * 0.5f, 0f) * transform.forward;
        Vector3 rightDirection = Quaternion.Euler(0f, visionAngle * 0.5f, 0f) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, leftDirection * visionRange);
        Gizmos.DrawRay(origin, rightDirection * visionRange);

        if (patrolPoints != null)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null)
                    continue;

                Gizmos.DrawSphere(patrolPoints[i].position, 0.25f);

                int nextIndex = i + 1;

                if (nextIndex >= patrolPoints.Length)
                    nextIndex = 0;

                if (patrolPoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(
                        patrolPoints[i].position,
                        patrolPoints[nextIndex].position
                    );
                }
            }
        }
    }
}