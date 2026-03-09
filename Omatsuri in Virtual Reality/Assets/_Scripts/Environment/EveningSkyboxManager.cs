using UnityEngine;

/// <summary>
/// 專為 Quest VR 優化嘅傍晚 Procedural Skybox 管理器。
/// 建議將此 Script 掛載喺 Scene 裡面嘅 Manager GameObject 或者 OVRCameraRig 上面。
/// </summary>
public class EveningSkyboxManager : MonoBehaviour
{
    [Header("Skybox Material (必須使用 Skybox/Procedural Shader)")]
    [Tooltip("請放入你建立嘅 Procedural Skybox Material")]
    public Material proceduralSkybox;

    [Header("Evening Settings (傍晚設定)")]
    [Tooltip("天空嘅色調，傍晚通常偏橙紅")]
    public Color skyTint = new Color(0.85f, 0.45f, 0.25f, 1f); // 橙紅色
    
    [Tooltip("地面顏色，傍晚通常較暗")]
    public Color groundColor = new Color(0.15f, 0.15f, 0.18f, 1f); // 暗灰藍色
    
    [Tooltip("大氣層厚度：數值大於 1 會產生黃昏/日落嘅視覺效果")]
    [Range(0f, 5f)]
    public float atmosphereThickness = 1.35f; 
    
    [Tooltip("太陽大小 (0 到 1)")]
    [Range(0f, 1f)]
    public float sunSize = 0.04f;

    [Tooltip("整體曝光度，傍晚需要稍微調低")]
    [Range(0f, 8f)]
    public float exposure = 0.75f;

    [Header("Light Source")]
    [Tooltip("場景中嘅 Directional Light (太陽光)")]
    public Light sunLight;

    void Start()
    {
        ApplyEveningSkybox();
    }

    /// <summary>
    /// 套用傍晚設定並更新環境光
    /// </summary>
    public void ApplyEveningSkybox()
    {
        if (proceduralSkybox == null)
        {
            Debug.LogWarning("[EveningSkyboxManager] 未指派 Skybox Material！");
            return;
        }

        // 檢查 Shader 係咪 Procedural
        if (proceduralSkybox.shader.name != "Skybox/Procedural")
        {
            Debug.LogError("[EveningSkyboxManager] 請確保 Material 係使用 'Skybox/Procedural' Shader！");
            return;
        }

        // 將 Material 設定為當前場景嘅 Skybox
        RenderSettings.skybox = proceduralSkybox;

        // 設定 Procedural Shader 嘅參數
        proceduralSkybox.SetColor("_SkyTint", skyTint);
        proceduralSkybox.SetColor("_GroundColor", groundColor);
        proceduralSkybox.SetFloat("_AtmosphereThickness", atmosphereThickness);
        proceduralSkybox.SetFloat("_SunSize", sunSize);
        proceduralSkybox.SetFloat("_Exposure", exposure);

        // 如果有綁定太陽光，一併調整光照角度同顏色配合傍晚
        if (sunLight != null)
        {
            // 將太陽光角度調低 (接近地平線)
            sunLight.transform.rotation = Quaternion.Euler(15f, -30f, 0f); 
            // 太陽光轉為暖色
            sunLight.color = new Color(1f, 0.8f, 0.6f);
            sunLight.intensity = 0.8f;
        }

        // 非常重要：強制 Unity 更新環境全局光照 (Global Illumination)，否則物件表面唔會反映黃昏嘅顏色
        DynamicGI.UpdateEnvironment();
        
        Debug.Log("[EveningSkyboxManager] 傍晚 Skybox 設定完成，環境光已更新。");
    }

    // 你可以喺 Inspector 更改數值後，透過 Context Menu 測試效果
    [ContextMenu("Test Evening Settings")]
    private void TestEveningSettings()
    {
        ApplyEveningSkybox();
    }
}