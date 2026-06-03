using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPC 路線巡邏控制器
/// 專為 Unity 6+ 及 Meta XR 環境優化
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPCWaypointPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("將包含所有巡邏點（子物件）的父物件拖曳到這裡")]
    public Transform routeParent;
    
    [Tooltip("到達節點後的停頓時間（秒）")]
    public float waitTimeAtWaypoint = 0.0f;

    [Header("Animation Settings")]
    [Tooltip("NPC的Animator，通常掛載在包含骨架的子物件上（選填）")]
    public Animator npcAnimator;

    // 改為隱藏的陣列，由程式自動填入
    private Transform[] _waypoints;
    
    private NavMeshAgent _navMeshAgent;
    private int _currentWaypointIndex;
    private bool _isWaiting;
    private float _waitTimer;

    void Start()
    {
        // 獲取 NavMeshAgent 組件
        _navMeshAgent = GetComponent<NavMeshAgent>();

        // [新增] 如果停頓時間為 0，關閉自動煞車 (Auto Braking) 以達到無縫巡邏
        if (waitTimeAtWaypoint <= 0.01f)
        {
            _navMeshAgent.autoBraking = false;
        }

        // 自動讀取父物件下的所有巡邏點
        InitializeWaypoints();

        // 確保有成功載入路線點
        if (_waypoints != null && _waypoints.Length > 0)
        {
            _currentWaypointIndex = 0;
            SetDestinationToCurrentWaypoint();
        }
        else
        {
            Debug.LogWarning("NPCWaypointPatrol: 找不到巡邏點！請確保 Route Parent 有設定並且內有子物件。", this);
        }
    }

    /// <summary>
    /// 自動從 routeParent 提取所有子物件作為巡邏點
    /// </summary>
    private void InitializeWaypoints()
    {
        if (routeParent == null) return;

        int childCount = routeParent.childCount;
        if (childCount == 0) return;

        _waypoints = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            // GetChild 會嚴格按照 Unity Hierarchy 中的順序（由上至下）讀取
            _waypoints[i] = routeParent.GetChild(i);
        }
        
        Debug.Log($"[NPCWaypointPatrol] 成功為 {gameObject.name} 載入 {_waypoints.Length} 個巡邏點。");
    }

    void Update()
    {
        // 更新動畫速度 (如果已設定 Animator)
        UpdateAnimation();

        // 如果沒有路線點，或者 Agent 尚未準備好，直接返回
        if (_waypoints == null || _waypoints.Length == 0 || !_navMeshAgent.isOnNavMesh) return;

        // 檢查是否正在等待
        if (_isWaiting)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= waitTimeAtWaypoint)
            {
                _isWaiting = false;
                MoveToNextWaypoint();
            }
        }
        else
        {
            // 檢查是否已經到達目標位置 (考慮 stoppingDistance)
            if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                // [修改] 根據停頓時間決定是否無縫切換
                if (waitTimeAtWaypoint <= 0.01f)
                {
                    // 不需要等待：直接前往下一個點，保持速度不減
                    MoveToNextWaypoint();
                }
                else
                {
                    // 抵達目標，開始等待
                    _isWaiting = true;
                    _waitTimer = 0f;
                }
            }
        }
    }

    /// <summary>
    /// 同步 NavMeshAgent 速度到 Animator
    /// </summary>
    private void UpdateAnimation()
    {
        if (npcAnimator != null && _navMeshAgent != null)
        {
            // 獲取當前的移動速度 (magnitude)
            float speed = _navMeshAgent.velocity.magnitude;
            
            // 傳遞給 Animator，需確保 Animator Controller 內有 "Speed" (Float) 參數
            npcAnimator.SetFloat("Speed", speed);
        }
    }

    /// <summary>
    /// 設定 NavMeshAgent 走到當前的目標節點
    /// </summary>
    private void SetDestinationToCurrentWaypoint()
    {
        if (_waypoints[_currentWaypointIndex] != null)
        {
            _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);
        }
    }

    /// <summary>
    /// 切換到下一個路線節點
    /// </summary>
    private void MoveToNextWaypoint()
    {
        // 循環回到第一個節點
        _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
        SetDestinationToCurrentWaypoint();
    }
}