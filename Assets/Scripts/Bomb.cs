using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Potion Data")]
    // PlayerAttackSystem에서 주입받을 포션 정보 (공격력, 속성 등)
    private PotionData potionData;

    [Header("Explosion Settings")]
    public float timeToExplode = 2.0f;       // 폭발까지 걸리는 시간
    public float envDestructionRadius = 1.5f; // 풀/장애물 제거 범위

    [Header("Prefabs (Explosion Generation)")]
    // 실제 데미지와 탄막을 담당할 관리자 프리팹
    public GameObject potionExplosionPrefab;

    // 탄막 패턴 생성에 필요한 프리팹들
    public GameObject bulletPatternPrefab;
    public GameObject bulletPrefabBlue;
    public GameObject bulletPrefabRed;
    public GameObject bulletPrefabGreen;

    // ★ 초기화 함수: PlayerAttackSystem에서 이 함수를 호출해 데이터를 넣어줍니다.
    public void Initialize(PotionData data)
    {
        this.potionData = data;
    }

    void Start()
    {
        StartCoroutine(ExplodeSequence());
    }

    IEnumerator ExplodeSequence()
    {
        yield return new WaitForSeconds(timeToExplode);
        Explode();
    }

    void Explode()
    {
        // 1. 탄막 패턴 관리자(PotionExplosion) 생성
        // 폭탄 본체는 터지면서 사라지고, 데미지 처리를 담당할 'Explosion'을 소환합니다.
        if (potionData != null && potionExplosionPrefab != null)
        {
            GameObject explosionObj = Instantiate(potionExplosionPrefab, transform.position, Quaternion.identity);
            PotionExplosion explosionScript = explosionObj.GetComponent<PotionExplosion>();

            if (explosionScript != null)
            {
                // 주입받은 포션 데이터와 탄알 프리팹들을 넘겨줘서 탄막을 시작시킴
                explosionScript.Initialize(
                    potionData,
                    bulletPatternPrefab,
                    bulletPrefabBlue,
                    bulletPrefabRed,
                    bulletPrefabGreen,
                    transform.position
                );
            }
        }
        else
        {
            // 데이터가 없을 경우(단순 테스트 등) 경고 로그
            if (potionData == null) Debug.LogWarning("Bomb: PotionData가 주입되지 않았습니다.");
            if (potionExplosionPrefab == null) Debug.LogError("Bomb: Explosion 프리팹이 연결되지 않았습니다.");
        }

        // 2. 환경 오브젝트(풀 등) 파괴
        // 탄막과 별개로 폭발 충격파가 주변 사물을 부수는 연출
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, envDestructionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Grass"))
            {
                Destroy(hit.gameObject);
            }
        }

        // 3. 폭탄 본체 삭제
        Destroy(gameObject);
    }
}