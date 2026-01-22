using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HandPresencePhysics : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    private Rigidbody rb;

    [Header("Ghost Hand Settings")]
    public Renderer nonPhysicalHand;
    public float showNonPhysicalHandDistance = 0.05f;

    [Header("Physical Hand Settings")]
    public Renderer physicalHandRenderer;
    private Collider[] handColliders;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        handColliders = GetComponentsInChildren<Collider>();
    }

    public void EnableHandCollider()
    {
        foreach (var item in handColliders)
        {
            item.enabled = true;
        }
    }

    public void EnableHandColliderDelay(float delay)
    {
        Invoke("EnableHandCollider", delay);
    }

    public void DisableHandCollider()
    {
        foreach (var item in handColliders)
        {
            item.enabled = false;
        }
    }

    public void EnableHandModel()
    {
        physicalHandRenderer.enabled = true;
    }

    public void DisableHandModel()
    {
        physicalHandRenderer.enabled = false;
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, target.position);

        // 如果手因為抓取而隱藏了，鬼手也隱藏
        bool isPhysicalHandVisible = physicalHandRenderer.enabled;

        // 防止瞬移後物理手被阻擋
        if (distance > 1.0f)
        {
            rb.position = target.position;
            rb.linearVelocity = Vector3.zero;
        }

        if (isPhysicalHandVisible && distance > showNonPhysicalHandDistance)
        {
            nonPhysicalHand.enabled = true;
        }
        else
        {
            nonPhysicalHand.enabled = false;
        }
    }

    void FixedUpdate()
    {
        // position
        rb.linearVelocity = (target.position - transform.position) / Time.fixedDeltaTime;

        // rotation
        Quaternion rotationDifference = target.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;

        rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
    }
}