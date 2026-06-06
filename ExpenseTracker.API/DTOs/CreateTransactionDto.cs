namespace ExpenseTracker.API.DTOs
{
    public class CreateTransactionDto
    {

        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
        public int CategoryId { get; set; }
    }
}
