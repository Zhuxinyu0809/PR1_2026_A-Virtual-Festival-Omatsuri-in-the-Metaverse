using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 紙網撈魚判定系統
/// 必須與 PoiDurabilitySystem 掛載在同一個紙網物件上
/// </summary>
public class PoiCatchSystem : MonoBehaviour
{
    [Header("系統關聯")]
    [Tooltip("紙網的耐久度系統 (自動抓取或手動拖入)")]
    [SerializeField] private PoiDurabilitySystem durabilitySystem;

    [Header("撈魚設定")]
    [Tooltip("玩家揮動網子多快會把魚甩飛 (公尺/秒)，太粗魯會掉魚")]
    [SerializeField] private float dropVelocityThreshold = 2.5f;

    [Tooltip("網面中心點 (設定魚被撈起時停靠的位置。請建立一個空物件放在紙網中心並拖入)")]
    [SerializeField] private Transform netCenter;

    // 紀錄進入網子 Trigger 但還沒出水的魚
    private List<GoldfishController> fishInWaterTrigger = new List<GoldfishController>();
    // 紀錄已經被成功撈出水的魚
    private List<GoldfishController> caughtFish = new List<GoldfishController>();
    
    private Vector3 lastPosition;

    void Start()
    {
        if (durabilitySystem == null) 
        {
            durabilitySystem = GetComponent<PoiDurabilitySystem>();
        }
        lastPosition = transform.position;
    }

    void Update()
    {
        // 計算紙網目前的移動速度
        float speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // 情況 1：如果網子破了，強制釋放所有網子上的魚
        if (durabilitySystem.IsBroken())
        {
            if (caughtFish.Count > 0)
            {
                ReleaseAllFish();
            }
            return;
        }

        // 情況 2：如果玩家動作太大 (揮動速度過快)，魚會從網子飛出去掉回水裡
        if (speed > dropVelocityThreshold && caughtFish.Count > 0)
        {
            ReleaseAllFish();
            Debug.Log("[PoiCatchSystem] 動作太大，金魚掉落！");
        }

        // 情況 3：核心判定 -> 當網子離開水面，且網子範圍內有魚，就觸發「正式撈起」
        if (!durabilitySystem.IsInWater() && fishInWaterTrigger.Count > 0)
        {
            CatchFishInTrigger();
        }

        // 清理已經被放入魚缸 (Scored) 的魚，避免殘留在名單內
        caughtFish.RemoveAll(fish => fish == null || fish.GetState() == GoldfishController.FishState.Scored);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goldfish"))
        {
            GoldfishController fish = other.GetComponentInParent<GoldfishController>();
            if (fish != null && fish.GetState() == GoldfishController.FishState.Swimming)
            {
                if (!fishInWaterTrigger.Contains(fish))
                {
                    fishInWaterTrigger.Add(fish);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Goldfish"))
        {
            GoldfishController fish = other.GetComponentInParent<GoldfishController>();
            if (fish != null && fishInWaterTrigger.Contains(fish))
            {
                fishInWaterTrigger.Remove(fish);
            }
        }
    }

    private void CatchFishInTrigger()
    {
        foreach (var fish in fishInWaterTrigger)
        {
            if (fish.GetState() == GoldfishController.FishState.Swimming)
            {
                fish.CatchFish(netCenter != null ? netCenter : transform); 
                caughtFish.Add(fish);
            }
        }
        fishInWaterTrigger.Clear();
    }

    public void ReleaseAllFish()
    {
        if (caughtFish.Count == 0) return;

        foreach (var fish in caughtFish)
        {
            if (fish != null && fish.GetState() == GoldfishController.FishState.Caught)
            {
                fish.ReleaseFish(); 
            }
        }
        caughtFish.Clear();
    }

    public void ForceClearCaughtFish()
    {
        fishInWaterTrigger.Clear();
        caughtFish.Clear();
    }

    // ==========================================
    // 供 FishbowlSystem 及 GameManager 呼叫的 API
    // ==========================================

    /// <summary>
    /// 取得目前網子上有多少條魚
    /// </summary>
    public int GetCaughtFishCount()
    {
        return caughtFish.Count;
    }

    /// <summary>
    /// 將網子上所有的魚轉換為得分狀態，並轉移到魚缸
    /// </summary>
    public void ScoreAllCaughtFish(Transform bowlTransform)
    {
        foreach (var fish in caughtFish)
        {
            if (fish != null && fish.GetState() == GoldfishController.FishState.Caught)
            {
                fish.ScoreFish(bowlTransform); // 呼叫金魚的入缸邏輯
            }
        }
        caughtFish.Clear(); // 魚已經交出，清空網子暫存
    }

    /// <summary>
    /// 重置紙網狀態（包含清空魚與耐久度）
    /// 供 GameManager 玩家離開區域時呼叫
    /// </summary>
    public void ResetPoiState()
    {
        ForceClearCaughtFish();
        
        if (durabilitySystem != null)
        {
            durabilitySystem.RepairNet(); // 呼叫修復方法，將耐久度回滿並修復破洞
        }
    }
}