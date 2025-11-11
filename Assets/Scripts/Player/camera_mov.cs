using UnityEngine;
using System.Collections;

public class camera_mov : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;

    // Store original local position and rotation
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    // Store player body rotation during camera transitions
    private Quaternion storedPlayerBodyRotation;
    private Quaternion originalPlayerBodyRotation;

    void Start()
    {
        // Store original local position/rotation relative to parent
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        if (playerBody == null)
        {
            Debug.LogError("PlayerBody not assigned to camera_mov! Assign it in Inspector.");
        }
        else
        {
            // Store the original player body rotation
            originalPlayerBodyRotation = playerBody.rotation;
        }

        // Make cursor visible and free to move (for UI interactions)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // NO UPDATE METHOD - Camera does NOT respond to mouse input

    public IEnumerator MoveCameraToOutcome(Transform outcomePoint, camera_mov playerCamera)
    {
        if (playerCamera == null || outcomePoint == null) yield break;

        // Store player body rotation
        playerCamera.StorePlayerBodyRotation();

        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;
        Vector3 targetPosition = outcomePoint.position;
        Quaternion targetRotation = outcomePoint.rotation;

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            playerCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            playerCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            // Keep player body locked
            playerCamera.LockPlayerBodyRotation();

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.position = targetPosition;
        playerCamera.transform.rotation = targetRotation;

        // Keep player body locked
        playerCamera.LockPlayerBodyRotation();
    }

    public static IEnumerator ReturnCameraToPlayer(camera_mov playerCamera, Vector3 originalCameraPosition, Quaternion originalCameraRotation)
    {
        if (playerCamera == null) yield break;

        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            playerCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, t);
            playerCamera.transform.rotation = Quaternion.Slerp(startRotation, originalCameraRotation, t);
            
            // CRITICAL: Keep player body at ORIGINAL rotation during return
            if (playerCamera.playerBody != null)
            {
                playerCamera.playerBody.rotation = playerCamera.originalPlayerBodyRotation;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Set final position/rotation
        playerCamera.transform.position = originalCameraPosition;
        playerCamera.transform.rotation = originalCameraRotation;

        // Restore player body to original rotation (not synced with camera)
        if (playerCamera != null && playerCamera.playerBody != null)
        {
            playerCamera.playerBody.rotation = playerCamera.originalPlayerBodyRotation;
        }
    }

    public IEnumerator LookAtTransform(Transform focusTarget, float duration = 1.0f)
    {
        if (focusTarget == null) yield break;

        // Store player body rotation
        StorePlayerBodyRotation();

        Quaternion startRotation = transform.rotation;

        // Calculate direction to target
        Vector3 direction = focusTarget.position - transform.position;

        if (direction.sqrMagnitude <= 0f)
        {
            yield break;
        }

        // Calculate target rotation using simple LookRotation
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            
            // Smoothly rotate camera to look at target
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            // Keep player body locked at stored rotation
            LockPlayerBodyRotation();

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Set final rotation
        transform.rotation = targetRotation;

        // Keep player body locked
        LockPlayerBodyRotation();
    }

    // Store the player body's current rotation before camera movement
    private void StorePlayerBodyRotation()
    {
        if (playerBody != null)
        {
            storedPlayerBodyRotation = playerBody.rotation;
        }
    }

    // Lock player body to stored rotation (prevents flipping)
    private void LockPlayerBodyRotation()
    {
        if (playerBody != null)
        {
            playerBody.rotation = storedPlayerBodyRotation;
        }
    }

    // Restore player body rotation (not used anymore, kept for compatibility)
    private void RestorePlayerBodyRotation()
    {
        if (playerBody != null)
        {
            // Keep player at original rotation
            playerBody.rotation = originalPlayerBodyRotation;
        }
    }

    // Keep these for backward compatibility (they do nothing but prevent errors)
    public void EnableLook()
    {
        // Camera never responds to mouse - this is a no-op for compatibility
    }

    public void DisableLook()
    {
        // Camera never responds to mouse - this is a no-op for compatibility
    }

    // Helper methods to get original position/rotation
    public Vector3 GetOriginalLocalPosition()
    {
        return originalLocalPosition;
    }

    public Quaternion GetOriginalLocalRotation()
    {
        return originalLocalRotation;
    }
}