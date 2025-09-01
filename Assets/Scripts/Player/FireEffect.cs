using UnityEngine;

public class FireEffect : MonoBehaviour
{
    public float speed = 5f;
    public float lifeTime = 1f;

    private Rigidbody rb;

 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            float direction = transform.rotation.eulerAngles.y == 0 ? 1f : -1f;
            rb.linearVelocity = new Vector3(direction * speed, 0, 0);
        }
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var baseEnemy = other.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(1); // 敵に1ダメージ
                Debug.Log("ファイアーがBaseEnemyに1ダメージを与えました");
            }
            Destroy(gameObject);
        }
    }
}