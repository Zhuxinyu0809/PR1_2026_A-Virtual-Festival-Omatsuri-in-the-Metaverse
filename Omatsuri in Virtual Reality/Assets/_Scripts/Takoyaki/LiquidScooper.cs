using UnityEngine;

/// <summary>
/// 液體舀取交互邏輯 (搭配 Meta XR Interaction SDK v85 使用)
/// 針對自訂的 _Fill 區間進行了精確映射調整，並支援將液體倒入章魚燒烤盤的多個洞中
/// </summary>
public class LiquidScooper : MonoBehaviour
{
    [Header("渲染器設定 (Renderers)")]
    [Tooltip("長柄勺上的液體 Renderer (需套用 LiquidShader)")]
    public Renderer ladleLiquidRenderer;
    [Tooltip("大碗上的液體 Renderer (需套用 LiquidShader)")]
    public Renderer bowlLiquidRenderer;

    [Header("液面 _Fill 精確數值對應 (Fill Mapping)")]
    [Tooltip("長柄勺：完全沒水時的 _Fill 值")]
    public float ladleEmptyFill = 0.527f;
    [Tooltip("長柄勺：裝滿水時的 _Fill 值")]
    public float ladleFullFill = 0.539f;

    [Tooltip("大碗：完全沒水時的 _Fill 值")]
    public float bowlEmptyFill = 0.487f;
    [Tooltip("大碗：裝滿水時的 _Fill 值")]
    public float bowlFullFill = 0.523f;

    [Header("章魚燒烤盤設定 (Takoyaki Pan Settings)")]
    [Tooltip("章魚燒烤盤上所有洞的液體 Renderers")]
    public Renderer[] takoyakiHoleRenderers;
    [Tooltip("章魚燒洞：完全沒水時的 _Fill 值")]
    public float holeEmptyFill = 0.539f;
    [Tooltip("章魚燒洞：裝滿水時的 _Fill 值")]
    public float holeFullFill = 0.559f;
    [Tooltip("判定液體能成功倒入洞口的 XZ 軸允許半徑")]
    public float pourTargetRadius = 0.05f;

    [Header("舀取與物理設定 (Scoop Settings)")]
    [Tooltip("碗液體從滿到空，一共可以被舀幾次")]
    public int maxScoops = 21;
    [Tooltip("液面上升/下降的平滑動畫速度")]
    public float fillLerpSpeed = 5f;
    [Tooltip("勺子往下傾斜超過這個角度(內積值)，裡面的水就會倒出。0 代表正好 90 度。")]
    public float pourThreshold = 0.0f;

    [Header("空間判定 (Spatial Check)")]
    [Tooltip("大碗的中心點 Transform")]
    public Transform bowlCenter;
    [Tooltip("判定長柄勺是否在碗內的 XZ 軸半徑範圍")]
    public float bowlRadius = 0.15f;
    [Tooltip("長柄勺的判定基準點 (通常放在勺斗的最底端)")]
    public Transform scoopPoint;

    // 內部變數
    private int fillPropertyID;
    private float currentLadleFill;
    private float targetLadleFill;
    private float currentBowlFill;
    private float targetBowlFill;
    
    // 章魚燒洞口狀態追蹤
    private float[] currentHoleFills;
    private float[] targetHoleFills;

    // 狀態追蹤
    private bool isUnderLiquidLastFrame = false;
    private bool hasSuccessfullyScooped = false; // 紀錄這次下水是否真的舀到「新」的液體

