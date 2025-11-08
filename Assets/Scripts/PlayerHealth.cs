using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int HP;
    public int maxHP;

    public TMP_Text healthBar;
    void Start()
    {
        HP = maxHP;
    }   
    void Update()
    {
        healthBar.text = "HP: " + HP + " / " + maxHP;
    }
    
    public void ChangeHealth(int amount)
    {
        HP += amount;
        if (HP <= 0)
        {
            Destroy(gameObject);
        }
        healthBar.text = "HP: " + HP + " / " + maxHP;
    }
}
