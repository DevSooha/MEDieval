using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public float attackRange = 0.8f;
    public float knockbackForce = 20f;
    public float stunTime = 0.2f;
    public int damageAmount = 2;
    public float attackCooldown = 2f;  // ✅ public으로 변경
    public float attackDuration = 0.5f;  // ✅ 공격 애니메이션 시간
    
    private float attackCounter = 0f;
    private float attackTimer = 0f;  // ✅ 공격 진행 시간
    private Transform attackPoint;
    private LayerMask playerLayer;
    private bool isAttacking = false;  // ✅ 공격 중인지 확인

    void Start()
    {
        attackPoint = transform.Find("AttackPoint");
        playerLayer = LayerMask.GetMask("Player");
    }

    void Update()
    {
        if (attackCounter > 0)
            attackCounter -= Time.deltaTime;
        
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
        else if (isAttacking)
            isAttacking = false;
    }

    // ✅ 공격 가능 여부 확인
    public bool CanAttack()
    {
        return attackCounter <= 0;
    }

    // ✅ 공격이 완료되었는지 확인
    public bool IsAttackFinished()
    {
        return !isAttacking;
    }

    public void Attack()
    {
        if (attackCounter > 0)
        {
            Debug.Log("Attack on cooldown!");
            return;
        }
        
        isAttacking = true;
        attackTimer = attackDuration;
        attackCounter = attackCooldown;
        
        Debug.Log("Executing attack!");
        
        // ✅ AttackPoint가 없으면 Enemy 위치 사용
        Vector2 attackPos = attackPoint != null ? attackPoint.position : transform.position;
        
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            attackPos, 
            Vector2.right * transform.localScale.x, 
            attackRange, 
            playerLayer
        );
        
        Debug.Log($"Attack hits: {hits.Length}");
        
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                Debug.Log($"Hit: {hit.collider.name}");
                
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    Debug.Log($"Dealing {damageAmount} damage!");
                    playerHealth.ChangeHealth(-damageAmount);
                }
                
                Player playerMovement = hit.collider.GetComponent<Player>();
                if (playerMovement != null)
                {
                    Debug.Log($"Applying knockback! Force: {knockbackForce}");
                    playerMovement.KnockBack(transform, knockbackForce, stunTime);
                }
            }
        }
        else
        {
            Debug.LogWarning("No hits detected!");
        }
    }
    
    // ✅ 디버그용 Gizmo
    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 direction = Vector3.right * transform.localScale.x;
            Gizmos.DrawRay(attackPoint.position, direction * attackRange);
        }
    }
}
