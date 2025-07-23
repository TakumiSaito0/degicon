using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI操作用

public class LifeManager : MonoBehaviour
{
    public int health = 3; // ハートは3つ
    public int maxHealth = 3; // 最大体力

    // ハート画像の配列（Inspectorで設定）
    public Image[] heartImages;

    [SerializeField] private GameObject gameOverPanel; // InspectorでゲームオーバーUIをセット
   
    [SerializeField] private Light mainLight;

    // 通常のハート画像（Inspectorで設定）
    public Sprite heartFullSprite;

    // ダメージ時のハート画像（Inspectorで設定）
    public Sprite heartEmptySprite;

    // シングルトンインスタンス
    public static LifeManager instance;

    private float originalIntensity;

    void Awake()
    {
        // シングルトンパターンの実装
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateHearts();
        if (mainLight != null) originalIntensity = mainLight.intensity;
    }

    public void TakeDamage(int damage = 1)
    {
        if (health <= 0) return; // 既に死亡している場合は何もしない

        health -= damage;
        health = Mathf.Max(health, 0);

        UpdateHearts();

        Debug.Log($"ダメージを受けました。現在の体力: {health}");

        // 体力が0になった場合の処理
        if (health <= 0)
        {
            OnPlayerDeath();
        }
    }

    public void Heal(int healAmount = 1)
    {
        health += healAmount;
        health = Mathf.Min(health, maxHealth);
        UpdateHearts();
        Debug.Log($"回復しました。現在の体力: {health}");
    }

    void OnPlayerDeath()
    {
        Debug.Log("プレイヤーが死亡しました");
        // ここにゲームオーバー処理を追加
       
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // ゲームオーバーUIを表示
        }
        if (mainLight != null) mainLight.intensity = 0.3f; // 暗くする
        Time.timeScale = 0f; // ゲームを停止
    }
    // リトライボタンから呼び出す
    public void OnRetryButton()
    {
        if (mainLight != null) mainLight.intensity = originalIntensity; // 元に戻す
        Time.timeScale = 1f; // 時間を元に戻す
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ハート画像の表示更新
    void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < health)
            {
                heartImages[i].sprite = heartFullSprite;
            }
            else
            {
                heartImages[i].sprite = heartEmptySprite;
            }
        }
    }

    // 現在の体力を取得
    public int GetHealth()
    {
        return health;
    }

    // 体力が満タンかチェック
    public bool IsFullHealth()
    {
        return health >= maxHealth;
    }

    // 死亡しているかチェック
    public bool IsDead()
    {
        return health <= 0;
    }
}