using UnityEngine;

public class EnemyRobotAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject laserPrefab;

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

    private int currentPatrolIndex = 0;
    private float nextFireTime = 0f;

    private void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            Patrol();
            return;
        }

        if (CanSeePlayer())
        {
            AttackPlayer();
        }
        else
        {
            Patrol();
        }
    }

    public void Initialize(Transform newPlayer, Transform[] newPatrolPoints)
    {
        player = newPlayer;
        patrolPoints = newPatrolPoints;
        currentPatrolIndex = 0;
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        if (targetPoint == null)
            return;

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
            {
                currentPatrolIndex = 0;
            }
        }
    }

    private void AttackPlayer()
    {
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

        nextFireTime = Time.time + fireRate;
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
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                }
            }
        }
    }
}