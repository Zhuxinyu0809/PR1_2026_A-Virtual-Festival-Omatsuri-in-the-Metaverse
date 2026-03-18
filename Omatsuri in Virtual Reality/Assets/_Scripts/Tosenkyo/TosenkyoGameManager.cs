using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction; // 確保使用 Meta XR Interaction SDK v85
using TMPro; // 新增：用於 World Space UI 文字顯示

/// <summary>
/// Meta XR v85 - 投扇興 (Tosenkyo) 遊戲邏輯管理器
/// 負責監測物理狀態並根據傳統規則計分
/// </summary>
public class TosenkyoGameManager : MonoBehaviour
{
    [Header("Game Objects (Assign in Inspector)")]
    public Rigidbody fanRb;       // 扇子
    public Rigidbody butterflyRb; // 蝶
    public Rigidbody pillowRb;    // 枕

    [Header("Transforms for Position Checks")]
    public Transform pillowTopPoint; // 枕頭頂部中心點

    [Header("Scoring System")]
    public TosenkyoScoringSystem scoringLogic; // 新增：對計分邏輯腳本的參照

    [Header("UI Elements (Canvas)")]
    public TextMeshProUGUI meiNameText; // 顯示得分類型（銘）
    public TextMeshProUGUI scoreText;   // 顯示分數

    [Header("Game State")]
    public bool isThrowing = false;
    public bool isScoring = false;

    // 狀態常數
    private const float SLEEP_VELOCITY = 0.05f;
    // (已經將 PILLOW_FALL_ANGLE 同 DISTANCE_TOLERANCE 移至 TosenkyoScoringSystem)

    // 記錄初始位置以便重置
    private Vector3 initFanPos, initButterflyPos, initPillowPos;
    private Quaternion initFanRot, initButterflyRot, initPillowRot;

    void Start()
    {
        // 儲存初始狀態
        initFanPos = fanRb.position; initFanRot = fanRb.rotation;
        initButterflyPos = butterflyRb.position; initButterflyRot = butterflyRb.rotation;
        initPillowPos = pillowRb.position; initPillowRot = pillowRb.rotation;
    }

    void Update()
    {
        // 假設你透過 Interaction SDK 嘅 GrabInteractor 監聽釋放事件來設定 isThrowing = true
        // 這裡我們檢測如果扇子被掟出，並且所有物件都停定，就開始計分
        if (isThrowing && !isScoring)
        {
            if (AreAllObjectsSleeping())
            {
                isScoring = true;
                CalculateScore();
            }
        }
    }

    /// <summary>
    /// 觸發投擲（請綁定到 Fan 嘅 Interactable Unselect Event）
    /// </summary>
    public void OnFanThrown()
    {
        isThrowing = true;
        isScoring = false;
        
        // 新增：俾玩家同開發者知道系統已經收到投擲指令，等緊物理靜止
        if (meiNameText != null) meiNameText.text = "等待物件停定...";
        if (scoreText != null) scoreText.text = "--";
        
        Debug.Log("[Tosenkyo] 扇子已投出，等待物理系統靜止...");
    }

    /// <summary>
    /// 檢查是否所有物理物件已經完全靜止
    /// </summary>
    private bool AreAllObjectsSleeping()
    {
        bool fanStopped = fanRb.linearVelocity.magnitude < SLEEP_VELOCITY && fanRb.angularVelocity.magnitude < SLEEP_VELOCITY;
        bool butterflyStopped = butterflyRb.linearVelocity.magnitude < SLEEP_VELOCITY && butterflyRb.angularVelocity.magnitude < SLEEP_VELOCITY;
        bool pillowStopped = pillowRb.linearVelocity.magnitude < SLEEP_VELOCITY && pillowRb.angularVelocity.magnitude < SLEEP_VELOCITY;

        return fanStopped && butterflyStopped && pillowStopped;
    }

    /// <summary>
    /// 核心計分觸發 (將實際計算交給 ScoringSystem)
    /// </summary>
    private void CalculateScore()
    {
        Debug.Log("[Tosenkyo] 物理靜止，開始呼叫計分系統結算...");

        if (scoringLogic == null)
        {
            Debug.LogError("[Tosenkyo] 缺少 TosenkyoScoringSystem！請在 Inspector 中指派。");
            return;
        }

        // 從獨立腳本獲取計算結果
        TosenkyoScoreResult result = scoringLogic.Evaluate(fanRb, butterflyRb, pillowRb, pillowTopPoint);

        // 更新牆上的計分牌
        if (meiNameText != null) meiNameText.text = result.MeiName;
        if (scoreText != null) scoreText.text = result.Score.ToString() + " Pts";
    }

    /// <summary>
    /// 重置遊戲場地
    /// </summary>
    public void ResetGame()
    {
        isThrowing = false;
        isScoring = false;

        // 重置 UI 顯示
        if (meiNameText != null) meiNameText.text = "等待投擲...";
        if (scoreText != null) scoreText.text = "-- 點";

        // 重置物理狀態
        ResetRigidbody(fanRb, initFanPos, initFanRot);
        ResetRigidbody(butterflyRb, initButterflyPos, initButterflyRot);
        ResetRigidbody(pillowRb, initPillowPos, initPillowRot);
    }

    private void ResetRigidbody(Rigidbody rb, Vector3 pos, Quaternion rot)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = pos;
        rb.rotation = rot;
    }
}