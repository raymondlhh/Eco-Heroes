using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class FinancialItem
{
    public string details;
    public float amount;
    
    public FinancialItem(string details, float amount)
    {
        this.details = details;
        this.amount = amount;
    }
}

public class PlayerFinance : MonoBehaviour
{
    [Header("Income Items")]
    [SerializeField] private List<FinancialItem> incomeItems = new List<FinancialItem>();
    
    [Header("Expense Items")]
    [SerializeField] private List<FinancialItem> expenseItems = new List<FinancialItem>();
    
    [Header("Purchased Items")]
    [SerializeField] private List<string> realEstateItems = new List<string>();
    [SerializeField] private List<string> businessItems = new List<string>();
    [SerializeField] private List<string> stockItems = new List<string>();
    [SerializeField] private List<string> unitTrustItems = new List<string>();
    [SerializeField] private List<string> insuranceItems = new List<string>();
    
    // Public properties - Auto-calculated from items
    public float TotalIncome 
    { 
        get { return incomeItems != null ? incomeItems.Sum(item => item.amount) : 0f; }
    }
    
    public float TotalExpenses 
    { 
        get { return expenseItems != null ? expenseItems.Sum(item => item.amount) : 0f; }
    }
    
    public float CurrentPayday => TotalIncome - TotalExpenses;
    
    // Event that fires when income or expenses change
    public System.Action<float> OnPaydayChanged;
    
    // Read-only access to income and expense lists
    public IReadOnlyList<FinancialItem> IncomeItems => incomeItems;
    public IReadOnlyList<FinancialItem> ExpenseItems => expenseItems;
    
    // Read-only access to item lists
    public IReadOnlyList<string> RealEstateItems => realEstateItems;
    public IReadOnlyList<string> BusinessItems => businessItems;
    public IReadOnlyList<string> StockItems => stockItems;
    public IReadOnlyList<string> UnitTrustItems => unitTrustItems;
    public IReadOnlyList<string> InsuranceItems => insuranceItems;
    
    void Start()
    {
        // Initialize with default values if needed
        if (incomeItems == null) incomeItems = new List<FinancialItem>();
        if (expenseItems == null) expenseItems = new List<FinancialItem>();
        if (realEstateItems == null) realEstateItems = new List<string>();
        if (businessItems == null) businessItems = new List<string>();
        if (stockItems == null) stockItems = new List<string>();
        if (unitTrustItems == null) unitTrustItems = new List<string>();
        if (insuranceItems == null) insuranceItems = new List<string>();
    }
    
    /// <summary>
    /// Adds an income item with details and amount
    /// </summary>
    public void AddIncomeItem(string details, float amount)
    {
        if (!string.IsNullOrEmpty(details) && amount > 0)
        {
            incomeItems.Add(new FinancialItem(details, amount));
            Debug.Log($"Income item added: {details} - {amount}. Total Income: {TotalIncome}");
            OnPaydayChanged?.Invoke(CurrentPayday);
        }
    }
    
    /// <summary>
    /// Adds an expense item with details and amount
    /// </summary>
    public void AddExpenseItem(string details, float amount)
    {
        if (!string.IsNullOrEmpty(details) && amount > 0)
        {
            expenseItems.Add(new FinancialItem(details, amount));
            Debug.Log($"Expense item added: {details} - {amount}. Total Expenses: {TotalExpenses}");
            OnPaydayChanged?.Invoke(CurrentPayday);
        }
    }
    
