using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 10;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

private void OnTriggerEnter(Collider other)
{
    if (other.isTrigger)
        return;

    if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        return;

    bool isNemico =
        other.CompareTag("Nemico") ||
        other.transform.root.CompareTag("Nemico");

    if (isNemico)
    {
        EnemyRobotAI enemy = other.GetComponentInParent<EnemyRobotAI>();

        if (enemy != null)
        {
            enemy.PrendiDanno(damage);
        }

        Destroy(gameObject);
        return;
    }

    Destroy(gameObject);
}

    private void Awake()
{
    Debug.Log("[PlayerProjectile] Creato in scena: " + gameObject.name);
}
}