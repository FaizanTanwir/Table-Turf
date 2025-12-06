using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInteractContinuous : MonoBehaviour
{
    [Header("Continuous Dwell Settings")]
    [SerializeField] private float minDistanceForNewInteraction = 0.5f;
    [SerializeField] private float waitTime = 6f; // Wait 6 seconds before firing
    
    // Reference to the original CameraInteract component
    private CameraInteract originalInteract;
    
    // Continuous dwell variables
    private Vector3 lastInteractionPosition;
    private Vector3 pendingInteractionPosition;
    private bool hasPreviousInteraction = false;
    private bool hasPendingInteraction = false;
    
    // Timing variables
    private float positionChangeTime = 0f;
    private bool isWaiting = false;

    void Start()
    {
        // Get reference to the original CameraInteract component
        originalInteract = GetComponent<CameraInteract>();
        if (originalInteract == null)
        {
            Debug.LogError("CameraInteractContinuous requires CameraInteract component on the same GameObject!");
        }
        
        lastInteractionPosition = Vector3.zero;
        pendingInteractionPosition = Vector3.zero;
    }

    void Update()
    {
        // Handle continuous dwell interaction
        if (CameraInteract.selected != null)
        {
            HandleContinuousDwell();
        }
        else
        {
            hasPreviousInteraction = false;
            hasPendingInteraction = false;
            isWaiting = false;
        }
        
        // Check if wait time has elapsed for pending interaction
        if (hasPendingInteraction && Time.time - positionChangeTime >= waitTime)
        {
            ExecutePendingInteraction();
        }
    }
    
    private void HandleContinuousDwell()
    {
        RaycastHit currentHit = CameraInteract.GetLatestHit();
        
        if (currentHit.transform != null)
        {
            Vector3 currentHitPosition = currentHit.point;
            
            // Check if this is a new position far enough from the last interaction
            if (!hasPreviousInteraction || 
                Vector3.Distance(currentHitPosition, lastInteractionPosition) >= minDistanceForNewInteraction)
            {
                if (!hasPreviousInteraction)
                {
                    // For the first interaction, let the base class handle it
                    hasPreviousInteraction = true;
                    lastInteractionPosition = currentHitPosition;
                }
                else
                {
                    // For subsequent interactions, set up pending interaction
                    if (!hasPendingInteraction || 
                        Vector3.Distance(currentHitPosition, pendingInteractionPosition) >= minDistanceForNewInteraction)
                    {
                        SetPendingInteraction(currentHitPosition);
                    }
                }
            }
        }
    }
    
    private void SetPendingInteraction(Vector3 newPosition)
    {
        pendingInteractionPosition = newPosition;
        positionChangeTime = Time.time;
        hasPendingInteraction = true;
        isWaiting = true;
        
        Debug.Log($"New position detected at: {newPosition}. Waiting {waitTime} seconds...");
        
        // Unselect to reset base class visual feedback
        UnselectAndReselect();
    }
    
    private void ExecutePendingInteraction()
    {
        if (!hasPendingInteraction) return;
        
        Interactive selectedObj = GetCurrentInteractive();
        if (selectedObj != null)
        {
            selectedObj.SendMessage("Interact");
            lastInteractionPosition = pendingInteractionPosition;
            Debug.Log($"Continuous dwell interaction executed at: {pendingInteractionPosition}");
        }
        
        // Reset pending state
        hasPendingInteraction = false;
        isWaiting = false;
    }
    
    private void UnselectAndReselect()
    {
        if (CameraInteract.selected == null) return;
        
        // Store reference to current selected object
        Interactive currentSelected = CameraInteract.selected;
        
        // Unselect by setting to null
        CameraInteract.selected = null;
        
        // Reselect after a short delay to reset base class timer
        StartCoroutine(ReselectAfterDelay(currentSelected));
    }
    
    private IEnumerator ReselectAfterDelay(Interactive interactiveObj)
    {
        // Wait for end of frame so base class Update runs with null selection
        yield return new WaitForEndOfFrame();
        
        // Reselect the object
        CameraInteract.selected = interactiveObj;
        
        Debug.Log("Base class selection reset for new position");
    }
    
    private Interactive GetCurrentInteractive()
    {
        RaycastHit currentHit = CameraInteract.GetLatestHit();
        if (currentHit.transform != null)
        {
            return currentHit.transform.GetComponent<Interactive>();
        }
        return null;
    }
    
    // Public method to get the current hit position
    public Vector3 GetCurrentHitPosition()
    {
        return CameraInteract.GetLatestHit().point;
    }
    
    // Public method to check if we're currently waiting
    public bool IsWaiting()
    {
        return isWaiting;
    }
    
    // Public method to get remaining wait time
    public float GetRemainingWaitTime()
    {
        if (!hasPendingInteraction) return 0f;
        return Mathf.Max(0f, waitTime - (Time.time - positionChangeTime));
    }
    
    // Public method to get the pending interaction position
    public Vector3 GetPendingInteractionPosition()
    {
        return hasPendingInteraction ? pendingInteractionPosition : Vector3.zero;
    }
}