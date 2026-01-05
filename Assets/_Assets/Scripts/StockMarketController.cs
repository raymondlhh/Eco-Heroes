using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StockMarketController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI stockValueText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("2D Panel Reference")]
    [SerializeField] private RectTransform graphPanel;
    [SerializeField] private float padding = 10f; // Padding from panel edges
    [SerializeField] private Material lineMaterial; // Material for the line (assign in Inspector)
    
    [Header("Game Timer Settings")]
    [SerializeField] private float gameTime = 30f; // 30 seconds
    
    [Header("Stock Settings")]
    [SerializeField] private float initialMoney = 2000f;
    [SerializeField] private float baseStockValue = 100f;
    [SerializeField] private float volatility = 3f;
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private int maxDataPoints = 50;
    [SerializeField] private Color lineColor = Color.green;
    [SerializeField] private float lineWidth = 3f; // Width in pixels for UI
    
    private float currentMoney;
    private float currentStockValue;
    private List<float> stockHistory = new List<float>();
    private float lastUpdateTime = 0f;
    private float remainingTime;
    private bool isGameOver = false;
    
    // Stable graph scaling
    private float graphMinValue;
    private float graphMaxValue;
    private bool graphRangeInitialized = false;
    
    // UI Graphics for drawing the stock line
    private UILineRenderer uiLineRenderer;
    private GameObject lineObject;
    
    void Start()
    {
        // Initialize values
        currentMoney = initialMoney;
        currentStockValue = baseStockValue;
        remainingTime = gameTime;
        isGameOver = false;
        
        // Find buttons if not assigned
        if (buyButton == null)
        {
            buyButton = GameObject.Find("BuyButton")?.GetComponent<Button>();
        }
        
        if (sellButton == null)
        {
            sellButton = GameObject.Find("SellButton")?.GetComponent<Button>();
        }
        
        // Find time text if not assigned
        if (timeText == null)
        {
            GameObject timeTextObj = GameObject.Find("TimeText");
            if (timeTextObj != null)
            {
                timeText = timeTextObj.GetComponent<TextMeshProUGUI>();
            }
        }
        
        // Find game over panel if not assigned
        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.Find("GameOverPanel");
        }
        
        // Ensure game over panel starts inactive
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Find graph panel if not assigned
        if (graphPanel == null)
        {
            GameObject panelObj = GameObject.Find("StockPanel");
            if (panelObj != null)
            {
                graphPanel = panelObj.GetComponent<RectTransform>();
            }
        }
        
        // Setup button listeners
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyClicked);
        }
        
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellClicked);
        }
        
        // Initialize stock history
        for (int i = 0; i < maxDataPoints; i++)
        {
            stockHistory.Add(baseStockValue);
        }
        
        // Setup line renderer
        SetupLineRenderer();
        
        UpdateUI();
    }
    
    void Update()
    {
        // Don't update if game is over
        if (isGameOver)
            return;
        
        // Update timer
        remainingTime -= Time.deltaTime;
        
        // Check if time is up
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            EndGame();
            return;
        }
        
        // Update stock value at intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateStockValue();
            DrawStockLine();
            UpdateUI();
            lastUpdateTime = Time.time;
        }
    }
    
    private void SetupLineRenderer()
    {
        if (graphPanel == null) return;
        
        // Create GameObject for line renderer
        lineObject = new GameObject("StockLine");
        lineObject.transform.SetParent(graphPanel, false);
        
        // Add RectTransform for proper UI positioning
        RectTransform rectTransform = lineObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Add UILineRenderer component (custom UI component)
        uiLineRenderer = lineObject.AddComponent<UILineRenderer>();
        
        // Assign material if provided, otherwise use default
        if (lineMaterial != null)
        {
            uiLineRenderer.material = lineMaterial;
        }
        else
        {
            // Try to get default material from Canvas
            Material defaultMat = Canvas.GetDefaultCanvasMaterial();
            if (defaultMat != null)
            {
                uiLineRenderer.material = defaultMat;
            }
            else
            {
                // Fallback: create a simple material with UI/Default shader
                uiLineRenderer.material = new Material(Shader.Find("UI/Default"));
            }
        }
        
        // Ensure color has full alpha
        Color finalColor = lineColor;
        finalColor.a = 1f; // Ensure fully opaque
        uiLineRenderer.color = finalColor;
        
        uiLineRenderer.LineWidth = lineWidth;
        uiLineRenderer.raycastTarget = false; // Don't block UI interactions
        
        // Ensure the component is enabled and visible
        uiLineRenderer.enabled = true;
        
        // Set the layer to UI layer (layer 5)
        lineObject.layer = 5;
        
        // Force all updates
        uiLineRenderer.SetAllDirty();
        
        Debug.Log($"Line renderer setup complete. Color: {finalColor}, Width: {lineWidth}, Panel: {graphPanel.name}, Material: {uiLineRenderer.material?.name ?? "None"}");
    }
    
    private void UpdateStockValue()
    {
        // Random movement for stock price (random walk)
        float change = Random.Range(-volatility, volatility);
        currentStockValue += change;
        
        // Keep stock value reasonable (minimum $10)
        currentStockValue = Mathf.Max(10f, currentStockValue);
        
        // Add to history
        stockHistory.Add(currentStockValue);
        
        // Remove oldest if we exceed max
        if (stockHistory.Count > maxDataPoints)
        {
            stockHistory.RemoveAt(0);
        }
    }
    
    private void DrawStockLine()
    {
        if (uiLineRenderer == null || graphPanel == null || stockHistory.Count < 2)
        {
            if (uiLineRenderer == null)
                Debug.LogWarning("UILineRenderer is null!");
            if (graphPanel == null)
                Debug.LogWarning("GraphPanel is null!");
            if (stockHistory.Count < 2)
                Debug.LogWarning($"Stock history has only {stockHistory.Count} points!");
            return;
        }
        
        // Get current min/max from history
        float[] historyArray = stockHistory.ToArray();
        float histMin = Mathf.Min(historyArray);
        float histMax = Mathf.Max(historyArray);
        
        // Initialize graph range on first draw
        if (!graphRangeInitialized)
        {
            float center = (histMin + histMax) * 0.5f;
            float initialRange = Mathf.Max(histMax - histMin, baseStockValue * 0.2f); // At least 20% of base value
            
            graphMinValue = center - initialRange * 0.5f;
            graphMaxValue = center + initialRange * 0.5f;
            graphRangeInitialized = true;
        }
        
        // Expand range only if new values go outside current range (don't shrink)
        if (histMin < graphMinValue)
        {
            graphMinValue = histMin;
        }
        if (histMax > graphMaxValue)
        {
            graphMaxValue = histMax;
        }
        
        float range = graphMaxValue - graphMinValue;
        
        // Prevent division by zero
        if (range < 0.1f)
        {
            range = 0.1f;
        }
        
        // Get panel dimensions (accounting for padding)
        float panelWidth = graphPanel.rect.width - (padding * 2f);
        float panelHeight = graphPanel.rect.height - (padding * 2f);
        
        // Ensure we have valid dimensions
        if (panelWidth <= 0 || panelHeight <= 0)
        {
            Debug.LogWarning($"Invalid panel dimensions: {panelWidth}x{panelHeight}");
            return;
        }
        
        // Create points for the line in UI space
        List<Vector2> points = new List<Vector2>();
        
        int pointCount = stockHistory.Count;
        for (int i = 0; i < pointCount; i++)
        {
            // Normalize value (0 to 1) using stable range
            float normalizedValue = (stockHistory[i] - graphMinValue) / range;
            
            // Clamp normalized value to 0-1 range
            normalizedValue = Mathf.Clamp01(normalizedValue);
            
            // Convert to UI coordinates (local space relative to RectTransform)
            // X: spread across panel width (left to right, with padding)
            // Y: map to panel height (bottom to top, with padding)
            // Use 0-based coordinates (0,0 is bottom-left of RectTransform)
            float x = padding + (i / (float)(pointCount - 1)) * panelWidth;
            float y = padding + normalizedValue * panelHeight;
            
            points.Add(new Vector2(x, y));
        }
        
        // Update UI line renderer
        uiLineRenderer.SetPoints(points.ToArray());
    }
    
    private void OnBuyClicked()
    {
        // Buy 1 share at current price
        if (currentMoney >= currentStockValue)
        {
            currentMoney -= currentStockValue;
            UpdateUI();
            Debug.Log($"Bought 1 share at ${currentStockValue:F2}. Remaining money: ${currentMoney:F2}");
        }
        else
        {
            Debug.LogWarning($"Not enough money! Need ${currentStockValue:F2}, have ${currentMoney:F2}");
        }
    }
    
    private void OnSellClicked()
    {
        // Sell 1 share at current price
        currentMoney += currentStockValue;
        UpdateUI();
        Debug.Log($"Sold 1 share at ${currentStockValue:F2}. Total money: ${currentMoney:F2}");
    }
    
    private void UpdateUI()
    {
        // Update money display
        if (moneyText != null)
        {
            moneyText.text = $"Money: ${currentMoney:F2}";
        }
        
        // Update stock value display
        if (stockValueText != null)
        {
            stockValueText.text = $"Price: ${currentStockValue:F2}";
        }
        
        // Update time display
        if (timeText != null)
        {
            int seconds = Mathf.CeilToInt(remainingTime);
            timeText.text = $"Time: {seconds}";
        }
    }
    
    private void EndGame()
    {
        isGameOver = true;
        
        // Disable buy/sell buttons
        if (buyButton != null)
        {
            buyButton.interactable = false;
        }
        
        if (sellButton != null)
        {
            sellButton.interactable = false;
        }
        
        // Activate game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        Debug.Log("Game Over! Final Money: $" + currentMoney.ToString("F2"));
    }
}

