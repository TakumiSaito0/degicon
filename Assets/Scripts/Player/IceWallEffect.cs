using UnityEngine;

public class IceWallEffect : MonoBehaviour
{
    [Header("氷壁のダメージ量")]
    public int damage = 1;
    [Header("氷壁の持続時間（秒）")]
    public float duration = 2f;
    private float activeTime = 0.2f; // 最初の0.2秒だけダメージ判定
    private float timer = 0f;
    private bool canDamage = true;

    void Start()
    {
        activeTime = Mathf.Min(0.2f, duration); // 壁の持続時間より短く
        timer = 0f;
        canDamage = true;
        Destroy(gameObject, duration); // 自動消滅
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (canDamage && timer > activeTime)
        {
            canDamage = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canDamage) return;
        var enemy = other.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"IceWallが{enemy.gameObject.name}に{damage}ダメージ");
        }
    }
}
