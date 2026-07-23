namespace CountryExplorer.Application.DTOs.Budget;

public class BudgetEstimateDto
{
    public decimal OriginalAmount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public decimal ConvertedAmount { get; set; }
    public string ToCurrency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
}