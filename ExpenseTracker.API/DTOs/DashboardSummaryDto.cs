namespace ExpenseTracker.API.DTOs
{
    public class DashboardSummaryDto
    {

        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}
