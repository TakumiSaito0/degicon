using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("HP設定")]
    public int hp = 3;

    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name}に{damage}ダメージ。残りHP: {hp}");
        if (hp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name}撃破");
        Destroy(gameObject);
    }
}
