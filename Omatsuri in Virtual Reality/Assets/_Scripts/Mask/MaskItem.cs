using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// 附加在每一個可以被抓取的面具上
/// 嚴格基於 Meta Interaction SDK v85 架構
/// </summary>
[RequireComponent(typeof(Grabbable))]
[RequireComponent(typeof(Rigidbody))]
public class MaskItem : MonoBehaviour
{
    [Tooltip("面具的唯一識別碼，例如 'FoxMask', 'TenguMask'")]
    public string maskID;
    
    [Tooltip("正常的 Layer (通常是 Default)")]
    public int defaultLayer = 0; // Default
    
    [Tooltip("戴上面部後隱藏的 Layer")]
    public int hiddenLayer = 6;

    [HideInInspector] public Grabbable grabbable;
    [HideInInspector] public Rigidbody rb;
    
    // 新增：標記面具是否已經成功吸附，用來解決放開手時掉落的衝突 (Race Condition)
    [HideInInspector] public bool isSnapped = false;
    
    // 新增：用來強制鎖定吸附目標的 Transform，防止 Interaction SDK 底層覆蓋位置
    [HideInInspector] public Transform snappedTarget = null;
    
    // 新增：用來確保同一個面具不會同時觸發多個 Socket 的 Ghost Mask
    [HideInInspector] public MaskSocket currentHoveredSocket = null;
    
    // v85 最佳實踐：直接讀取 Interaction SDK 的抓取點數量
    public bool isGrabbed => grabbable.SelectingPointsCount > 0;

    private bool wasGrabbed = false;
    private PhysicsGrabbable physicsGrabbable;

    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();
        
        int layerIndex = LayerMask.NameToLayer("HiddenFaceMask");
        if (layerIndex != -1) hiddenLayer = layerIndex;

        // 關閉 Interaction SDK 自動加入的 PhysicsGrabbable
        physicsGrabbable = GetComponent<PhysicsGrabbable>();
        if (physicsGrabbable != null)
        {
            physicsGrabbable.enabled = false;
        }
    }

    private void Update()
    {
        bool currentlyGrabbed = isGrabbed;

        if (currentlyGrabbed && !wasGrabbed)
        {
            // 剛被抓起
            wasGrabbed = true;
            isSnapped = false; // 抓起時解除吸附狀態
            snappedTarget = null; // 解除位置鎖定
            
            rb.isKinematic = false;
            rb.useGravity = true;
            SetLayerRecursively(gameObject, defaultLayer);
            transform.SetParent(null);
        }
        else if (!currentlyGrabbed && wasGrabbed)
        {
            // 剛被放開
            wasGrabbed = false;
            
            // 核心修復：只有在「沒有被 Socket 吸附」的情況下，才恢復重力掉落
            if (!isSnapped)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
    }

    private void LateUpdate()
    {
        // 核心修復：在 LateUpdate 強制覆蓋位置，確保 100% 貼合 Ghost Mask，不受任何其他腳本影響
        if (isSnapped && snappedTarget != null && !isGrabbed)
        {
            transform.SetPositionAndRotation(snappedTarget.position, snappedTarget.rotation);
        }
    }

    public void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child != null)
                SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}