    void Start()
    {
        // 效能優化：快取 Shader Property ID
        fillPropertyID = Shader.PropertyToID("_Fill");

        if (scoopPoint == null) scoopPoint = this.transform;

        // 初始化：長柄勺是空的
        targetLadleFill = ladleEmptyFill;
        currentLadleFill = ladleEmptyFill;
        ladleLiquidRenderer.material.SetFloat(fillPropertyID, currentLadleFill);

        // 初始化：大碗是滿的
        targetBowlFill = bowlFullFill;
        currentBowlFill = bowlFullFill;
        if (bowlLiquidRenderer != null)
        {
            bowlLiquidRenderer.material.SetFloat(fillPropertyID, currentBowlFill);
        }

        // 初始化：章魚燒烤盤所有的洞都是空的
        if (takoyakiHoleRenderers != null && takoyakiHoleRenderers.Length > 0)
        {
            currentHoleFills = new float[takoyakiHoleRenderers.Length];
            targetHoleFills = new float[takoyakiHoleRenderers.Length];
            
            for (int i = 0; i < takoyakiHoleRenderers.Length; i++)
            {
                currentHoleFills[i] = holeEmptyFill;
                targetHoleFills[i] = holeEmptyFill;
                
                if (takoyakiHoleRenderers[i] != null)
                {
                    takoyakiHoleRenderers[i].material.SetFloat(fillPropertyID, holeEmptyFill);
                }
            }
        }
    }

    void Update()
    {
        if (bowlLiquidRenderer == null || ladleLiquidRenderer == null) return;

        // --- 1. 計算碗內液面的真實世界高度 (World Y) ---
        // 使用 InverseLerp 將當前的 _Fill 轉換為 0.0 ~ 1.0 的真實液面百分比
        float bowlFillPercentage = Mathf.InverseLerp(bowlEmptyFill, bowlFullFill, currentBowlFill);
        
        float bowlBottomY = bowlLiquidRenderer.bounds.min.y;
        float bowlHeight = bowlLiquidRenderer.bounds.size.y;
        float currentLiquidSurfaceY = bowlBottomY + (bowlHeight * bowlFillPercentage);

        // --- 2. 空間判定邏輯 ---
        // 判定勺子是否在大碗的上方 (忽略 Y 軸，只算 XZ 平面的距離)
        float distanceXZ = Vector2.Distance(
            new Vector2(scoopPoint.position.x, scoopPoint.position.z),
            new Vector2(bowlCenter.position.x, bowlCenter.position.z)
        );
        bool isInsideBowlRadius = distanceXZ <= bowlRadius;

        // 判定勺斗底部是否低於當前的真實液面
        bool isUnderLiquid = scoopPoint.position.y < currentLiquidSurfaceY;

        // --- 3. 狀態機與互動邏輯 ---
        if (isInsideBowlRadius)
        {
            if (isUnderLiquid)
            {
                // 條件 A：勺子進入碗的液面下
                if (!isUnderLiquidLastFrame)
                {
                    // 剛進入液面的瞬間，檢查勺子是否還有空間 (避免滿的勺子重複舀水)
                    if (targetLadleFill < ladleFullFill && bowlFillPercentage > 0.0f)
                    {
                        targetLadleFill = ladleFullFill;
                        hasSuccessfullyScooped = true; // 標記為成功獲取新液體
                    }
                    else
                    {
                        hasSuccessfullyScooped = false;
                    }
                }
                isUnderLiquidLastFrame = true;
            }
            else if (isUnderLiquidLastFrame)
            {
                // 條件 B：勺子剛剛離開液面 (上一幀在水下，這一幀在水上)
                // 只有當這是一次「有效舀水」時，才扣除碗裡的水量
                if (hasSuccessfullyScooped)
                {
                    // 計算要扣除的絕對 _Fill 數值差 (總容量均分為 maxScoops 份)
                    float fillDropAmount = (bowlFullFill - bowlEmptyFill) / maxScoops;
                    
                    // 使用 Mathf.Max 確保不會扣到低於 empty 的數值
                    targetBowlFill = Mathf.Max(bowlEmptyFill, targetBowlFill - fillDropAmount);
                    
                    // 重置舀水狀態
                    hasSuccessfullyScooped = false;
                }
                isUnderLiquidLastFrame = false;
            }
        }
        else
        {
            isUnderLiquidLastFrame = false;
            hasSuccessfullyScooped = false;

            // 條件 C：勺子不在大碗裡時，判斷是否傾斜倒水
            // Vector3.Dot 計算向上向量，內積 <= 0 代表正好傾斜 90 度或超過 90 度
            if (Vector3.Dot(scoopPoint.up, Vector3.up) <= pourThreshold)
            {
                // 如果長柄勺內有液體
                if (targetLadleFill > ladleEmptyFill)
                {
                    // 1. 長柄勺必定倒空
                    targetLadleFill = ladleEmptyFill;

                    // 2. 檢測是否倒在某個章魚燒洞的上方
                    if (takoyakiHoleRenderers != null)
                    {
                        int closestHoleIndex = -1;
                        float minDistance = pourTargetRadius; // 最大允許倒入誤差半徑

                        for (int i = 0; i < takoyakiHoleRenderers.Length; i++)
                        {
                            if (takoyakiHoleRenderers[i] == null) continue;

                            // 取得洞的中心 XZ 坐標 (利用 Renderer 的 Bounds 會比直接用 Transform 準確)
                            Vector2 holePosXZ = new Vector2(takoyakiHoleRenderers[i].bounds.center.x, takoyakiHoleRenderers[i].bounds.center.z);
                            Vector2 scoopPosXZ = new Vector2(scoopPoint.position.x, scoopPoint.position.z);

                            float dist = Vector2.Distance(scoopPosXZ, holePosXZ);
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                closestHoleIndex = i; // 找到最近的洞
                            }
                        }

                        // 如果成功命中某個洞口
                        if (closestHoleIndex != -1)
                        {
                            // 逐漸增加至滿。如果該洞已經滿了，這個賦值不會產生視覺變化，完美符合需求。
                            targetHoleFills[closestHoleIndex] = holeFullFill;
                        }
                    }
                }
            }
        }