    /// <summary>
    /// Removes an income item by details (removes first matching item)
    /// </summary>
    public bool RemoveIncomeItem(string details)
    {
        var item = incomeItems.FirstOrDefault(i => i.details == details);
        if (item != null)
        {
            incomeItems.Remove(item);
            Debug.Log($"Income item removed: {details}. Total Income: {TotalIncome}");
            OnPaydayChanged?.Invoke(CurrentPayday);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Removes an expense item by details (removes first matching item)
    /// </summary>
    public bool RemoveExpenseItem(string details)
    {
        var item = expenseItems.FirstOrDefault(i => i.details == details);
        if (item != null)
        {
            expenseItems.Remove(item);
            Debug.Log($"Expense item removed: {details}. Total Expenses: {TotalExpenses}");
            OnPaydayChanged?.Invoke(CurrentPayday);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Updates an income item's amount by details (updates first matching item)
    /// </summary>
    public bool UpdateIncomeItem(string details, float newAmount)
    {
        var item = incomeItems.FirstOrDefault(i => i.details == details);
        if (item != null && newAmount > 0)
        {
            item.amount = newAmount;
            Debug.Log($"Income item updated: {details} - {newAmount}. Total Income: {TotalIncome}");
            OnPaydayChanged?.Invoke(CurrentPayday);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Updates an expense item's amount by details (updates first matching item)
    /// </summary>
    public bool UpdateExpenseItem(string details, float newAmount)
    {
        var item = expenseItems.FirstOrDefault(i => i.details == details);
        if (item != null && newAmount > 0)
        {
            item.amount = newAmount;
            Debug.Log($"Expense item updated: {details} - {newAmount}. Total Expenses: {TotalExpenses}");
            OnPaydayChanged?.Invoke(CurrentPayday);
            return true;
        }
        return false;
    }
    
    // Legacy methods for backward compatibility
    /// <summary>
    /// Adds income to the player's total income (legacy method - creates item with "Income" as details)
    /// </summary>
    [System.Obsolete("Use AddIncomeItem(string details, float amount) instead")]
    public void AddIncome(float amount)
    {
        AddIncomeItem("Income", amount);
    }
    
    /// <summary>
    /// Adds expense to the player's total expenses (legacy method - creates item with "Expense" as details)
    /// </summary>
    [System.Obsolete("Use AddExpenseItem(string details, float amount) instead")]
    public void AddExpense(float amount)
    {
        AddExpenseItem("Expense", amount);
    }
    
    /// <summary>
    /// Adds a Real Estate item to the player's inventory
    /// </summary>
    public void AddRealEstate(string itemName)
    {
        if (!string.IsNullOrEmpty(itemName) && !realEstateItems.Contains(itemName))
        {
            realEstateItems.Add(itemName);
            Debug.Log($"Real Estate added: {itemName}");
        }
    }
    
    /// <summary>
    /// Adds a Business item to the player's inventory
    /// </summary>
    public void AddBusiness(string itemName)
    {
        if (!string.IsNullOrEmpty(itemName) && !businessItems.Contains(itemName))
        {
            businessItems.Add(itemName);
            Debug.Log($"Business added: {itemName}");
        }
    }
    
    /// <summary>
    /// Adds a Stock item to the player's inventory
    /// </summary>
    public void AddStock(string itemName)
    {
        if (!string.IsNullOrEmpty(itemName) && !stockItems.Contains(itemName))
        {
            stockItems.Add(itemName);
            Debug.Log($"Stock added: {itemName}");
        }
    }
    
    /// <summary>
    /// Adds a Unit Trust item to the player's inventory
    /// </summary>
    public void AddUnitTrust(string itemName)
    {
        if (!string.IsNullOrEmpty(itemName) && !unitTrustItems.Contains(itemName))
        {
            unitTrustItems.Add(itemName);
            Debug.Log($"Unit Trust added: {itemName}");
        }
    }
    
    /// <summary>
    /// Adds an Insurance item to the player's inventory
    /// </summary>
    public void AddInsurance(string itemName)
    {
        if (!string.IsNullOrEmpty(itemName) && !insuranceItems.Contains(itemName))
        {
            insuranceItems.Add(itemName);
            Debug.Log($"Insurance added: {itemName}");
        }
    }
    
    /// <summary>
    /// Removes a Real Estate item from the player's inventory
    /// </summary>
    public bool RemoveRealEstate(string itemName)
    {
        if (realEstateItems.Remove(itemName))
        {
            Debug.Log($"Real Estate removed: {itemName}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Removes a Business item from the player's inventory
    /// </summary>
    public bool RemoveBusiness(string itemName)
    {
        if (businessItems.Remove(itemName))
        {
            Debug.Log($"Business removed: {itemName}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Removes a Stock item from the player's inventory
    /// </summary>
    public bool RemoveStock(string itemName)
    {
        if (stockItems.Remove(itemName))
        {
            Debug.Log($"Stock removed: {itemName}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Removes a Unit Trust item from the player's inventory
    /// </summary>
    public bool RemoveUnitTrust(string itemName)
    {
        if (unitTrustItems.Remove(itemName))
        {
            Debug.Log($"Unit Trust removed: {itemName}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Removes an Insurance item from the player's inventory
    /// </summary>
    public bool RemoveInsurance(string itemName)
    {
        if (insuranceItems.Remove(itemName))
        {
            Debug.Log($"Insurance removed: {itemName}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Resets all financial data (useful for new game or testing)
    /// </summary>
    public void ResetFinance()
    {
        incomeItems.Clear();
        expenseItems.Clear();
        realEstateItems.Clear();
        businessItems.Clear();
        stockItems.Clear();
        unitTrustItems.Clear();
        insuranceItems.Clear();
        OnPaydayChanged?.Invoke(CurrentPayday);
        Debug.Log("Player finance data reset.");
    }
    
    /// <summary>
    /// Gets a summary of the player's financial status
    /// </summary>
    public string GetFinanceSummary()
    {
        string summary = $"Total Income: {TotalIncome:F2}\n" +
                         $"Total Expenses: {TotalExpenses:F2}\n" +
                         $"Current Payday: {CurrentPayday:F2}\n\n";
        
        summary += "Income Items:\n";
        foreach (var item in incomeItems)
        {
            summary += $"  - {item.details}: {item.amount:F2}\n";
        }
        
        summary += "\nExpense Items:\n";
        foreach (var item in expenseItems)
        {
            summary += $"  - {item.details}: {item.amount:F2}\n";
        }
        
        summary += $"\nReal Estate Items: {realEstateItems.Count}\n" +
                   $"Business Items: {businessItems.Count}\n" +
                   $"Stock Items: {stockItems.Count}\n" +
                   $"Unit Trust Items: {unitTrustItems.Count}\n" +
                   $"Insurance Items: {insuranceItems.Count}";
        
        return summary;
    }
}
