using UnityEngine;

/// <summary>
/// 魚缸判定系統
/// 掛載在魚缸的 Trigger Collider 上，偵測紙網或金魚進入
/// </summary>
public class FishbowlSystem : MonoBehaviour
{
    [Tooltip("連結到場景中的遊戲管理器")]
    [SerializeField] private KingyoGameManager gameManager;
    
    [Tooltip("魚缸內部中心點 (金魚掉進去後會停靠或圍繞這個點)")]
    [SerializeField] private Transform bowlCenter;

    private void OnTriggerEnter(Collider other)
    {
        // 1. 【重點新增】偵測是否為「紙網 (Poi)」進入魚缸範圍
        // 只要網子進入魚缸，就把網子上的魚全部自動轉移進去，手感最好！
        PoiCatchSystem poi = other.GetComponentInParent<PoiCatchSystem>();
        if (poi != null)
        {
            int caughtCount = poi.GetCaughtFishCount();
            if (caughtCount > 0)
            {
                // 讓網子上的魚全部進入得分狀態，並移動到魚缸中心
                poi.ScoreAllCaughtFish(bowlCenter != null ? bowlCenter : transform);
                
                // 依照轉移的魚數量加分
                if (gameManager != null)
                {
                    for (int i = 0; i < caughtCount; i++)
                    {
                        gameManager.AddScore();
                    }
                }
            }
        }

        // 2. 保留原本邏輯：偵測單獨的金魚 (防呆機制，如果金魚真的被物理拋進去)
        if (other.CompareTag("Goldfish"))
        {
            GoldfishController fish = other.GetComponentInParent<GoldfishController>();
            
            if (fish != null && fish.GetState() != GoldfishController.FishState.Scored)
            {
                fish.ScoreFish(bowlCenter != null ? bowlCenter : transform);
                
                if (gameManager != null)
                {
                    gameManager.AddScore();
                }
            }
        }
    }
}