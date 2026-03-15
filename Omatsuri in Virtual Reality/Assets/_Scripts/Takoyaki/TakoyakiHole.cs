using System.Collections;
using UnityEngine;
using Oculus.Interaction; // Meta XR Interaction SDK v85

[RequireComponent(typeof(Collider))] 
public class TakoyakiHole : MonoBehaviour
{
    public enum HoleState
    {
        Empty,              // 空的
        Oiled,              // 已刷油
        Batter1,            // 第一次倒麪糊 (液態)
        OctopusAdded,       // 已加章魚
        HalfCooked,         // 凝固成半球
        Flipped,            // 已翻轉90度
        Batter2,            // 第二次加麪糊
        ThreeQuarterCooked, // 凝固成四分之三球
        Perfect,            // 完美球體
        Plated              // 已夾起裝盤
    }

    [Header("狀態與計時")]
    public HoleState currentState = HoleState.Empty;
    public float cookTime = 3.0f; // 麪糊凝固所需時間

    [Header("模型與特效引用")]
    public GameObject oilVFX;
    public GameObject batterFluidVFX; 
    public GameObject octopusModel;
    public GameObject halfCookedModel;
    public GameObject threeQuarterModel;
    public GameObject perfectModel;
    public GameObject perfectSaucedModel;

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip sizzleSound;
    public AudioClip dingSound;

    private int flipCount = 0;
    private Vector3 lastSkewerPosition;

    private void Start()
    {
        UpdateVisuals();
    }

    private void OnTriggerEnter(Collider other)
    {
        TakoyakiTool tool = other.GetComponent<TakoyakiTool>();
        if (tool == null) return;

        ProcessToolInteraction(tool);
    }

    private void OnTriggerStay(Collider other)
    {
        TakoyakiTool tool = other.GetComponent<TakoyakiTool>();
        if (tool != null && tool.isPouring)
        {
            // 修正：加上 TakoyakiTool. 前綴
            if (tool.type == TakoyakiTool.ToolType.Ladle)
            {
                if (currentState == HoleState.Oiled)
                    ChangeState(HoleState.Batter1);
                else if (currentState == HoleState.Flipped)
                    ChangeState(HoleState.Batter2);
            }
        }
    }

    private void ProcessToolInteraction(TakoyakiTool tool)
    {
        // 修正：所有 case 都加上 TakoyakiTool. 前綴
        switch (tool.type)
        {
            case TakoyakiTool.ToolType.OilBrush:
                if (currentState == HoleState.Empty)
                    ChangeState(HoleState.Oiled);
                break;

            case TakoyakiTool.ToolType.OctopusPiece:
                if (currentState == HoleState.Batter1 || currentState == HoleState.HalfCooked)
                {
                    Destroy(tool.gameObject); 
                    ChangeState(HoleState.OctopusAdded);
                }
                break;

            case TakoyakiTool.ToolType.Skewer:
                HandleSkewerInteraction(tool);
                break;

            case TakoyakiTool.ToolType.SauceBottle:
                if (currentState == HoleState.Perfect && tool.isPouring)
                {
                    ChangeState(HoleState.Plated);
                    UpdateVisuals(true);
                }
                break;
        }
    }

    private void HandleSkewerInteraction(TakoyakiTool skewer)
    {
        lastSkewerPosition = skewer.transform.position;

        if (currentState == HoleState.HalfCooked)
        {
            ChangeState(HoleState.Flipped);
        }
        else if (currentState == HoleState.ThreeQuarterCooked)
        {
            flipCount++;
            if (flipCount >= 2)
            {
                ChangeState(HoleState.Perfect);
                if (audioSource && dingSound) audioSource.PlayOneShot(dingSound);
            }
            else
            {
                if(threeQuarterModel != null) 
                    threeQuarterModel.transform.Rotate(90, 0, 0); 
            }
        }
        else if (currentState == HoleState.Perfect)
        {
            ChangeState(HoleState.Plated);
            Debug.Log("章魚燒已被挑起！請在此處 Instantiate 可抓取的章魚燒 Prefab。");
        }
    }

    private void ChangeState(HoleState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        UpdateVisuals();

        if (newState == HoleState.Batter1 || newState == HoleState.Batter2)
        {
            if (audioSource && sizzleSound) audioSource.PlayOneShot(sizzleSound);
            StartCoroutine(CookRoutine(newState == HoleState.Batter1 ? HoleState.HalfCooked : HoleState.ThreeQuarterCooked));
        }
    }

    private IEnumerator CookRoutine(HoleState targetState)
    {
        yield return new WaitForSeconds(cookTime);
        
        if (currentState == HoleState.Batter1 && targetState == HoleState.HalfCooked)
            ChangeState(HoleState.HalfCooked);
        else if (currentState == HoleState.Batter2 && targetState == HoleState.ThreeQuarterCooked)
            ChangeState(HoleState.ThreeQuarterCooked);
        else if (currentState == HoleState.OctopusAdded) 
            ChangeState(HoleState.HalfCooked);
    }

    private void UpdateVisuals(bool forceSauce = false)
    {
        if (oilVFX) oilVFX.SetActive(false);
        if (octopusModel) octopusModel.SetActive(false);
        if (halfCookedModel) halfCookedModel.SetActive(false);
        if (threeQuarterModel) threeQuarterModel.SetActive(false);
        if (perfectModel) perfectModel.SetActive(false);
        if (perfectSaucedModel) perfectSaucedModel.SetActive(false);
        if (batterFluidVFX) batterFluidVFX.SetActive(currentState == HoleState.Batter1 || currentState == HoleState.Batter2);

        switch (currentState)
        {
            case HoleState.Oiled:
                if (oilVFX) oilVFX.SetActive(true);
                break;
            case HoleState.OctopusAdded:
                if (halfCookedModel) halfCookedModel.SetActive(true);
                if (octopusModel) octopusModel.SetActive(true);
                break;
            case HoleState.HalfCooked:
                if (halfCookedModel) halfCookedModel.SetActive(true);
                if (octopusModel) octopusModel.SetActive(true);
                break;
            case HoleState.Flipped:
                if (halfCookedModel) halfCookedModel.SetActive(true);
                if (halfCookedModel) halfCookedModel.transform.localRotation = Quaternion.Euler(180, 0, 0);
                break;
            case HoleState.ThreeQuarterCooked:
                if (threeQuarterModel) threeQuarterModel.SetActive(true);
                break;
            case HoleState.Perfect:
                if (perfectModel) perfectModel.SetActive(true);
                break;
            case HoleState.Plated:
                if (forceSauce && perfectSaucedModel) perfectSaucedModel.SetActive(true);
                break;
        }
    }
}