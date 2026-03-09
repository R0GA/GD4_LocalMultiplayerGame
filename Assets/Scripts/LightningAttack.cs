using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningAttack : MonoBehaviour
{
    [Header("Auto-Aim Settings")]
    [SerializeField] private float aimRange = 12f;
    [SerializeField] private float aimFOV = 30f;

    [Header("Chain Settings")]
    [SerializeField] private float chainRadius = 5f;
    [SerializeField] private float chainDamageMultiplier = 0.6f;

    [Header("Damage")]
    [SerializeField] private float primaryDamage = 30f;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 0.4f;

    [Header("Visual")]
    [SerializeField] private LineRenderer lightningRenderer;
    [SerializeField] private int lightningSegments = 12;
    [SerializeField] private float lightningJitter = 0.4f; 
    [SerializeField] private float lightningDisplayTime = 0.15f;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private LineRenderer chainLightningRenderer;

    private float lastFireTime = -999f;
    private Coroutine hideCoroutine;
    private Coroutine hideChainCoroutine;

    private void Awake()
    {
        if (lightningRenderer != null)
            lightningRenderer.enabled = false;

        if (chainLightningRenderer != null)
            chainLightningRenderer.enabled = false;
    }


    public void Fire(Vector3 origin, Vector3 aimDir)
    {
        if (Time.time < lastFireTime + cooldown) return;

        CreatureController primaryTarget = FindTargetInCone(origin, aimDir);
        if (primaryTarget == null) return;

        lastFireTime = Time.time;

        primaryTarget.TakeDamage(primaryDamage);
        DrawLightning(lightningRenderer, origin, primaryTarget.transform.position);

        CreatureController chainTarget = FindChainTarget(primaryTarget);
        if (chainTarget != null)
        {
            chainTarget.TakeDamage(primaryDamage * chainDamageMultiplier);
            DrawLightning(chainLightningRenderer, primaryTarget.transform.position, chainTarget.transform.position);

            if (hideChainCoroutine != null) StopCoroutine(hideChainCoroutine);
            hideChainCoroutine = StartCoroutine(HideRendererAfterDelay(chainLightningRenderer, lightningDisplayTime));
        }
        else if (chainLightningRenderer != null)
        {
            chainLightningRenderer.enabled = false;
        }

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideRendererAfterDelay(lightningRenderer, lightningDisplayTime));
    }


    private CreatureController FindTargetInCone(Vector3 origin, Vector3 aimDir)
    {
        Collider[] candidates = Physics.OverlapSphere(origin, aimRange, enemyLayerMask);

        CreatureController best = null;
        float bestDist = Mathf.Infinity;

        foreach (Collider col in candidates)
        {
            CreatureController creature = col.GetComponent<CreatureController>();
            if (creature == null) continue;

            Vector3 toEnemy = (col.transform.position - origin);
            toEnemy.y = 0f;
            float angle = Vector3.Angle(aimDir, toEnemy.normalized);

            if (angle <= aimFOV)
            {
                float dist = toEnemy.magnitude;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = creature;
                }
            }
        }

        return best;
    }

    private CreatureController FindChainTarget(CreatureController primaryTarget)
    {
        Vector3 primaryPos = primaryTarget.transform.position;
        Collider[] candidates = Physics.OverlapSphere(primaryPos, chainRadius, enemyLayerMask);

        CreatureController best = null;
        float bestDist = Mathf.Infinity;

        foreach (Collider col in candidates)
        {
            CreatureController creature = col.GetComponent<CreatureController>();
            if (creature == null || creature == primaryTarget) continue;

            float dist = Vector3.Distance(primaryPos, col.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = creature;
            }
        }

        return best;
    }


    /*
    Title: Lightning Visualization
    Author: Claude AI
    Date: 3/9/2026
    Code version: 1
    Availability: N/A (Claude Generated)
    Note: Specifically the following lightning method and Gizmo Draws
    */

    // ---- Lightning Visualization ----

    /// <summary>
    /// Draws a jagged lightning bolt between two world positions using a LineRenderer.
    /// </summary>
    private void DrawLightning(LineRenderer lr, Vector3 start, Vector3 end)
    {
        if (lr == null) return;

        lr.enabled = true;
        lr.positionCount = lightningSegments + 1;

        Vector3 direction = end - start;
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.up);

        for (int i = 0; i <= lightningSegments; i++)
        {
            float t = (float)i / lightningSegments;
            Vector3 point = Vector3.Lerp(start, end, t);

            // Add random jitter perpendicular to the bolt (skip first and last points)
            if (i > 0 && i < lightningSegments)
            {
                float jitter = Random.Range(-lightningJitter, lightningJitter);
                // Jitter in both perpendicular axes for 3D look
                Vector3 perp2 = Vector3.Cross(direction.normalized, perpendicular);
                point += perpendicular * jitter + perp2 * Random.Range(-lightningJitter * 0.5f, lightningJitter * 0.5f);
            }

            lr.SetPosition(i, point);
        }
    }

    private IEnumerator HideRendererAfterDelay(LineRenderer lr, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (lr != null) lr.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the aim cone in the editor
        Gizmos.color = new Color(0.4f, 0.6f, 1f, 0.3f);
        Vector3 forward = transform.forward;
        float halfFOVRad = aimFOV * Mathf.Deg2Rad;

        int steps = 20;
        Vector3 prev = transform.position + Quaternion.AngleAxis(-aimFOV, Vector3.up) * forward * aimRange;
        for (int i = 1; i <= steps; i++)
        {
            float angle = Mathf.Lerp(-aimFOV, aimFOV, i / (float)steps);
            Vector3 next = transform.position + Quaternion.AngleAxis(angle, Vector3.up) * forward * aimRange;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(-aimFOV, Vector3.up) * forward * aimRange);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(aimFOV, Vector3.up) * forward * aimRange);

        // Chain radius (shown as a separate sphere — position this in play mode)
        Gizmos.color = new Color(0.4f, 0.6f, 1f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, chainRadius);
    }
}