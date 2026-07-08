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

    [Header("Audio Impatto")]
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float impactVolume = 1f;

private bool haGiaColpito = false;

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

        
        laserBody.localRotation = Quaternion.Euler(90f, 0f, 0f);

      
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

       
        capsule.direction = 2;

        capsule.radius = laserRadius;
        capsule.height = laserLength;
        capsule.center = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

      
        if (other.CompareTag("Nemico") || other.transform.root.CompareTag("Nemico") ||
            other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy"))
        {
            return;
        }

        RobotController robot = other.GetComponentInParent<RobotController>();

        if (robot != null)
        {
            RiproduciSuonoImpatto();
            robot.ColpitoDaLaser(damage);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    private void RiproduciSuonoImpatto()
    {
        if (haGiaColpito)
            return;

        haGiaColpito = true;

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(
                impactSound,
                transform.position,
                impactVolume
            );
        }
    }
}   