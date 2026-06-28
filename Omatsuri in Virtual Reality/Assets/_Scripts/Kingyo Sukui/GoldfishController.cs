using UnityEngine;

/// <summary>
/// 金魚控制器
/// 專為 Meta Quest 獨立頭顯優化，使用 Transform 位移而非 Rigidbody 物理，以提升效能。
/// </summary>
public class GoldfishController : MonoBehaviour
{
    public enum FishState
    {
        Swimming, // 在水中游動
        Caught,   // 被紙網撈起
        Scored    // 進入魚缸得分
    }

    [Header("狀態 (State)")]
    [SerializeField] private FishState currentState = FishState.Swimming;

    [Header("游動設定 (Swimming Settings)")]
    [Tooltip("金魚游動的平均速度")]
    [SerializeField] private float minSwimSpeed = 0.5f;
    [SerializeField] private float maxSwimSpeed = 1.2f;
    
    [Tooltip("金魚轉向目標的速度")]
    [SerializeField] private float turnSpeed = 2.0f;
    
    [Tooltip("定義金魚活動範圍的 BoxCollider (通常放在水池中心)")]
    [SerializeField] private BoxCollider swimArea;

    [Header("視覺與動畫 (Visuals)")]
    [Tooltip("金魚的模型本體 (必須是掛載此腳本的子物件)")]
    [SerializeField] private Transform fishModel;

    [Tooltip("游動時身體擺動的幅度")]
    [SerializeField] private float wiggleAmount = 15f;
    [Tooltip("游動時身體擺動的頻率")]
    [SerializeField] private float wiggleSpeed = 10f;

    [Header("浮沉效果 (Bobbing)")]
    [Tooltip("游動時垂直浮沉的幅度")]
    [SerializeField] private float bobbingAmount = 0.02f;
    [Tooltip("游動時垂直浮沉的頻率")]
    [SerializeField] private float bobbingSpeed = 2f;

    // 內部變數
    private float currentSpeed;
    private Vector3 targetWaypoint;
    private Transform caughtParent; // 被撈起時跟隨的目標 (紙網)
    private Vector3 caughtOffset;   // 在紙網上的偏移量
    
    private Quaternion initialModelRotation; // 紀錄模型初始的局部旋轉
    private Vector3 initialModelPosition;    // 紀錄模型初始的局部位置 (用於浮沉)
    private float randomPhase;               // 隨機相位，讓每條魚的動畫不同步
    private float stuckTimer = 0f;           // 防卡死計時器

    // 【新增】紀錄金魚遊戲開始時的世界座標與旋轉，用於重置
    private Vector3 initialWorldPosition;
    private Quaternion initialWorldRotation;

    void Start()
    {
        currentSpeed = Random.Range(minSwimSpeed, maxSwimSpeed);
        randomPhase = Random.Range(0f, 100f); // 產生隨機相位，避免所有魚同步擺動
        
        // 【新增】紀錄初始狀態
        initialWorldPosition = transform.position;
        initialWorldRotation = transform.rotation;

        if (fishModel != null)
        {
            // 紀錄模型最初的旋轉角度。
            // 這樣開發者只要在 Editor 把 FishModel 轉到面向 Z 軸 (藍色箭頭)，程式就會自動適應。
            initialModelRotation = fishModel.localRotation;
            // 紀錄模型最初的局部位置，作為上下浮沉的基準點
            initialModelPosition = fishModel.localPosition;
        }

        if (swimArea != null)
        {
            PickNewWaypoint();
        }
        else
        {
            Debug.LogWarning("[GoldfishController] 未設定 Swim Area，金魚將無法游動！");
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case FishState.Swimming:
                HandleSwimming();
                HandleWiggleAnimation();
                break;
            case FishState.Caught:
                HandleCaughtState();
                break;
            case FishState.Scored:
                // 得分後可能停止游動，或者在魚缸內小範圍游動 (可後續擴展)
                HandleWiggleAnimation(); 
                break;
        }
    }

    /// <summary>
    /// 處理在水池中的隨機游動
    /// </summary>
    private void HandleSwimming()
    {
        if (swimArea == null) return;

        // 計算與目標點的距離
        float distanceToTarget = Vector3.Distance(transform.position, targetWaypoint);

        // 防卡死機制 1：如果接近目標點，就找下一個新目標 (放寬判定距離至 0.2f)
        if (distanceToTarget < 0.2f)
        {
            PickNewWaypoint();
        }

        // 防卡死機制 2：如果卡在原地/同一個目標太久 (例如 5 秒)，強制換目標
        stuckTimer += Time.deltaTime;
        if (stuckTimer > 5f)
        {
            PickNewWaypoint();
        }

        // 轉向目標點
        Vector3 direction = (targetWaypoint - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            // 只在 Y 軸旋轉 (保持金魚水平)
            lookRotation.x = 0;
            lookRotation.z = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }

        // 預測下一步的位置
        Vector3 nextPosition = transform.position + transform.forward * currentSpeed * Time.deltaTime;

        // 取得預測位置在邊界內的最近點 (空氣牆)
        Vector3 clampedPosition = swimArea.bounds.ClosestPoint(nextPosition);

        // 防卡死機制 3：如果預測位置和限制位置有落差，代表撞到了水池邊界
        if (Vector3.Distance(nextPosition, clampedPosition) > 0.001f)
        {
            PickNewWaypoint(); // 立刻選擇新目標，讓魚轉頭
        }

        // 正式移動到修正後的位置
        transform.position = clampedPosition;
    }

