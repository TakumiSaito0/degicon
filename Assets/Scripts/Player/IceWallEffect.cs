using UnityEngine;

public class IceWallEffect : MonoBehaviour
{
    [Header("氷壁のダメージ量")]
    public int damage = 1;
    [Header("氷壁の持続時間（秒）")]
    public float duration = 2f;
    [Header("せり上がり演出の高さ（Y座標）")]
    public float riseHeight = 1.0f;
    [Header("せり上がり演出の時間（秒）")]
    public float riseTime = 0.3f;
    private float activeTime = 0.2f; // 最初の0.2秒だけダメージ判定
    private float timer = 0f;
    private bool canDamage = true;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float riseTimer = 0f;
    private bool rising = true;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + new Vector3(0, riseHeight, 0);
        transform.position = startPos;
        riseTimer = 0f;
        rising = true;
        activeTime = Mathf.Min(0.2f, duration);
        timer = 0f;
        canDamage = true;
        Destroy(gameObject, duration);
    }

    void Update()
    {
        // せり上がり演出
        if (rising)
        {
            riseTimer += Time.deltaTime;
            float t = Mathf.Clamp01(riseTimer / riseTime);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            if (t >= 1f)
            {
                rising = false;
            }
        }
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