// Custom UI Line Renderer Component for drawing lines in UI
[RequireComponent(typeof(RectTransform))]
public class UILineRenderer : Graphic
{
    [SerializeField] private Vector2[] points = new Vector2[0];
    [SerializeField] private float lineWidth = 3f;
    
    public Vector2[] Points
    {
        get { return points; }
        set
        {
            points = value;
            SetAllDirty(); // Force complete update
        }
    }
    
    public float LineWidth
    {
        get { return lineWidth; }
        set
        {
            lineWidth = value;
            SetAllDirty(); // Force complete update
        }
    }
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        if (points == null || points.Length < 2)
        {
            Debug.LogWarning($"UILineRenderer: Not enough points! Count: {(points == null ? 0 : points.Length)}");
            return;
        }
        
        int segmentsAdded = 0;
        
        // Create vertices for each line segment
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 start = points[i];
            Vector2 end = points[i + 1];
            
            // Skip if points are too close (prevents division by zero)
            if (Vector2.Distance(start, end) < 0.001f)
                continue;
            
            // Calculate direction and perpendicular
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            Vector2 offset = perpendicular * (lineWidth * 0.5f);
            
            // Create quad vertices
            UIVertex[] verts = new UIVertex[4];
            
            verts[0].position = start - offset;
            verts[0].color = color;
            verts[0].uv0 = Vector2.zero;
            
