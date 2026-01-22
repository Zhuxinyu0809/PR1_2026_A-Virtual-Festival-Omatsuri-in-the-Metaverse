using UnityEngine;
using UnityEngine.InputSystem;

public class HandMenuToggle : MonoBehaviour
{
    [Header("UI 設定")]
    public GameObject handMenuCanvas;

    [Header("按鍵設定")]
    public InputActionProperty toggleButtonAction;

    private void Start()
    {
        // 遊戲開始時先隱藏選單
        if (handMenuCanvas != null)
        {
            handMenuCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        // 偵測按鈕是否在這一幀被按下
        if (toggleButtonAction.action != null && toggleButtonAction.action.WasPressedThisFrame())
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (handMenuCanvas == null) return;

        bool isActive = !handMenuCanvas.activeSelf;
        handMenuCanvas.SetActive(isActive);
    }
}