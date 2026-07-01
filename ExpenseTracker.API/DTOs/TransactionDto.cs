using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public Decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }

        public string CategoryName { get; set; } 
    }
}
