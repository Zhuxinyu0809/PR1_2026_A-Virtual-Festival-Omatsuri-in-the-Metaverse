using UnityEngine;

/// <summary>
/// 醬料瓶擠出判定 (搭配 Meta XR Interaction SDK v85 使用)
/// 使用 SphereCast 確保在 VR 空間中更容易命中章魚燒
/// </summary>
public class SauceBottle : MonoBehaviour
{
    public TakoyakiPanManager panManager;
    
    [Header("醬料擠出設定")]
    [Tooltip("醬料擠出的判斷基準點 (請放置在瓶口位置)")]
    public Transform spout;
    [Tooltip("瓶口朝下超過此角度(內積值)即算倒醬料，0 為水平，-1 為垂直朝下")]
    public float pourThreshold = 0f; 
    [Tooltip("擠出醬料的最大有效距離 (公尺)")]
    public float pourDistance = 0.4f;
    [Tooltip("醬料的粗細半徑 (使用 SphereCast，數值越大越容易命中)")]
    public float sauceRadius = 0.08f;

    [Header("除錯 (Debug)")]
    [Tooltip("開啟後可在 Unity Scene 視窗看到醬料的紅色判定線")]
    public bool showDebugRay = true;

    // 避免一秒內執行過多次判定
    private float lastPourTime = 0f;
    private float pourCooldown = 0.2f;

    void Update()
    {
        // 判斷瓶口是否朝下
        if (Vector3.Dot(spout.up, Vector3.up) < pourThreshold)
        {
            if (showDebugRay)
            {
                // 在 Scene 畫出紅線，幫助開發者確認方向和距離是否正確
                Debug.DrawRay(spout.position, Vector3.down * pourDistance, Color.red);
            }

            // 冷卻時間控制
            if (Time.time - lastPourTime < pourCooldown) return;

            // 使用 SphereCastAll 往下偵測 (帶有粗細的圓柱體範圍)
            RaycastHit[] hits = Physics.SphereCastAll(spout.position, sauceRadius, Vector3.down, pourDistance);
            
            foreach (RaycastHit hit in hits)
            {
                // 嘗試在目標本身、父物件、子物件尋找 TakoyakiFlipTrigger
                TakoyakiFlipTrigger trigger = hit.collider.GetComponent<TakoyakiFlipTrigger>();
                if (trigger == null) trigger = hit.collider.GetComponentInParent<TakoyakiFlipTrigger>();
                if (trigger == null) trigger = hit.collider.GetComponentInChildren<TakoyakiFlipTrigger>();

                if (trigger != null && panManager != null)
                {
                    // 呼叫 PanManager，嘗試淋上醬料
                    panManager.ApplySauce(trigger.holeIndex);
                    lastPourTime = Time.time;
                }
            }
        }
    }
}