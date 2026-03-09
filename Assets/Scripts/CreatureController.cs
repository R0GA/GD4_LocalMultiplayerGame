using Mono.Cecil.Cil;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static Unity.VisualScripting.Member;

/*
Title: Creature Controller
Author: Davies, R.
Date: 3/9/2026
Code version: 1
Availability: N/A (Previously Submitted Project - 3rd year final)
*/

public class CreatureController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private bool isMelee = true;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float rangedOptimalDistance = 5f;
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("Combat Settings")]
    [SerializeField] private CreatureProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Enemy Stats")]
    [SerializeField] private float health;
    [SerializeField] private float attackDamage;
    [SerializeField] private float speed;
    [SerializeField] private float attackSpeed;

    [Header("Death VFX")]
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private float deathVFXDuration = 2f;

    [SerializeField] private Animator animator;
    private bool isAttacking = false;
    private NavMeshAgent navMeshAgent;
    private Transform currentTarget;
    private bool isInCombat = false;
    private float lastAttackTime = 0f;
    private float lastAnimAttackTime = 0f;
    private bool isDead = false;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        SetupNavMeshAgent();
    }

    private void SetupNavMeshAgent()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = speed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.angularSpeed = 360f;
            navMeshAgent.acceleration = 8f;
        }
    }

    private void Update()
    {
        if (isDead || health <= 0) return;

        FindTarget();
        HandleCombatBehavior();

        if (!isAttacking && navMeshAgent.velocity.magnitude > 0.1f)
        {
            animator.SetInteger("AnimState", 1);
        }
        else if (!isAttacking)
        {
            animator.SetInteger("AnimState", 0);
        }
    }

    private void FindTarget()
    {
        if (currentTarget != null)
        {
            PlayerController targetPlayer = currentTarget.GetComponent<PlayerController>();
            if (targetPlayer == null || targetPlayer.health <= 0)
            {
                currentTarget = null;
                isInCombat = false;
            }
        }

        if (currentTarget == null)
        {
            LayerMask targetLayerMask = LayerMask.GetMask("Player");
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, targetLayerMask);
            float closestDistance = Mathf.Infinity;
            Transform closestTarget = null;

            foreach (var hitCollider in hitColliders)
            {
                PlayerController potentialPlayer = hitCollider.GetComponent<PlayerController>();
                bool isValid = potentialPlayer != null && potentialPlayer.health > 0;

                if (isValid)
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = hitCollider.transform;
                    }
                }
            }

            if (closestTarget != null)
            {
                currentTarget = closestTarget;
                isInCombat = true;
            }
            else
            {
                isInCombat = false;
            }
        }
    }

    private void HandleCombatBehavior()
    {
        if (!isInCombat || currentTarget == null)
        {
            if (navMeshAgent.isActiveAndEnabled)
                navMeshAgent.isStopped = true;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (isMelee)
            HandleMeleeCombat(distanceToTarget);
        else
            HandleRangedCombat(distanceToTarget);
    }

    private void HandleMeleeCombat(float distanceToTarget)
    {
        if (distanceToTarget <= attackRange)
        {
            if (navMeshAgent.isActiveAndEnabled)
                navMeshAgent.isStopped = true;

            TryAttack();
        }
        else
        {
            if (navMeshAgent.isActiveAndEnabled && currentTarget != null)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(currentTarget.position);
            }
        }
    }

    private void HandleRangedCombat(float distanceToTarget)
    {
        if (distanceToTarget <= attackRange && distanceToTarget >= rangedOptimalDistance * 0.8f)
        {
            if (navMeshAgent.isActiveAndEnabled)
                navMeshAgent.isStopped = true;

            TryAttack();
        }
        else if (distanceToTarget < rangedOptimalDistance * 0.8f)
        {
            if (navMeshAgent.isActiveAndEnabled && currentTarget != null)
            {
                navMeshAgent.isStopped = false;
                Vector3 directionAway = (transform.position - currentTarget.position).normalized;
                navMeshAgent.SetDestination(transform.position + directionAway * rangedOptimalDistance);
            }
        }
        else
        {
            if (navMeshAgent.isActiveAndEnabled && currentTarget != null)
            {
                navMeshAgent.isStopped = false;
                Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
                navMeshAgent.SetDestination(currentTarget.position - directionToTarget * rangedOptimalDistance);
            }
        }
    }

    private void TryAttack()
    {
        if (Time.time >= lastAttackTime + 1f / attackSpeed)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void Attack()
    {
        if (isMelee)
            PerformMeleeAttack();
        else
            PerformRangedAttack();
    }

    private void PerformMeleeAttack()
    {
        if (currentTarget == null) return;

        PlayerController targetPlayer = currentTarget.GetComponent<PlayerController>();
        if (targetPlayer != null)
            targetPlayer.TakeDamage(attackDamage);

        StartCoroutine(MeleeAttackAnimation());
    }

    private void PerformRangedAttack()
    {
        if (currentTarget == null) return;

        if (projectilePrefab != null)
        {
            CreatureProjectile projectile = Instantiate(
                projectilePrefab,
                projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position,
                Quaternion.identity
            );
            projectile.Initialize(attackDamage, currentTarget);
            StartCoroutine(MeleeAttackAnimation());
        }
        else
        {
            PlayerController targetPlayer = currentTarget.GetComponent<PlayerController>();
            if (targetPlayer != null)
                targetPlayer.TakeDamage(attackDamage);
        }
    }

    private IEnumerator MeleeAttackAnimation()
    {
        Debug.Log("Melee Attack Triggered");
        if (!isAttacking)
        {
            Debug.Log("Melee Attack Animation Started");
            isAttacking = true;
            animator.SetInteger("AnimState", 2);
            Vector3 originalPosition = transform.position;
            StartCoroutine(ResetAnim());
            yield return null;
        }
    }
    private IEnumerator ResetAnim()
    {
        Debug.Log("ResetAnim Coroutine Started");

        yield return new WaitUntil(() => Time.time >= lastAttackTime + 1f / attackSpeed || Time.time > lastAttackTime + 0.5f);
        Debug.Log("ResetAnim Coroutine Executing");
        animator.SetInteger("AnimState", 0);
        isAttacking = false;
        lastAnimAttackTime = Time.time;  
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        if (health <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        if (deathVFXPrefab != null)
        {
            GameObject vfx = Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, deathVFXDuration);
        }

        Destroy(gameObject, 0.1f);
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
        isInCombat = target != null;
    }

    public bool IsInCombat() => isInCombat;
    public Transform GetCurrentTarget() => currentTarget;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (!isMelee)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, rangedOptimalDistance);
        }

        if (currentTarget != null)
        {
            Gizmos.color = isInCombat ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}