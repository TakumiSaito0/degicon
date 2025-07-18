using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    [Header("�G�̊�{�ݒ�")]
    public EnemyType enemyType = EnemyType.Chaser;
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public int damage = 1;
    public float attackCooldown = 1f;
    
    [Header("�p�g���[���ݒ�")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;
    
    [Header("�ǐՐݒ�")]
    public float chaseRange = 7f;
    public float loseTargetRange = 10f;
    
    [Header("�̗͐ݒ�")]
    public int maxHealth = 3;
    public int currentHealth;
    
    [Header("���o����")]
    public Color normalColor = Color.white;
    public Color aggressiveColor = Color.red;
    public Color damagedColor = Color.yellow;
    
    // �v���C�x�[�g�ϐ�
    private Transform player;
    private Renderer enemyRenderer;
    private Color originalColor;
    
    // ��ԊǗ�
    private EnemyState currentState = EnemyState.Patrol;
    private bool canAttack = true;
    private float lastAttackTime;
    
    // �p�g���[���p
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    
    // �ړ�����
    private Vector3 targetPosition;
    private bool isMoving = false;

    // �G�̎��
    public enum EnemyType
    {
        Chaser,     // �v���C���[��ǐ�
        Patrol,     // �p�g���[��
        Stationary  // �Î~
    }
    
    // �G�̏��
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
        // �v���C���[������
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Renderer���擾
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
        
        // �̗͂�������
        currentHealth = maxHealth;
        
        // ������Ԃ�ݒ�
        SetState(EnemyState.Patrol);
        
        // �p�g���[���|�C���g���ݒ肳��Ă��Ȃ��ꍇ�͌��݈ʒu���g�p
        if (patrolPoints.Length == 0)
        {
            enemyType = EnemyType.Stationary;
        }
        
        Debug.Log($"�G����������܂����B���: {enemyType}, �̗�: {currentHealth}");
    }

    void Update()
    {
        if (player == null) return;
        
        // �v���C���[�Ƃ̋������v�Z
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // ��Ԃɉ������s��
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
        
        // �ړ�����
        if (isMoving)
        {
            MoveToTarget();
        }
    }
    
    void HandlePatrolState(float distanceToPlayer)
    {
        // �v���C���[�����o�͈͓��ɓ������ꍇ
        if (distanceToPlayer <= detectionRange && enemyType != EnemyType.Stationary)
        {
            SetState(EnemyState.Chase);
            return;
        }
        
        // �p�g���[���s��
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
        // �v���C���[���U���͈͓��ɓ������ꍇ
        if (distanceToPlayer <= 1.5f)
        {
            SetState(EnemyState.Attack);
            return;
        }
        
        // �v���C���[���ǐՔ͈͊O�ɏo���ꍇ
        if (distanceToPlayer > loseTargetRange)
        {
            SetState(EnemyState.Patrol);
            return;
        }
        
        // �v���C���[��ǐ�
        targetPosition = player.position;
        isMoving = true;
    }
    
    void HandleAttackState(float distanceToPlayer)
    {
        // �v���C���[���U���͈͊O�ɏo���ꍇ
        if (distanceToPlayer > 2f)
        {
            SetState(EnemyState.Chase);
            return;
        }
        
        // �U������
        if (canAttack)
        {
            AttackPlayer();
        }
    }
    
    void HandleIdleState(float distanceToPlayer)
    {
        // �v���C���[�����o�͈͓��ɓ������ꍇ
        if (distanceToPlayer <= detectionRange)
        {
            SetState(EnemyState.Chase);
        }
    }
    
    void HandleDamagedState()
    {
        // �_���[�W��Ԃ����
        if (Time.time - lastAttackTime > 1f)
        {
            SetState(EnemyState.Patrol);
        }
    }
    
    void SetState(EnemyState newState)
    {
        currentState = newState;
        
        // ��Ԃɉ������F�ύX
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
        
        Debug.Log($"�G�̏�Ԃ��ύX����܂���: {newState}");
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
            
            // �p�g���[�����̏ꍇ�͎��̃|�C���g��
            if (currentState == EnemyState.Patrol && patrolPoints.Length > 0)
            {
                StartCoroutine(WaitAtPatrolPoint());
            }
        }
        else
        {
            // Y���̈ړ��𐧌��i�n�ʂɉ����Ĉړ��j
            direction.y = 0;
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
            
            // �v���C���[�̕���������
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
        
        // ���̃p�g���[���|�C���g��
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }
    
    void AttackPlayer()
    {
        if (!canAttack) return;
        
        canAttack = false;
        lastAttackTime = Time.time;
        
        // �v���C���[�Ƀ_���[�W��^����
        if (player != null)
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                // �v���C���[�̖��G���Ԃ��`�F�b�N
                if (!playerManager.IsInvincible())
                {
                    // LifeManager��ʂ��ă_���[�W��^����
                    if (LifeManager.instance != null)
                    {
                        LifeManager.instance.TakeDamage(damage);
                        
                        // �v���C���[�̖��G���Ԃ��J�n
                        playerManager.StartInvincibility();
                        
                        Debug.Log($"�G���v���C���[���U�����܂����I�_���[�W: {damage}");
                    }
                }
            }
        }
        
        // �U���G�t�F�N�g�i�ȒP�ȐF�ύX�j
        StartCoroutine(AttackEffect());
        
        // �U���N�[���_�E��
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
    
    // �G���_���[�W���󂯂�
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        SetState(EnemyState.Damaged);
        lastAttackTime = Time.time;
        
        Debug.Log($"�G���_���[�W���󂯂܂����B�c��̗�: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("�G���|����܂���");
        // ���S�G�t�F�N�g��h���b�v�A�C�e���̏����������ɒǉ�
        Destroy(gameObject);
    }
    
    // �O������̐���p���\�b�h
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
    
    // �f�o�b�O�p�F���o�͈͂����o��
    void OnDrawGizmosSelected()
    {
        // ���o�͈�
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // �ǐՔ͈�
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // �p�g���[���|�C���g
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);
                    
                    // �p�g���[�����[�g�̐���`��
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
