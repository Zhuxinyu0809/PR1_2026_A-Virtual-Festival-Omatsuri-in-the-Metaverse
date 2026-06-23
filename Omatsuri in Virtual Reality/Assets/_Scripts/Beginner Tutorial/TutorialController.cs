using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // 引入場景管理

public class TutorialController : MonoBehaviour
{
    public enum TutorialPhase
    {
        ControllerButtons,
        GrabMask,
        PlaceMask,
        SwitchToHands,
        Completed
    }

    [Header("UI References (Phase 1)")]
    public TextMeshProUGUI instructionText;
    public Image instructionImage;

    [Header("UI References (Phase 2, 3 & Transition)")]
    [Tooltip("用來顯示 Phase 2, 3 嘅文字")]
    public TextMeshProUGUI phase2And3Text;
    [Tooltip("用來顯示 3, 2, 1 倒數嘅圖片")]
    public Image countdownImage;

    [Header("Phase 1: Controller Inputs")]
    public TutorialStep[] buttonSteps;
    private int currentButtonStepIndex = 0;

    [Header("Phase 2: Grab & Place")]
    [Tooltip("The GameObject containing the table and the mask.")]
    public GameObject tableAndMaskEnvironment;

    [Header("Transition & Countdown Settings")]
    public Sprite countdown3;
    public Sprite countdown2;
    public Sprite countdown1;
    [Tooltip("倒數結束後要切換嘅場景名稱")]
    public string nextSceneName = "MainScene";

    private TutorialPhase currentPhase = TutorialPhase.ControllerButtons;
    private bool isTransitioning = false;
    private bool isTutorialCompleted = false; // 避免重複觸發完成流程

    // 加入咗 ButtonX, ButtonY, ButtonA, ButtonB
    public enum InputType { LeftGrip, RightGrip, LeftTrigger, RightTrigger, ButtonX, ButtonY, ButtonA, ButtonB, LeftThumbstick, RightThumbstick }

    [System.Serializable]
    public class TutorialStep
    {
        public string stepName;
        [TextArea] public string promptText;
        public Sprite promptImage;
        public InputType requiredInput;
    }

    void Start()
    {
        // 確保初始狀態隱藏桌子、面具、以及新加入嘅 UI 欄位
        if (tableAndMaskEnvironment != null)
            tableAndMaskEnvironment.SetActive(false);
            
        if (phase2And3Text != null)
            phase2And3Text.gameObject.SetActive(false);
            
        if (countdownImage != null)
            countdownImage.gameObject.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        if (isTransitioning || isTutorialCompleted) return;

        switch (currentPhase)
        {
            case TutorialPhase.ControllerButtons:
                if (CheckCurrentInput())
                {
                    StartCoroutine(AdvanceButtonStep());
                }
                break;

            case TutorialPhase.SwitchToHands:
                CheckForHandTracking();
                break;
        }
    }

