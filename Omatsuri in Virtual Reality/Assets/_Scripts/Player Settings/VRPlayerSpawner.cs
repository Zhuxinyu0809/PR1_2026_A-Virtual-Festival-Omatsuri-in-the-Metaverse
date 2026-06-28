using System.Collections;
using UnityEngine;

/// <summary>
/// 基於 Meta XR SDK v85 嘅玩家出生點控制器
/// 支援標準 Rig 移動以及進階嘅「頭部精準對齊」功能
/// </summary>
public class VRPlayerSpawner : MonoBehaviour
{
    [Header("Meta XR 參考 (v85)")]
    [Tooltip("請放入場景中嘅 OVRCameraRig 根節點 (或者 Building Block 嘅 Camera Rig)")]
    public OVRCameraRig cameraRig;

    [Header("出生點設定")]
    [Tooltip("目標出生點 (Spawn Point) - 建議創建一個 Empty GameObject 並標示方向")]
    public Transform spawnPoint;

    [Tooltip("是否在 Start 時自動傳送玩家?")]
    public bool spawnOnStart = true;

    [Header("進階對齊設定")]
    [Tooltip("【強烈建議開啟】是否要精準對齊玩家真實頭部到 Spawn Point？\n" +
             "如果為 false：只對齊 Rig 根節點 (如果玩家現實中行開咗，遊戲入面都會偏離)。\n" +
             "如果為 true：會計算 HMD 偏移量，確保玩家一擘大眼就正正喺 Spawn Point 中心。")]
    public bool alignHead = true;

    private void Start()
    {
        if (spawnOnStart && cameraRig != null && spawnPoint != null)
        {
            StartCoroutine(SpawnPlayerRoutine());
        }
    }

    /// <summary>
    /// 等待一幀以確保 Meta XR 系統已經完成第一次 HMD Tracking 初始化
    /// 否則 CenterEyeAnchor 嘅位置會係 Vector3.zero
    /// </summary>
    private IEnumerator SpawnPlayerRoutine()
    {
        // 喺 v85 架構下，等待一幀確保 OVRManager 更新 tracking pose
        yield return null; 

        MovePlayerToSpawnPoint();
    }

    public void MovePlayerToSpawnPoint()
    {
        if (cameraRig == null || spawnPoint == null) return;

        if (alignHead)
        {
            // 1. 計算頭部與 Rig Root 嘅平面偏移量 (Ignore Y axis)
            Vector3 headOffset = cameraRig.centerEyeAnchor.position - cameraRig.transform.position;
            headOffset.y = 0f; // 高度由玩家現實身高同 Floor Level 決定，唔可以強行覆蓋

            // 2. 移動 Rig 根節點以抵銷偏移
            cameraRig.transform.position = spawnPoint.position - headOffset;

            // 3. 處理旋轉 (Y軸對齊) - 確保玩家望向 Spawn Point 嘅前方
            float rotationDiff = spawnPoint.eulerAngles.y - cameraRig.centerEyeAnchor.eulerAngles.y;
            cameraRig.transform.RotateAround(cameraRig.centerEyeAnchor.position, Vector3.up, rotationDiff);
        }
        else
        {
            // 標準做法：直接對齊 Tracking Space 根節點 (Floor level)
            cameraRig.transform.position = spawnPoint.position;

            // 簡單 Y 軸旋轉對齊
            Vector3 rigRotation = cameraRig.transform.eulerAngles;
            rigRotation.y = spawnPoint.eulerAngles.y;
            cameraRig.transform.eulerAngles = rigRotation;
        }

        Debug.Log($"[VRPlayerSpawner] 玩家已成功傳送至: {spawnPoint.name} (頭部對齊: {alignHead})");
    }
}