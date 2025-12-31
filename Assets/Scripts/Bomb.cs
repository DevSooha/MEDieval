using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Potion Data")]
    // 던질 때 어떤 포션인지 주입받을 변수
    private PotionData potionData;

    [Header("Explosion Settings")]
    public float timeToExplode = 2.0f;
    public float envDestructionRadius = 1.5f; // 풀 베기용 범위

    [Header("Prefabs (Explosion Generation)")]
    public GameObject potionExplosionPrefab; // PotionExplosion 스크립트가 붙은 프리팹
    public GameObject bulletPatternPrefab;   // 패턴 생성기 프리팹
    public GameObject bulletPrefabBlue;
    public GameObject bulletPrefabRed;
    public GameObject bulletPrefabGreen;

    // ★ 생성자 역할: 폭탄을 던질 때 데이터를 넣어주는 함수
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
        // 1. 탄막 패턴 관리자(Explosion) 생성
        if (potionData != null && potionExplosionPrefab != null)
        {
            GameObject explosionObj = Instantiate(potionExplosionPrefab, transform.position, Quaternion.identity);
            PotionExplosion explosionScript = explosionObj.GetComponent<PotionExplosion>();

            if (explosionScript != null)
            {
                // 포션 데이터와 탄알 프리팹들을 넘겨줘서 탄막을 시작시킴
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
            Debug.LogError("Bomb: PotionData가 없거나 Explosion 프리팹이 연결되지 않았습니다.");
        }

        // 2. (옵션) 환경 오브젝트(풀 등) 파괴는 여전히 폭발 즉시 처리
        // 탄막은 적을 공격하고, 폭발 충격파는 풀을 벤다는 느낌
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, envDestructionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Grass"))
            {
                Destroy(hit.gameObject);
            }
            // 주의: 적(Boss/Enemy)에게 주는 데미지는 여기서 주지 않습니다. (탄막이 줄 것임)
        }

        // 3. 폭탄 본체 삭제
        Destroy(gameObject);
    }
}