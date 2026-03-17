using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 章魚燒烤盤邏輯管理器 (搭配 Meta XR Interaction SDK v85)
/// 支援三階段烘烤：半熟 -> 四分之三熟 -> 全熟 -> 淋醬料，以及多次竹籤翻轉與刺起互動
/// </summary>
public class TakoyakiPanManager : MonoBehaviour
{
    [Header("烤盤洞口與模型設定 (Holes & Models)")]
    [Tooltip("章魚燒洞的液體 Renderers")]
    public Renderer[] holeLiquidRenderers;
    [Tooltip("半熟章魚燒實體模型 GameObject")]
    public GameObject[] halfTakoyakiModels;
    [Tooltip("四分之三熟章魚燒實體模型 GameObject")]
    public GameObject[] threeQuarterTakoyakiModels;
    [Tooltip("全熟(完整)章魚燒實體模型 GameObject")]
    public GameObject[] fullTakoyakiModels;
    [Tooltip("加了醬料的全熟章魚燒實體模型 GameObject")]
    public GameObject[] saucedTakoyakiModels;

    [Header("烹飪判定與設定 (Cooking Settings)")]
    [Tooltip("液體 _Fill 大於多少視為「洞裡有加滿麵糊」")]
    public float filledThreshold = 0.550f; 
    [Tooltip("每個階段獨立的烘烤所需時間 (秒)")]
    public float cookTime = 5f;

    [Header("翻面設定 (Flip Settings)")]
    [Tooltip("需要綁定 LiquidScooper 以便翻面/刺起時重置液體狀態為空")]
    public LiquidScooper liquidScooper;

    [Header("事件 (Events)")]
    public UnityEvent OnSingleCookingStarted;
    public UnityEvent OnSingleCookingFinished;

    // 定義章魚燒的 11 個完整生命週期狀態
    private enum HoleState 
    { 
        Empty,                  // 0: 空洞
        CookingPhase1,          // 1: 第一次倒入麵糊，烘烤中 (5秒)
        HalfCooked,             // 2: 半個模型出現，等待翻面
        HalfFlipped,            // 3: 半個模型被挑過，等待第二次倒液體
        CookingPhase2,          // 4: 第二次倒入液體，烘烤中 (5秒)
        ThreeQuarterCooked,     // 5: 3/4 模型出現 (-90,90,90)，等待竹籤挑動
        ThreeQuarterFlipped1,   // 6: 3/4 模型挑過 (-30,90,90)，等待第三次倒液體
        ThreeQuarterFilled,     // 7: 3/4 模型被倒滿液體，等待第二次竹籤挑動
        CookingPhase3,          // 8: 竹籤挑過 (0,90,90)，開始第三次烘烤 (5秒)
        Finished,               // 9: 完整章魚燒完成，等待淋醬料
        Sauced                  // 10: 已經淋上醬料，等待竹籤刺起
    }

    private HoleState[] holeStates;
    private float[] cookTimers;
    private int fillPropertyID;

    void Start()
    {
        fillPropertyID = Shader.PropertyToID("_Fill");
        int holeCount = holeLiquidRenderers != null ? holeLiquidRenderers.Length : 0;
        
        holeStates = new HoleState[holeCount];
        cookTimers = new float[holeCount];

        // 隱藏所有階段的實體模型
        for (int i = 0; i < holeCount; i++)
        {
            holeStates[i] = HoleState.Empty;
            cookTimers[i] = 0f;

            if (halfTakoyakiModels != null && i < halfTakoyakiModels.Length && halfTakoyakiModels[i] != null)
                halfTakoyakiModels[i].SetActive(false);

            if (threeQuarterTakoyakiModels != null && i < threeQuarterTakoyakiModels.Length && threeQuarterTakoyakiModels[i] != null)
                threeQuarterTakoyakiModels[i].SetActive(false);

            if (fullTakoyakiModels != null && i < fullTakoyakiModels.Length && fullTakoyakiModels[i] != null)
                fullTakoyakiModels[i].SetActive(false);
                
            if (saucedTakoyakiModels != null && i < saucedTakoyakiModels.Length && saucedTakoyakiModels[i] != null)
                saucedTakoyakiModels[i].SetActive(false);
        }
    }

