using UnityEngine;
using System.Collections;

public class camera_mov : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private bool invertY = false;

    [Header("Rotation Limits")]
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;

    [Header("References")]
    [SerializeField] private Transform playerBody;

    private float xRotation = 0f;
    private bool canLook = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!canLook) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        if (invertY) mouseY = -mouseY;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    public IEnumerator MoveCameraToOutcome(Transform outcomePoint, camera_mov playerCamera)
    {
        if (playerCamera == null || outcomePoint == null) yield break;

        playerCamera.DisableLook();
        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;
        Vector3 targetPosition = outcomePoint.position;
        Quaternion targetRotation = outcomePoint.rotation;

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            playerCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            playerCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.position = targetPosition;
        playerCamera.transform.rotation = targetRotation;
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
            float t = elapsed / duration;
            playerCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, t);
            playerCamera.transform.rotation = Quaternion.Lerp(startRotation, originalCameraRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.position = originalCameraPosition;
        playerCamera.transform.rotation = originalCameraRotation;
        playerCamera.EnableLook();
    }

    public void EnableLook()
    {
        canLook = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void DisableLook()
    {
        canLook = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}