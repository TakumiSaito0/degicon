using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int damage = 1;
    private Transform player;
    private int health = 2; // 体力2

    void Start()
    {
        // Playerタグのオブジェクトを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // Playerが見つかっていれば追いかける
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // 地面に沿って移動
            transform.position += direction * moveSpeed * Time.deltaTime;
            // 進行方向に体を向ける
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
            // PlayerのHPを参照してダメージを与える
            var playerScript = collision.collider.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log($"EnemyがPlayerにダメージ: {damage}");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"Enemyの体力: {health}");
        if (health <= 0)
        {
            // 体力が0になった場合の処理
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemyが消滅しました");
        // 死亡エフェクトやスコア加算などの処理をここに追加可能
        Destroy(gameObject);
    }
}