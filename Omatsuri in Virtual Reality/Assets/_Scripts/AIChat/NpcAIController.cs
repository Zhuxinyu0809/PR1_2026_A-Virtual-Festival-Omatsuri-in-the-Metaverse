using UnityEngine;
// 引入 Meta XR v85 AI Blocks 官方命名空間
using Meta.XR.BuildingBlocks.AIBlocks; 
using TMPro;

public class NpcAIController : MonoBehaviour
{
    [Header("Meta XR v85 Agents")]
    [Tooltip("拖入包含 Speech To Text Agent 嘅 GameObject")]
    public SpeechToTextAgent speechToTextAgent; 
    
    [Tooltip("拖入包含 Llm Agent 嘅 GameObject")]
    public LlmAgent llmAgent;          
    
    [Tooltip("拖入包含 Text To Speech Agent 嘅 GameObject")]
    public TextToSpeechAgent textToSpeechAgent; 

    [Header("NPC RAG & 身份設定")]
    [Tooltip("NPC 嘅專屬 RAG 背景文檔或系統提示詞")]
    [TextArea(5, 10)]
    public string npcRagContext = "你是遙香，一位熱情友善的傳統面具店老闆娘，在熱鬧的祭典（Omatsuri）中經營著一家面具店。你對日本祭典面具充滿熱情，並樂於以生動有趣的方式與遊客分享它們豐富的民間傳說、神話故事和精神內涵。";

    [Header("Interaction Settings")]
    [Tooltip("拖入 AI Triggers 物件上的 Collider (例如 SphereCollider)")]
    public Collider aiTriggerCollider;

    [Header("UI Settings")]
    [Tooltip("拖入用來顯示 Debug 訊息的 TextMeshProUGUI")]
    public TextMeshProUGUI debugText;

    private bool isPlayerNearby = false;
    private bool isListening = false;

    void Start()
    {
        // 1. 設定 LLM 嘅 System Prompt (NPC 人設)
        if (llmAgent != null)
        {
            llmAgent.SystemPrompt = npcRagContext;
        }

        // 2. 自動綁定 Agent 嘅回調事件 (免除 Inspector 手動拉線嘅煩惱)
        if (speechToTextAgent != null)
        {
            speechToTextAgent.onTranscript.AddListener(OnTranscript);
        }
        
        if (llmAgent != null)
        {
            llmAgent.onResponseReceived.AddListener(OnResponseReceived);
        }

        // 3. 設定玩家觸發區域 (AI Triggers)
        if (aiTriggerCollider != null)
        {
            aiTriggerCollider.isTrigger = true;
            AITriggerForwarder forwarder = aiTriggerCollider.GetComponent<AITriggerForwarder>();
            if (forwarder == null)
            {
                forwarder = aiTriggerCollider.gameObject.AddComponent<AITriggerForwarder>();
            }
            forwarder.mainController = this;
            
            UpdateDebugText("[NPC AI] Successfully configured manual AI Triggers collider and forwarder.");
        }
        else
        {
            UpdateDebugError("[NPC AI] AI Trigger Collider not assigned!");
        }
    }

    void OnDestroy()
    {
        // 養成良好習慣，銷毀時解除綁定以防 Memory Leak
        if (speechToTextAgent != null) speechToTextAgent.onTranscript.RemoveListener(OnTranscript);
        if (llmAgent != null) llmAgent.onResponseReceived.RemoveListener(OnResponseReceived);
    }

    void Update()
    {
        // 當玩家喺範圍內，撳 Quest 手掣嘅 B 掣開始錄音
        if (isPlayerNearby && OVRInput.GetDown(OVRInput.Button.Two))
        {
            StartListening();
        }
        else if (isPlayerNearby && OVRInput.GetUp(OVRInput.Button.Two))
        {
            StopListening();
        }
    }

    public void HandleTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            UpdateDebugText($"[NPC AI] Player entered {gameObject.name} area. Press and hold B button to talk.");
        }
    }

    public void HandleTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            UpdateDebugText($"[NPC AI] Player left {gameObject.name} area.");
        }
    }

    // --- 核心 AI 流程 ---

    // 1. 開始語音識別 (STT)
    private void StartListening()
    {
        if (isListening || speechToTextAgent == null) return;
        isListening = true;
        UpdateDebugText("[NPC AI] Started recording...");
        
        // 呼叫官方 API
        speechToTextAgent.StartListening();
    }

    // 2. 停止錄音並等待轉換
    private void StopListening()
    {
        if (!isListening || speechToTextAgent == null) return;
        isListening = false;
        UpdateDebugText("[NPC AI] Stopped recording, waiting for transcription...");
        
        // 呼叫官方 API
        speechToTextAgent.StopNow();
    }

    // 3. 接收到 STT 的文字結果
    private void OnTranscript(string playerText)
    {
        if (string.IsNullOrWhiteSpace(playerText)) return;
        
        UpdateDebugText($"\n<color=#55FF55>Player:</color> {playerText}");
        GenerateLlmResponse(playerText);
    }

    // 4. 結合人設，呼叫 LLM
    private void GenerateLlmResponse(string playerText)
    {
        if (llmAgent == null) return;
        UpdateDebugText("[NPC AI] Calling Large Language Model...");
        
        // 官方 API SendPromptAsync 會自動結合 SystemPrompt 同埋 userText
        _ = llmAgent.SendPromptAsync(playerText); 
    }

    // 5. 接收到 LLM 的文字結果
    private void OnResponseReceived(string llmReply)
    {
        if (string.IsNullOrWhiteSpace(llmReply)) return;

        // 清理 LLM 可能產生的特殊 Token，避免 TTS 發生錯誤或讀出亂碼
        string cleanReply = llmReply.Replace("<|im_start|>", "").Replace("<|im_end|>", "").Replace("assistant\n", "").Trim();

        UpdateDebugText($"\n<color=#FFFF55>NPC:</color> {cleanReply}");
        SpeakResponse(cleanReply);
    }

    // 6. 呼叫 TTS 播放語音
    private void SpeakResponse(string textToSpeak)
    {
        if (textToSpeechAgent == null) return;
        UpdateDebugText("[NPC AI] Calling TTS to synthesize speech...");
        
        // 呼叫官方 API
        textToSpeechAgent.SpeakText(textToSpeak);
    }

    // --- Helper Methods ---
    private string logHistory = "";

    private void UpdateDebugText(string message)
    {
        Debug.Log(message);
        if (debugText != null)
        {
            // 將新訊息附加到歷史記錄後面，避免瞬間覆蓋
            logHistory += message + "\n";
            
            // 限制字數避免 UI 爆滿 (保留最後 1500 字元)
            if (logHistory.Length > 1500)
            {
                logHistory = logHistory.Substring(logHistory.Length - 1500);
            }
            debugText.text = logHistory;
        }
    }

    private void UpdateDebugError(string message)
    {
        Debug.LogError(message);
        if (debugText != null)
        {
            logHistory += $"<color=red>{message}</color>\n";
            debugText.text = logHistory;
        }
    }
}

// 內部轉發器類別：用於掛載喺 AI Triggers 上面接收物理事件
public class AITriggerForwarder : MonoBehaviour
{
    public NpcAIController mainController;

    private void OnTriggerEnter(Collider other)
    {
        if (mainController != null) 
        {
            mainController.HandleTriggerEnter(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (mainController != null) 
        {
            mainController.HandleTriggerExit(other);
        }
    }
}