        // --- 4. 視覺平滑過渡 (Lerp) 與寫入 Shader ---
        
        // 平滑處理長柄勺
        if (Mathf.Abs(currentLadleFill - targetLadleFill) > 0.0001f)
        {
            currentLadleFill = Mathf.Lerp(currentLadleFill, targetLadleFill, Time.deltaTime * fillLerpSpeed);
            ladleLiquidRenderer.material.SetFloat(fillPropertyID, currentLadleFill);
        }

        // 平滑處理大碗
        if (Mathf.Abs(currentBowlFill - targetBowlFill) > 0.0001f)
        {
            currentBowlFill = Mathf.Lerp(currentBowlFill, targetBowlFill, Time.deltaTime * fillLerpSpeed);
            bowlLiquidRenderer.material.SetFloat(fillPropertyID, currentBowlFill);
        }

        // 平滑處理所有的章魚燒洞口
        if (takoyakiHoleRenderers != null)
        {
            for (int i = 0; i < takoyakiHoleRenderers.Length; i++)
            {
                if (takoyakiHoleRenderers[i] == null) continue;

                if (Mathf.Abs(currentHoleFills[i] - targetHoleFills[i]) > 0.0001f)
                {
                    currentHoleFills[i] = Mathf.Lerp(currentHoleFills[i], targetHoleFills[i], Time.deltaTime * fillLerpSpeed);
                    takoyakiHoleRenderers[i].material.SetFloat(fillPropertyID, currentHoleFills[i]);
                }
            }
        }
    }

    /// <summary>
    /// 重置指定章魚燒洞的液體數值 (供 Manager 翻面後，將洞口重置為「空」以重新倒麵糊)
    /// </summary>
    public void ResetTakoyakiHole(int holeIndex)
    {
        if (takoyakiHoleRenderers != null && holeIndex >= 0 && holeIndex < takoyakiHoleRenderers.Length)
        {
            // 將追蹤的數值強行拉回 Empty (0.539)
            currentHoleFills[holeIndex] = holeEmptyFill;
            targetHoleFills[holeIndex] = holeEmptyFill;
            
            // 同步覆寫 Shader 表現
            if (takoyakiHoleRenderers[holeIndex] != null)
            {
                takoyakiHoleRenderers[holeIndex].material.SetFloat(fillPropertyID, holeEmptyFill);
            }
        }
    }
}