    /// <summary>
    /// 簡單的程式化動畫：讓魚的模型左右擺動模擬游水
    /// </summary>
    private void HandleWiggleAnimation()
    {
        if (fishModel != null)
        {
            // 1. 左右擺尾 (Wiggle) - 加入 randomPhase 避免同步
            float wiggle = Mathf.Sin(Time.time * wiggleSpeed + randomPhase) * wiggleAmount;
            // 基於初始紀錄的旋轉角度進行 Y 軸擺動，這樣就不會影響原本手動校正好的正面方向
            fishModel.localRotation = initialModelRotation * Quaternion.Euler(0, wiggle, 0);

            // 2. 上下浮沉 (Bobbing) - 加入數學 Sine 波浪效果
            float bobbing = Mathf.Sin(Time.time * bobbingSpeed + randomPhase) * bobbingAmount;
            fishModel.localPosition = initialModelPosition + new Vector3(0, bobbing, 0);
        }
    }

    /// <summary>
    /// 在 Swim Area 範圍內隨機挑選一個點
    /// </summary>
    private void PickNewWaypoint()
    {
        Bounds bounds = swimArea.bounds;
        Vector3 extents = bounds.extents;

        // 加入 Margin，預留轉彎空間，避免目標點完全貼住邊緣
        float marginX = Mathf.Min(0.2f, extents.x * 0.5f);
        float marginY = Mathf.Min(0.2f, extents.y * 0.5f);
        float marginZ = Mathf.Min(0.2f, extents.z * 0.5f);

        float randomX = Random.Range(bounds.min.x + marginX, bounds.max.x - marginX);
        float randomY = Random.Range(bounds.min.y + marginY, bounds.max.y - marginY);
        float randomZ = Random.Range(bounds.min.z + marginZ, bounds.max.z - marginZ);

        targetWaypoint = new Vector3(randomX, randomY, randomZ);
        
        // 重置狀態
        stuckTimer = 0f;
        currentSpeed = Random.Range(minSwimSpeed, maxSwimSpeed);
    }

    /// <summary>
    /// 被紙網撈起時的處理
    /// </summary>
    private void HandleCaughtState()
    {
        if (caughtParent != null)
        {
            // 平滑地跟隨紙網移動
            transform.position = Vector3.Lerp(transform.position, caughtParent.position + caughtOffset, Time.deltaTime * 10f);
            
            // 讓金魚在網上稍微掙扎跳動 (改為沿著紙網的法線/上方跳動，這樣傾斜紙網時跳動方向才正確)
            float struggleY = Mathf.PingPong(Time.time * 5f, 0.05f);
            transform.position += caughtParent.up * struggleY;
            
            // 稍微掙扎旋轉，增加真實感
            float struggleRotation = Mathf.Sin(Time.time * 20f) * 2f;
            transform.Rotate(0, struggleRotation, 0);
        }
    }

    // ==========================================
    // 公開 API：供紙網 (Poi) 或系統呼叫
    // ==========================================

    /// <summary>
    /// 被紙網成功撈起
    /// </summary>
    public void CatchFish(Transform netTransform)
    {
        if (currentState != FishState.Swimming) return;

        currentState = FishState.Caught;
        caughtParent = netTransform;
        
        // 紀錄被抓起時，相對於網子中心的隨機偏移量，讓魚不會全部疊在同一個點
        caughtOffset = new Vector3(Random.Range(-0.05f, 0.05f), 0.02f, Random.Range(-0.05f, 0.05f));
    }

    /// <summary>
    /// 從紙網掉落 (網破了，或者玩家動作太大)
    /// </summary>
    public void ReleaseFish()
    {
        if (currentState == FishState.Caught)
        {
            currentState = FishState.Swimming;
            caughtParent = null;
            // 重新選擇一個目標點，讓它游回去
            PickNewWaypoint();
        }
    }

    /// <summary>
    /// 【更新】成功放入魚缸
    /// </summary>
    public void ScoreFish(Transform bowlTransform)
    {
        currentState = FishState.Scored;
        caughtParent = null;
        
        // 將金魚放入魚缸位置
        transform.position = bowlTransform.position + new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
        // 可以加入一些魚在缸內的隨機旋轉
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    /// <summary>
    /// 【新增】重置金魚狀態 (回到水池初始位置)
    /// </summary>
    public void ResetFish()
    {
        currentState = FishState.Swimming;
        caughtParent = null;
        
        transform.position = initialWorldPosition;
        transform.rotation = initialWorldRotation;
        
        stuckTimer = 0f;
        PickNewWaypoint();
    }
    
    // 提供給外部查詢的方法
    public FishState GetState() => currentState;
}