using UnityEngine;
using TMPro;

/// <summary>
/// 金魚撈核心管理器
/// 負責分數統計、更新 UI 以及離開區域重置遊戲的邏輯
/// </summary>
public class KingyoGameManager : MonoBehaviour
{
    [Header("UI 設定")]
    [Tooltip("用來顯示分數的 TextMeshPro 元件")]
    [SerializeField] private TMP_Text scoreText;

    [Header("遊玩區域設定 (Play Area)")]
    [Tooltip("玩家的頭部或攝影機 (例如 OVRCameraRig 的 CenterEyeAnchor)")]
    [SerializeField] private Transform playerHeadset;
    
    [Tooltip("玩家離開這個距離(半徑)就會觸發分數清零與重置")]
    [SerializeField] private float playAreaRadius = 3.0f;

    private int currentScore = 0;
    private Vector3 playAreaCenter;

    void Start()
    {
        // 將 Manager 放置的位置設定為遊玩區域的中心點
        playAreaCenter = transform.position;
        UpdateScoreUI();
    }

    void Update()
    {
        // 檢查玩家是否離開遊玩區域
        if (playerHeadset != null)
        {
            float distanceFromCenter = Vector3.Distance(playerHeadset.position, playAreaCenter);
            if (distanceFromCenter > playAreaRadius && currentScore > 0)
            {
                ResetGame();
            }
        }
    }

    /// <summary>
    /// 加分並更新 UI
    /// </summary>
    public void AddScore()
    {
        currentScore++;
        UpdateScoreUI();
        Debug.Log($"[KingyoGameManager] 得分！目前分數：{currentScore}");
        // 可選：在這裡播放得分音效
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $" Current Score: {currentScore}";
        }
    }

    /// <summary>
    /// 重置遊戲：分數清零、金魚回歸原位
    /// </summary>
    private void ResetGame()
    {
        currentScore = 0;
        UpdateScoreUI();

        // 1. 尋找場上所有的金魚並強制重置
        GoldfishController[] allFish = FindObjectsByType<GoldfishController>(FindObjectsSortMode.None);
        foreach (var fish in allFish)
        {
            fish.ResetFish();
        }

        // 2. 清理紙網內可能殘留的紀錄並恢復耐久度
        PoiCatchSystem[] allPois = FindObjectsByType<PoiCatchSystem>(FindObjectsSortMode.None);
        foreach (var poi in allPois)
        {
            poi.ResetPoiState();
        }

        Debug.Log("[KingyoGameManager] 玩家離開遊玩區域，遊戲已重置！");
    }

    // 畫出輔助線，方便在 Editor 觀察遊玩區域範圍
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, playAreaRadius);
    }
}