    void Update()
    {
        if (holeLiquidRenderers == null) return;

        for (int i = 0; i < holeLiquidRenderers.Length; i++)
        {
            if (holeLiquidRenderers[i] == null) continue;

            float currentFill = holeLiquidRenderers[i].material.GetFloat(fillPropertyID);

            switch (holeStates[i])
            {
                case HoleState.Empty:
                    if (currentFill >= filledThreshold)
                    {
                        holeStates[i] = HoleState.CookingPhase1;
                        cookTimers[i] = 0f;
                        OnSingleCookingStarted?.Invoke();
                    }
                    break;

                case HoleState.CookingPhase1:
                    cookTimers[i] += Time.deltaTime;
                    if (cookTimers[i] >= cookTime)
                    {
                        holeStates[i] = HoleState.HalfCooked;
                        holeLiquidRenderers[i].enabled = false;

                        if (halfTakoyakiModels != null && halfTakoyakiModels[i] != null)
                        {
                            halfTakoyakiModels[i].SetActive(true);
                            halfTakoyakiModels[i].transform.localEulerAngles = new Vector3(-90, 90, 0); // 依舊邏輯初始設定
                        }
                        OnSingleCookingFinished?.Invoke();
                    }
                    break;

                case HoleState.HalfFlipped:
                    if (currentFill >= filledThreshold)
                    {
                        holeStates[i] = HoleState.CookingPhase2;
                        cookTimers[i] = 0f;
                        OnSingleCookingStarted?.Invoke();
                    }
                    break;

                case HoleState.CookingPhase2:
                    cookTimers[i] += Time.deltaTime;
                    if (cookTimers[i] >= cookTime)
                    {
                        holeStates[i] = HoleState.ThreeQuarterCooked;
                        holeLiquidRenderers[i].enabled = false;

                        // 隱藏半個模型，顯示 3/4 模型
                        if (halfTakoyakiModels != null && halfTakoyakiModels[i] != null)
                            halfTakoyakiModels[i].SetActive(false);

                        if (threeQuarterTakoyakiModels != null && threeQuarterTakoyakiModels[i] != null)
                        {
                            threeQuarterTakoyakiModels[i].SetActive(true);
                            threeQuarterTakoyakiModels[i].transform.localEulerAngles = new Vector3(0, 90, 90);
                        }
                        OnSingleCookingFinished?.Invoke();
                    }
                    break;

                case HoleState.ThreeQuarterFlipped1:
                    if (currentFill >= filledThreshold)
                    {
                        holeStates[i] = HoleState.ThreeQuarterFilled;
                        // 這裡不觸發 CookingStarted，因為還要等玩家用竹籤戳第二次才會烘烤
                    }
                    break;

                case HoleState.CookingPhase3:
                    cookTimers[i] += Time.deltaTime;
                    if (cookTimers[i] >= cookTime)
                    {
                        holeStates[i] = HoleState.Finished;
                        holeLiquidRenderers[i].enabled = false;

                        // 隱藏 3/4 模型，顯示全熟模型
                        if (threeQuarterTakoyakiModels != null && threeQuarterTakoyakiModels[i] != null)
                            threeQuarterTakoyakiModels[i].SetActive(false);

                        if (fullTakoyakiModels != null && fullTakoyakiModels[i] != null)
                            fullTakoyakiModels[i].SetActive(true);
                            
                        OnSingleCookingFinished?.Invoke();
                    }
                    break;

                // 這些狀態都在等待外部 FlipTakoyaki, ApplySauce 或 TryPickUpTakoyaki 觸發
                case HoleState.HalfCooked:
                case HoleState.ThreeQuarterCooked:
                case HoleState.ThreeQuarterFilled:
                case HoleState.Finished:
                case HoleState.Sauced:
                    break;
            }
        }
    }