            verts[1].position = start + offset;
            verts[1].color = color;
            verts[1].uv0 = Vector2.up;
            
            verts[2].position = end + offset;
            verts[2].color = color;
            verts[2].uv0 = Vector2.one;
            
            verts[3].position = end - offset;
            verts[3].color = color;
            verts[3].uv0 = Vector2.right;
            
            // Add quad to mesh
            int index = vh.currentVertCount;
            vh.AddVert(verts[0]);
            vh.AddVert(verts[1]);
            vh.AddVert(verts[2]);
            vh.AddVert(verts[3]);
            
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
            
            segmentsAdded++;
        }
        
        if (segmentsAdded == 0 && points.Length >= 2)
        {
            Debug.LogWarning($"UILineRenderer: No segments were added! Points: {points.Length}");
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        // Ensure we have a default material for UI rendering
        // Graphic components need a material to render
        if (material == null)
        {
            // Try to get default material from Canvas
            Material defaultMat = Canvas.GetDefaultCanvasMaterial();
            if (defaultMat != null)
            {
                material = defaultMat;
            }
            else
            {
                // Fallback: create a simple material with UI/Default shader
                material = new Material(Shader.Find("UI/Default"));
            }
        }
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        // Force complete update when enabled
        SetAllDirty();
    }
    
    public void SetPoints(Vector2[] newPoints)
    {
        if (newPoints == null)
        {
            Debug.LogWarning("UILineRenderer: SetPoints called with null array!");
            return;
        }
        
        Points = newPoints;
    }
}
