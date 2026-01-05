using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Invincibility (무적)")]
    public float iframeDuration = 1.0f;
    public int numberOfFlashes = 3;
    private bool isInvincible = false;

    [Header("References")]
    private SpriteRenderer spriteRenderer;
    public TMP_Text healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"플레이어 피격! 남은 체력: {currentHealth}");

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        Debug.Log($"플레이어 회복! 현재 체력: {currentHealth}");

        UpdateHealthBar();
    }

    void Die()
    {
        Debug.Log("플레이어 사망...");
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.text = "HP: " + currentHealth + " / " + maxHealth;
        }
        else
        {
            Debug.LogWarning("HealthBar가 연결되지 않았습니다! 인스펙터를 확인하세요.");
        }
    }
}