using UnityEngine;

public class CreatureProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private GameObject impactEffect;

    private float damage;
    private Transform target;
    private bool isInitialized = false;

    private void Start()
    {
        Destroy(gameObject, 10f);
    }

    public void Initialize(float projectileDamage, Transform projectileTarget)
    {
        damage = projectileDamage;
        target = projectileTarget;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(target);

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            OnHit(target.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized) return;

        if (other.transform == target)
        {
            OnHit(other.gameObject);
        }
    }

    private void OnHit(GameObject hitObject)
    {
        PlayerController targetPlayer = hitObject.GetComponent<PlayerController>();
        if (targetPlayer != null)
            targetPlayer.TakeDamage(damage);

        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}