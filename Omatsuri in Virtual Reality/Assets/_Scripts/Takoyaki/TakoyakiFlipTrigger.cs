using UnityEngine;

/// <summary>
/// 掛載在半熟、四分之三熟章魚燒模型上的 Trigger，用來偵測竹籤的物理碰撞並通知 Manager 翻面
/// 適用於 Meta XR Interaction SDK v85 的物理抓取環境
/// </summary>
public class TakoyakiFlipTrigger : MonoBehaviour
{
    [Tooltip("負責管理的烤盤 Manager")]
    public TakoyakiPanManager panManager;
    
    [Tooltip("這個章魚燒在陣列中對應的索引值 (例如第一個洞填 0)")]
    public int holeIndex;

    // 加入冷卻時間，避免 VR 中手部些微抖動造成一秒內觸發多次碰撞
    private float lastFlipTime = 0f;
    private float flipCooldown = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        // 檢查碰撞到的物件是否為玩家手中的竹籤，並確保過了冷卻時間
        if (other.CompareTag("Skewer") && Time.time - lastFlipTime > flipCooldown)
        {
            if (panManager != null)
            {
                // 通知管理器將此洞口翻面
                panManager.FlipTakoyaki(holeIndex);
                lastFlipTime = Time.time;
            }
        }
    }
}