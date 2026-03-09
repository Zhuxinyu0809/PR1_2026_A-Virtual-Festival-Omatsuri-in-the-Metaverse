using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wobble : MonoBehaviour
{
    Renderer rend;
    Vector3 lastPos;
    Vector3 velocity;
    Vector3 lastRot;  
    Vector3 angularVelocity;
    
    [Header("搖晃設定 (Wobble Settings)")]
    [Tooltip("最大搖晃幅度")]
    public float MaxWobble = 0.03f;
    [Tooltip("搖晃的速度 (頻率)")]
    public float WobbleSpeed = 1f;
    [Tooltip("液體恢復平靜的速度")]
    public float Recovery = 1f;
    
    float wobbleAmountX;
    float wobbleAmountZ;
    float wobbleAmountToAddX;
    float wobbleAmountToAddZ;
    float pulse;
    float time = 0.5f;

    void Start()
    {
        // 取得物件上的 Renderer 組件，以便後續與 Material (Shader) 溝通
        rend = GetComponent<Renderer>();
    }

    private void Update()
    {
        time += Time.deltaTime;
        
        // 隨時間逐漸減少搖晃幅度 (模擬阻力與恢復平靜的過程)
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * (Recovery));
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * (Recovery));

        // 利用正弦波 (Sine Wave) 產生來回搖晃的平滑數值
        pulse = 2 * Mathf.PI * WobbleSpeed;
        wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);

        // 將計算好的搖晃數值傳遞給 Shader Graph 中的變數
        // 注意：這裡的 "_WobbleX" 和 "_WobbleZ" 必須與你 Shader Graph 中建立的 Reference 名稱完全一致！
        rend.material.SetFloat("_WobbleX", wobbleAmountX);
        rend.material.SetFloat("_WobbleZ", wobbleAmountZ);

        // --- 物理計算 ---
        
        // 計算速度 (Velocity) = (上一幀位置 - 當前位置) / 時間差
        velocity = (lastPos - transform.position) / Time.deltaTime;
        
        // 計算角速度 (Angular Velocity) = 當前旋轉角度 - 上一幀旋轉角度
        angularVelocity = transform.rotation.eulerAngles - lastRot;

        // 處理旋轉角度跨越 0 度 / 360 度的情況 (確保數值連續)
        // 例如從 359 度轉到 1 度時，不能被視為轉了 -358 度
        angularVelocity.x = ClampAngle(angularVelocity.x);
        angularVelocity.y = ClampAngle(angularVelocity.y);
        angularVelocity.z = ClampAngle(angularVelocity.z);

        // 將速度與角速度相加，並限制在最大搖晃幅度內 (Clamp)
        // 這裡把 X 軸的移動和 Z 軸的旋轉混合，讓搖晃看起來更真實
        wobbleAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
        wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);

        // 記錄當前的位置與旋轉角度，供下一幀計算使用
        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }

    // 輔助函式：確保角度計算保持在 -180 到 180 度之間
    private float ClampAngle(float angle)
    {
        if (angle > 180)
            return angle - 360;
        if (angle < -180)
            return angle + 360;
        return angle;
    }
}