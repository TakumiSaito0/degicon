using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    [Header("敵の基本設定")]
    public EnemyType enemyType = EnemyType.Chaser;
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public int damage = 1;
    public float attackCooldown = 1f;
    
    [Header("パトロール設定")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;
    
    [Header("追跡設定")]
    public float chaseRange = 7f;
    public float loseTargetRange = 10f;
    
    [Header("体力設定")]
    public int maxHealth = 3;
    public int currentHealth;
    
    [Header("視覚効果")]
    public Color normalColor = Color.white;
    public Color aggressiveColor = Color.red;
    public Color damagedColor = Color.yellow;
    
    // プライベート変数
    private Transform player;
    private Renderer enemyRenderer;
    private Color originalColor;
    
    // 状態管理
    private EnemyState currentState = EnemyState.Patrol;
    private bool canAttack = true;
    private float lastAttackTime;
    
    // パトロール用
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    
    // 移動制御
    private Vector3 targetPosition;
    private bool isMoving = false;

    // 敵の種類
    public enum EnemyType
    {
        Chaser,     // プレイヤーを追跡
        Patrol,     // パトロール
        Stationary  // 静止
    }
    
    // 敵の状態
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Idle,
        Damaged
    }

    void Start()
    {
        // プレイヤーを検索
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Rendererを取得
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
        
        // 体力を初期化
        currentHealth = maxHealth;
        
        // 初期状態を設定
        SetState(EnemyState.Patrol);
        
        // パトロールポイントが設定されていない場合は現在位置を使用
        if (patrolPoints.Length == 0)
        {
            enemyType = EnemyType.Stationary;
        }
        
        Debug.Log($"敵が生成されました。種類: {enemyType}, 体力: {currentHealth}");
    }

    void Update()
    {
        if (player == null) return;
        
        // プレイヤーとの距離を計算
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 状態に応じた行動
        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrolState(distanceToPlayer);
                break;
            case EnemyState.Chase:
                HandleChaseState(distanceToPlayer);
                break;
            case EnemyState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
            case EnemyState.Idle:
                HandleIdleState(distanceToPlayer);
                break;
            case EnemyState.Damaged:
                HandleDamagedState();
                break;
        }
        
        // 移動処理
        if (isMoving)
        {
            MoveToTarget();
        }
    }
    
    void HandlePatrolState(float distanceToPlayer)
    {
        // プレイヤーが検出範囲内に入った場合
        if (distanceToPlayer <= detectionRange && enemyType != EnemyType.Stationary)
        {
            SetState(EnemyState.Chase);
            return;
        }
        
        // パトロール行動
        if (enemyType == EnemyType.Patrol && patrolPoints.Length > 0 && !isWaiting)
        {
            if (!isMoving)
            {
                StartPatrol();
            }
        }
    }
    
    void HandleChaseState(float distanceToPlayer)
    {
        // プレイヤーが攻撃範囲内に入った場合
        if (distanceToPlayer <= 1.5f)
        {
            SetState(EnemyState.Attack);
            return;
        }
        
        // プレイヤーが追跡範囲外に出た場合
        if (distanceToPlayer > loseTargetRange)
        {
            SetState(EnemyState.Patrol);
            return;
        }
        
        // プレイヤーを追跡
        targetPosition = player.position;
        isMoving = true;
    }
    
    void HandleAttackState(float distanceToPlayer)
    {
        // プレイヤーが攻撃範囲外に出た場合
        if (distanceToPlayer > 2f)
        {
            SetState(EnemyState.Chase);
            return;
        }
        
        // 攻撃処理
        if (canAttack)
        {
            AttackPlayer();
        }
    }
    
    void HandleIdleState(float distanceToPlayer)
    {
        // プレイヤーが検出範囲内に入った場合
        if (distanceToPlayer <= detectionRange)
        {
            SetState(EnemyState.Chase);
        }
    }
    
    void HandleDamagedState()
    {
        // ダメージ状態から回復
        if (Time.time - lastAttackTime > 1f)
        {
            SetState(EnemyState.Patrol);
        }
    }
    
    void SetState(EnemyState newState)
    {
        currentState = newState;
        
        // 状態に応じた色変更
        if (enemyRenderer != null)
        {
            switch (newState)
            {
                case EnemyState.Chase:
                case EnemyState.Attack:
                    enemyRenderer.material.color = aggressiveColor;
                    break;
                case EnemyState.Damaged:
                    enemyRenderer.material.color = damagedColor;
                    break;
                default:
                    enemyRenderer.material.color = normalColor;
                    break;
            }
        }
        
        Debug.Log($"敵の状態が変更されました: {newState}");
    }
    
    void StartPatrol()
    {
        if (patrolPoints.Length == 0) return;
        
        targetPosition = patrolPoints[currentPatrolIndex].position;
        isMoving = true;
    }
    
    void MoveToTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (distance < 0.1f)
        {
            isMoving = false;
            
            // パトロール中の場合は次のポイントへ
            if (currentState == EnemyState.Patrol && patrolPoints.Length > 0)
            {
                StartCoroutine(WaitAtPatrolPoint());
            }
        }
        else
        {
            // Y軸の移動を制限（地面に沿って移動）
            direction.y = 0;
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
            
            // プレイヤーの方向を向く
            if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
            {
                Vector3 lookDirection = (player.position - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
        }
    }
    
    IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(patrolWaitTime);
        
        // 次のパトロールポイントへ
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }
    
    void AttackPlayer()
    {
        if (!canAttack) return;
        
        canAttack = false;
        lastAttackTime = Time.time;
        
        // プレイヤーにダメージを与える
        if (player != null)
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                // プレイヤーの無敵時間をチェック
                if (!playerManager.IsInvincible())
                {
                    // LifeManagerを通じてダメージを与える
                    if (LifeManager.instance != null)
                    {
                        LifeManager.instance.TakeDamage(damage);
                        
                        // プレイヤーの無敵時間を開始
                        playerManager.StartInvincibility();
                        
                        Debug.Log($"敵がプレイヤーを攻撃しました！ダメージ: {damage}");
                    }
                }
            }
        }
        
        // 攻撃エフェクト（簡単な色変更）
        StartCoroutine(AttackEffect());
        
        // 攻撃クールダウン
        StartCoroutine(AttackCooldown());
    }
    
    IEnumerator AttackEffect()
    {
        if (enemyRenderer != null)
        {
            Color originalColor = enemyRenderer.material.color;
            enemyRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            enemyRenderer.material.color = originalColor;
        }
    }
    
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    // 敵がダメージを受ける
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        SetState(EnemyState.Damaged);
        lastAttackTime = Time.time;
        
        Debug.Log($"敵がダメージを受けました。残り体力: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("敵が倒されました");
        // 死亡エフェクトやドロップアイテムの処理をここに追加
        Destroy(gameObject);
    }
    
    // 外部からの制御用メソッド
    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
    }
    
    public void SetEnemyType(EnemyType newType)
    {
        enemyType = newType;
    }
    
    public bool IsPlayerInRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }
    
    public EnemyState GetCurrentState()
    {
        return currentState;
    }
    
    // デバッグ用：検出範囲を視覚化
    void OnDrawGizmosSelected()
    {
        // 検出範囲
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 追跡範囲
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // パトロールポイント
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);
                    
                    // パトロールルートの線を描画
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    else if (i == patrolPoints.Length - 1 && patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }
    }
}
