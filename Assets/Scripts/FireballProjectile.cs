using UnityEngine;
using System.Collections;
public class FireballProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 18f;
    [SerializeField] private float lifetime = 4f;

    [Header("Damage Settings")]
    [SerializeField] private float directDamage = 15f;
    [SerializeField] private float aoeRadius = 2.5f;
    [SerializeField] private float aoeDamage = 25f;
    [SerializeField] private LayerMask enemyLayerMask;

    [Header("VFX")]
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private float explosionVFXDuration = 1.5f;

    private Rigidbody rb;
    private bool hasExploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void Launch(Vector3 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) return;

        CreatureController directHit = other.GetComponent<CreatureController>();
        if (directHit != null)
        {
            Explode(other.transform.position);
        }
        else if (!other.isTrigger)
        {
            Explode(transform.position);
        }
    }

    private void Explode(Vector3 center)
    {
        hasExploded = true;

        if (explosionVFXPrefab != null)
        {
            GameObject vfx = Instantiate(explosionVFXPrefab, center, Quaternion.identity);
            Destroy(vfx, explosionVFXDuration);
        }

        Collider[] hits = Physics.OverlapSphere(center, aoeRadius, enemyLayerMask);
        foreach (Collider col in hits)
        {
            CreatureController creature = col.GetComponent<CreatureController>();
            if (creature != null)
            {
                // Made the damage scale depending how close you are, we could change this :P
                float dist = Vector3.Distance(center, col.transform.position);
                float falloff = 1f - Mathf.Clamp01(dist / aoeRadius); //idk how this math works, i stole from a forum
                float damage = Mathf.Lerp(directDamage, aoeDamage, 1f - falloff) * falloff + directDamage * (1f - falloff);
                creature.TakeDamage(aoeDamage * falloff + directDamage * (dist < 0.1f ? 1f : 0f));
            }
        }

        Destroy(gameObject);
    }

    //gizmos for us to check in the editor 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}