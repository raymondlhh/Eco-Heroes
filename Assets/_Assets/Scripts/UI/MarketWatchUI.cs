using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MarketWatchUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject marketWatchUIPanel;
    
    [Header("Display Elements")]
    [SerializeField] private TextMeshProUGUI cardTitleText;
    [SerializeField] private TextMeshProUGUI cardDescriptionText;
    [SerializeField] private TextMeshProUGUI conditionText; // For conditional effects (e.g., "OFFER ONLY FOR RESIDENTIAL UNIT")
    
    [Header("Simple Effect UI")]
    [SerializeField] private GameObject simpleEffectPanel;
    [SerializeField] private TextMeshProUGUI effectAmountText;
    [SerializeField] private Button confirmButton;
    
    [Header("Choice Effect UI")]
    [SerializeField] private GameObject choiceEffectPanel;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button keepButton;
    [SerializeField] private TextMeshProUGUI sellOptionText;
    [SerializeField] private TextMeshProUGUI keepOptionText;
    
    [Header("References")]
    [SerializeField] private MarketWatchData marketWatchData;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private RealEstateData realEstateData;
    [SerializeField] private BusinessData businessData;
    
    private MarketWatchData.MarketWatchCard currentCard;
    private CardController currentCardController;
    private bool isProcessing = false;
    
    // Events
    public System.Action OnEffectComplete;
    
    private void Start()
    {
        // Find MarketWatchUI panel if not assigned
        if (marketWatchUIPanel == null)
        {
            GameObject marketWatchUIObj = GameObject.Find("MarketWatchUI");
            if (marketWatchUIObj != null)
            {
                marketWatchUIPanel = marketWatchUIObj;
            }
        }
        
        // Setup button listeners
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
        
        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnSellClicked);
        }
        
        if (keepButton != null)
        {
            keepButton.onClick.RemoveAllListeners();
            keepButton.onClick.AddListener(OnKeepClicked);
        }
        
        // Find PlayerManager if not assigned
        if (playerManager == null)
        {
            playerManager = FindAnyObjectByType<PlayerManager>();
        }
        
        // Find data references if not assigned
        if (realEstateData == null)
        {
            realEstateData = Resources.FindObjectsOfTypeAll<RealEstateData>().FirstOrDefault();
        }
        
        if (businessData == null)
        {
            businessData = Resources.FindObjectsOfTypeAll<BusinessData>().FirstOrDefault();
        }
        
        // Hide UI initially
        if (marketWatchUIPanel != null)
        {
            marketWatchUIPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows the MarketWatchUI with card information
    /// </summary>
    public void ShowMarketWatchUI(MarketWatchData.MarketWatchCard card, CardController cardController)
    {
        if (card == null)
        {
            Debug.LogError("Cannot show MarketWatchUI: Card is null!");
            return;
        }
        
        if (isProcessing)
        {
            Debug.LogWarning("MarketWatchUI is already processing another card!");
            return;
        }
        
        currentCard = card;
        currentCardController = cardController;
        isProcessing = true;
        
        // Update title and description
        if (cardTitleText != null)
        {
            cardTitleText.text = card.displayName;
        }
        
        if (cardDescriptionText != null)
        {
            cardDescriptionText.text = card.description;
        }
        
        // Show/hide condition text
        if (conditionText != null)
        {
            bool showCondition = card.conditionType != MarketWatchData.ConditionType.None;
            conditionText.gameObject.SetActive(showCondition);
            
            if (showCondition)
            {
                switch (card.conditionType)
                {
                    case MarketWatchData.ConditionType.ResidentialOnly:
                        conditionText.text = "OFFER ONLY FOR RESIDENTIAL UNIT";
                        break;
                    case MarketWatchData.ConditionType.CommercialOnly:
                        conditionText.text = "OFFER ONLY FOR COMMERCIAL UNIT";
                        break;
                    case MarketWatchData.ConditionType.SpecificProperty:
                        conditionText.text = $"OFFER ONLY FOR {card.specificPropertyName}";
                        break;
                }
            }
        }
        
        // Configure UI based on effect type
        bool isChoiceEffect = card.effectType == MarketWatchData.EffectType.ChoiceSellOrKeep;
        
        if (simpleEffectPanel != null)
        {
            simpleEffectPanel.SetActive(!isChoiceEffect);
        }
        
        if (choiceEffectPanel != null)
        {
            choiceEffectPanel.SetActive(isChoiceEffect);
        }
        
        if (isChoiceEffect)
        {
            // Setup choice buttons
            if (sellOptionText != null && !string.IsNullOrEmpty(card.sellOptionText))
            {
                sellOptionText.text = card.sellOptionText;
            }
            else if (sellOptionText != null)
            {
                sellOptionText.text = $"SELL AND MAKE ${card.oneTimeCashAmount:N0}";
            }
            
            if (keepOptionText != null && !string.IsNullOrEmpty(card.keepOptionText))
            {
                keepOptionText.text = card.keepOptionText;
            }
            else if (keepOptionText != null)
            {
                keepOptionText.text = $"KEEP AND INCREASE ${card.rentalIncreaseAmount:N0} RENTAL";
            }
        }
        else
        {
            // Setup simple effect display
            if (effectAmountText != null)
            {
                string amountText = "";
                switch (card.effectType)
                {
                    case MarketWatchData.EffectType.ReduceCashFlow:
                        amountText = $"-${Mathf.Abs(card.cashFlowAmount):N0}";
                        break;
                    case MarketWatchData.EffectType.IncreaseCashFlow:
                        amountText = $"+${card.cashFlowAmount:N0}";
                        break;
                    case MarketWatchData.EffectType.OneTimeCash:
                        if (card.oneTimeCashAmount >= 0)
                            amountText = $"+${card.oneTimeCashAmount:N0}";
                        else
                            amountText = $"-${Mathf.Abs(card.oneTimeCashAmount):N0}";
                        break;
                }
                effectAmountText.text = amountText;
            }
        }
        
        // Show UI
        if (marketWatchUIPanel != null)
        {
            marketWatchUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("MarketWatchUI panel is null! Cannot show UI.");
        }
        
        // Check if current player is AI and make decision automatically
        if (playerManager != null && playerManager.CurrentPlayer != null && playerManager.CurrentPlayer.IsAI)
        {
            StartCoroutine(MakeAIDecision());
        }
    }
    
    /// <summary>
    /// Makes a decision for AI players
    /// </summary>
    private IEnumerator MakeAIDecision()
    {
        if (currentCard == null || playerManager == null || playerManager.CurrentPlayer == null)
        {
            yield break;
        }
        
        // Wait a bit for visual feedback
        yield return new WaitForSeconds(1f);
        
        // For choice effects, AI will choose based on simple logic
        if (currentCard.effectType == MarketWatchData.EffectType.ChoiceSellOrKeep)
        {
            // Simple AI logic: choose sell if one-time cash is high, otherwise keep
            bool shouldSell = currentCard.oneTimeCashAmount > currentCard.rentalIncreaseAmount * 12; // If cash > 12 months of rental increase
            if (shouldSell)
            {
                OnSellClicked();
            }
            else
            {
                OnKeepClicked();
            }
        }
        else
        {
            // For simple effects, just confirm
            OnConfirmClicked();
        }
    }
    
    private void OnConfirmClicked()
    {
        if (currentCard == null)
        {
            Debug.LogError("Cannot process effect: Current card is null!");
            return;
        }
        
        ApplyEffect(currentCard);
        CloseUI();
    }
    
    private void OnSellClicked()
    {
        if (currentCard == null)
        {
            Debug.LogError("Cannot process sell option: Current card is null!");
            return;
        }
        
        if (currentCard.effectType != MarketWatchData.EffectType.ChoiceSellOrKeep)
        {
            Debug.LogWarning("Sell option clicked but card is not a choice effect!");
            return;
        }
        
        // Apply sell effect: give one-time cash
        PlayerFinance playerFinance = GetCurrentPlayerFinance();
        if (playerFinance != null)
        {
            playerFinance.AddCash(currentCard.oneTimeCashAmount);
            Debug.Log($"Player received ${currentCard.oneTimeCashAmount:N0} from selling property.");
        }
        
        CloseUI();
    }
    
    private void OnKeepClicked()
    {
        if (currentCard == null)
        {
            Debug.LogError("Cannot process keep option: Current card is null!");
            return;
        }
        
        if (currentCard.effectType != MarketWatchData.EffectType.ChoiceSellOrKeep)
        {
            Debug.LogWarning("Keep option clicked but card is not a choice effect!");
            return;
        }
        
        // Apply keep effect: increase rental income for applicable properties
        ApplyRentalIncrease(currentCard.rentalIncreaseAmount);
        
        CloseUI();
    }
    
    /// <summary>
    /// Applies the effect of the MarketWatch card
    /// </summary>
    private void ApplyEffect(MarketWatchData.MarketWatchCard card)
    {
        PlayerFinance playerFinance = GetCurrentPlayerFinance();
        if (playerFinance == null)
        {
            Debug.LogError("PlayerFinance is null! Cannot apply MarketWatch effect.");
            return;
        }
        
        Player currentPlayer = playerManager != null ? playerManager.CurrentPlayer : null;
        if (currentPlayer == null)
        {
            Debug.LogError("Current player is null! Cannot apply MarketWatch effect.");
            return;
        }
        
        switch (card.effectType)
        {
            case MarketWatchData.EffectType.ReduceCashFlow:
                ApplyCashFlowReduction(card.cashFlowAmount, card);
                break;
                
            case MarketWatchData.EffectType.IncreaseCashFlow:
                ApplyCashFlowIncrease(card.cashFlowAmount, card);
                break;
                
            case MarketWatchData.EffectType.OneTimeCash:
                playerFinance.AddCash(card.oneTimeCashAmount);
                Debug.Log($"Player received one-time cash: ${card.oneTimeCashAmount:N0}");
                break;
                
            case MarketWatchData.EffectType.RemoveBusiness:
                RemoveBusinessAssets(card.assetCount);
                break;
                
            case MarketWatchData.EffectType.RemoveRealEstate:
                RemoveRealEstateAssets(card.assetCount);
                break;
                
            case MarketWatchData.EffectType.IncreaseRentalIncome:
                ApplyRentalIncrease(card.rentalIncreaseAmount);
                break;
                
            default:
                Debug.LogWarning($"Unhandled effect type: {card.effectType}");
                break;
        }
    }
    
    /// <summary>
    /// Applies cash flow reduction to applicable assets
    /// </summary>
    private void ApplyCashFlowReduction(float amount, MarketWatchData.MarketWatchCard card)
    {
        PlayerFinance playerFinance = GetCurrentPlayerFinance();
        if (playerFinance == null) return;
        
        // Get applicable assets based on target type and condition
        List<string> applicableAssets = GetApplicableAssets(card);
        
        foreach (string assetName in applicableAssets)
        {
            // Find income item and reduce it
            var incomeItem = playerFinance.IncomeItems.FirstOrDefault(i => i.details == assetName);
            if (incomeItem != null)
            {
                float newAmount = Mathf.Max(0, incomeItem.amount - Mathf.Abs(amount));
                playerFinance.UpdateIncomeItem(assetName, newAmount);
                Debug.Log($"Reduced cash flow for {assetName} by ${Mathf.Abs(amount):N0}. New amount: ${newAmount:N0}");
            }
        }
        
        // If no specific assets found, add as expense
        if (applicableAssets.Count == 0)
        {
            playerFinance.AddExpenseItem($"MarketWatch_{card.cardName}", Mathf.Abs(amount));
            Debug.Log($"Added expense item: MarketWatch_{card.cardName} - ${Mathf.Abs(amount):N0}");
        }
    }
    
    /// <summary>
    /// Applies cash flow increase to applicable assets
    /// </summary>
    private void ApplyCashFlowIncrease(float amount, MarketWatchData.MarketWatchCard card)
    {
        PlayerFinance playerFinance = GetCurrentPlayerFinance();
        if (playerFinance == null) return;
        
        // Get applicable assets based on target type and condition
        List<string> applicableAssets = GetApplicableAssets(card);
        
        foreach (string assetName in applicableAssets)
        {
            // Find income item and increase it
            var incomeItem = playerFinance.IncomeItems.FirstOrDefault(i => i.details == assetName);
            if (incomeItem != null)
            {
                float newAmount = incomeItem.amount + amount;
                playerFinance.UpdateIncomeItem(assetName, newAmount);
                Debug.Log($"Increased cash flow for {assetName} by ${amount:N0}. New amount: ${newAmount:N0}");
            }
        }
    }
    
    /// <summary>
    /// Applies rental income increase to applicable real estate properties
    /// </summary>
    private void ApplyRentalIncrease(float amount)
    {
        PlayerFinance playerFinance = GetCurrentPlayerFinance();
        if (playerFinance == null) return;
        
        Player currentPlayer = playerManager != null ? playerManager.CurrentPlayer : null;
        if (currentPlayer == null) return;
        
        // Get all real estate properties owned by player
        List<string> realEstateAssets = new List<string>();
        foreach (var item in currentPlayer.OwnedPlayerItems)
        {
            if (item != null && item.name.StartsWith("PlayerItem_RealEstate"))
            {
                string assetName = item.name.Replace("PlayerItem_", "");
                realEstateAssets.Add(assetName);
            }
        }
        
        // Check condition (e.g., residential only) - use currentCard if available
        MarketWatchData.MarketWatchCard card = currentCard;
        if (card != null && card.conditionType == MarketWatchData.ConditionType.ResidentialOnly)
        {
            // Filter to only residential units (typically RealEstate01-09)
            realEstateAssets = realEstateAssets.Where(a => 
            {
                // Check if it's a residential unit (typically numbered 01-09)
                string numberPart = a.Replace("RealEstate", "");
                if (int.TryParse(numberPart, out int num))
                {
                    return num >= 1 && num <= 9;
                }
                return false;
            }).ToList();
        }
        
        // Increase income for each applicable property
        foreach (string assetName in realEstateAssets)
        {
            var incomeItem = playerFinance.IncomeItems.FirstOrDefault(i => i.details == assetName);
            if (incomeItem != null)
            {
                float newAmount = incomeItem.amount + amount;
                playerFinance.UpdateIncomeItem(assetName, newAmount);
                Debug.Log($"Increased rental income for {assetName} by ${amount:N0}. New amount: ${newAmount:N0}");
            }
        }
    }
    
    /// <summary>
    /// Removes business assets and their income
    /// </summary>
    private void RemoveBusinessAssets(int count)
    {
        Player currentPlayer = playerManager != null ? playerManager.CurrentPlayer : null;
        if (currentPlayer == null) return;
        
        PlayerFinance playerFinance = GetCurrentPlayerFinance();
        if (playerFinance == null) return;
        
        // Get all business PlayerItems
        List<GameObject> businessItems = new List<GameObject>();
        foreach (var item in currentPlayer.OwnedPlayerItems)
        {
            if (item != null && item.name.StartsWith("PlayerItem_Business"))
            {
                businessItems.Add(item);
            }
        }
        
        // Remove up to 'count' business items
        int removed = 0;
        foreach (var item in businessItems)
        {
            if (removed >= count) break;
            
            string assetName = item.name.Replace("PlayerItem_", "");
            
            // Remove income item
            playerFinance.RemoveIncomeItem(assetName);
            
            // Remove investment income items (remove all matching items)
            while (playerFinance.RemoveInvestmentIncomeItem(assetName))
            {
                // Continue removing until none are left
            }
            
            // Remove from player's owned items
            currentPlayer.RemovePlayerItem(item);
            
            // Destroy the GameObject
            Destroy(item);
            
            removed++;
            Debug.Log($"Removed business asset: {assetName}");
        }
    }
    
    /// <summary>
    /// Removes real estate assets and their income
    /// </summary>
    private void RemoveRealEstateAssets(int count)
    {
        Player currentPlayer = playerManager != null ? playerManager.CurrentPlayer : null;
        if (currentPlayer == null) return;
        
        PlayerFinance playerFinance = GetCurrentPlayerFinance();
        if (playerFinance == null) return;
        
        // Get all real estate PlayerItems
        List<GameObject> realEstateItems = new List<GameObject>();
        foreach (var item in currentPlayer.OwnedPlayerItems)
        {
            if (item != null && item.name.StartsWith("PlayerItem_RealEstate"))
            {
                realEstateItems.Add(item);
            }
        }
        
        // Remove up to 'count' real estate items
        int removed = 0;
        foreach (var item in realEstateItems)
        {
            if (removed >= count) break;
            
            string assetName = item.name.Replace("PlayerItem_", "");
            
            // Remove income item
            playerFinance.RemoveIncomeItem(assetName);
            
            // Remove investment income items (remove all matching items)
            while (playerFinance.RemoveInvestmentIncomeItem(assetName))
            {
                // Continue removing until none are left
            }
            
            // Remove from player's owned items
            currentPlayer.RemovePlayerItem(item);
            
            // Destroy the GameObject
            Destroy(item);
            
            removed++;
            Debug.Log($"Removed real estate asset: {assetName}");
        }
    }
    
    /// <summary>
    /// Gets list of applicable asset names based on card's target type and conditions
    /// </summary>
    private List<string> GetApplicableAssets(MarketWatchData.MarketWatchCard card)
    {
        List<string> applicableAssets = new List<string>();
        
        Player currentPlayer = playerManager != null ? playerManager.CurrentPlayer : null;
        if (currentPlayer == null) return applicableAssets;
        
        // Get all owned assets
        foreach (var item in currentPlayer.OwnedPlayerItems)
        {
            if (item == null) continue;
            
            string assetName = item.name.Replace("PlayerItem_", "");
            
            // Check target type
            bool matchesTarget = false;
            if (card.targetAssetType == MarketWatchData.TargetAssetType.Both)
            {
                matchesTarget = true;
            }
            else if (card.targetAssetType == MarketWatchData.TargetAssetType.RealEstate && assetName.StartsWith("RealEstate"))
            {
                matchesTarget = true;
            }
            else if (card.targetAssetType == MarketWatchData.TargetAssetType.Business && assetName.StartsWith("Business"))
            {
                matchesTarget = true;
            }
            
            if (!matchesTarget) continue;
            
            // Check conditions
            bool matchesCondition = true;
            if (card.conditionType == MarketWatchData.ConditionType.SpecificProperty)
            {
                matchesCondition = assetName == card.specificPropertyName;
            }
            else if (card.conditionType == MarketWatchData.ConditionType.ResidentialOnly)
            {
                // Only residential units (typically RealEstate01-09)
                if (assetName.StartsWith("RealEstate"))
                {
                    string numberPart = assetName.Replace("RealEstate", "");
                    if (int.TryParse(numberPart, out int num))
                    {
                        matchesCondition = num >= 1 && num <= 9;
                    }
                    else
                    {
                        matchesCondition = false;
                    }
                }
                else
                {
                    matchesCondition = false;
                }
            }
            
            if (matchesCondition)
            {
                applicableAssets.Add(assetName);
            }
        }
        
        return applicableAssets;
    }
    
    private void CloseUI()
    {
        isProcessing = false;
        
        // Hide UI
        if (marketWatchUIPanel != null)
        {
            marketWatchUIPanel.SetActive(false);
        }
        
        // Destroy the card
        if (currentCardController != null)
        {
            Destroy(currentCardController.gameObject);
        }
        
        // Notify that effect is complete
        OnEffectComplete?.Invoke();
    }
    
    /// <summary>
    /// Get the current player's PlayerFinance
    /// </summary>
    private PlayerFinance GetCurrentPlayerFinance()
    {
        if (playerManager != null && playerManager.CurrentPlayer != null)
        {
            return playerManager.CurrentPlayer.PlayerFinance;
        }
        
        return null;
    }
}
