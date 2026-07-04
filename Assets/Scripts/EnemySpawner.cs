using UnityEngine;
using System.Collections;

public class EnemyTriggerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool spawnOnlyOnce = true;
    [SerializeField] private int enemyNumber = 2;
    [SerializeField] private float spacing = 3f;
    [SerializeField] private float spawnDelay = 1f;


    private bool hasSpawned = false;


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (spawnOnlyOnce && hasSpawned)
            return;

        hasSpawned = true;

        StartCoroutine(SpawnEnemiesWithDelay());
    }

   private IEnumerator SpawnEnemiesWithDelay()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab non assegnato.");
            yield break;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        for (int i = 0; i < enemyNumber; i++)
        {
            Vector3 offset = new Vector3(i * spacing, 0f, 0f);

            Instantiate(enemyPrefab, position + offset, rotation);

            Debug.Log("Enemy spawned at: " + (position + offset));

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}