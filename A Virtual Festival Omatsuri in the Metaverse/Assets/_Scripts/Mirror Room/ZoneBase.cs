using UnityEngine;

public abstract class ZoneBase : MonoBehaviour
{
    [Header("Base Settings")]
    public Transform attachPoint;
    public string targetLayer = "Default"; // HeadZone 設為 Virtual Head，SnapZone 設為 Default

    [Header("Ghost Visuals")]
    public MeshRenderer ghostRenderer;
    public MeshFilter ghostFilter;

    // 每個子類別必須自己實作這兩個功能
    public abstract bool ValidateMesh(Mesh incomingMesh); // 檢查這個面具能不能放這裡
    public abstract void ShowGhost(Mesh incomingMesh);    // 顯示 Ghost
    public abstract void HideGhost();                     // 隱藏 Ghost
}