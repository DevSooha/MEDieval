using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Combat")]
    public int enemyHP = 3;
    public int damageAmount = 1;

    [Header("Drop")]
    public GameObject worldItemPrefab;
    //public ItemData pastelbloomItemData;
    public int dropAmount = 1;
    public float dropChance = 1f;

    private bool isDead = false;

    public void EnemyTakeDamage(int amount)
    {
        // ★ 추가: 이미 죽은 상태면 아무것도 하지 마라!
        if (isDead) return;

        enemyHP -= amount;

        if (enemyHP <= 0)
        {
            Die(); // 죽는 로직을 따로 분리하면 깔끔합니다
        }
    }

    void Die()
    {
        // ★ 핵심: 들어오자마자 "나 죽었다" 표시
        isDead = true;

        DropItem();
        Destroy(gameObject);
    }

    void DropItem()
    {
        if (Random.value > dropChance) return;

        Vector3 dropPos =
            transform.position + (Vector3)Random.insideUnitCircle.normalized * 0.5f;

        GameObject item = Instantiate(
            worldItemPrefab,
            dropPos,
            Quaternion.identity
        );

        WorldItem wi = item.GetComponent<WorldItem>();

        // WorldItem이 확실히 있는지 체크 (안전장치)
        //if (wi != null)
        //{
        //    wi.Init(pastelbloomItemData, dropAmount);

        //    SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        //    if (sr != null)
        //    {
        //        sr.sprite = pastelbloomItemData.icon;
        //        sr.sortingLayerName = "Item";
        //    }
        //}
    }

}
