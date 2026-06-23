using UnityEngine;

/// <summary>
/// 掛載於物件上，用於在 Console 顯示碰撞資訊。
/// 完美兼容 Meta XR Interaction SDK v85 的 GrabInteractable 物件。
/// </summary>
[RequireComponent(typeof(Collider))]
public class CollisionReporter : MonoBehaviour
{
    [Tooltip("是否在 Console 顯示實體碰撞資訊")]
    public bool logCollisions = true;

    [Tooltip("是否在 Console 顯示觸發器 (Trigger) 重疊資訊")]
    public bool logTriggers = true;

    // 當兩個帶有 Collider 且至少一方帶有非 Kinematic Rigidbody 的物件發生實體碰撞時觸發
    private void OnCollisionEnter(Collision collision)
    {
        if (logCollisions)
        {
            // collision.gameObject 係同你撞到嗰個物件
            Debug.Log($"[CollisionReporter - 實體碰撞] 💥 {gameObject.name} 撞到咗: {collision.gameObject.name}");
            
            // 你仲可以獲取碰撞點嘅法線或位置，對 VR 遊戲做打擊特效好有用
            // ContactPoint contact = collision.GetContact(0);
            // Debug.Log($"碰撞位置: {contact.point}");
        }
    }

    // 當有帶有 Rigidbody 嘅物件進入標記為 "Is Trigger" 的 Collider 範圍時觸發
    private void OnTriggerEnter(Collider other)
    {
        if (logTriggers)
        {
            // other.gameObject 係進入觸發範圍嘅物件
            Debug.Log($"[CollisionReporter - 觸發器] 👻 {gameObject.name} 同觸發器重疊: {other.gameObject.name}");
        }
    }
}