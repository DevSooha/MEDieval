using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;

    // ★ 추가: 탄막이 전달할 특수 효과 정보
    private PotionEffect potionEffect;
    private float effectDuration;

    private bool isActive = false;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!isActive) return;
        if (rb != null) rb.linearVelocity = direction * speed;
    }

    // ★ 초기화 함수에 효과(Effect)와 지속시간(Time) 인자를 추가합니다.
    public void Initialize(Vector2 dir, float spd, int dmg, PotionEffect effect, float duration)
    {
        this.direction = dir.normalized;
        this.speed = spd;
        this.damage = dmg;
        this.potionEffect = effect;
        this.effectDuration = duration; // PotionData나 PatternData에서 받아온 시간

        isActive = true;

        // 탄막 진행 방향으로 회전 (시각적 디테일)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        // 1. 적과 충돌 시: 데미지 + 디버프
        if (collision.CompareTag("Enemy"))
        {
            EnemyCombat enemy = collision.GetComponent<EnemyCombat>();
            if (enemy != null)
            {
                // 데미지 적용
                enemy.EnemyTakeDamage(damage);

                // ★ 효과에 따른 디버프 적용
                if (potionEffect == PotionEffect.EnemySpeed2X) // 예: 적 속도 변환 (기획서에 따라 느려지게 할 수도 있음)
                {
                    // EnemyCombat이나 EnemyMovement에 해당 함수를 구현해야 합니다.
                    // enemy.ApplySpeedDebuff(0.5f, effectDuration); 
                    Debug.Log("적 속도 변경 디버프 적용!");
                }
                else if (potionEffect == PotionEffect.EnemyStun)
                {
                    // enemy.ApplyStun(effectDuration);
                    Debug.Log("적 스턴 적용!");
                }
            }
            Destroy(gameObject);
        }

        // 2. 플레이어와 충돌 시: 데미지(리스크) + 버프(리턴)
        else if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            Player playerMove = collision.GetComponent<Player>();

            if (playerHealth != null)
            {
                // 힐 탄막이 아니면 아군 오폭 데미지 (기획서: 플레이어도 데미지 입음)
                if (potionEffect == PotionEffect.Heal)
                    playerHealth.Heal(1); // 힐인 경우 회복
                else
                    playerHealth.TakeDamage(1); // 그 외엔 약한 데미지 (리스크)
            }

            // ★ 버프 적용
            if (playerMove != null)
            {
                if (potionEffect == PotionEffect.PlayerSpeed2X)
                {
                    playerMove.ApplySpeedBuff(effectDuration);
                    Debug.Log("플레이어 속도 증가 버프 획득!");
                }
                // 다른 버프 처리...
            }

            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall")) // 벽에 닿으면 삭제
        {
            Destroy(gameObject);
        }
    }
}