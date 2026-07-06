using System.Collections;
using UnityEngine;

[System.Serializable]
public class PatrolZone
{
    public string zoneName;
    public Transform spawnPoint;
    public Transform[] patrolPoints;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private bool spawnOnlyOnce = true;
    [SerializeField] private float spawnDelay = 1f;

    [Header("Enemy Setup")]
    [SerializeField] private Transform player;
    [SerializeField] private PatrolZone[] patrolZones;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private float volumeSpawnSound = 1f;

    private bool hasSpawned = false;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.CompareTag("Player") || other.transform.root.CompareTag("Player");

        if (!isPlayer)
            return;

        if (spawnOnlyOnce && hasSpawned)
            return;

        hasSpawned = true;
        PlaySpawnSound();
        StartCoroutine(SpawnEnemiesWithDelay());
    }

    private IEnumerator SpawnEnemiesWithDelay()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab non assegnato.");
            yield break;
        }

        if (patrolZones == null || patrolZones.Length == 0)
        {
            Debug.LogError("Nessuna Patrol Zone assegnata.");
            yield break;
        }

        for (int i = 0; i < patrolZones.Length; i++)
        {
            PatrolZone selectedZone = patrolZones[i];

            if (selectedZone.spawnPoint == null)
            {
                Debug.LogError("SpawnPoint mancante nella zona: " + selectedZone.zoneName);
                continue;
            }

            yield return new WaitForSeconds(spawnDelay);
            
            GameObject enemy = Instantiate(
                enemyPrefab,
                selectedZone.spawnPoint.position,
                selectedZone.spawnPoint.rotation
            );

            EnemyRobotAI enemyAI = enemy.GetComponent<EnemyRobotAI>();

            if (enemyAI != null)
            {
                enemyAI.Initialize(player, selectedZone.patrolPoints);
            }
            else
            {
                Debug.LogWarning("Il prefab nemico non ha EnemyRobotAI.cs.");
            }

            Debug.Log("Robot spawnato nella zona: " + selectedZone.zoneName);

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void PlaySpawnSound()
{
    if (spawnSound == null)
    {
        Debug.LogWarning("[EnemySpawner] Spawn sound non assegnato.");
        return;
    }

    if (audioSource != null)
    {
        audioSource.PlayOneShot(spawnSound, volumeSpawnSound);
    }
    else
    {
        AudioSource.PlayClipAtPoint(spawnSound, transform.position, volumeSpawnSound);
    }

    // Effetto luce rossa globale quando parte la sirena
    if (GlobalAlarmLightEffect.Instance != null)
    {
        GlobalAlarmLightEffect.Instance.PlayAlarm();
    }
}
}