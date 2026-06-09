namespace ExpenseTracker.API.DTOs
{
    public class DashboardSummaryDto
    {

        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal CurrentBalance { get; set; }

        public decimal? MonthlyBudget { get; set; }
        public decimal BudgetUsagePercentage { get; set; }
        public string BudgetWarningMessage { get; set; } = string.Empty;
    }
}
