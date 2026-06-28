using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction; // 確保有 import Meta XR Interaction SDK v85
using TMPro; // 引入 TextMeshPro 支援

/// <summary>
/// 金魚撈紙網 (Poi) 耐久度系統
/// 專為 Meta XR SDK v85 設計，支援 Interaction SDK 整合
/// </summary>
public class PoiDurabilitySystem : MonoBehaviour
{
    [Header("耐久度設定 (Durability Settings)")]
    [Tooltip("紙網初始最大耐久度")]
    [SerializeField] private float maxDurability = 100f;
    
    [Tooltip("在水中每秒扣除的基礎耐久度")]
    [SerializeField] private float baseDepletionRate = 10f;
    
    [Tooltip("在水中移動速度對耐久度的懲罰倍率 (越快爛得越快)")]
    [SerializeField] private float movePenaltyMultiplier = 50f;

    [Header("物理與判定 (Physics & Collision)")]
    [Tooltip("代表水池的 Tag 名稱")]
    [SerializeField] private string waterTag = "Water";
    
    [Tooltip("紙網用來撈魚的 Trigger Collider")]
    [SerializeField] private Collider netTrigger;

    [Header("視覺與狀態 (Visuals & State)")]
    [Tooltip("完整的紙網模型 (GameObject)")]
    [SerializeField] private GameObject intactNetVisual;
    
    [Tooltip("破裂的紙網模型 (GameObject)")]
    [SerializeField] private GameObject brokenNetVisual;

    [Header("事件回調 (Events)")]
    [Tooltip("紙網破裂時觸發的事件 (可用來掛接 Meta Haptics 或音效)")]
    public UnityEvent OnNetBroken;

    [Header("調試與 UI (Debug & UI)")]
    [Tooltip("是否在畫面上顯示耐久度文字")]
    [SerializeField] private bool showDurabilityText = true;
    
    [Tooltip("用來顯示耐久度的 TextMeshPro 元件 (可選)")]
    [SerializeField] private TMP_Text durabilityText;

    [Header("修復區域 (Repair Zone)")]
    [Tooltip("代表修復區域的 Tag 名稱")]
    [SerializeField] private string healZoneTag = "HealZone";
    
    [Tooltip("需要停留在修復區域多久才能回滿耐久度 (秒)")]
    [SerializeField] private float timeRequiredToHeal = 3f;

    // 內部狀態變數
    private float currentDurability;
    private bool isInWater = false;
    private bool isBroken = false;
    
    // 修復區域狀態變數
    private bool isInHealZone = false;
    private float healTimer = 0f;
    
    // 計算速度用
    private Vector3 lastPosition;
    private float currentSpeed;

    void Start()
    {
        // 初始化狀態
        currentDurability = maxDurability;
        isBroken = false;
        
        // 確保視覺模型正確顯示
        if (intactNetVisual) intactNetVisual.SetActive(true);
        if (brokenNetVisual) brokenNetVisual.SetActive(false);
        
        if (netTrigger) netTrigger.enabled = true;
        
        lastPosition = transform.position;
    }

    void Update()
    {
        // 處理修復邏輯 (即使紙網破了，只要在修復區依然可以計時修復)
        if (isInHealZone)
        {
            healTimer += Time.deltaTime;
            if (healTimer >= timeRequiredToHeal)
            {
                RepairNet();
                healTimer = 0f; // 修復完成後重置計時器，避免每幀重複執行
            }
        }

        // 更新 UI 顯示 (可在 Inspector 動態開關)
        if (durabilityText != null)
        {
            durabilityText.gameObject.SetActive(showDurabilityText);
            if (showDurabilityText)
            {
                durabilityText.text = $"Durability: {Mathf.CeilToInt(currentDurability)} / {maxDurability}";
            }
        }

        // 如果紙網已經破咗，就唔需要再計算扣減邏輯
        if (isBroken) return;

        // 計算當前紙網嘅移動速度
        CalculateSpeed();

        // 只有在水中時才扣除耐久度
        if (isInWater)
        {
            // 總消耗 = 基礎消耗 + (移動速度 * 懲罰倍率)
            float depletion = (baseDepletionRate + (currentSpeed * movePenaltyMultiplier)) * Time.deltaTime;
            currentDurability -= depletion;

            // 檢查是否破裂
            if (currentDurability <= 0)
            {
                BreakNet();
            }
        }
    }

    /// <summary>
    /// 計算紙網移動速度
    /// </summary>
    private void CalculateSpeed()
    {
        // 距離 / 時間 = 速度
        currentSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    /// <summary>
    /// 當紙網碰到水面或離開水面時的判定
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 1. 先檢查是否進入修復區域
        if (other.CompareTag(healZoneTag))
        {
            isInHealZone = true;
            healTimer = 0f; // 進入時重新計時
        }

        if (isBroken) return;

        // 2. 檢查是否碰到水池
        if (other.CompareTag(waterTag))
        {
            isInWater = true;
            // 可選：喺度可以觸發「入水」嘅輕微 Haptics 或音效
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 1. 檢查是否離開修復區域
        if (other.CompareTag(healZoneTag))
        {
            isInHealZone = false;
            healTimer = 0f;
        }

        // 2. 檢查是否離開水池
        if (other.CompareTag(waterTag))
        {
            isInWater = false;
            // 可選：喺度可以觸發「出水」嘅音效
        }
    }

    /// <summary>
    /// 紙網修復邏輯 (回復滿血狀態)
    /// </summary>
    public void RepairNet()
    {
        if (currentDurability >= maxDurability && !isBroken) return; // 如果已經滿血就略過

        currentDurability = maxDurability;
        isBroken = false;

        // 重新啟用撈魚判定
        if (netTrigger) netTrigger.enabled = true;

        // 切換模型顯示 (破爛 -> 完整)
        if (intactNetVisual) intactNetVisual.SetActive(true);
        if (brokenNetVisual) brokenNetVisual.SetActive(false);

        Debug.Log("[PoiDurabilitySystem] 紙網已在修復區成功回滿耐久度！");
    }

    /// <summary>
    /// 紙網破裂邏輯
    /// </summary>
    private void BreakNet()
    {
        isBroken = true;
        isInWater = false;
        currentDurability = 0;

        // 1. 關閉撈魚判定，玩家無法再撈起金魚
        if (netTrigger) netTrigger.enabled = false;

        // 2. 切換模型顯示 (完整 -> 破爛)
        if (intactNetVisual) intactNetVisual.SetActive(false);
        if (brokenNetVisual) brokenNetVisual.SetActive(true);

        // 3. 觸發 UnityEvent，通知外部系統 (播放音效、震動手柄等)
        OnNetBroken?.Invoke();
        
        Debug.Log("[PoiDurabilitySystem] 紙網已破裂！");
    }

    // 提供給外部查詢的方法
    public bool IsBroken() => isBroken;
    public bool IsInWater() => isInWater;
    public float GetDurabilityPercentage() => Mathf.Clamp01(currentDurability / maxDurability);
}