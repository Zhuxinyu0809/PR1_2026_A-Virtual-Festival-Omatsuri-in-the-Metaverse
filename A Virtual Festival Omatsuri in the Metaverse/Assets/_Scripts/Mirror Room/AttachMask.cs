using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AttachMask : MonoBehaviour
{
    [Header("Setup")]
    public Transform maskAttachPoint;

    [Tooltip("遊戲開始時，面具默認掛在哪個 Zone？(通常是 SnapZone)")]
    public ZoneBase startingZone; // 這裡改成 ZoneBase

    [Header("Visuals & Audio")]
    public MeshFilter myMeshFilter; 
    public AudioClip attachSound;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private AudioSource audioSource;
    
    // 使用 ZoneBase 清單來同時管理 HeadZone 和 SnapZone
    private List<ZoneBase> intersectedZones = new List<ZoneBase>();
    private ZoneBase targetZone; 
    
    private bool isAttached = false;
    private int originalLayer;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        originalLayer = gameObject.layer;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        if (GetComponent<Rigidbody>() == null) rb = gameObject.AddComponent<Rigidbody>();
        if (myMeshFilter == null) myMeshFilter = GetComponentInChildren<MeshFilter>();
    }

    void Start()
    {
        if (startingZone != null)
        {
            SnapToZone(startingZone, playSound: false);
        }
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.AddListener(OnRelease);
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.RemoveListener(OnRelease);
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }

    void Update()
    {
        if (isAttached || intersectedZones.Count == 0) return;
        CalculateClosestZone();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isAttached) return;

        // 嘗試獲取 ZoneBase (不管是 HeadZone 還是 SnapZone 都能抓到)
        ZoneBase zone = other.GetComponent<ZoneBase>();
        
        if (zone != null)
        {
            // --- 關鍵修改：先檢查這個 Zone 是否接受我的 Mesh ---
            if (zone.ValidateMesh(myMeshFilter.sharedMesh))
            {
                if (!intersectedZones.Contains(zone)) intersectedZones.Add(zone);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isAttached) return;

        ZoneBase zone = other.GetComponent<ZoneBase>();
        if (zone != null)
        {
            if (intersectedZones.Contains(zone))
            {
                intersectedZones.Remove(zone);
                if (targetZone == zone)
                {
                    targetZone.HideGhost();
                    targetZone = null;
                }
            }
        }
    }

    private void CalculateClosestZone()
    {
        ZoneBase closestZone = null;
        float minDistance = float.MaxValue;

        foreach (var zone in intersectedZones)
        {
            if (zone == null) continue;

            float dist = Vector3.Distance(maskAttachPoint.position, zone.attachPoint.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestZone = zone;
            }
        }

        if (closestZone != targetZone)
        {
            if (targetZone != null) targetZone.HideGhost();
            
            targetZone = closestZone;
            
            // 呼叫 Zone 顯示 Ghost (Zone 自己會決定是顯示固定的還是動態的)
            if (targetZone != null && myMeshFilter != null)
            {
                targetZone.ShowGhost(myMeshFilter.sharedMesh);
            }
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (isAttached) Detach();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (targetZone != null && !isAttached)
        {
            SnapToZone(targetZone, playSound: true);
        }
        else
        {
            if (rb != null && !isAttached)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                transform.SetParent(null); 
            }
        }
    }

    private void SnapToZone(ZoneBase zone, bool playSound)
    {
        isAttached = true;
        
        if (zone != null) zone.HideGhost();
        intersectedZones.Clear();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        transform.SetParent(zone.attachPoint);
        AlignTransforms(zone.attachPoint, maskAttachPoint, transform);
        SetLayerRecursively(gameObject, LayerMask.NameToLayer(zone.targetLayer));

        if (playSound && audioSource != null && attachSound != null)
        {
            audioSource.PlayOneShot(attachSound);
        }
    }

    private void Detach()
    {
        isAttached = false;
        SetLayerRecursively(gameObject, originalLayer);
        Debug.Log("Mask Detached");
    }

    private void AlignTransforms(Transform targetAttach, Transform sourceAttach, Transform rootToMove)
    {
        Quaternion targetRotation = targetAttach.rotation * Quaternion.Inverse(sourceAttach.localRotation);
        rootToMove.rotation = targetRotation;
        Vector3 positionOffset = sourceAttach.position - rootToMove.position;
        rootToMove.position = targetAttach.position - positionOffset;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (newLayer < 0 || newLayer > 31) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
    }
}