using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    [Header("Combat")]
    public int enemyHP = 3;
    public int damageAmount = 1; // 플레이어와 충돌 시 줄 데미지 (충돌 처리에서 사용)

    [Header("Drop")]
    public GameObject worldItemPrefab;
    public ItemData pastelbloomItemData;
    public int dropAmount = 1;
    public float dropChance = 1f;

    [Header("Feedback")]
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private bool isDead = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void EnemyTakeDamage(int amount)
    {
        if (isDead) return;

        enemyHP -= amount;

        if (spriteRenderer != null)
        {
            StopCoroutine("FlashColor");
            StartCoroutine(FlashColor());
        }

        if (enemyHP <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashColor()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        DropItem();
        Destroy(gameObject);
    }

    void DropItem()
    {
        if (Random.value > dropChance) return;

        Vector3 dropPos = transform.position + (Vector3)Random.insideUnitCircle.normalized * 0.5f;

        GameObject item = Instantiate(
            worldItemPrefab,
            dropPos,
            Quaternion.identity
        );

        WorldItem wi = item.GetComponent<WorldItem>();

        if (wi != null)
        {
            wi.Init(pastelbloomItemData, dropAmount);

            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = pastelbloomItemData.icon;
                sr.sortingLayerName = "Item";
            }
        }
    }
}