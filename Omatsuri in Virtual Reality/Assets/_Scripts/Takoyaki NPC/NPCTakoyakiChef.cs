using System.Collections;
using UnityEngine;

/// <summary>
/// NPC 章魚燒廚師自動化腳本
/// 依賴 Unity Animation Rigging 或 Animator IK 控制手部 target
/// </summary>
public class NPCTakoyakiChef : MonoBehaviour
{
    [Header("系統關聯")]
    public TakoyakiPanManager panManager;
    
    [Header("IK 目標 (綁定到 Animation Rigging 的 Target)")]
    [Tooltip("控制 NPC 右手的 Transform")]
    public Transform rightHandIKTarget;
    [Tooltip("NPC 右手的真實骨骼 (用來把工具 Parent 上去)")]
    public Transform rightHandBone;

    [Header("工作站點 (Transform 空物件)")]
    public Transform idlePosition;       // 閒置時手的位置
    public Transform ladleRestPosition;  // 勺子放置處
    public Transform bowlPosition;       // 麵糊碗的位置
    public Transform[] panHolePositions; // 烤盤上每個洞的位置

    [Header("工具關聯")]
    public GameObject ladleTool;         // 長柄勺物件

    [Header("動畫設定")]
    public float handMoveSpeed = 2.0f;   // 手部移動速度
    public float actionDelay = 0.5f;     // 動作之間的停頓時間

    // 紀錄當前手上的工具
    private GameObject currentTool;

    void Start()
    {
        // 確保 IK Target 初始在閒置位置
        if (rightHandIKTarget != null && idlePosition != null)
        {
            rightHandIKTarget.position = idlePosition.position;
            rightHandIKTarget.rotation = idlePosition.rotation;
        }

        StartCooking();
    }

    /// <summary>
    /// 外部呼叫：開始製作章魚燒的流程
    /// </summary>
    public void StartCooking()
    {
        StartCoroutine(CookingRoutine());
    }

    // ==========================================
    // 核心流程控制 (State Machine)
    // ==========================================
    private IEnumerator CookingRoutine()
    {
        Debug.Log("NPC: 開始製作章魚燒！");

        // 1. 拿起勺子
        yield return StartCoroutine(MoveHandTo(ladleRestPosition.position, ladleRestPosition.rotation));
        GrabTool(ladleTool);
        yield return new WaitForSeconds(actionDelay);

        // 2. 舀麵糊 (移動到大碗)
        Debug.Log("NPC: 舀麵糊...");
        yield return StartCoroutine(MoveHandTo(bowlPosition.position, bowlPosition.rotation));
        // 稍微往下壓入液面
        yield return StartCoroutine(MoveHandTo(bowlPosition.position + Vector3.down * 0.1f, bowlPosition.rotation));
        yield return new WaitForSeconds(actionDelay);
        // 抬起
        yield return StartCoroutine(MoveHandTo(bowlPosition.position + Vector3.up * 0.2f, bowlPosition.rotation));

        // 3. 倒麵糊到每個洞口
        Debug.Log("NPC: 開始倒麵糊...");
        for (int i = 0; i < panHolePositions.Length; i++)
        {
            // 移動到洞口上方
            Transform targetHole = panHolePositions[i];
            Vector3 aboveHole = targetHole.position + Vector3.up * 0.15f;
            yield return StartCoroutine(MoveHandTo(aboveHole, rightHandIKTarget.rotation));

            // 傾斜手腕倒出麵糊 (觸發 LiquidScooper 的 Dot 判定)
            Quaternion pourRotation = Quaternion.Euler(targetHole.eulerAngles.x + 90f, targetHole.eulerAngles.y, targetHole.eulerAngles.z);
            yield return StartCoroutine(MoveHandTo(aboveHole, pourRotation, 3f)); // 轉快一點
            
            // 停頓讓液體流出 (給予 LiquidScooper 更新的時間)
            yield return new WaitForSeconds(0.8f);

            // 轉正手腕
            yield return StartCoroutine(MoveHandTo(aboveHole, rightHandIKTarget.rotation, 3f));
        }

        // 4. 放回勺子
        Debug.Log("NPC: 放回勺子...");
        yield return StartCoroutine(MoveHandTo(ladleRestPosition.position, ladleRestPosition.rotation));
        ReleaseTool();
        
        // 5. 回到閒置狀態
        yield return StartCoroutine(MoveHandTo(idlePosition.position, idlePosition.rotation));
        
        Debug.Log("NPC: 麵糊倒完了，等待烘烤...");
        // 這裡可以繼續擴充：等待 panManager 狀態變成 HalfCooked，然後去拿竹籤翻面...
    }

    // ==========================================
    // 基礎動作方法
    // ==========================================
    
    /// <summary>
    /// 平滑移動 IK Target 到指定位置與旋轉
    /// </summary>
    private IEnumerator MoveHandTo(Vector3 targetPos, Quaternion targetRot, float speedMultiplier = 1f)
    {
        float t = 0;
        Vector3 startPos = rightHandIKTarget.position;
        Quaternion startRot = rightHandIKTarget.rotation;

        // 計算距離來決定移動時間，避免短距離也走很久
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / (handMoveSpeed * speedMultiplier);
        if (duration < 0.1f) duration = 0.1f; // 避免除以零或太快

        while (t < 1)
        {
            t += Time.deltaTime / duration;
            // 使用 Lerp 平滑過渡
            rightHandIKTarget.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));
            rightHandIKTarget.rotation = Quaternion.Lerp(startRot, targetRot, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
    }

    /// <summary>
    /// 抓取工具：將工具 Parent 到手中，並關閉工具原本的物理
    /// </summary>
    private void GrabTool(GameObject tool)
    {
        currentTool = tool;
        Rigidbody rb = currentTool.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; // 關閉物理掉落

        // 綁定到手上
        currentTool.transform.SetParent(rightHandBone);
        // 根據你模型的軸向，這裡可能需要微調 Local Position/Rotation
        currentTool.transform.localPosition = Vector3.zero; 
        currentTool.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 釋放工具
    /// </summary>
    private void ReleaseTool()
    {
        if (currentTool != null)
        {
            currentTool.transform.SetParent(null); // 解除綁定
            Rigidbody rb = currentTool.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false; // 恢復物理
            currentTool = null;
        }
    }
}