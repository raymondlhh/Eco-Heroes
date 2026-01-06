using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Spawner References")]
    [SerializeField] private Transform firstSpawner;
    [SerializeField] private Transform secondSpawner;
    
    [Header("Dice References")]
    [SerializeField] private DiceController firstDice;
    [SerializeField] private DiceController secondDice;
    [SerializeField] private GameObject dicePrefab;
    
    [Header("Dice Settings")]
    [SerializeField] private float diceCheckInterval = 0.1f;
    
    [Header("Debug Settings")]
    [SerializeField] private bool IsDebugging = false; // Enable debug mode to use fixed movement steps
    [SerializeField] private int debugFixedSteps = 1; // Fixed number of steps to move when IsDebugging is true
    
    [Header("Player Reference")]
    [SerializeField] private PlayerController player;
    
    [Header("Card Manager Reference")]
    [SerializeField] private CardsManager cardsManager;
    
    [Header("UI References")]
    [SerializeField] private GameObject miniGamesUI;
    
    private int diceSum = 0;
    private bool isRolling = false;
    private float lastCheckTime = 0f;
    private DebugManager keyboardManager;
    private bool isProcessingDiceResult = false;
    
    public int DiceSum => diceSum;
    public bool IsRolling => isRolling;
    public bool CanRollDice => !isRolling && !isProcessingDiceResult && (cardsManager == null || !cardsManager.IsCardAnimating) && (player == null || !player.IsMoving) && !IsMiniGameActive();
    
    // Display current dice values
    public int FirstDiceValue => firstDice != null ? firstDice.CurrentValue : 0;
    public int SecondDiceValue => secondDice != null ? secondDice.CurrentValue : 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find spawners if not assigned
        if (firstSpawner == null || secondSpawner == null)
        {
            FindSpawners();
        }
        
        // Load dice prefab if not assigned
        if (dicePrefab == null)
        {
            LoadDicePrefab();
        }
        
        // Always spawn dice at start (remove any existing dice first)
        SpawnDice();
        
        // Find player if not assigned
        if (player == null)
        {
            FindPlayer();
        }
        
        // Find CardsManager if not assigned
        if (cardsManager == null)
        {
            cardsManager = FindAnyObjectByType<CardsManager>();
        }
        
        // Find KeyboardManager to check MiniGameStockMarket status
        keyboardManager = FindAnyObjectByType<DebugManager>();
        
        // Subscribe to player movement complete event
        if (player != null)
        {
            player.OnMovementComplete += OnPlayerMovementComplete;
        }
        
        // Find and hide MiniGamesUI at start
        if (miniGamesUI == null)
        {
            GameObject miniGamesUIObj = GameObject.Find("MiniGamesUI");
            if (miniGamesUIObj != null)
            {
                miniGamesUI = miniGamesUIObj;
            }
        }
        
        // Hide MiniGamesUI at start
        if (miniGamesUI != null)
        {
            miniGamesUI.SetActive(false);
        }
    }
    
    private bool IsMiniGameActive()
    {
        // Check if MiniGamesUI is active (use activeInHierarchy to account for parent inactive state)
        if (miniGamesUI != null && miniGamesUI.activeInHierarchy)
        {
            Debug.Log("Dice blocked: MiniGamesUI is active");
            return true;
        }
        
        // Check if MiniGameStockMarket is active via KeyboardManager
        if (keyboardManager != null && keyboardManager.IsMiniGameActive)
        {
            Debug.Log("Dice blocked: MiniGameStockMarket is active");
            return true;
        }
        
        // Fallback: Try to find MiniGameStockMarket GameObject directly
        GameObject miniGame = GameObject.Find("MiniGameStockMarket");
        if (miniGame != null && miniGame.activeInHierarchy)
        {
            Debug.Log("Dice blocked: MiniGameStockMarket found and active");
            return true;
        }
        
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        // Handle input for web (mouse click) and mobile (touch)
        // Only allow rolling if dice are not rolling, player is not moving, and card is not animating
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            if (!CanRollDice)
            {
                // Debug why dice cannot be rolled
                if (isRolling)
                    Debug.Log("Cannot roll dice: Dice are currently rolling");
                else if (cardsManager != null && cardsManager.IsCardAnimating)
                    Debug.Log("Cannot roll dice: Card is animating");
                else if (player != null && player.IsMoving)
                    Debug.Log("Cannot roll dice: Player is moving");
                else if (IsMiniGameActive())
                    Debug.Log("Cannot roll dice: Mini game is active");
                else
                    Debug.Log("Cannot roll dice: Unknown reason");
            }
            else if (CanRollDice)
            {
                RollDice();
            }
        }
        
        // Check if dice have finished rolling
        if (isRolling && Time.time - lastCheckTime >= diceCheckInterval)
        {
            lastCheckTime = Time.time;
            
            if (!firstDice.IsRolling && !secondDice.IsRolling)
            {
                isRolling = false;
                StartCoroutine(ProcessDiceResult());
            }
        }
    }
    
    private void FindSpawners()
    {
        // Try to find spawners by name
        GameObject firstSpawnerObj = GameObject.Find("FirstSpawner");
        GameObject secondSpawnerObj = GameObject.Find("SecondSpawner");
        
        if (firstSpawnerObj != null)
        {
            firstSpawner = firstSpawnerObj.transform;
        }
        
        if (secondSpawnerObj != null)
        {
            secondSpawner = secondSpawnerObj.transform;
        }
    }
    
    private void LoadDicePrefab()
    {
        // Try to load dice prefab from Resources folder
        // Note: The prefab must be in a "Resources" folder for this to work at runtime
        dicePrefab = Resources.Load<GameObject>("Dice");
        
        // If still null, the prefab should be assigned in the Inspector
        // or placed in a Resources folder
        if (dicePrefab == null)
        {
            Debug.LogWarning("Dice Prefab not found! Please assign it in the Inspector or place it in a Resources folder.");
        }
    }
    
    private void SpawnDice()
    {
        // Remove any existing dice from first spawner
        if (firstSpawner != null)
        {
            // Destroy all children (existing dice)
            for (int i = firstSpawner.childCount - 1; i >= 0; i--)
            {
                Destroy(firstSpawner.GetChild(i).gameObject);
            }
            
            // Spawn new dice at first spawner
            if (dicePrefab != null)
            {
                GameObject spawnedDice = Instantiate(dicePrefab, firstSpawner.position, firstSpawner.rotation, firstSpawner);
                spawnedDice.name = "FirstDice";
                firstDice = spawnedDice.GetComponent<DiceController>();
                
                if (firstDice == null)
                {
                    firstDice = spawnedDice.AddComponent<DiceController>();
                }
            }
            else
            {
                Debug.LogError("Dice Prefab is not assigned! Cannot spawn dice.");
            }
        }
        
        // Remove any existing dice from second spawner
        if (secondSpawner != null)
        {
            // Destroy all children (existing dice)
            for (int i = secondSpawner.childCount - 1; i >= 0; i--)
            {
                Destroy(secondSpawner.GetChild(i).gameObject);
            }
            
            // Spawn new dice at second spawner
            if (dicePrefab != null)
            {
                GameObject spawnedDice = Instantiate(dicePrefab, secondSpawner.position, secondSpawner.rotation, secondSpawner);
                spawnedDice.name = "SecondDice";
                secondDice = spawnedDice.GetComponent<DiceController>();
                
                if (secondDice == null)
                {
                    secondDice = spawnedDice.AddComponent<DiceController>();
                }
            }
            else
            {
                Debug.LogError("Dice Prefab is not assigned! Cannot spawn dice.");
            }
        }
    }
    
    private void FindDice()
    {
        // Try to find dice in spawners first
        if (firstSpawner != null)
        {
            firstDice = firstSpawner.GetComponentInChildren<DiceController>();
        }
        
        if (secondSpawner != null)
        {
            secondDice = secondSpawner.GetComponentInChildren<DiceController>();
        }
        
        // Fallback: Try to find dice by name
        if (firstDice == null)
        {
            GameObject firstDiceObj = GameObject.Find("FirstDice");
            if (firstDiceObj != null)
            {
                firstDice = firstDiceObj.GetComponent<DiceController>();
                if (firstDice == null)
                {
                    firstDice = firstDiceObj.AddComponent<DiceController>();
                }
            }
        }
        
        if (secondDice == null)
        {
            GameObject secondDiceObj = GameObject.Find("SecondDice");
            if (secondDiceObj != null)
            {
                secondDice = secondDiceObj.GetComponent<DiceController>();
                if (secondDice == null)
                {
                    secondDice = secondDiceObj.AddComponent<DiceController>();
                }
            }
        }
    }
    
    public void RollDice()
    {
        if (isRolling || firstDice == null || secondDice == null)
        {
            return;
        }
        
        isRolling = true;
        diceSum = 0;
        
        // Play rolling dice sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("RollingDice");
        }
        
        // Roll both dice
        firstDice.RollDice();
        secondDice.RollDice();
        
        lastCheckTime = Time.time;
    }
    
    private IEnumerator ProcessDiceResult()
    {
        isProcessingDiceResult = true;
        
        if (firstDice != null && secondDice != null)
        {
            // Calculate dice sum
            diceSum = firstDice.CurrentValue + secondDice.CurrentValue;
            
            // Display the dice sum prominently
            string message = $"=== DICE ROLL RESULT ===\n" +
                           $"First Dice: {firstDice.CurrentValue}\n" +
                           $"Second Dice: {secondDice.CurrentValue}\n" +
                           $"TOTAL SUM: {diceSum}\n" +
                           $"========================";
            
            Debug.Log(message);
            DisplayDiceSum();
            
            // Move dice back to their spawners
            yield return StartCoroutine(MoveDiceToSpawners());
            
            // Wait 2 seconds to show the dice numbers
            yield return new WaitForSeconds(2f);
            
            // Destroy the dice
            if (firstDice != null)
            {
                Destroy(firstDice.gameObject);
                firstDice = null;
            }
            
            if (secondDice != null)
            {
                Destroy(secondDice.gameObject);
                secondDice = null;
            }
            
            // Determine movement steps: use fixed value if debugging, otherwise use dice sum
            int movementSteps = IsDebugging ? debugFixedSteps : diceSum;
            
            // Log debug mode status if enabled
            if (IsDebugging)
            {
                Debug.Log($"DEBUG MODE: Using fixed steps ({debugFixedSteps}) instead of dice sum ({diceSum})");
            }
            
            // Move player based on calculated movement steps
            if (player != null && movementSteps > 0)
            {
                player.OnDiceRollComplete(movementSteps);
            }
            else if (player == null)
            {
                Debug.LogWarning("Player not found! Cannot move player.");
                isProcessingDiceResult = false;
            }
            // Note: isProcessingDiceResult will be set to false in OnPlayerMovementComplete
        }
        else
        {
            isProcessingDiceResult = false;
        }
    }
    
    private IEnumerator MoveDiceToSpawners()
    {
        float moveDuration = 0.5f; // Duration for moving dice back
        float elapsedTime = 0f;
        
        Vector3 firstDiceStartPos = firstDice.transform.position;
        Vector3 firstDiceTargetPos = firstSpawner != null ? firstSpawner.position : firstDiceStartPos;
        Quaternion firstDiceStartRot = firstDice.transform.rotation;
        
        Vector3 secondDiceStartPos = secondDice.transform.position;
        Vector3 secondDiceTargetPos = secondSpawner != null ? secondSpawner.position : secondDiceStartPos;
        Quaternion secondDiceStartRot = secondDice.transform.rotation;
        
        // Calculate target rotations to show the rolled values on top
        // Get the base spawner rotations
        Quaternion firstSpawnerBaseRot = firstSpawner != null ? firstSpawner.rotation : Quaternion.identity;
        Quaternion secondSpawnerBaseRot = secondSpawner != null ? secondSpawner.rotation : Quaternion.identity;
        
        // Calculate rotation needed to show first dice value on top
        Quaternion firstDiceValueRot = firstDice != null ? firstDice.GetRotationForValueOnTop(firstDice.CurrentValue) : Quaternion.identity;
        Quaternion firstDiceTargetRot = firstSpawnerBaseRot * firstDiceValueRot;
        
        // Calculate rotation needed to show second dice value on top
        Quaternion secondDiceValueRot = secondDice != null ? secondDice.GetRotationForValueOnTop(secondDice.CurrentValue) : Quaternion.identity;
        Quaternion secondDiceTargetRot = secondSpawnerBaseRot * secondDiceValueRot;
        
        // Make dice kinematic so they can be moved smoothly
        Rigidbody firstRb = firstDice.GetComponent<Rigidbody>();
        Rigidbody secondRb = secondDice.GetComponent<Rigidbody>();
        
        if (firstRb != null)
        {
            firstRb.isKinematic = true;
            firstRb.useGravity = false;
        }
        
        if (secondRb != null)
        {
            secondRb.isKinematic = true;
            secondRb.useGravity = false;
        }
        
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            
            // Smooth curve (ease in-out)
            float curve = t * t * (3f - 2f * t);
            
            // Move first dice
            if (firstDice != null)
            {
                firstDice.transform.position = Vector3.Lerp(firstDiceStartPos, firstDiceTargetPos, curve);
                firstDice.transform.rotation = Quaternion.Lerp(firstDiceStartRot, firstDiceTargetRot, curve);
            }
            
            // Move second dice
            if (secondDice != null)
            {
                secondDice.transform.position = Vector3.Lerp(secondDiceStartPos, secondDiceTargetPos, curve);
                secondDice.transform.rotation = Quaternion.Lerp(secondDiceStartRot, secondDiceTargetRot, curve);
            }
            
            yield return null;
        }
        
        // Ensure exact final positions
        if (firstDice != null)
        {
            firstDice.transform.position = firstDiceTargetPos;
            firstDice.transform.rotation = firstDiceTargetRot;
        }
        
        if (secondDice != null)
        {
            secondDice.transform.position = secondDiceTargetPos;
            secondDice.transform.rotation = secondDiceTargetRot;
        }
    }
    
    private void OnPlayerMovementComplete()
    {
        // Check if there is a card animating
        if (cardsManager != null && cardsManager.IsCardAnimating)
        {
            // Wait for card to be destroyed, then spawn dice
            StartCoroutine(WaitForCardAndRespawnDice());
        }
        else
        {
            // No card, spawn dice immediately
            SpawnDice();
            isProcessingDiceResult = false;
        }
    }
    
    private IEnumerator WaitForCardAndRespawnDice()
    {
        // Wait until card animation is complete (card is destroyed)
        while (cardsManager != null && cardsManager.IsCardAnimating)
        {
            yield return null;
        }
        
        // Card has been destroyed, spawn dice back
        SpawnDice();
        isProcessingDiceResult = false;
    }
    
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<PlayerController>();
            if (player == null)
            {
                player = playerObj.AddComponent<PlayerController>();
            }
        }
        else
        {
            Debug.LogWarning("Player GameObject not found in scene!");
        }
    }
    
    
    /// <summary>
    /// Displays the current dice sum. Can be called anytime to check the current values.
    /// </summary>
    public void DisplayDiceSum()
    {
        if (firstDice != null && secondDice != null)
        {
            string status = $"Current Dice Status:\n" +
                          $"First Dice Value: {firstDice.CurrentValue}\n" +
                          $"Second Dice Value: {secondDice.CurrentValue}\n" +
                          $"Total Sum: {diceSum}";
            Debug.Log(status);
        }
        else
        {
            Debug.LogWarning("Dice not found! Cannot display dice sum.");
        }
    }
    
    public void ResetDice()
    {
        if (firstDice != null)
        {
            firstDice.ResetDice();
        }
        
        if (secondDice != null)
        {
            secondDice.ResetDice();
        }
        
        diceSum = 0;
        isRolling = false;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from player movement complete event
        if (player != null)
        {
            player.OnMovementComplete -= OnPlayerMovementComplete;
        }
    }
}
