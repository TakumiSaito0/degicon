using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    
    // 無敵時間関連
    public float invincibilityDuration = 1f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    // 点滅エフェクト用
    private Renderer playerRenderer;
    private Color originalColor;

    void Start()
    {
        // プレイヤーのRendererを取得
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log($"MoveInput: {moveInput}");
    }

    void Update()
    {
        // 移動処理
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // 無敵時間の処理
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // 点滅エフェクト
            if (playerRenderer != null)
            {
                float alpha = Mathf.Sin(Time.time * 20f) * 0.5f + 0.5f;
                Color flickerColor = originalColor;
                flickerColor.a = alpha;
                playerRenderer.material.color = flickerColor;
            }

            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                if (playerRenderer != null)
                {
                    playerRenderer.material.color = originalColor;
                }
            }
        }
    }

    // 無敵状態を開始する（外部から呼び出し用）
    public void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    // 無敵状態かどうかを返す
    public bool IsInvincible()
    {
        return isInvincible;
    }

    // 敵との衝突処理
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !isInvincible)
        {
            // ダメージを受ける
            if (LifeManager.instance != null)
            {
                LifeManager.instance.TakeDamage(1);
                
                // 無敵時間を開始
                StartInvincibility();
                
                Debug.Log("敵に攻撃されました！");
            }
        }
    }

    // 別の衝突検出方法（Rigidbodyを使用している場合）
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
        {
            // ダメージを受ける
            if (LifeManager.instance != null)
            {
                LifeManager.instance.TakeDamage(1);
                
                // 無敵時間を開始
                StartInvincibility();
                
                Debug.Log("敵に攻撃されました！");
            }
        }
    }
}