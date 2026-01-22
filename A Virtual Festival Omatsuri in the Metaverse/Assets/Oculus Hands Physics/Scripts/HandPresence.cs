using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandPresence : MonoBehaviour
{
    [Header("Component References")]
    public Animator handAnimator;

    [Header("Input Actions (New Input System)")]
    [Tooltip("對應 Trigger (板機鍵) 的輸入動作")]
    public InputActionProperty triggerAction;

    [Tooltip("對應 Grip (側邊握持鍵) 的輸入動作")]
    public InputActionProperty gripAction;

    // 緩衝值，讓動畫更平滑
    private float _currentTrigger;
    private float _currentGrip;
    
    // 平滑移動速度
    public float smoothSpeed = 20f;

    void Update()
    {
        UpdateHandAnimation();
    }

    void UpdateHandAnimation()
    {
        // 讀取 Trigger 數值 (範圍 0.0 ~ 1.0)
        float targetTrigger = 0;
        // 檢查 action 是否存在且已綁定
        if (triggerAction.action != null)
        {
            targetTrigger = triggerAction.action.ReadValue<float>();
        }

        // 讀取 Grip 數值 (範圍 0.0 ~ 1.0)
        float targetGrip = 0;
        if (gripAction.action != null)
        {
            targetGrip = gripAction.action.ReadValue<float>();
        }

        // 平滑插值 (Lerp) 處理，讓手指動作不要瞬間跳變
        _currentTrigger = Mathf.MoveTowards(_currentTrigger, targetTrigger, Time.deltaTime * smoothSpeed);
        _currentGrip = Mathf.MoveTowards(_currentGrip, targetGrip, Time.deltaTime * smoothSpeed);

        // 設定 Animator 參數
        if (handAnimator != null)
        {
            handAnimator.SetFloat("Trigger", _currentTrigger);
            handAnimator.SetFloat("Grip", _currentGrip);
        }
    }
}