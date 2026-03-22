using UnityEngine;

public class TriggerNPCAnimation : MonoBehaviour 
{
    [SerializeField] private Animator npcAnimator;  // Inspector 拖 NPC 的 Animator 入去

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && npcAnimator != null)
        {
            npcAnimator.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && npcAnimator != null)
        {
            npcAnimator.enabled = false;
        }
    }
}