using UnityEngine;
using Oculus.Interaction;

public class DropZone : MonoBehaviour
{
    [Tooltip("Reference to the Tutorial Controller to notify when placed.")]
    public TutorialController tutorialController;
    
    [Header("Target Settings")]
    [Tooltip("請將作為放置區域的 Trigger Collider 拖曳到這裡")]
    public Collider targetTrigger;

    [Tooltip("請將面具物件拖曳到這裡")]
    public GameObject maskObject;

    // 用來緩存面具上的互動組件
    private IInteractableView[] maskInteractables;

    private void Start()
    {
        if (maskObject != null)
        {
            // 在開始時獲取面具上的所有互動組件 (包含子物件)
            maskInteractables = maskObject.GetComponentsInChildren<IInteractableView>(true);
        }
        else
        {
            Debug.LogWarning("DropZone: 面具物件未設定！請在 Inspector 中賦值。");
        }
    }

    private void Update()
    {
        // 確保參數都已經正確設定
        if (targetTrigger == null || maskObject == null || maskInteractables == null) return;

        // 檢查面具是否在 targetTrigger 的範圍內
        // 說明：ClosestPoint 會回傳 Collider 上距離目標最近的點。
        // 如果目標已經在 Collider 內部，它會直接回傳目標的座標。這是最精準的區域檢測方法。
        Vector3 closestPoint = targetTrigger.ClosestPoint(maskObject.transform.position);
        bool isMaskInsideTrigger = Vector3.Distance(closestPoint, maskObject.transform.position) < 0.01f;

        if (isMaskInsideTrigger)
        {
            bool isCurrentlyGrabbed = false;

            // 檢查所有互動組件，看是否有任何一個處於 Select (被抓取) 狀態
            foreach (var interactable in maskInteractables)
            {
                if (interactable.State == InteractableState.Select)
                {
                    isCurrentlyGrabbed = true;
                    break;
                }
            }

            // 如果面具在 Trigger 內，且沒有被玩家抓住
            if (!isCurrentlyGrabbed)
            {
                tutorialController.OnMaskPlaced();
                this.enabled = false; // 成功放置後停用此腳本，避免重複觸發
            }
        }
    }
}