using UnityEngine;
using Meta.XR.BuildingBlocks.AIBlocks;

// 將 Editor 的 using 移到最上方，並用前置處理器指令包住
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AIChatTesting : MonoBehaviour
{
    [Header("Meta XR v85 Agents")]
    [Tooltip("拖入包含 Speech To Text Agent 嘅 GameObject")]
    public SpeechToTextAgent speechToTextAgent;

    [Tooltip("拖入包含 Llm Agent 嘅 GameObject")]
    public LlmAgent llmAgent;

    [Tooltip("拖入包含 Text To Speech Agent 嘅 GameObject")]
    public TextToSpeechAgent textToSpeechAgent;

    [Header("Test Settings")]
    [Tooltip("測試用嘅 NPC RAG 人設 (System Prompt)")]
    [TextArea(3, 5)]
    public string testSystemPrompt = "你是遙香，一位熱情友善的傳統面具店老闆娘。請簡短、熱情地回答。";

    private void Start()
    {
        // 1. 設定測試用嘅 System Prompt
        if (llmAgent != null)
        {
            llmAgent.SystemPrompt = testSystemPrompt;
            // 綁定 LLM 回調
            llmAgent.onResponseReceived.AddListener(OnLlmResponse);
        }

        // 2. 綁定 STT 回調
        if (speechToTextAgent != null)
        {
            speechToTextAgent.onTranscript.AddListener(OnTranscript);
        }

        Debug.Log("<color=#00FFFF>[PC Test]</color> AIChatTesting 初始化完成。請在 Inspector 按下 Start Listening 按鈕開始測試。");
    }

    private void OnDestroy()
    {
        // 解除綁定以防 Memory Leak
        if (speechToTextAgent != null) speechToTextAgent.onTranscript.RemoveListener(OnTranscript);
        if (llmAgent != null) llmAgent.onResponseReceived.RemoveListener(OnLlmResponse);
    }

    // --- Runtime Controls 函數 ---

    public void StartListening()
    {
        if (speechToTextAgent == null) return;
        Debug.Log("<color=#00FFFF>[PC Test]</color> 🔴 開始錄音...");
        speechToTextAgent.StartListening();
    }

    public void StopListening()
    {
        if (speechToTextAgent == null) return;
        Debug.Log("<color=#00FFFF>[PC Test]</color> ⏹ 停止錄音，等待 ElevenLabs 語音轉文字...");
        speechToTextAgent.StopNow();
    }

    public void ClearTranscript()
    {
        if (speechToTextAgent == null) return;
        Debug.Log("<color=#00FFFF>[PC Test]</color> ✖ 清除文字紀錄。");
        speechToTextAgent.ClearLastTranscript();
    }

    // --- 自動化流程回調 ---

    private void OnTranscript(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        Debug.Log($"<color=#55FF55>[PC Test] 聽到聲音 (Player):</color> {text}");
        
        if (llmAgent != null)
        {
            Debug.Log("<color=#00FFFF>[PC Test]</color> 正在呼叫 LLM (Qwen 0.5B)...");
            _ = llmAgent.SendPromptAsync(text);
        }
    }

    private void OnLlmResponse(string reply)
    {
        if (string.IsNullOrWhiteSpace(reply)) return;

        // 清理 LLM 可能產生的特殊 Token
        string cleanReply = reply.Replace("<|im_start|>", "").Replace("<|im_end|>", "").Replace("assistant\n", "").Trim();
        Debug.Log($"<color=#FFFF55>[PC Test] LLM 回答 (NPC):</color> {cleanReply}");

        if (textToSpeechAgent != null)
        {
            Debug.Log("<color=#00FFFF>[PC Test]</color> 正在呼叫 TTS 播放語音...");
            textToSpeechAgent.SpeakText(cleanReply);
        }
    }
}

// ==========================================================
// 自訂 Editor 面板：在 Inspector 繪製 Runtime Controls 按鈕
// ==========================================================
#if UNITY_EDITOR
[CustomEditor(typeof(AIChatTesting))]
public class AIChatTestingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 畫出原本的參數欄位
        DrawDefaultInspector();

        AIChatTesting script = (AIChatTesting)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Runtime Controls (只在 Play Mode 有效)", EditorStyles.boldLabel);

        // 如果遊戲未運行，禁用按鈕
        GUI.enabled = Application.isPlaying;

        // 橫向排列按鈕
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Start Listening", GUILayout.Height(30)))
        {
            script.StartListening();
        }
        if (GUILayout.Button("Stop Listening", GUILayout.Height(30)))
        {
            script.StopListening();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Clear Transcript", GUILayout.Height(25)))
        {
            script.ClearTranscript();
        }

        GUI.enabled = true;
    }
}
#endif