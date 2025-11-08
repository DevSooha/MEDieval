using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyCombat enemyCombat;
    private Transform player;
    private EnemyState enemyState;
    private float facingDirection = -1;
    
    [SerializeField] private Transform detectionPoint;
    public float movespeed = 2f;
    public float attackRange = 1f;
    public float detectRange = 5f;
    public LayerMask playerLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyCombat = GetComponent<EnemyCombat>();
        detectionPoint = transform.Find("DetectionPoint");
        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        CheckForPlayer();

        switch (enemyState)
        {
            case EnemyState.Chasing:
                Chase();
                break;

            case EnemyState.Attacking:
                rb.linearVelocity = Vector2.zero;
                Attack();
                if (enemyCombat != null && enemyCombat.IsAttackFinished())
                {
                    // ✅ CheckForPlayer()가 상태를 다시 결정하도록 함
                    // 여기서는 아무것도 하지 않음
                }
                break;

            default:
                Stop();
                break;
        }
    }
    private void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }
    private void CheckForPlayer()
    {
        if (detectionPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, detectRange, playerLayer);

        if (hits.Length > 0)
        {
            player = hits[0].transform;
            float distance = Vector2.Distance(detectionPoint.position, player.position);

            if (distance <= attackRange && enemyCombat != null && enemyCombat.CanAttack())
            {
                if (enemyState != EnemyState.Attacking || enemyCombat.IsAttackFinished())
                {
                    ChangeState(EnemyState.Attacking);
                }
            }
            else if (distance <= detectRange && distance > attackRange)
            {
                if (enemyState == EnemyState.Attacking && !enemyCombat.IsAttackFinished())
                {
                    //??
                }
                else
                {
                    ChangeState(EnemyState.Chasing);
                }
            }
        }
        else
        {
            // ✅ 감지 범위 밖
            if (enemyState == EnemyState.Attacking && !enemyCombat.IsAttackFinished())
            {
                // 공격 중이면 완료까지 기다림
            }
            else if (enemyState != EnemyState.Idle)
            {
                ChangeState(EnemyState.Idle);
            }
        }
    }

    private void Chase()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > detectRange)
        {
            ChangeState(EnemyState.Idle);
            Stop();
            return;
        }
        
        Vector2 direction = (player.position - transform.position).normalized;

        if ((direction.x < 0 && facingDirection == 1) || (direction.x > 0 && facingDirection == -1))
        {
            FlipX();
        }
        
        rb.linearVelocity = direction * movespeed;
    }

    private void FlipX()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    private void ChangeState(EnemyState newState)
    {
        if (enemyState != newState)
        {
            Debug.Log($"State: {enemyState} → {newState}");
            enemyState = newState;
            
            if (anim != null)
            {
                anim.SetBool("IsMoving", newState == EnemyState.Chasing);
            }
        }
    }

    private void Attack()
    {
        Debug.Log("Enemy attacking!");
        if (enemyCombat != null)
        {
            enemyCombat.Attack();
        }
    }

    private void OnDrawGizmos()
    {
        if (detectionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionPoint.position, detectRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(detectionPoint.position, attackRange);
        }
    }
}

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking,
}
