using UnityEngine;
using System.Collections;

public class camera_mov : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Quaternion storedPlayerBodyRotation;
    private Quaternion originalPlayerBodyRotation;

    void Start()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        if (playerBody == null)
        {
            Debug.LogError("PlayerBody not assigned to camera_mov! Assign it in Inspector.");
        }
        else
        {
            originalPlayerBodyRotation = playerBody.rotation;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public IEnumerator MoveCameraToOutcome(Transform outcomePoint, camera_mov playerCamera)
    {
        if (playerCamera == null || outcomePoint == null) yield break;

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

            playerCamera.LockPlayerBodyRotation();

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.position = targetPosition;
        playerCamera.transform.rotation = targetRotation;

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

        if (playerCamera != null && playerCamera.playerBody != null)
        {
            playerCamera.playerBody.rotation = playerCamera.originalPlayerBodyRotation;
        }
    }

    public IEnumerator LookAtTransform(Transform focusTarget, float duration = 1.0f)
    {
        if (focusTarget == null) yield break;

        StorePlayerBodyRotation();

        Quaternion startRotation = transform.rotation;

        // Calculate direction to target
        Vector3 direction = focusTarget.position - transform.position;

        if (direction.sqrMagnitude <= 0f)
        {
            yield break;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            LockPlayerBodyRotation();

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        LockPlayerBodyRotation();
    }

    private void StorePlayerBodyRotation()
    {
        if (playerBody != null)
        {
            storedPlayerBodyRotation = playerBody.rotation;
        }
    }

    private void LockPlayerBodyRotation()
    {
        if (playerBody != null)
        {
            playerBody.rotation = storedPlayerBodyRotation;
        }
    }



    public void EnableLook()
    {
    }

    public void DisableLook()
    {
    }

    public Vector3 GetOriginalLocalPosition()
    {
        return originalLocalPosition;
    }

    public Quaternion GetOriginalLocalRotation()
    {
        return originalLocalRotation;
    }
}