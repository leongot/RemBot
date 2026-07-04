using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 35f;
    [SerializeField] private float lifeTime = 2f;

    [Header("Visual")]
    [SerializeField] private Transform laserBody;
    [SerializeField] private float laserLength = 2f;
    [SerializeField] private float laserRadius = 0.08f;

    [Header("Damage")]
    [SerializeField] private int damage = 10;

    private void Awake()
    {
        SetupVisual();
        SetupCollider();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void SetupVisual()
    {
        if (laserBody == null)
            return;

        // Il Cylinder di Unity è lungo sull'asse Y.
        // Lo ruotiamo per farlo puntare sull'asse Z del proiettile.
        laserBody.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Il cilindro di Unity è alto circa 2 unità,
        // quindi usiamo laserLength * 0.5f per ottenere la lunghezza desiderata.
        laserBody.localScale = new Vector3(
            laserRadius,
            laserLength * 0.5f,
            laserRadius
        );

        laserBody.localPosition = Vector3.zero;
    }

    private void SetupCollider()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();

        if (capsule == null)
            return;

        capsule.isTrigger = true;

        // 0 = X, 1 = Y, 2 = Z
        capsule.direction = 2;

        capsule.radius = laserRadius;
        capsule.height = laserLength;
        capsule.center = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        // Ignora i nemici, così il laser del nemico non colpisce il nemico stesso
        if (other.CompareTag("Nemico") || other.transform.root.CompareTag("Nemico") ||
            other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy"))
        {
            return;
        }

        RobotController robot = other.GetComponentInParent<RobotController>();

        if (robot != null)
        {
            robot.ColpitoDaLaser(damage);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}   