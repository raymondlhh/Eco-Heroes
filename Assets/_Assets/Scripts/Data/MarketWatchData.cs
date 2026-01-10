using UnityEngine;

[CreateAssetMenu(fileName = "MarketWatchData", menuName = "Game Data/Market Watch Data")]
public class MarketWatchData : ScriptableObject
{
    /// <summary>
    /// Types of MarketWatch effects
    /// </summary>
    public enum EffectType
    {
        /// <summary>Simple cash flow reduction (e.g., -$100 monthly)</summary>
        ReduceCashFlow,
        
        /// <summary>Simple cash flow increase (e.g., +$200 monthly)</summary>
        IncreaseCashFlow,
        
        /// <summary>Choice: Sell for cash OR keep for increased rent</summary>
        ChoiceSellOrKeep,
        
        /// <summary>Remove a business card and all its income</summary>
        RemoveBusiness,
        
        /// <summary>Remove a real estate card and all its income</summary>
        RemoveRealEstate,
        
        /// <summary>One-time cash payment (positive or negative)</summary>
        OneTimeCash,
        
        /// <summary>Increase rental income for a property</summary>
        IncreaseRentalIncome
    }
    
    /// <summary>
    /// Target asset type for the effect
    /// </summary>
    public enum TargetAssetType
    {
        RealEstate,
        Business,
        Both,
        None // For effects that don't target specific assets
    }
    
    /// <summary>
    /// Condition for applying the effect
    /// </summary>
    public enum ConditionType
    {
        None,
        ResidentialOnly, // Only applies to residential units
        CommercialOnly,  // Only applies to commercial units
        SpecificProperty // Only applies to a specific property name
    }
    
    [System.Serializable]
    public class MarketWatchCard
    {
        [Header("Card Info")]
        [Tooltip("Unique identifier for this card (e.g., 'MarketWatch01', 'MarketWatch02')")]
        public string cardName;
        
        [Tooltip("Display name shown to player (e.g., 'Real Estate Market Slump')")]
        public string displayName;
        
        [Tooltip("Description of the card effect")]
        [TextArea(2, 4)]
        public string description;
        
        [Header("Effect Configuration")]
        [Tooltip("Type of effect this card has")]
        public EffectType effectType;
        
        [Tooltip("What type of asset this effect targets")]
        public TargetAssetType targetAssetType;
        
        [Tooltip("Condition for applying this effect")]
        public ConditionType conditionType;
        
        [Tooltip("Specific property name if conditionType is SpecificProperty (e.g., 'RealEstate01')")]
        public string specificPropertyName;
        
        [Header("Effect Parameters")]
        [Tooltip("Amount for cash flow changes (positive or negative)")]
        public float cashFlowAmount;
        
        [Tooltip("One-time cash amount (for OneTimeCash or ChoiceSellOrKeep sell option)")]
        public float oneTimeCashAmount;
        
        [Tooltip("Rental income increase amount (for ChoiceSellOrKeep keep option or IncreaseRentalIncome)")]
        public float rentalIncreaseAmount;
        
        [Tooltip("Number of assets to affect (e.g., remove 1 business card)")]
        public int assetCount = 1;
        
        [Header("Choice Options (for ChoiceSellOrKeep)")]
        [Tooltip("Text for the sell option (e.g., 'SELL AND MAKE $30,000')")]
        public string sellOptionText;
        
        [Tooltip("Text for the keep option (e.g., 'KEEP AND INCREASE $200 RENTAL')")]
        public string keepOptionText;
    }
    
    [Header("Market Watch Cards")]
    [Tooltip("Array of MarketWatch cards. Each card can have different effects.")]
    public MarketWatchCard[] cards = new MarketWatchCard[12];
    
    /// <summary>
    /// Gets card data by card name (e.g., "MarketWatch01", "MarketWatch02")
    /// </summary>
    public MarketWatchCard GetCardByName(string cardName)
    {
        if (cards == null) return null;
        
        foreach (var card in cards)
        {
            if (card != null && card.cardName == cardName)
            {
                return card;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets card data by index (0-11)
    /// </summary>
    public MarketWatchCard GetCardByIndex(int index)
    {
        if (cards == null || index < 0 || index >= cards.Length)
        {
            return null;
        }
        
        return cards[index];
    }
    
    /// <summary>
    /// Gets a random card from the available cards
    /// </summary>
    public MarketWatchCard GetRandomCard()
    {
        if (cards == null || cards.Length == 0) return null;
        
        // Filter out null cards
        System.Collections.Generic.List<MarketWatchCard> validCards = new System.Collections.Generic.List<MarketWatchCard>();
        foreach (var card in cards)
        {
            if (card != null && !string.IsNullOrEmpty(card.cardName))
            {
                validCards.Add(card);
            }
        }
        
        if (validCards.Count == 0) return null;
        
        int randomIndex = Random.Range(0, validCards.Count);
        return validCards[randomIndex];
    }
}