    // --- Phase 1: Controller Buttons Logic ---
    private bool CheckCurrentInput()
    {
        if (currentButtonStepIndex >= buttonSteps.Length) return false;
        InputType required = buttonSteps[currentButtonStepIndex].requiredInput;

        switch (required)
        {
            case InputType.LeftGrip: return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.5f;
            case InputType.RightGrip: return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.5f;
            case InputType.LeftTrigger: return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) > 0.5f;
            case InputType.RightTrigger: return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.5f;
            
            case InputType.ButtonX: return OVRInput.Get(OVRInput.RawButton.X);
            case InputType.ButtonY: return OVRInput.Get(OVRInput.RawButton.Y);
            case InputType.ButtonA: return OVRInput.Get(OVRInput.RawButton.A);
            case InputType.ButtonB: return OVRInput.Get(OVRInput.RawButton.B);
            
            case InputType.LeftThumbstick: return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).magnitude > 0.5f;
            case InputType.RightThumbstick: return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).magnitude > 0.5f;
            default: return false;
        }
    }

    private IEnumerator AdvanceButtonStep()
    {
        isTransitioning = true;
        currentButtonStepIndex++;
        yield return new WaitForSeconds(0.5f);

        if (currentButtonStepIndex < buttonSteps.Length)
        {
            UpdateUI();
        }
        else
        {
            // 按鍵教學完成，進入抓取面具階段
            currentPhase = TutorialPhase.GrabMask;
            if (tableAndMaskEnvironment != null) tableAndMaskEnvironment.SetActive(true);
            UpdateUI();
        }
        isTransitioning = false;
    }

    // --- Phase 2: Grab and Place Logic ---
    public void OnMaskGrabbed()
    {
        if (currentPhase == TutorialPhase.GrabMask)
        {
            currentPhase = TutorialPhase.PlaceMask;
            UpdateUI();
        }
    }

    public void OnMaskPlaced()
    {
        if (currentPhase == TutorialPhase.PlaceMask)
        {
            currentPhase = TutorialPhase.SwitchToHands;
            UpdateUI();
        }
    }

    // --- Phase 3: Hand Tracking Logic ---
    private void CheckForHandTracking()
    {
        OVRInput.Controller activeController = OVRInput.GetActiveController();
        if (activeController == OVRInput.Controller.LHand || 
            activeController == OVRInput.Controller.RHand || 
            activeController == OVRInput.Controller.Hands)
        {
            currentPhase = TutorialPhase.Completed;
            UpdateUI();
        }
    }

    // --- UI Management ---
    private void UpdateUI()
    {
        if (instructionText == null || instructionImage == null || phase2And3Text == null) return;

        switch (currentPhase)
        {
            case TutorialPhase.ControllerButtons:
                instructionText.gameObject.SetActive(true);
                instructionImage.gameObject.SetActive(true);
                phase2And3Text.gameObject.SetActive(false);

                instructionText.text = buttonSteps[currentButtonStepIndex].promptText;
                instructionImage.sprite = buttonSteps[currentButtonStepIndex].promptImage;
                break;

            case TutorialPhase.GrabMask:
                // 進入 Phase 2 時隱藏圖片及 Phase 1 嘅文字，顯示專屬 Text
                instructionText.gameObject.SetActive(false);
                instructionImage.gameObject.SetActive(false);
                phase2And3Text.gameObject.SetActive(true);

                phase2And3Text.text = "Great! Now grab the mask on the table using the Grip button.";
                break;

            case TutorialPhase.PlaceMask:
                phase2And3Text.text = "Place the mask into the highlighted target zone.";
                break;

            case TutorialPhase.SwitchToHands:
                phase2And3Text.text = "Put down your controllers. Hold your hands up to activate Hand Tracking.";
                break;

            case TutorialPhase.Completed:
                isTutorialCompleted = true; // 標記完成，唔好再更新
                phase2And3Text.text = "Hand Tracking Activated! Tutorial Complete.";
                Debug.Log("Tutorial fully completed! Starting transition sequence...");
                
                // 開始倒數及切換場景嘅流程
                StartCoroutine(CountdownAndTransition());
                break;
        }
    }

    // --- Transition Sequence ---
    private IEnumerator CountdownAndTransition()
    {
        // 1. 等待兩秒
        yield return new WaitForSeconds(2f);

        // 2. 出現文字提醒玩家準備進入 Omatsuri
        phase2And3Text.text = "Ready to enter Omatsuri";

        // 3. 提示文字出現兩秒後消失
        yield return new WaitForSeconds(2f);
        phase2And3Text.gameObject.SetActive(false);

        // 4. 出現圖片 "3"
        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(true);
            countdownImage.sprite = countdown3;
        }

        // 5. 一秒後變為圖片 "2"
        yield return new WaitForSeconds(1f);
        if (countdownImage != null) countdownImage.sprite = countdown2;

        // 6. 一秒後變為圖片 "1"
        yield return new WaitForSeconds(1f);
        if (countdownImage != null) countdownImage.sprite = countdown1;

        // 7. 一秒後切換到另一個場景
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nextSceneName);
    }
}