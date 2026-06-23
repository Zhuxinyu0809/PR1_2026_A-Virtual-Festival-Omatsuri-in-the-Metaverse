using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection; // 加入 Reflection 命名空間
using Meta.XR.BuildingBlocks.AIBlocks;

public class ElevenLabsKeyManager : MonoBehaviour
{
    [Header("Meta XR v85 Integration")]
    [Tooltip("請將場景中嘅 Eleven Labs Provider 拖入此欄位")]
    [SerializeField] private ElevenLabsProvider elevenLabsProvider;

    [Header("API Keys Pool")]
    [Tooltip("將你所有嘅 ElevenLabs API Keys 放入呢個 List")]
    [SerializeField] private List<string> apiKeysPool = new List<string>();

    // 用嚟標記係咪已經搵到可用嘅 Key
    public bool IsKeyReady { get; private set; } = false;

    private const string ELEVENLABS_SUBSCRIPTION_URL = "https://api.elevenlabs.io/v1/user/subscription";

    void Start()
    {
        if (elevenLabsProvider == null)
        {
            Debug.LogError("[Meta XR v85] ElevenLabsProvider 未綁定！請喺 Inspector 拖入 Provider。");
            return;
        }

        if (apiKeysPool == null || apiKeysPool.Count == 0)
        {
            Debug.LogError("API Keys Pool 係空嘅，請提供最少一個 API Key。");
            return;
        }

        // 開始自動檢測並切換 API Key 嘅 Coroutine
        StartCoroutine(FindAndAssignValidKeyRoutine());
    }

    private IEnumerator FindAndAssignValidKeyRoutine()
    {
        Debug.Log($"開始檢測 API Keys，總共有 {apiKeysPool.Count} 個 Keys 需要測試...");

        foreach (string rawKey in apiKeysPool)
        {
            // 清除可能存在嘅隱藏空白或換行符號
            string currentKey = rawKey.Trim(); 
            
            if (string.IsNullOrEmpty(currentKey)) continue;

            Debug.Log($"正在測試 Key: {MaskApiKey(currentKey)} ...");

            // 發送輕量級請求去檢查帳號狀態 (包含 Credit 狀態)
            using (UnityWebRequest request = UnityWebRequest.Get(ELEVENLABS_SUBSCRIPTION_URL))
            {
                request.SetRequestHeader("xi-api-key", currentKey);

                // 等待伺服器回應
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // 如果成功返回 200 OK，代表呢個 Key 有效且未被封鎖/用盡
                    Debug.Log($"<color=green>成功搵到可用嘅 API Key: {MaskApiKey(currentKey)}</color>");
                    
                    // 用 Reflection 強制注入 API Key，解決 CS1061 報錯問題
                    InjectApiKeyToProvider(currentKey);
                    
                    IsKeyReady = true;
                    
                    // 搵到就直接離開 Loop，唔需要再試後面嘅 Key
                    yield break; 
                }
                else
                {
                    // 401 代表權限錯誤或 Credit 用盡
                    Debug.LogWarning($"Key {MaskApiKey(currentKey)} 測試失敗 ({request.responseCode}): {request.error}。準備嘗試下一個...");
                }
            }
        }

        // 如果行到呢度，代表所有 Key 都死晒
        if (!IsKeyReady)
        {
            Debug.LogError("<color=red>所有 ElevenLabs API Keys 都已經失效或 Credit 用盡！請登入後台充值或更換新 Keys。</color>");
        }
    }

    // 利用 Reflection 動態尋找並設定 API Key 變數
    private void InjectApiKeyToProvider(string validKey)
    {
        System.Type type = elevenLabsProvider.GetType();
        bool keyInjected = false;

        // 1. 先嘗試尋找公開嘅 Property (無視大小寫，涵蓋 APIKey, ApiKey, Token 等可能性)
        string[] possibleProperties = { "APIKey", "ApiKey", "Token", "CustomApiKey" };
        foreach (var propName in possibleProperties)
        {
            PropertyInfo prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(elevenLabsProvider, validKey);
                keyInjected = true;
                break;
            }
        }

        // 2. 如果無 Public Setter，強行寫入私有欄位 (涵蓋 apiKey, _apiKey 等 Meta 常用命名)
        if (!keyInjected)
        {
            string[] possibleFields = { "apiKey", "_apiKey", "m_ApiKey", "token", "_token" };
            foreach (var fieldName in possibleFields)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field != null)
                {
                    field.SetValue(elevenLabsProvider, validKey);
                    keyInjected = true;
                    break;
                }
            }
        }

        if (keyInjected)
        {
            Debug.Log("[Meta XR v85] API Key 已經成功強行注入 Provider！");
        }
        else
        {
            Debug.LogError("[Meta XR v85] 嚴重錯誤：無法透過 Reflection 搵到 ElevenLabsProvider 嘅 API Key 欄位！請聯絡專家。");
        }
    }

    // 輔助函數：隱藏部分 API Key 避免喺 Console 直接暴露出嚟
    private string MaskApiKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 8) return "INVALID_KEY";
        return key.Substring(0, 4) + "..." + key.Substring(key.Length - 4);
    }
}