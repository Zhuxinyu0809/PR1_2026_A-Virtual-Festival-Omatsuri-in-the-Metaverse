using UnityEngine;

public class SnapZone : ZoneBase
{
    [Header("Snap Requirement")]
    [Tooltip("這個架子只接受這種 Mesh 的面具")]
    public Mesh requiredMesh;

    void Start()
    {
        HideGhost();
        // 確保 Ghost Filter 已經有固定的 Mesh
        if (ghostFilter != null && requiredMesh != null)
        {
            ghostFilter.mesh = requiredMesh;
        }
    }

    // 只有當進入的面具 Mesh 等於我們要求的 Mesh 時，才回傳 true
    public override bool ValidateMesh(Mesh incomingMesh)
    {
        return incomingMesh == requiredMesh;
    }

    // 只負責開關 Renderer，不改 Mesh (因為是固定的)
    public override void ShowGhost(Mesh incomingMesh)
    {
        // 雙重確認：雖然 ValidateMesh 檢查過了，但為了安全再檢查一次
        if (incomingMesh != requiredMesh) return;

        if (ghostRenderer != null)
        {
            ghostRenderer.enabled = true;
        }
    }

    public override void HideGhost()
    {
        if (ghostRenderer != null) ghostRenderer.enabled = false;
    }
}