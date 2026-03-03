using UnityEngine;

public class HeadZone : ZoneBase
{
    void Start()
    {
        HideGhost();
        targetLayer = "Virtual Head"; // 強制設定 Layer
    }

    // 頭部接受任何面具
    public override bool ValidateMesh(Mesh incomingMesh)
    {
        return true; 
    }

    // 頭部的 Ghost 會變成面具的樣子
    public override void ShowGhost(Mesh incomingMesh)
    {
        if (ghostRenderer != null && ghostFilter != null)
        {
            ghostFilter.mesh = incomingMesh; // 動態更換
            ghostRenderer.enabled = true;
        }
    }

    public override void HideGhost()
    {
        if (ghostRenderer != null) ghostRenderer.enabled = false;
        if (ghostFilter != null) ghostFilter.mesh = null;
    }
}