    /// <summary>
    /// 由 TakoyakiFlipTrigger 呼叫，根據當前狀態決定竹籤挑動的後果
    /// </summary>
    public void FlipTakoyaki(int holeIndex)
    {
        if (holeIndex < 0 || holeIndex >= holeStates.Length) return;

        HoleState state = holeStates[holeIndex];

        if (state == HoleState.HalfCooked)
        {
            // [第一次挑動] 半熟翻面
            holeStates[holeIndex] = HoleState.HalfFlipped;
            if (halfTakoyakiModels != null && halfTakoyakiModels[holeIndex] != null)
                halfTakoyakiModels[holeIndex].transform.localEulerAngles = new Vector3(0, 90, 0);

            ResetHoleLiquid(holeIndex);
        }
        else if (state == HoleState.ThreeQuarterCooked)
        {
            // [第二次挑動] 3/4熟 第一次翻面
            holeStates[holeIndex] = HoleState.ThreeQuarterFlipped1;
            if (threeQuarterTakoyakiModels != null && threeQuarterTakoyakiModels[holeIndex] != null)
                threeQuarterTakoyakiModels[holeIndex].transform.localEulerAngles = new Vector3(60, 90, 90);

            ResetHoleLiquid(holeIndex);
        }
        else if (state == HoleState.ThreeQuarterFilled)
        {
            // [第三次挑動] 3/4熟 第二次翻面，開始最終烘烤
            holeStates[holeIndex] = HoleState.CookingPhase3;
            cookTimers[holeIndex] = 0f;
            
            if (threeQuarterTakoyakiModels != null && threeQuarterTakoyakiModels[holeIndex] != null)
                threeQuarterTakoyakiModels[holeIndex].transform.localEulerAngles = new Vector3(90, 90, 90);

            // 保持液體顯示並開始倒數計時
            OnSingleCookingStarted?.Invoke();
        }
    }

    /// <summary>
    /// 供料理瓶 (SauceBottle) 呼叫。如果是全熟章魚燒，將其替換為加了醬料的模型
    /// </summary>
    public void ApplySauce(int holeIndex)
    {
        if (holeIndex < 0 || holeIndex >= holeStates.Length) return;

        if (holeStates[holeIndex] == HoleState.Finished)
        {
            holeStates[holeIndex] = HoleState.Sauced;

            if (fullTakoyakiModels != null && fullTakoyakiModels[holeIndex] != null)
                fullTakoyakiModels[holeIndex].SetActive(false);

            if (saucedTakoyakiModels != null && saucedTakoyakiModels[holeIndex] != null)
                saucedTakoyakiModels[holeIndex].SetActive(true);
        }
    }

    /// <summary>
    /// 供竹籤 (TakoyakiSkewer) 呼叫。嘗試將加了醬料的章魚燒刺起離開烤盤。
    /// 回傳 true 代表成功刺起，洞口將重置回 Empty 狀態。
    /// </summary>
    public bool TryPickUpTakoyaki(int holeIndex)
    {
        if (holeIndex < 0 || holeIndex >= holeStates.Length) return false;

        if (holeStates[holeIndex] == HoleState.Sauced)
        {
            // 隱藏醬料模型
            if (saucedTakoyakiModels != null && saucedTakoyakiModels[holeIndex] != null)
                saucedTakoyakiModels[holeIndex].SetActive(false);

            // 將洞口重置為徹底的 Empty，準備接納下一次的麵糊
            holeStates[holeIndex] = HoleState.Empty;
            cookTimers[holeIndex] = 0f;
            ResetHoleLiquid(holeIndex);

            return true; // 成功刺起
        }

        return false;
    }

    /// <summary>
    /// 輔助函式：翻面/被刺走後重置洞內液體為「空」並啟動顯示
    /// </summary>
    private void ResetHoleLiquid(int holeIndex)
    {
        if (holeLiquidRenderers != null && holeLiquidRenderers[holeIndex] != null)
            holeLiquidRenderers[holeIndex].enabled = true;

        if (liquidScooper != null)
            liquidScooper.ResetTakoyakiHole(holeIndex);
    }
}