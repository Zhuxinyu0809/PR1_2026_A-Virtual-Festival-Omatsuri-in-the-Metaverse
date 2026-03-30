using UnityEngine;
using UnityEngine.Events;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC 对话配置")]
    [Tooltip("玩家进入范围时NPC说的第一句话")]
    public string greetingMessage = "你好，旅行者！对我说出你的问题吧。";
    [Tooltip("玩家物体的标签")]
    public string playerTag = "Player";
    
    [Header("手柄/键盘按键配置")]
    [Tooltip("触发对话的按键 (Space是空格方便电脑测试)")]
    public KeyCode talkButton = KeyCode.Space; 

    [Header("事件绑定 (在面板中拖拽赋值)")]
    public UnityEvent<string> OnPlayGreetingTTS; // 触发文字转语音 (TTS)
    public UnityEvent OnStartRecordingSTT;       // 触发开始录音 (STT)
    // 注意：我们把 Stop 事件删了，因为你使用了更聪明的“自动停止检测”

    private bool isPlayerInRange = false;
    private bool hasGreeted = false; 

    // 🌟 当有任何物体进入触发范围时
    private void OnTriggerEnter(Collider other)
    {
        // 【照妖镜代码】：只要有东西碰到绿盒子，不管是不是玩家，都强行打印出来！
        Debug.Log($"⚠️ 物理系统工作正常！NPC碰到了物体：[{other.gameObject.name}]，它的标签(Tag)是：[{other.tag}]");

        // 检查这个物体是不是玩家
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            Debug.Log("✅ 身份确认成功！真的是玩家进入了范围！");
            
            // 如果还没打过招呼，就播放招呼语
            if (!hasGreeted)
            {
                OnPlayGreetingTTS.Invoke(greetingMessage);
                hasGreeted = true; 
            }
        }
        else
        {
            Debug.Log($"❌ 身份不匹配。我需要标签为 [{playerTag}] 的物体，但来的是 [{other.tag}]");
        }
    }

    // 当玩家离开触发范围
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            hasGreeted = false; // 离开后重置，下次回来重新打招呼
            Debug.Log("玩家离开了NPC范围！");
        }
    }

    // 每一帧检测按键输入
    private void Update()
    {
        if (!isPlayerInRange) return;

        // 【按下按键】：触发一次录音 (按一下即可，无需按住)
        if (Input.GetKeyDown(talkButton))
        {
            Debug.Log("🎤 按键已按下，正在唤醒耳朵(STT)开始录音...");
            OnStartRecordingSTT.Invoke();
        }
    }
}