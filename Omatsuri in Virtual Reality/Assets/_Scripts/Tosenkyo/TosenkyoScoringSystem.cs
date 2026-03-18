using UnityEngine;

/// <summary>
/// 投扇興得分結果結構
/// </summary>
public struct TosenkyoScoreResult
{
    public string MeiName;
    public int Score;
}

/// <summary>
/// Meta XR v85 - 投扇興計分邏輯系統
/// 獨立處理空間判斷與規則匹配，方便在 Inspector 調整容錯參數
/// </summary>
public class TosenkyoScoringSystem : MonoBehaviour
{
    [Header("Scoring Tolerances (容錯參數)")]
    [Tooltip("枕頭被判定為倒下的角度")]
    public float pillowFallAngle = 45f;
    [Tooltip("判定物件重疊或靠在一起的水平容錯距離 (米)")]
    public float distanceTolerance = 0.15f; 

    /// <summary>
    /// 根據物理物件目前的 Transform 計算最終得分
    /// </summary>
    public TosenkyoScoreResult Evaluate(Rigidbody fanRb, Rigidbody butterflyRb, Rigidbody pillowRb, Transform pillowTopPoint)
    {
        // 1. 基礎空間數據提取
        bool isPillowFallen = Vector3.Angle(pillowRb.transform.forward, Vector3.up) > pillowFallAngle;
        
        float fanY = fanRb.transform.position.y;
        float bflyY = butterflyRb.transform.position.y;
        float pillowTopY = pillowTopPoint.position.y;

        // 計算水平距離 (XZ平面)
        Vector2 fanXZ = new Vector2(fanRb.position.x, fanRb.position.z);
        Vector2 bflyXZ = new Vector2(butterflyRb.position.x, butterflyRb.position.z);
        Vector2 pillowXZ = new Vector2(pillowTopPoint.position.x, pillowTopPoint.position.z);

        float fanToPillowXZ = Vector2.Distance(fanXZ, pillowXZ);
        float bflyToPillowXZ = Vector2.Distance(bflyXZ, pillowXZ);
        float fanToBflyXZ = Vector2.Distance(fanXZ, bflyXZ);
        float fanToBflyDist = Vector3.Distance(fanRb.position, butterflyRb.position);

        // 2. 狀態 Flag 定義 (Fuzzy Logic 模糊判定)
        bool isBflyOnPillow = bflyY >= pillowTopY - 0.05f && bflyToPillowXZ < distanceTolerance;
        bool isBflyOnGround = bflyY < 0.06f; 
        bool isBflyLeaningPillow = !isBflyOnGround && !isBflyOnPillow && bflyToPillowXZ < 0.2f;
        bool isBflyOnFan = bflyY > fanY + 0.01f && fanToBflyXZ < 0.15f;
        bool isBflyTouchingFan = fanToBflyDist < 0.15f;

        bool isFanOnPillow = fanY >= pillowTopY - 0.05f && fanToPillowXZ < 0.15f;
        bool isFanOnGround = fanY < 0.08f;
        bool isFanLeaningPillow = !isFanOnGround && !isFanOnPillow && fanToPillowXZ < 0.25f;
        bool isFanHanging = !isFanOnGround && isFanOnPillow && fanToPillowXZ >= 0.08f;

        // 3. 模式匹配
        string meiName = "Tenarai";
        int score = 0;

        if (isPillowFallen)
        {
            meiName = "Nowaki";
            score = -30;
        }
        else if (isBflyOnPillow) 
        {
            if (isFanLeaningPillow || isFanOnPillow) {
                meiName = "Yurari";
                score = 2;
            } else if (isFanOnGround && fanToPillowXZ < 0.3f) {
                meiName = "Kotsun"; 
                score = -2;
            } else {
                meiName = "Tenarai";
                score = 0;
            }
        }
        else 
        {
            if (isFanOnPillow) 
            {
                if (isBflyOnFan) {
                    meiName = "Ukifune"; score = 20;
                } else if (isBflyLeaningPillow) {
                    meiName = "Hahakigi"; score = 80;
                } else if (isFanHanging && isBflyTouchingFan && !isBflyOnGround) {
                    meiName = "Yume no Ukihashi"; score = 100;
                } else if (isBflyTouchingFan && !isBflyOnGround) {
                    meiName = "Kiritsubo"; score = 50;
                } else if (isFanHanging && isBflyOnGround) {
                    meiName = "Hotaru"; score = 45;
                } else {
                    meiName = "Kocho"; score = 85;
                }
            }
            else if (isFanLeaningPillow) 
            {
                if (isBflyTouchingFan && !isBflyOnGround) {
                    meiName = "Yomogiu"; score = 35;
                } else if (isBflyOnFan) {
                    meiName = "Hatsune"; score = 10;
                } else if (isBflyTouchingFan && isBflyOnGround) {
                    meiName = "Maboroshi"; score = 20;
                } else {
                    meiName = "Akashi"; score = 20; 
                }
            }
            else if (isFanOnGround) 
            {
                if (isBflyTouchingFan && !isBflyOnGround) {
                    meiName = "Miotsukushi"; score = 35;
                } else if (isBflyOnFan) {
                    meiName = "Yugao"; score = 8;
                } else if (isBflyTouchingFan) {
                    meiName = "Tokonatsu"; score = 10;
                } else if (isBflyOnGround) {
                    if (fanToBflyXZ < 0.15f) {
                        meiName = "Yugiri"; score = 5;
                    } else if (fanToBflyXZ >= 0.15f && fanToBflyXZ < 0.35f) {
                        meiName = "Wakamurasaki"; score = 15;
                    } else if (fanToBflyXZ >= 0.35f && fanToBflyXZ < 0.55f) {
                        meiName = "Sawarabi"; score = 8;
                    } else {
                        meiName = "Azumaya"; score = 10;
                    }
                } else {
                    meiName = "Hanachirusato"; score = 3;
                }
            }
        }

        Debug.Log($"[Tosenkyo Logic] 結算結果: {meiName} | 得分: {score}");

        return new TosenkyoScoreResult {
            MeiName = meiName,
            Score = score
        };
    }
}