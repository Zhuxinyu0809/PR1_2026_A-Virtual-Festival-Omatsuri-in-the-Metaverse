using UnityEngine;

/// <summary>
/// Meta XR v85 - 投扇興 (Tosenkyo) 物理屬性自動化配置
/// 確保投擲手感、空氣阻力與碰撞反應符合真實祭典體驗
/// 自動適配模型匯入時的各類 Rotation 偏移 (例如 -90 度)
/// </summary>
public class TosenkyoPhysicsSetup : MonoBehaviour
{
    [Header("Game Objects")]
    public GameObject fanObject;       // 扇子
    public GameObject pillowObject;    // 枕 (木台)
    public GameObject butterflyObject; // 蝶 (標的)
    public GameObject carpetObject;    // 紅毯 (地面)

    [Header("Center of Mass Offsets (World Downward Distance)")]
    [Tooltip("扇子重心向下的偏移量（米）。腳本會自動轉換為對應的 Local Axis，無視模型本身 Rotation")]
    public float fanCenterOfMassOffset = 0.05f; 
    [Tooltip("蝶的重心向下偏移量（米），模擬下方掛著的紅繩銅錢，使其容易掛在邊緣。")]
    public float butterflyCenterOfMassOffset = 0.08f;

    void Start()
    {
        SetupMaterials();
        SetupFanPhysics();
        SetupPillowPhysics();
        SetupButterflyPhysics();
        SetupCarpetPhysics();
    }

    private void SetupMaterials()
    {
        // 1. 紅毯材質 (高摩擦、無彈性，吸收衝擊)
        PhysicsMaterial carpetMat = new PhysicsMaterial("CarpetMaterial");
        carpetMat.dynamicFriction = 0.6f;
        carpetMat.staticFriction = 0.7f;
        carpetMat.bounciness = 0.0f; // 絕對不彈
        carpetMat.frictionCombine = PhysicsMaterialCombine.Average;
        carpetMat.bounceCombine = PhysicsMaterialCombine.Minimum;
        ApplyMaterial(carpetObject, carpetMat);

        // 2. 木頭材質 (枕)
        PhysicsMaterial woodMat = new PhysicsMaterial("WoodMaterial");
        woodMat.dynamicFriction = 0.5f;
        woodMat.staticFriction = 0.6f;
        woodMat.bounciness = 0.1f;
        ApplyMaterial(pillowObject, woodMat);

        // 3. 紙/竹材質 (扇與蝶)
        PhysicsMaterial paperMat = new PhysicsMaterial("PaperMaterial");
        paperMat.dynamicFriction = 0.3f; // 讓扇子落地後能稍微滑行
        paperMat.staticFriction = 0.4f;
        paperMat.bounciness = 0.15f; // 輕微彈性
        ApplyMaterial(fanObject, paperMat);
        ApplyMaterial(butterflyObject, paperMat);
    }

    private void SetupFanPhysics()
    {
        Rigidbody rb = GetOrAddRigidbody(fanObject);
        rb.mass = 0.03f;          // 30克 (非常輕)
        rb.linearDamping = 2.5f;           // 高空氣阻力！這是扇子會「飄」的關鍵
        rb.angularDamping = 1.5f;    // 避免在空中像直升機一樣瘋狂旋轉
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // VR 投擲物件必備，防穿模
        rb.interpolation = RigidbodyInterpolation.Interpolate;         // 讓 Quest 3 顯示平滑
        
        // 智能偏移重心：利用 InverseTransformDirection 計算出「世界正下方」在目前物件旋轉角度下對應的 Local 向量
        Vector3 localDown = fanObject.transform.InverseTransformDirection(Vector3.down);
        rb.centerOfMass = localDown * fanCenterOfMassOffset; 
    }

    private void SetupPillowPhysics()
    {
        Rigidbody rb = GetOrAddRigidbody(pillowObject);
        rb.mass = 0.35f;          // 350克 (有一定重量)
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.5f;
        rb.useGravity = true;
        // 枕頭不應輕易穿模，但速度慢，用 Discrete 即可節省效能
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete; 
    }

    private void SetupButterflyPhysics()
    {
        Rigidbody rb = GetOrAddRigidbody(butterflyObject);
        rb.mass = 0.015f;         // 15克 (極輕，被扇子掃到就會飛走)
        rb.linearDamping = 0.5f;           // 輕微空氣阻力
        rb.angularDamping = 1.0f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // 智能偏移重心：無視 (-90, 90, 0) 的奇怪旋轉，強行將重心設定在世界座標的下方
        Vector3 localDown = butterflyObject.transform.InverseTransformDirection(Vector3.down);
        rb.centerOfMass = localDown * butterflyCenterOfMassOffset;
    }

    private void SetupCarpetPhysics()
    {
        // 紅毯通常是靜態的，不需要 Rigidbody，只需確保有 Collider 並套用 Material
        Collider col = carpetObject.GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("[Tosenkyo] 紅毯缺少 Collider！請加入 BoxCollider。");
        }
    }

    // --- Helper Methods ---
    private Rigidbody GetOrAddRigidbody(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) rb = obj.AddComponent<Rigidbody>();
        return rb;
    }

    private void ApplyMaterial(GameObject obj, PhysicsMaterial mat)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.material = mat;
        }
    }
}