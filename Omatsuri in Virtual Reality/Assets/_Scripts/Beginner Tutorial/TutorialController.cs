using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("UI References")]
    public TextMeshProUGUI instructionText;
    public Image instructionImage;

    [Header("Phase 1: Controller Inputs")]
    public TutorialStep[] buttonSteps;
    private int currentButtonStepIndex = 0;

    [Header("Phase 2: Grab & Place")]
    [Tooltip("The GameObject containing the table and the mask.")]
    public GameObject tableAndMaskEnvironment;
    [Tooltip("Image to show when asking to grab the mask.")]
    public Sprite grabMaskImage;
    [Tooltip("Image to show when asking to place the mask.")]
    public Sprite placeMaskImage;

    [Header("Phase 3: Hand Tracking")]
    [Tooltip("Image to show when asking to put down controllers.")]
    public Sprite handTrackingImage;

    private TutorialPhase currentPhase = TutorialPhase.ControllerButtons;
    private bool isTransitioning = false;

    public enum InputType { LeftGrip, RightGrip, LeftTrigger, RightTrigger, LeftThumbstick, RightThumbstick }

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
        // 確保初始狀態隱藏桌子同面具
        if (tableAndMaskEnvironment != null)
            tableAndMaskEnvironment.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        if (isTransitioning) return;

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
    // 呢個 method 會由 Interaction SDK 嘅 Event Wrapper 觸發
    public void OnMaskGrabbed()
    {
        if (currentPhase == TutorialPhase.GrabMask)
        {
            currentPhase = TutorialPhase.PlaceMask;
            UpdateUI();
        }
    }

    // 呢個 method 會由我哋自訂嘅 DropZone 觸發
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
        // 喺 v85 中，當玩家放低手掣並舉起雙手，Active Controller 會自動切換為 Hands/LHand/RHand
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
        if (instructionText == null || instructionImage == null) return;

        instructionImage.gameObject.SetActive(true);

        switch (currentPhase)
        {
            case TutorialPhase.ControllerButtons:
                instructionText.text = buttonSteps[currentButtonStepIndex].promptText;
                instructionImage.sprite = buttonSteps[currentButtonStepIndex].promptImage;
                break;
            case TutorialPhase.GrabMask:
                instructionText.text = "Great! Now grab the mask on the table using the Grip button.";
                instructionImage.sprite = grabMaskImage;
                break;
            case TutorialPhase.PlaceMask:
                instructionText.text = "Place the mask into the highlighted target zone.";
                instructionImage.sprite = placeMaskImage;
                break;
            case TutorialPhase.SwitchToHands:
                instructionText.text = "Put down your controllers. Hold your hands up to activate Hand Tracking.";
                instructionImage.sprite = handTrackingImage;
                break;
            case TutorialPhase.Completed:
                instructionText.text = "Hand Tracking Activated! Tutorial Complete.";
                instructionImage.gameObject.SetActive(false);
                Debug.Log("Tutorial fully completed!");
                break;
        }
    }
}