using UnityEngine;

// 將此腳本掛載在工具的「觸發點」上（例如竹籤尖端、油刷毛、醬料瓶噴口）
// 記得在該 GameObject 上加一個 Collider 並勾選 Is Trigger
public class TakoyakiTool : MonoBehaviour
{
    public enum ToolType
    {
        OilBrush,    // 油刷
        Ladle,       // 長柄勺 (麪糊)
        OctopusPiece,// 小章魚
        Skewer,      // 竹籤
        SauceBottle  // 醬料瓶
    }

    [Tooltip("定義這是什麼工具")]
    public ToolType type;

    [Tooltip("對於長柄勺或醬料瓶，是否正在倒出？")]
    public bool isPouring = false;

    private void Update()
    {
        // 針對長柄勺/醬料瓶：判斷是否向下傾斜 (v85 建議使用 transform 方向判斷以節省效能)
        if (type == ToolType.Ladle || type == ToolType.SauceBottle)
        {
            // 如果工具的 Y 軸朝下超過一定角度，視為正在倒出
            isPouring = Vector3.Dot(transform.up, Vector3.down) > 0.5f;
        }
    }
}