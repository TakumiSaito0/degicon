using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("HP�ݒ�")]
    public int hp = 3;

    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name}��{damage}�_���[�W�B�c��HP: {hp}");
        if (hp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name}���j");
        Destroy(gameObject);
    }
}
