using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 3f;

    [Header("Damage")]
    [SerializeField] private bool distruggiNemico = true;

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

        // Ignora il robottino buono / player
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
            return;

        bool isNemico =
            other.CompareTag("Nemico") ||
            other.transform.root.CompareTag("Nemico");

        if (isNemico)
        {
            Debug.Log("[PlayerProjectile] Nemico colpito: " + other.name);

            if (distruggiNemico)
            {
                Destroy(other.transform.root.gameObject);
            }

            Destroy(gameObject);
            return;
        }

        // Se colpisce muro/ostacolo, il proiettile sparisce
        Destroy(gameObject);
    }

    private void Awake()
{
    Debug.Log("[PlayerProjectile] Creato in scena: " + gameObject.name);
}
}