using UnityEngine;
using System.Collections;
using TMPro;

public class KeyboardManager : MonoBehaviour
{
    [Header("MiniGame Settings")]
    [SerializeField] private GameObject miniGamesUI;
    [SerializeField] private GameObject miniGameStockMarket;
    [SerializeField] private float autoReturnTimer = 5f; // Time in seconds before auto-returning
    [SerializeField] private TextMeshProUGUI timerText; // Timer display text
    
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform miniCamPosition;
    [SerializeField] private Transform cameraOriginalPosition;
    [SerializeField] private bool useSmoothTransition = true;
    [SerializeField] private float transitionSpeed = 2f;
    
    private bool isAtMiniCam = false; // Track current camera position
    private Coroutine autoReturnCoroutine; // Track the auto-return coroutine
    
    // Public property to check if MiniGamesUI is active (use activeInHierarchy to account for parent inactive state)
    public bool IsMiniGameActive => miniGamesUI != null && miniGamesUI.activeInHierarchy;
    
    private void Start()
    {
        // Find MiniGamesUI GameObject if not assigned
        if (miniGamesUI == null)
        {
            GameObject foundObject = GameObject.Find("MiniGamesUI");
            if (foundObject != null)
            {
                miniGamesUI = foundObject;
            }
            else
            {
                Debug.LogWarning("MiniGamesUI GameObject not found in scene!");
            }
        }
        
        // Find MiniGameStockMarket GameObject if not assigned (optional, for reference)
        if (miniGameStockMarket == null)
        {
            GameObject foundObject = GameObject.Find("MiniGameStockMarket");
            if (foundObject != null)
            {
                miniGameStockMarket = foundObject;
            }
        }
        
        // Find TimerText if not assigned
        if (timerText == null)
        {
            GameObject timerTextObj = GameObject.Find("TimerText");
            if (timerTextObj != null)
            {
                timerText = timerTextObj.GetComponent<TextMeshProUGUI>();
                if (timerText == null)
                {
                    Debug.LogWarning("TimerText GameObject found but doesn't have TextMeshProUGUI component!");
                }
            }
        }
        
        // Hide timer text at start
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
        
        // Find camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindAnyObjectByType<Camera>();
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

    private void Update()
    {
        // Check for M key press to toggle MiniGamesUI and switch camera position
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMiniGamesUI();
            SwitchCameraPosition();
        }
    }
    
    private void ToggleMiniGamesUI()
    {
        if (miniGamesUI == null)
        {
            Debug.LogWarning("MiniGamesUI GameObject is not assigned!");
            return;
        }
        
        // Cancel any existing auto-return coroutine
        if (autoReturnCoroutine != null)
        {
            StopCoroutine(autoReturnCoroutine);
            autoReturnCoroutine = null;
        }
        
        // Toggle the active state
        bool wasActive = miniGamesUI.activeSelf;
        miniGamesUI.SetActive(!wasActive);
        
        Debug.Log($"MiniGamesUI is now {(miniGamesUI.activeSelf ? "shown" : "hidden")}");
        
        // If MiniGamesUI is now active, start the auto-return timer
        if (miniGamesUI.activeSelf)
        {
            // Show timer text
            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
            }
            autoReturnCoroutine = StartCoroutine(AutoReturnAfterDelay());
        }
        else
        {
            // Hide timer text when MiniGamesUI is deactivated
            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }
        }
    }
    
    private IEnumerator AutoReturnAfterDelay()
    {
        float elapsedTime = 0f;
        
        // Update timer display every frame
        while (elapsedTime < autoReturnTimer)
        {
            elapsedTime += Time.deltaTime;
            float remainingTime = autoReturnTimer - elapsedTime;
            
            // Update timer text
            if (timerText != null && timerText.gameObject.activeSelf)
            {
                timerText.text = $"Timer: {Mathf.Ceil(remainingTime)}";
            }
            
            yield return null;
        }
        
        // Auto-return: Hide MiniGamesUI and return camera to original position
        if (miniGamesUI != null && miniGamesUI.activeSelf)
        {
            Debug.Log("Auto-returning: Hiding MiniGamesUI and returning camera to original position");
            
            // Hide timer text
            if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }
            
            // Hide MiniGamesUI
            miniGamesUI.SetActive(false);
            
            // Return camera to original position if we're at mini cam position
            if (isAtMiniCam)
            {
                SwitchCameraPosition();
            }
        }
        
        autoReturnCoroutine = null;
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
