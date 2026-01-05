using UnityEngine;

public class TestingManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform miniCamPosition;
    [SerializeField] private Transform cameraOriginalPosition;
    [SerializeField] private bool useSmoothTransition = true;
    [SerializeField] private float transitionSpeed = 2f;
    
    private bool isAtMiniCam = false; // Track current camera position
    
    void Start()
    {
        // Find camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }
        
        // Find positions if not assigned
        if (miniCamPosition == null)
        {
            GameObject miniCamObj = GameObject.Find("MiniCamPosition");
            if (miniCamObj != null)
            {
                miniCamPosition = miniCamObj.transform;
            }
        }
        
        if (cameraOriginalPosition == null)
        {
            GameObject originalCamObj = GameObject.Find("CameraOriginalPosition");
            if (originalCamObj != null)
            {
                cameraOriginalPosition = originalCamObj.transform;
            }
        }
        
        // Initialize camera to original position
        if (mainCamera != null && cameraOriginalPosition != null)
        {
            mainCamera.transform.position = cameraOriginalPosition.position;
            mainCamera.transform.rotation = cameraOriginalPosition.rotation;
            isAtMiniCam = false;
        }
    }

    void Update()
    {
        // Check for M key press to switch camera position
        if (Input.GetKeyDown(KeyCode.M))
        {
            SwitchCameraPosition();
        }
    }
    
    private void SwitchCameraPosition()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera is not assigned!");
            return;
        }
        
        Transform targetPosition = isAtMiniCam ? cameraOriginalPosition : miniCamPosition;
        
        if (targetPosition == null)
        {
            Debug.LogWarning($"Target position {(isAtMiniCam ? "CameraOriginalPosition" : "MiniCamPosition")} is not assigned!");
            return;
        }
        
        if (useSmoothTransition)
        {
            StartCoroutine(SmoothMoveCamera(targetPosition));
        }
        else
        {
            // Instant transition
            mainCamera.transform.position = targetPosition.position;
            mainCamera.transform.rotation = targetPosition.rotation;
            isAtMiniCam = !isAtMiniCam;
        }
    }
    
    private System.Collections.IEnumerator SmoothMoveCamera(Transform targetPosition)
    {
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        Vector3 targetPos = targetPosition.position;
        Quaternion targetRot = targetPosition.rotation;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * transitionSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRot, t);
            
            yield return null;
        }
        
        // Ensure we're exactly at the target
        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;
        isAtMiniCam = !isAtMiniCam;
    }
}
