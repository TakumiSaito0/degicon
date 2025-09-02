using UnityEngine;

public class SlimeEnemy : BaseEnemy
{
    public float moveSpeed = 2f;
    public int damage = 1;
    private Transform player;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead) return;
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            transform.position += direction * moveSpeed * Time.deltaTime;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            var playerScript = collision.collider.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log($"EnemyがPlayerにダメージ: {damage}");
                if (animator != null)
                {
                    animator.SetTrigger("Melee Attack"); // MeleeAttackアニメーション再生
                }
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        if (isDead) return;
        hp -= damage;
        if (animator != null)
        {
            animator.SetTrigger("Take Damage"); // TakeDamageアニメーション再生
        }
        if (hp <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        if (animator != null)
        {
            animator.SetTrigger("Die"); // Animatorの"Die"トリガーをセット
        }
        Destroy(gameObject, 1.0f); // アニメーション再生後に消滅（1秒後など調整可）
    }
}