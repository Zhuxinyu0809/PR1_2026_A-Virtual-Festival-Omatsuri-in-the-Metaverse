using UnityEngine;
using System.Collections;

/// <summary>
/// 掛載在竹籤上的腳本，處理刺起章魚燒、放入嘴巴與紙碗的邏輯
/// 最新升級：取消動態角度調整，改用全套 Fixed Transform 強制鎖定 (Position, Rotation, Scale)
/// </summary>
public class TakoyakiSkewer : MonoBehaviour
{
    [Header("竹籤視覺設定")]
    [Tooltip("預先掛在竹籤底下的章魚燒子物件 (平時隱藏，刺中時顯示)")]
    public GameObject skewerTakoyakiVisual;
    
    [Tooltip("獨立的「加了醬料的章魚燒實體」Prefab (用於掉入紙碗時生成具有重力的實體)")]
    public GameObject standaloneTakoyakiPrefab;

    [Header("強制鎖定參數 (Fixed Transform)")]
    [Tooltip("固定子物件的 Local Position (防止 Unity 矩陣運算跑位)")]
    public Vector3 fixedLocalPosition = new Vector3(0f, -0.675f, 0f);
    [Tooltip("固定子物件的 Local Rotation (預設為不旋轉)")]
    public Vector3 fixedLocalRotation = Vector3.zero;
    [Tooltip("固定子物件的 Local Scale (防止極端縮放變形)")]
    public Vector3 fixedLocalScale = new Vector3(20.96902f, 524.2255f, 524.2253f);

    // 狀態追蹤：竹籤上目前是否有章魚燒
    private bool hasTakoyaki = false;

    // 處理倒數計時用的 Coroutine 參考
    private Coroutine eatCoroutine;
    private Coroutine bowlCoroutine;

    void Start()
    {
        // 遊戲開始時，確保竹籤上的章魚燒是隱藏的狀態
        if (skewerTakoyakiVisual != null)
        {
            skewerTakoyakiVisual.SetActive(false);
        }
    }

    // ==========================================
    // 物理偵測 (支援 Trigger 穿透與實體碰撞雙模式)
    // ==========================================
    private void OnTriggerEnter(Collider other)
    {
        HandlePierce(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandlePierce(collision.gameObject);
    }

    // ==========================================
    // 核心刺入邏輯
    // ==========================================
    private void HandlePierce(GameObject hitObj)
    {
        // === 情況 A：竹籤上目前沒有章魚燒，可以進行刺擊 ===
        if (!hasTakoyaki)
        {
            // 1. 嘗試從烤盤上刺起
            TakoyakiFlipTrigger trigger = hitObj.GetComponent<TakoyakiFlipTrigger>();
            if (trigger == null) trigger = hitObj.GetComponentInParent<TakoyakiFlipTrigger>();
            if (trigger == null) trigger = hitObj.GetComponentInChildren<TakoyakiFlipTrigger>();

            if (trigger != null && trigger.panManager != null)
            {
                if (trigger.panManager.TryPickUpTakoyaki(trigger.holeIndex))
                {
                    // 刺中烤盤上的章魚燒！不傳入 targetTransform 的角度了
                    PickUpTakoyaki();
                }
            }
            // 2. 嘗試從紙碗中刺起 (紙碗裡的章魚燒會有 "BowlTakoyaki" 標籤)
            else if (hitObj.CompareTag("BowlTakoyaki"))
            {
                PickUpTakoyaki();
                Destroy(hitObj); // 刪除碗裡的實體，因為畫面已經由竹籤上的子物件接管了
            }
        }
        // === 情況 B：竹籤上已經有章魚燒，可以放進嘴巴或紙碗 ===
        else
        {
            if (hitObj.CompareTag("Mouth"))
            {
                if (eatCoroutine != null) StopCoroutine(eatCoroutine);
                eatCoroutine = StartCoroutine(EatRoutine());
            }
            else if (hitObj.CompareTag("PaperBowl"))
            {
                if (bowlCoroutine != null) StopCoroutine(bowlCoroutine);
                bowlCoroutine = StartCoroutine(DropIntoBowlRoutine(hitObj.transform));
            }
        }
    }

    // ==========================================
    // 離開判定
    // ==========================================
    private void OnTriggerExit(Collider other)
    {
        CancelAction(other.gameObject);
    }

    private void OnCollisionExit(Collision collision)
    {
        CancelAction(collision.gameObject);
    }

    private void CancelAction(GameObject exitObj)
    {
        if (exitObj.CompareTag("Mouth") && eatCoroutine != null)
        {
            StopCoroutine(eatCoroutine);
            eatCoroutine = null;
        }
        else if (exitObj.CompareTag("PaperBowl") && bowlCoroutine != null)
        {
            StopCoroutine(bowlCoroutine);
            bowlCoroutine = null;
        }
    }

    // ==========================================
    // 顯示切換 (取消相對角度對齊)
    // ==========================================
    private void PickUpTakoyaki()
    {
        if (skewerTakoyakiVisual != null)
        {
            hasTakoyaki = true;
            
            // 1. 顯示竹籤下的子物件
            skewerTakoyakiVisual.SetActive(true);
        }
        else
        {
            Debug.LogError("【竹籤錯誤】請在 Inspector 中設定 SkewerTakoyakiVisual！");
        }
    }

    // ==========================================
    // 計時器邏輯
    // ==========================================
    private IEnumerator EatRoutine()
    {
        yield return new WaitForSeconds(2f); 
        if (hasTakoyaki)
        {
            hasTakoyaki = false;
            if (skewerTakoyakiVisual != null)
            {
                skewerTakoyakiVisual.SetActive(false); // 吃掉了，隱藏子物件
            }
        }
    }

    private IEnumerator DropIntoBowlRoutine(Transform bowlTransform)
    {
        yield return new WaitForSeconds(1f); 
        if (hasTakoyaki)
        {
            hasTakoyaki = false;
            
            if (skewerTakoyakiVisual != null)
            {
                // 1. 隱藏竹籤上的視覺子物件
                skewerTakoyakiVisual.SetActive(false);
                
                // 2. 在剛才視覺物件的位置與旋轉，生成一個具有物理的獨立章魚燒掉入碗中
                if (standaloneTakoyakiPrefab != null)
                {
                    GameObject droppedTako = Instantiate(standaloneTakoyakiPrefab, skewerTakoyakiVisual.transform.position, skewerTakoyakiVisual.transform.rotation);
                    droppedTako.transform.SetParent(bowlTransform);
                    
                    Rigidbody rb = droppedTako.GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = false; 

                    droppedTako.tag = "BowlTakoyaki"; 
                }
            }
        }
    }
}