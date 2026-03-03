using UnityEngine;
using Oculus.Interaction; 
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AttachMask : MonoBehaviour
{
    [Header("Setup")]
    public Transform maskAttachPoint;
    public ZoneBase startingZone;

    [Header("Visuals & Audio")]
    public MeshFilter myMeshFilter; 
    public AudioClip attachSound;

    private Rigidbody rb;
    private AudioSource audioSource;
    private List<ZoneBase> intersectedZones = new List<ZoneBase>();
    private ZoneBase targetZone; 
    
    private bool isAttached = false;
    private int originalLayer;
    private InteractableUnityEventWrapper eventWrapper;

    void Awake()
    {
        eventWrapper = GetComponent<InteractableUnityEventWrapper>();
        rb = GetComponent<Rigidbody>();
        originalLayer = gameObject.layer;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        if (myMeshFilter == null) myMeshFilter = GetComponentInChildren<MeshFilter>();
    }

    void Start()
    {
        if (startingZone != null) SnapToZone(startingZone, playSound: false);
    }

    void OnEnable()
    {
        if (eventWrapper != null)
        {
            eventWrapper.WhenSelect.AddListener(OnGrab);     
            eventWrapper.WhenUnselect.AddListener(OnRelease); 
        }
    }

    void OnDisable()
    {
        if (eventWrapper != null)
        {
            eventWrapper.WhenSelect.RemoveListener(OnGrab);
            eventWrapper.WhenUnselect.RemoveListener(OnRelease);
        }
    }

    void Update()
    {
        if (isAttached || intersectedZones.Count == 0) return;
        CalculateClosestZone();
    }

    void OnTriggerEnter(Collider other)
    {
        // Debug 1: 檢查有冇撞到嘢
        Debug.Log($"[AttachMask] OnTriggerEnter 碰到了: {other.gameObject.name}");

        ZoneBase zone = other.GetComponent<ZoneBase>();
        if (zone != null)
        {
            // Debug 2: 檢查是否找到 Zone
            Debug.Log($"[AttachMask] 發現 Zone: {zone.name}。準備驗證 Mesh...");

            if (myMeshFilter == null || myMeshFilter.sharedMesh == null)
            {
                Debug.LogError("[AttachMask] 面具身上沒有設定 MeshFilter 或 Mesh 是空的！");
                return;
            }

            bool isValid = zone.ValidateMesh(myMeshFilter.sharedMesh);
            
            // Debug 3: 檢查 ValidateMesh 是否通過
            Debug.Log($"[AttachMask] Mesh 驗證結果: {isValid}");

            if (isValid)
            {
                if (!intersectedZones.Contains(zone)) intersectedZones.Add(zone);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
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
            
            if (targetZone != null && myMeshFilter != null)
            {
                targetZone.ShowGhost(myMeshFilter.sharedMesh);
            }
        }
    }

    private void OnGrab()
    {
        if (isAttached) Detach();
    }

    private void OnRelease()
    {
        if (targetZone == null) 
        {
            foreach(var zone in intersectedZones) zone.HideGhost();
        }

        if (targetZone != null && !isAttached)
        {
            SnapToZone(targetZone, playSound: true);
        }
        // 注意：這裡移除了自己寫的 rb.isKinematic = false 邏輯
        // 因為我們現在應該依賴 Meta SDK 的 PhysicsGrabbable 組件來處理掉落！
    }

    private void SnapToZone(ZoneBase zone, bool playSound)
    {
        isAttached = true;
        if (zone != null) zone.HideGhost();

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
        transform.SetParent(null); 
        SetLayerRecursively(gameObject, originalLayer);
    }

    private void AlignTransforms(Transform targetAttach, Transform sourceAttach, Transform rootToMove)
    {
        Quaternion relativeRotation = Quaternion.Inverse(sourceAttach.rotation) * rootToMove.rotation;
        rootToMove.rotation = targetAttach.rotation * relativeRotation;
        rootToMove.position += (targetAttach.position - sourceAttach.position);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (newLayer < 0 || newLayer > 31) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
    }
}