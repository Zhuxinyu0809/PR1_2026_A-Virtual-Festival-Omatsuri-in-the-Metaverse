using UnityEngine;

/// <summary>
/// 附加在面具牆的 Trigger 區域，以及 CenterEyeAnchor 下的臉部 Trigger 區域
/// </summary>
[RequireComponent(typeof(Collider))]
public class MaskSocket : MonoBehaviour
{
    [Tooltip("這個 Socket 接受的面具 ID。如果留空或設為 'Any'，代表接受所有面具")]
    public string acceptedMaskID = "Any";
    
    [Tooltip("半透明的提示模型 (Ghost Visual)")]
    public GameObject ghostVisual;

    [Tooltip("吸附後是否將面具設為隱藏 Layer？(正前方建議打勾，側邊可依需求決定)")]
    public bool hideWhenSnapped = false;

    [Tooltip("遊戲開始時，預設掛在這個 Socket 上的面具（選填）")]
    public MaskItem startingMask;

    private MaskItem currentHoveredMask;
    
    // 用來計算進入 Trigger 的碰撞體數量，防止手部微小抖動導致的 Ghost 閃爍
    private int triggerCount = 0;

    private void Start()
    {
        if (ghostVisual != null) ghostVisual.SetActive(false);
        GetComponent<Collider>().isTrigger = true;

        if (startingMask != null)
        {
            SnapMask(startingMask);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        MaskItem mask = other.GetComponentInParent<MaskItem>();
        if (mask != null && (acceptedMaskID == "Any" || mask.maskID == acceptedMaskID))
        {
            // 【防衝突機制】：確保面具沒有被其他 Socket (例如另一個臉部位置) 佔用
            if (mask.currentHoveredSocket == null || mask.currentHoveredSocket == this)
            {
                if (currentHoveredMask == null)
                {
                    currentHoveredMask = mask;
                    mask.currentHoveredSocket = this; // 宣告佔用
                    triggerCount = 0;
                    
                    // 【新增：根據進入的面具動態改變 Ghost 的形狀】
                    UpdateDynamicGhost(mask);
                }
                
                if (mask == currentHoveredMask)
                {
                    triggerCount++;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MaskItem mask = other.GetComponentInParent<MaskItem>();
        if (mask != null && mask == currentHoveredMask)
        {
            triggerCount--;
            
            // 只有當面具的「所有碰撞體」都離開 Trigger 時，才真正清空狀態
            if (triggerCount <= 0)
            {
                currentHoveredMask.currentHoveredSocket = null; // 釋放佔用
                currentHoveredMask = null;
                triggerCount = 0;
                if (ghostVisual != null) ghostVisual.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (currentHoveredMask != null)
        {
            if (currentHoveredMask.isGrabbed)
            {
                // 玩家正在抓著，確保顯示 Ghost
                if (ghostVisual != null && !ghostVisual.activeSelf) 
                {
                    ghostVisual.SetActive(true);
                }
            }
            else
            {
                // 玩家沒抓著了（剛放開手）
                if (ghostVisual != null && ghostVisual.activeSelf) 
                {
                    ghostVisual.SetActive(false); // 關閉 Ghost
                }

                // 如果還沒有被標記為吸附，則執行吸附
                if (!currentHoveredMask.isSnapped)
                {
                    SnapMask(currentHoveredMask);
                }
            }
        }
    }

    private void SnapMask(MaskItem mask)
    {
        mask.isSnapped = true; 
        
        Transform targetTransform = (ghostVisual != null) ? ghostVisual.transform : this.transform;
        mask.snappedTarget = targetTransform; 
        
        mask.rb.isKinematic = true;
        mask.rb.useGravity = false;
        
        mask.rb.velocity = Vector3.zero; 
        mask.rb.angularVelocity = Vector3.zero;

        mask.transform.SetParent(this.transform);
        mask.transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);

        if (hideWhenSnapped)
        {
            mask.SetLayerRecursively(mask.gameObject, mask.hiddenLayer);
        }
        else
        {
            // 如果不需要隱藏（例如側邊面具想要被看到），確保恢復為 defaultLayer
            mask.SetLayerRecursively(mask.gameObject, mask.defaultLayer);
        }
    }

    // 【新增方法】：動態抓取面具的 Mesh 並替換給 Ghost
    private void UpdateDynamicGhost(MaskItem mask)
    {
        if (ghostVisual == null || mask == null) return;

        Mesh targetMesh = null;

        // 1. 嘗試從面具中取得普通的 MeshFilter
        MeshFilter maskMf = mask.GetComponentInChildren<MeshFilter>();
        if (maskMf != null && maskMf.sharedMesh != null)
        {
            targetMesh = maskMf.sharedMesh;
        }
        else
        {
            // 2. 如果面具是有動畫/Blendshape 的，嘗試取得 SkinnedMeshRenderer
            SkinnedMeshRenderer maskSmr = mask.GetComponentInChildren<SkinnedMeshRenderer>();
            if (maskSmr != null && maskSmr.sharedMesh != null)
            {
                targetMesh = maskSmr.sharedMesh;
            }
        }

        // 3. 如果成功取得面具的 Mesh，將其套用到 Ghost 的 MeshFilter 上
        if (targetMesh != null)
        {
            MeshFilter ghostMf = ghostVisual.GetComponentInChildren<MeshFilter>();
            if (ghostMf != null)
            {
                ghostMf.sharedMesh = targetMesh;
            }
            else
            {
                // 防呆機制：如果 ghostVisual 原本沒有 MeshFilter，自動補上
                ghostMf = ghostVisual.AddComponent<MeshFilter>();
                ghostMf.sharedMesh = targetMesh;
            }
        }
    }
}