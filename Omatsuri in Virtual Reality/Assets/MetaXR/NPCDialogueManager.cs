using UnityEngine;
using Meta.XR.BuildingBlocks.AIBlocks;  // 注意：命名空间可能需要调整

public class NPCDialogueController : MonoBehaviour
{
    [Header("AI Agents")]
    public LlmAgent llmAgent;
    public SpeechToTextAgent sttAgent;
    public TextToSpeechAgent ttsAgent;
    // public LlmAgentHelper helper;  // 可能不需要这个

    [Header("NPC 设置")]
    public AudioSource npcAudioSource;
    public string npcPersonalityPrompt = "你是一个幽默的精灵导师，讲话简短风趣，喜欢用比喻。";

    private void Start()
    {
        // 订阅事件
        if (sttAgent != null)
        {
            // 根据实际的事件名称调整，可能是 OnTranscription、OnResult 等
            // sttAgent.OnTranscript += OnPlayerSpoke;
        }

        if (llmAgent != null)
        {
            // 订阅 LLM 响应事件
            // llmAgent.OnResponse += OnLLMResponse;
        }

        if (ttsAgent != null)
        {
            // 订阅 TTS 音频就绪事件
            // ttsAgent.OnAudioReady += OnTTSClipReady;
        }
    }

    // 玩家说话后触发
    private void OnPlayerSpoke(string transcript)
    {
        if (llmAgent == null) return;

        string fullPrompt = $"{npcPersonalityPrompt}\n玩家说：{transcript}\n请用第一人称回复，保持角色一致性。";

        // 根据实际 API 调整，可能是 SetPrompt、GenerateResponse 等
        // llmAgent.SetPrompt(fullPrompt);
        // 或者
        // llmAgent.GenerateResponse(fullPrompt);
    }

    // LLM 回复后转 TTS
    private void OnLLMResponse(string response)
    {
        if (ttsAgent == null) return;

        // 根据实际 API 调整
        // ttsAgent.Speak(response);  // 或 ttsAgent.Synthesize(response);
    }

    // TTS 生成音频后播放
    private void OnTTSClipReady(AudioClip clip)
    {
        if (npcAudioSource == null) return;

        npcAudioSource.clip = clip;
        npcAudioSource.spatialBlend = 1f;
        npcAudioSource.Play();
    }

    // 手动触发对话
    public void StartConversation()
    {
        if (sttAgent != null)
        {
            // 根据实际 API 调整
            // sttAgent.StartRecording();  // 或 sttAgent.Listen();
        }
    }

    // 如果需要清理资源
    private void OnDestroy()
    {
        // 取消订阅事件
        // if (sttAgent != null) sttAgent.OnTranscript -= OnPlayerSpoke;
        // if (llmAgent != null) llmAgent.OnResponse -= OnLLMResponse;
        // if (ttsAgent != null) ttsAgent.OnAudioReady -= OnTTSClipReady;
    }
}